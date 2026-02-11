/**
 * Stok düzenleme isteği modeli
 */
export interface CreateStockAdjustmentRequest {
  adjustmentDate: Date;
  description: string;
  file: File;
}

/**
 * Stok düzenleme yanıt modeli
 */
export interface CreateStockAdjustmentResponse {
  id: number;
  processedItemsCount: number;
  skippedItemsCount: number;
  warnings: StockAdjustmentWarning[];
  message: string;
}

export interface StockAdjustmentWarning {
  row: number;
  productCode: string;
  reason: string;
}
