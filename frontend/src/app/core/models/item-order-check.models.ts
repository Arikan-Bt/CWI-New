export interface ItemOrderCheckRequest {
  sku: string;
  page?: number;
  pageSize?: number;
}

export interface ItemOrderCheckResponse {
  summary: ItemOrderSummary;
  movements: ItemMovement[];
  totalMovements: number;
}

export interface ItemOrderSummary {
  itemCode: string;
  purchaseQty: number;
  salesQty: number;
  wareHouseQty: number;
  reserveQty: number;
  availableQty: number;
  incomingStock: number;
}

export interface ItemMovement {
  status: string;
  account: string;
  qty: number;
  itemCode: string;
}
