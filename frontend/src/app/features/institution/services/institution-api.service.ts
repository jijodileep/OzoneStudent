import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { EnvironmentConfigService } from '../../../core/services/environment-config.service';
import { ServerTableQuery } from '../../../shared/components/data-table/data-table.types';

export interface AcademicYearDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  status: string;
}

export interface ListAcademicYearsResult {
  items: AcademicYearDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateAcademicYearRequest {
  name: string;
  startDate: string;
  endDate: string;
  setAsCurrent?: boolean;
}

@Injectable({ providedIn: 'root' })
export class InstitutionApiService {
  private readonly http = inject(HttpClient);
  private readonly env = inject(EnvironmentConfigService);

  listAcademicYears(query: ServerTableQuery): Observable<ListAcademicYearsResult> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize);

    if (query.search) {
      params = params.set('search', query.search);
    }

    if (query.sortBy) {
      params = params.set('sortBy', query.sortBy);
      params = params.set('sortDirection', query.sortDirection);
    }

    const status = query.filters['status'];
    if (status) {
      params = params.set('status', status);
    }

    const current = query.filters['current'];
    if (current === 'yes') {
      params = params.set('isCurrent', 'true');
    } else if (current === 'no') {
      params = params.set('isCurrent', 'false');
    }

    return this.http
      .get<ListAcademicYearsResult>(`${this.env.apiBaseUrl}/academic-years`, { params })
      .pipe(map(normalizeListAcademicYearsResult));
  }

  createAcademicYear(body: CreateAcademicYearRequest): Observable<AcademicYearDto> {
    return this.http.post<AcademicYearDto>(`${this.env.apiBaseUrl}/academic-years`, body);
  }

  setCurrentAcademicYear(id: string): Observable<AcademicYearDto> {
    return this.http.post<AcademicYearDto>(
      `${this.env.apiBaseUrl}/academic-years/${id}/set-current`,
      {},
    );
  }
}

function normalizeListAcademicYearsResult(raw: ListAcademicYearsResult & {
  Items?: AcademicYearDto[];
  TotalCount?: number;
  Page?: number;
  PageSize?: number;
}): ListAcademicYearsResult {
  const items = raw.items ?? raw.Items ?? [];
  const totalCount = raw.totalCount ?? raw.TotalCount ?? items.length;

  return {
    items,
    totalCount,
    page: raw.page ?? raw.Page ?? 1,
    pageSize: raw.pageSize ?? raw.PageSize ?? 10,
  };
}
