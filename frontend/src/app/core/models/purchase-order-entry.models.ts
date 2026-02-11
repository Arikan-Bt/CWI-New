export interface CreatePurchaseOrderFromExcelRequest {
  vendorCode: string;
  orderDate: Date;
  deliveryDate?: Date;
  description: string;
  file: File;
}

export interface CreatePurchaseOrderFromExcelResponse {
  id: number;
  orderNumber: string;
  processedItemsCount: number;
  message: string;
}
