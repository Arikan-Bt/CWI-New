import { BaseRequest } from './base.models';

/**
 * Sipariş raporu filtreleme ve API isteği modeli
 */
export interface OrdersReportRequest extends BaseRequest {
  /** Başlangıç tarihi (ISO string) */
  startDate: string;
  /** Bitiş tarihi (ISO string) */
  endDate: string;
  /** Cari hesap kodu (Opsiyonel) */
  currentAccountCode?: string;
  /** Sipariş durumu (Opsiyonel) */
  orderStatus?: string;
  /** Ürün fotoğrafı gösterilsin mi? */
  displayProductPhoto: boolean;
  page?: number;
  pageSize?: number;
  sortField?: string;
  sortOrder?: number;
  brand?: string;
}

/**
 * Sipariş raporu liste satırı modeli
 */
export interface OrderReportItem {
  /** Cari hesap kodu */
  currentAccountCode: string;
  /** Cari hesap açıklaması */
  currentAccountDescription: string;
  /** Sipariş detayı/numarası */
  orderDetails: string;
  /** Sipariş durumu */
  status: string;
  /** Marka */
  brand: string;
  /** Sipariş tarihi */
  orderDate: string | Date;
  /** Talep edilen sevkiyat tarihi */
  requestedShipmentDate: string | Date;
  /** Toplam miktar */
  totalQty: number;
  /** İskonto tutarı */
  discount: number;
  /** Toplam tutar */
  total: number;
  /** Sipariş ID (Detaylar için) */
  orderId: number;

  // Edit paneli için eklenen alanlar
  address?: string;
  paymentType?: string;
  shipmentMethod?: string;
  orderDescription?: string;
  subTotal: number;
  grandTotal: number;
  season?: string;
}

/**
 * Sipariş güncelleme isteği
 */
export interface UpdateOrderRequest {
  orderId: number;
  paymentType?: string;
  discountPercent: number;
  requestedShipmentDate?: string;
  shipmentMethod?: string;
  status?: string;
  orderDescription?: string;
  removedProductCodes?: string[];
  warehouseSelections?: OrderWarehouseSelectionDto[];
}

export interface OrderWarehouseSelectionDto {
  productCode: string;
  warehouseId: number;
}

/**
 * Sipariş detay isteği modeli
 */
export interface OrderDetailRequest extends BaseRequest {
  /** Sipariş ID */
  orderId: number;
  /** Marka */
  brand?: string;
  /** Sayfa numarası */
  pageNumber: number;
  /** Sayfa boyutu */
  pageSize: number;
}

/**
 * Sipariş detay yanıt modeli
 */
export interface OrderDetailResponse {
  /** Ürün detayları */
  data: OrderDetailItem[];
  /** Toplam kayıt sayısı */
  totalCount: number;
}

/**
 * Sipariş detay satırı modeli
 */
export interface OrderDetailItem {
  /** Satır ID */
  id: number;
  /** Ürün kodu */
  productCode: string;
  /** Ürün adı */
  productName: string;
  /** Ürün resmi */
  picture?: string;
  /** Miktar */
  qty: number;
  /** Birim fiyat / miktar */
  amount: number;
  /** Satır toplamı */
  total: number;
  /** Satır notu */
  notes?: string;
  /** Ürün özellikleri (JSON) */
  attributes?: string;
}

/**
 * Sipariş kalemi güncelleme isteği
 */
export interface UpdateOrderItemRequest {
  orderId: number;
  productCode: string;
  qty: number;
  amount: number;
  notes?: string;
}

/**
 * Sipariş kalemi silme isteği
 */
export interface RemoveOrderItemRequest {
  orderId: number;
  productCode: string;
}

/**
 * Sipariş raporu yanıt modeli
 */
export interface OrdersReportResponse {
  /** Markalar listesi */
  brands: string[];
  /** Sipariş verileri */
  data: OrderReportItem[];
  totalCount: number;
}

export interface PackingListDto {
  items: PackingListItemDto[];
  cartons: PackingListCartonDto[];
}

export interface PackingListItemDto {
  orderItemId: number;
  productCode: string;
  productName: string;
  qty: number;
  cartonNo: string;
}

export interface PackingListCartonDto {
  id: number;
  cartonNo: string;
  netWeight?: number;
  grossWeight?: number;
  measurements: string;
}

export interface SavePackingListCommand {
  orderId: number;
  items: {
    orderItemId: number;
    cartonNo: string;
    qty: number;
  }[];
  cartons: PackingListCartonDto[];
}
