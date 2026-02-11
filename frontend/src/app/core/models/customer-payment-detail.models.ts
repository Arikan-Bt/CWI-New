export interface CustomerPaymentDetailReportRequest {
  customerCode?: string | null;
  startDate?: Date;
  endDate?: Date;
  page?: number;
  pageSize?: number;
  sortField?: string;
  sortOrder?: number;
}

export interface CustomerPaymentDetailItem {
  date: string | Date;
  refNo1: string;
  description: string;
  invoiceNo: string;
  docType: string;
  refNo2: string;
  debit: number;
  credit: number;
  balance: number;
  receiptFilePath?: string | null;
}

export interface CustomerPaymentDetailReportResponse {
  data: CustomerPaymentDetailItem[];
  totalDebit: number;
  totalCredit: number;
  totalBalance: number;
  totalCount: number;
}
