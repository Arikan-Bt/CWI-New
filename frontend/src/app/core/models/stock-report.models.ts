export interface StockReportRequest {
  searchValue?: string;
  brand?: string;
  page?: number;
  pageSize?: number;
  sortField?: string;
  sortOrder?: number;
  filterItemCode?: string;
  filterItemDescription?: string;
  filterShelfNumber?: string;
}

export interface StockReportResponse {
  items: StockReportItem[];
  totalCount: number;
}

export interface StockReportItem {
  productId: number;
  picture: string;
  itemCode: string;
  itemDescription: string;
  stock: number;
  reserved: number;
  available: number;
  incomingStock: number;
  retailSalesPrice: number;
  specialNote: string;
  brand: string;
  shelfNumber?: string;
  details?: StockReportDetail[];
}

export interface StockReportDetail {
  shelfNumber?: string;
  packList?: string;
  receiveDate?: string;
  occurredAt?: string;
  quantity: number;
  movementType?: string;
  movementGroup?: string;
  referenceNo?: string;
  sourceDocumentType?: string;
  warehouseId?: number;
  warehouseName?: string;
  supplierName?: string;
}

export interface UpdateStockNoteRequest {
  itemCode: string;
  note: string;
}
