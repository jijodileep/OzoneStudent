import { NgTemplateOutlet } from '@angular/common';
import {
  Component,
  ContentChildren,
  DestroyRef,
  EventEmitter,
  Input,
  Output,
  QueryList,
  afterNextRender,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, debounceTime } from 'rxjs';
import {
  TableQueryState,
  applyTableQuery,
  defaultTableQueryState,
} from '../../utils/table-query.util';
import { DataTableCellDirective } from './data-table-cell.directive';
import { DataTableColumn, ServerTableQuery } from './data-table.types';

@Component({
  selector: 'app-data-table-wrapper',
  standalone: true,
  imports: [FormsModule, NgTemplateOutlet, TranslateModule],
  templateUrl: './data-table-wrapper.component.html',
  styleUrl: './data-table-wrapper.component.scss',
})
export class DataTableWrapperComponent<T = unknown> {
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchSubject = new Subject<string>();

  @ContentChildren(DataTableCellDirective)
  cellTemplates!: QueryList<DataTableCellDirective>;

  @Input({ required: true }) columns: DataTableColumn<T>[] = [];
  @Input({ required: true }) set rows(value: T[]) {
    this.allRows.set(value ?? []);
  }

  readonly loading = input(false);
  readonly serverSide = input(false);
  readonly totalCount = input(0);
  readonly searchPlaceholder = input('table.search');
  readonly emptyMessage = input('table.empty');
  readonly showSearch = input(true);
  readonly pageSizeOptions = input<number[]>([5, 10, 25, 50]);

  @Output() queryChange = new EventEmitter<ServerTableQuery>();

  readonly allRows = signal<T[]>([]);
  readonly query = signal<TableQueryState>(defaultTableQueryState());

  readonly clientResult = computed(() =>
    applyTableQuery(this.allRows(), this.columns, this.query()),
  );

  readonly displayedRows = computed(() =>
    this.serverSide() ? this.allRows() : this.clientResult().rows,
  );

  readonly totalRecords = computed(() => {
    if (this.serverSide()) {
      const total = this.totalCount();
      return total > 0 ? total : this.allRows().length;
    }

    return this.clientResult().total;
  });

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalRecords() / this.pageSize())),
  );

  readonly currentPage = computed(() => this.query().page);
  readonly pageSize = computed(() => this.query().pageSize);

  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const windowSize = 5;
    const start = Math.max(1, current - Math.floor(windowSize / 2));
    const end = Math.min(total, start + windowSize - 1);
    const adjustedStart = Math.max(1, end - windowSize + 1);
    return Array.from({ length: end - adjustedStart + 1 }, (_, i) => adjustedStart + i);
  });

  readonly rangeStart = computed(() => {
    if (this.totalRecords() === 0) {
      return 0;
    }
    return (this.currentPage() - 1) * this.pageSize() + 1;
  });

  readonly rangeEnd = computed(() =>
    Math.min(this.currentPage() * this.pageSize(), this.totalRecords()),
  );

  constructor() {
    afterNextRender(() => {
      if (this.serverSide()) {
        this.emitQueryIfServerSide();
      }
    });

    this.searchSubject
      .pipe(debounceTime(300), takeUntilDestroyed(this.destroyRef))
      .subscribe((search) => {
        this.query.update((state) => ({ ...state, search, page: 1 }));
        this.emitQueryIfServerSide();
      });
  }

  templateForColumn(columnId: string): DataTableCellDirective | undefined {
    return this.cellTemplates?.find((template) => template.columnId === columnId);
  }

  trackRow(row: T, index: number): unknown {
    const candidate = row as { id?: string | number };
    return candidate.id ?? index;
  }

  onSearchInput(value: string): void {
    if (this.serverSide()) {
      this.searchSubject.next(value);
      return;
    }

    this.onSearchChange(value);
  }

  onSearchChange(value: string): void {
    this.query.update((state) => ({ ...state, search: value, page: 1 }));
    this.emitQueryIfServerSide();
  }

  onFilterChange(columnId: string, value: string): void {
    this.query.update((state) => ({
      ...state,
      filters: { ...state.filters, [columnId]: value },
      page: 1,
    }));
    this.emitQueryIfServerSide();
  }

  filterValue(columnId: string): string {
    return this.query().filters[columnId] ?? '';
  }

  sort(columnId: string): void {
    this.query.update((state) => {
      if (state.sortColumn !== columnId) {
        return { ...state, sortColumn: columnId, sortDirection: 'asc' };
      }

      if (state.sortDirection === 'asc') {
        return { ...state, sortDirection: 'desc' };
      }

      return { ...state, sortColumn: null, sortDirection: 'asc' };
    });
    this.emitQueryIfServerSide();
  }

  sortIcon(columnId: string): string {
    const state = this.query();
    if (state.sortColumn !== columnId) {
      return 'bi-arrow-down-up text-muted';
    }

    return state.sortDirection === 'asc' ? 'bi-sort-down-alt' : 'bi-sort-up';
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }

    this.query.update((state) => ({ ...state, page }));
    this.emitQueryIfServerSide();
  }

  onPageSizeChange(value: string): void {
    const pageSize = Number(value);
    this.query.update((state) => ({ ...state, pageSize, page: 1 }));
    this.emitQueryIfServerSide();
  }

  clearFilters(): void {
    this.query.set(defaultTableQueryState(this.pageSize()));
    this.emitQueryIfServerSide();
  }

  hasActiveFilters(): boolean {
    const state = this.query();
    return (
      !!state.search.trim() ||
      Object.values(state.filters).some((value) => !!value?.trim()) ||
      !!state.sortColumn
    );
  }

  private emitQueryIfServerSide(): void {
    if (!this.serverSide()) {
      return;
    }

    const state = this.query();
    const sortColumn = state.sortColumn
      ? this.columns.find((column) => column.id === state.sortColumn)
      : undefined;

    this.queryChange.emit({
      page: state.page,
      pageSize: state.pageSize,
      search: state.search.trim(),
      sortBy: sortColumn?.sortKey ?? sortColumn?.id ?? null,
      sortDirection: state.sortDirection,
      filters: { ...state.filters },
    });
  }
}
