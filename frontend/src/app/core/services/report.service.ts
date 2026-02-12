import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Result } from '../models/auth.models';
import {
  ReportFilterDto,
  OrderReportDto,
  CustomerBalanceReportDto,
  StockReportDto,
} from '../models/report.models';
import {
  OrdersReportRequest,
  OrdersReportResponse,
  OrderDetailItem,
  OrderDetailRequest,
  OrderDetailResponse,
  UpdateOrderRequest,
  UpdateOrderItemRequest,
  RemoveOrderItemRequest,
  PackingListDto,
  SavePackingListCommand,
} from '../models/orders-report.models';
import {
  StockReportRequest,
  StockReportResponse,
  UpdateStockNoteRequest,
} from '../models/stock-report.models';
import {
  PurchaseOrderInvoiceReportRequest,
  PurchaseOrderInvoiceReportResponse,
} from '../models/purchase-order-invoice-report.models';
import {
  CustomerBalanceReportRequest,
  CustomerBalanceReportResponse,
  CustomerReferenceDto,
  CustomerReferencesResponse,
  CancelledInvoicesResponse,
  CreateDebitNoteRequest,
} from '../models/customer-balance.report.models';
import {
  CustomerPaymentDetailReportRequest,
  CustomerPaymentDetailReportResponse,
} from '../models/customer-payment-detail.models';
import {
  SummaryCustomerReportRequest,
  SummaryCustomerReportResponse,
} from '../models/summary-customer.report.models';
import { ItemOrderCheckRequest, ItemOrderCheckResponse } from '../models/item-order-check.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  private apiUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  private createParams(filter: ReportFilterDto): HttpParams {
    let params = new HttpParams();
    if (filter.startDate) params = params.set('startDate', filter.startDate);
    if (filter.endDate) params = params.set('endDate', filter.endDate);
    if (filter.currAccCode) params = params.set('currAccCode', filter.currAccCode);
    if (filter.categoryCode) params = params.set('categoryCode', filter.categoryCode);
    if (filter.projectType) params = params.set('projectType', filter.projectType);
    return params;
  }

  exportOrderReport(filter: ReportFilterDto): Observable<Blob> {
    const params = this.createParams(filter);
    return this.http.get(`${this.apiUrl}/orders/export`, { params, responseType: 'blob' });
  }

  exportCustomerBalanceReport(filter: ReportFilterDto): Observable<Blob> {
    const params = this.createParams(filter);
    return this.http.get(`${this.apiUrl}/customer-balance/export`, {
      params,
      responseType: 'blob',
    });
  }

  getStockReportData(request: StockReportRequest): Observable<Result<StockReportResponse>> {
    return this.http.post<Result<StockReportResponse>>(`${this.apiUrl}/stock`, request);
  }

  updateStockNote(request: UpdateStockNoteRequest): Observable<Result<boolean>> {
    return this.http.post<Result<boolean>>(`${this.apiUrl}/stock/update-note`, request);
  }

  getBrands(): Observable<Result<{ id: number; name: string }[]>> {
    return this.http.get<Result<{ id: number; name: string }[]>>(`${this.apiUrl}/brands`);
  }

  exportStockReport(brand: string, searchValue: string): Observable<Blob> {
    let params = new HttpParams();
    if (brand) params = params.set('brand', brand);
    if (searchValue) params = params.set('searchValue', searchValue);
    return this.http.get(`${this.apiUrl}/stock/export`, { params, responseType: 'blob' });
  }

  getOrdersReport(request: OrdersReportRequest): Observable<Result<OrdersReportResponse>> {
    return this.http.post<Result<OrdersReportResponse>>(`${this.apiUrl}/orders`, request);
  }

  getOrderDetails(request: OrderDetailRequest): Observable<Result<OrderDetailResponse>> {
    let params = new HttpParams()
      .set('pageNumber', request.pageNumber.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.brand) params = params.set('brand', request.brand);

    return this.http.get<Result<OrderDetailResponse>>(
      `${this.apiUrl}/orders/${request.orderId}/details`,
      {
        params,
      },
    );
  }

  getCustomers(): Observable<Result<{ label: string; value: string }[]>> {
    return this.http.get<Result<{ label: string; value: string }[]>>(`${this.apiUrl}/customers`);
  }

  getOrderStatuses(): Observable<Result<{ label: string; value: string }[]>> {
    return this.http.get<Result<{ label: string; value: string }[]>>(
      `${this.apiUrl}/order-statuses`,
    );
  }

  getPurchaseOrderInvoiceReport(
    request: PurchaseOrderInvoiceReportRequest,
  ): Observable<Result<PurchaseOrderInvoiceReportResponse>> {
    return this.http.post<Result<PurchaseOrderInvoiceReportResponse>>(
      `${this.apiUrl}/purchase-order-invoices`,
      request,
    );
  }

  getCustomerBalanceData(
    request: CustomerBalanceReportRequest,
  ): Observable<Result<CustomerBalanceReportResponse>> {
    return this.http.post<Result<CustomerBalanceReportResponse>>(
      `${this.apiUrl}/customer-balance`,
      request,
    );
  }

  getCustomerPaymentDetails(
    request: CustomerPaymentDetailReportRequest,
  ): Observable<Result<CustomerPaymentDetailReportResponse>> {
    return this.http.post<Result<CustomerPaymentDetailReportResponse>>(
      `${this.apiUrl}/payment-details`,
      request,
    );
  }

  getSummaryCustomerData(
    request: SummaryCustomerReportRequest,
  ): Observable<Result<SummaryCustomerReportResponse>> {
    return this.http.post<Result<SummaryCustomerReportResponse>>(
      `${this.apiUrl}/summary-customer`,
      request,
    );
  }

  /**
   * Müşteriye ait balance > 0 olan referansları getirir
   * Add Payment modal'ında Reference Code seçimi için kullanılır
   */
  getCustomerReferences(customerCode: string): Observable<Result<CustomerReferencesResponse>> {
    return this.http.get<Result<CustomerReferencesResponse>>(
      `${this.apiUrl}/customer-references/${customerCode}`,
    );
  }

  getCustomerCancelledInvoices(
    customerCode: string,
  ): Observable<Result<CancelledInvoicesResponse>> {
    return this.http.get<Result<CancelledInvoicesResponse>>(
      `${this.apiUrl}/customer-cancelled-invoices/${customerCode}`,
    );
  }

  createDebitNoteAndExport(request: CreateDebitNoteRequest): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/debit-note/export`, request, {
      responseType: 'blob',
    });
  }

  getProducts(): Observable<Result<{ label: string; value: string }[]>> {
    return this.http.get<Result<{ label: string; value: string }[]>>(`${this.apiUrl}/products`);
  }

  getItemOrderCheckData(
    request: ItemOrderCheckRequest,
  ): Observable<Result<ItemOrderCheckResponse>> {
    return this.http.post<Result<ItemOrderCheckResponse>>(
      `${this.apiUrl}/item-order-check`,
      request,
    );
  }

  getPaymentMethods(): Observable<Result<{ label: string; value: string }[]>> {
    return this.http.get<Result<{ label: string; value: string }[]>>(
      `${this.apiUrl}/payment-methods`,
    );
  }

  getShipmentTerms(): Observable<Result<{ label: string; value: string }[]>> {
    return this.http.get<Result<{ label: string; value: string }[]>>(
      `${this.apiUrl}/shipment-terms`,
    );
  }

  updateOrder(request: UpdateOrderRequest): Observable<Result<boolean>> {
    return this.http.post<Result<boolean>>(`${this.apiUrl}/orders/update`, request);
  }

  updateOrderItem(request: UpdateOrderItemRequest): Observable<Result<boolean>> {
    return this.http.post<Result<boolean>>(`${this.apiUrl}/orders/items/update`, request);
  }

  removeOrderItem(request: RemoveOrderItemRequest): Observable<Result<boolean>> {
    return this.http.post<Result<boolean>>(`${this.apiUrl}/orders/items/remove`, request);
  }

  getProformaInvoice(orderId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/orders/${orderId}/proforma-invoice`, {
      responseType: 'blob',
    });
  }

  getInvoice(orderId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/orders/${orderId}/invoice`, {
      responseType: 'blob',
    });
  }

  getPackingList(orderId: number): Observable<Result<PackingListDto>> {
    return this.http.get<Result<PackingListDto>>(`${this.apiUrl}/orders/${orderId}/packing-list`);
  }

  savePackingList(request: SavePackingListCommand): Observable<Result<boolean>> {
    return this.http.post<Result<boolean>>(`${this.apiUrl}/orders/packing-list`, request);
  }
}
