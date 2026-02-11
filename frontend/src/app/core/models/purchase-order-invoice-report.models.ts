export interface PurchaseOrderInvoiceReportRequest {
  startDate?: Date;
  endDate?: Date;
  searchQuery?: string;
  page: number;
  pageSize: number;
  sortField?: string;
  sortOrder?: number;
}

export interface PurchaseOrderInvoiceReportItem {
  id: number;
  invoiceDate: string | Date;
  invoiceRefNum: string;
  orderRefNo: string;
  invoiceQty: number;
  invoiceAmount: number;
  orderQty: number;
  orderAmount: number;
  pendingQty: number;
  pendingAmount: number;
}

export interface PurchaseOrderInvoiceReportResponse {
  data: PurchaseOrderInvoiceReportItem[];
  totalCount: number;
}
