export interface DataTableFilterOption {
  label: string;
  value: string;
}

export interface DataTableColumn<T = unknown> {
  id: string;
  header: string;
  sortable?: boolean;
  searchable?: boolean;
  /** API sort field when using server-side mode */
  sortKey?: string;
  filter?: {
    type: 'text' | 'select';
    placeholder?: string;
    options?: DataTableFilterOption[];
    /** Maps filter value to API query param name */
    serverParam?: string;
    match?: (row: T, value: string) => boolean;
  };
  displayValue: (row: T) => string;
  sortValue?: (row: T) => string | number | boolean;
  headerClass?: string;
  cellClass?: string;
}

export interface ServerTableQuery {
  page: number;
  pageSize: number;
  search: string;
  sortBy: string | null;
  sortDirection: 'asc' | 'desc';
  filters: Record<string, string>;
}
