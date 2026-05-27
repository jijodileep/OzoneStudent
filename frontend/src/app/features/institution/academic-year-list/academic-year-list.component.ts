import { Component, inject, signal } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { LocaleService } from '../../../core/services/locale.service';
import { DataTableCellDirective } from '../../../shared/components/data-table/data-table-cell.directive';
import { DataTableWrapperComponent } from '../../../shared/components/data-table/data-table-wrapper.component';
import {
  DataTableColumn,
  ServerTableQuery,
} from '../../../shared/components/data-table/data-table.types';
import { formatDateOnlyDisplay } from '../../../shared/utils/date-only.util';
import { CreateAcademicYearModalService } from '../services/create-academic-year-modal.service';
import {
  AcademicYearDto,
  InstitutionApiService,
} from '../services/institution-api.service';

@Component({
  selector: 'app-academic-year-list',
  standalone: true,
  imports: [TranslateModule, DataTableWrapperComponent, DataTableCellDirective],
  templateUrl: './academic-year-list.component.html',
  styleUrl: './academic-year-list.component.scss',
})
export class AcademicYearListComponent {
  private readonly api = inject(InstitutionApiService);
  private readonly createModal = inject(CreateAcademicYearModalService);
  readonly locale = inject(LocaleService);

  readonly items = signal<AcademicYearDto[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  private lastQuery = signal<ServerTableQuery>(this.defaultQuery());

  readonly columns: DataTableColumn<AcademicYearDto>[] = [
    {
      id: 'name',
      header: 'table.name',
      sortKey: 'name',
      displayValue: (row) => row.name,
      sortValue: (row) => row.name,
      cellClass: 'fw-semibold',
    },
    {
      id: 'dates',
      header: 'table.period',
      sortKey: 'startdate',
      displayValue: (row) => `${row.startDate} – ${row.endDate}`,
      sortValue: (row) => row.startDate,
    },
    {
      id: 'status',
      header: 'table.status',
      sortKey: 'status',
      filter: {
        type: 'select',
        placeholder: 'table.allStatuses',
        options: [
          { label: 'table.statusActive', value: 'Active' },
          { label: 'table.statusUpcoming', value: 'Upcoming' },
          { label: 'table.statusClosed', value: 'Closed' },
        ],
      },
      displayValue: (row) => row.status,
      sortValue: (row) => row.status,
    },
    {
      id: 'current',
      header: 'table.current',
      sortKey: 'iscurrent',
      filter: {
        type: 'select',
        placeholder: 'table.all',
        options: [
          { label: 'table.currentOnly', value: 'yes' },
          { label: 'table.notCurrent', value: 'no' },
        ],
      },
      displayValue: (row) => (row.isCurrent ? 'Yes' : 'No'),
      sortValue: (row) => row.isCurrent,
    },
    {
      id: 'actions',
      header: 'table.actions',
      sortable: false,
      searchable: false,
      headerClass: 'text-end',
      cellClass: 'text-end',
      displayValue: () => '',
    },
  ];

  onQueryChange(query: ServerTableQuery): void {
    this.lastQuery.set(query);
    this.load(query);
  }

  openCreateModal(): void {
    this.createModal.open().subscribe((created) => {
      if (created) {
        this.load(this.lastQuery());
      }
    });
  }

  load(query?: ServerTableQuery): void {
    const activeQuery = query ?? this.lastQuery();
    this.lastQuery.set(activeQuery);
    this.loading.set(true);
    this.errorMessage.set(null);

    this.api.listAcademicYears(activeQuery).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('academicYears.loadFailed');
        this.loading.set(false);
      },
    });
  }

  setCurrent(item: AcademicYearDto): void {
    this.api.setCurrentAcademicYear(item.id).subscribe({
      next: () => this.load(this.lastQuery()),
      error: () => this.errorMessage.set('academicYears.setCurrentFailed'),
    });
  }

  formatDate(value: string): string {
    return formatDateOnlyDisplay(value, this.locale.currentLanguage().code);
  }

  statusBadgeClass(status: string): string {
    switch (status) {
      case 'Active':
        return 'text-bg-success';
      case 'Upcoming':
        return 'text-bg-info';
      case 'Closed':
        return 'text-bg-secondary';
      default:
        return 'text-bg-light';
    }
  }

  private defaultQuery(): ServerTableQuery {
    return {
      page: 1,
      pageSize: 10,
      search: '',
      sortBy: null,
      sortDirection: 'asc',
      filters: {},
    };
  }
}
