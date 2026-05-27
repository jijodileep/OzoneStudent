import { DataTableColumn } from '../components/data-table/data-table.types';

export interface TableQueryState {
  search: string;
  filters: Record<string, string>;
  sortColumn: string | null;
  sortDirection: 'asc' | 'desc';
  page: number;
  pageSize: number;
}

export interface TableQueryResult<T> {
  rows: T[];
  total: number;
  totalPages: number;
}

export function defaultTableQueryState(pageSize = 10): TableQueryState {
  return {
    search: '',
    filters: {},
    sortColumn: null,
    sortDirection: 'asc',
    page: 1,
    pageSize,
  };
}

export function applyTableQuery<T>(
  rows: T[],
  columns: DataTableColumn<T>[],
  state: TableQueryState,
): TableQueryResult<T> {
  let result = [...rows];

  const searchTerm = state.search.trim().toLowerCase();
  if (searchTerm) {
    const searchableColumns = columns.filter((c) => c.searchable !== false);
    result = result.filter((row) =>
      searchableColumns.some((column) => {
        const value = column.displayValue(row);
        return value.toLowerCase().includes(searchTerm);
      }),
    );
  }

  for (const column of columns) {
    const filterValue = state.filters[column.id]?.trim();
    if (!filterValue) {
      continue;
    }

    if (column.filter?.match) {
      result = result.filter((row) => column.filter!.match!(row, filterValue));
      continue;
    }

    result = result.filter((row) => {
      const cell = (column.displayValue(row) ?? '').toLowerCase();
      return cell.includes(filterValue.toLowerCase());
    });
  }

  if (state.sortColumn) {
    const column = columns.find((c) => c.id === state.sortColumn);
    if (column) {
      const direction = state.sortDirection === 'asc' ? 1 : -1;
      result.sort((left, right) => {
        const leftValue = column.sortValue?.(left) ?? column.displayValue(left);
        const rightValue = column.sortValue?.(right) ?? column.displayValue(right);

        if (typeof leftValue === 'number' && typeof rightValue === 'number') {
          return (leftValue - rightValue) * direction;
        }

        if (typeof leftValue === 'boolean' && typeof rightValue === 'boolean') {
          return (Number(leftValue) - Number(rightValue)) * direction;
        }

        return String(leftValue).localeCompare(String(rightValue), undefined, {
          numeric: true,
          sensitivity: 'base',
        }) * direction;
      });
    }
  }

  const total = result.length;
  const totalPages = Math.max(1, Math.ceil(total / state.pageSize));
  const page = Math.min(state.page, totalPages);
  const start = (page - 1) * state.pageSize;
  const pagedRows = result.slice(start, start + state.pageSize);

  return { rows: pagedRows, total, totalPages };
}
