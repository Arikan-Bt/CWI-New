/**
 * Sipariş durumları için sabitler
 */
export const OrderStatus = {
  Pending: 'Pending',
  PreOrder: 'PreOrder',
  Approved: 'Approved',
  PackedAndWaitingShipment: 'PackedAndWaitingShipment',
  Shipped: 'Shipped',
  Delivered: 'Delivered',
  Canceled: 'Canceled',
} as const;

/**
 * Stok kontrolü tetikleyecek durumlar
 */
export const STOCK_CHECK_TRIGGER_STATUSES: string[] = [OrderStatus.PackedAndWaitingShipment];
