export interface SummaryCustomerReportRequest {
  startDate?: Date;
  endDate?: Date;
  page?: number;
  pageSize?: number;
  sortField?: string;
  sortOrder?: number;
}

export interface SummaryCustomerItem {
  accountDescription: string;
  debit: number;
  credit: number;
  recBalance: number;
  currency: string;
}

export interface SummaryCustomerReportResponse {
  data: SummaryCustomerItem[];
  totalDebit: number;
  totalCredit: number;
  totalRecBalance: number;
  totalCount: number;
}
