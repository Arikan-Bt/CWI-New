export interface CustomerBalanceReportRequest {
  startDate?: Date;
  endDate?: Date;
  page?: number;
  pageSize?: number;
  sortField?: string;
  sortOrder?: number;
}

// ... existing item interface ...

export interface CustomerBalanceReportItem {
  currAccCode: string;
  currAccDescription: string;
  date: string | Date;
  referenceId: string;
  totalAmount: number;
  totalPayment: number;
  balance: number;
  orderStatus: string;
  status: string;
}

export interface CustomerBalanceReportResponse {
  data: CustomerBalanceReportItem[];
  totalCount: number;
}

/**
 * Müşteri referans bilgisi - Add Payment modal'ında Reference Code seçimi için kullanılır
 */
export interface CustomerReferenceDto {
  referenceId: string;
  date: Date | string;
  totalAmount: number;
  totalPayment: number;
  balance: number;
}

export interface CustomerReferencesResponse {
  data: CustomerReferenceDto[];
}
