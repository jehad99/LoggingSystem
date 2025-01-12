export interface Log {
  message: string;
  level: 'Info' | 'Warning' | 'Error';
  timestamp: string;
}

export interface LogResponse {
  page: number;
  pageSize: number;
  totalRecords: number;
  data: Log[];
}