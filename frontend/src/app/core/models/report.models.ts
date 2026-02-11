export interface ReportFilterDto {
  startDate?: string;
  endDate?: string;
  currAccCode?: string;
  categoryCode?: string;
  projectType: 'CWI' | 'AWC';
}

export interface OrderReportDto {
  startDate: string;
  endDate: string;
  totalOrders: number;
  pendingOrders: number;
  approvedOrders: number;
  cancelledOrders: number;
  totalAmount: number;
  items: OrderReportItemDto[];
}

export interface OrderReportItemDto {
  orderNumber: string;
  orderDate: string;
  currAccCode: string;
  currAccDescription: string;
  status: string;
  totalItems: number;
  grandTotal: number;
}

export interface CustomerBalanceReportDto {
  reportDate: string;
  totalCustomers: number;
  totalBalance: number;
  totalDebit: number;
  totalCredit: number;
  items: CustomerBalanceReportItemDto[];
}

export interface CustomerBalanceReportItemDto {
  currAccCode: string;
  currAccDescription: string;
  debit: number;
  credit: number;
  balance: number;
  creditLimit: number;
  availableLimit: number;
}

export interface StockReportDto {
  reportDate: string;
  totalProducts: number;
  totalWarehouses: number;
  totalStockQuantity: number;
  totalStockValue: number;
  items: StockReportItemDto[];
}

export interface StockReportItemDto {
  itemCode: string;
  itemDescription: string;
  categoryDescription: string;
  listPrice: number;
  totalQuantity: number;
  totalValue: number;
  warehouseDetails: WarehouseStockDto[];
}

export interface WarehouseStockDto {
  warehouseCode: string;
  warehouseName: string;
  quantity: number;
}
