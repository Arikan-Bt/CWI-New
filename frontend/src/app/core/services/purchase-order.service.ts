import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Result } from '../models/auth.models';
import { environment } from '../../../environments/environment';
import {
  CreatePurchaseOrderFromExcelRequest,
  CreatePurchaseOrderFromExcelResponse,
} from '../models/purchase-order-entry.models';

export interface PurchaseOrderDto {
  id: string;
  date: Date;
  orderRefNo: string;
  documentNumber: string;
  customerSvc: string;
  qty: number;
  amount: number;
  status: 'Active' | 'Inactive';
}

export interface PurchaseOrderDetailDto {
  id: string;
  orderRefNo: string;
  date: Date;
  items: PurchaseOrderItemDto[];
}

export interface PurchaseOrderItemDto {
  id: string;
  productCode: string;
  productName: string;
  orderQty: number;
  orderUnitPrice: number;
  orderAmount: number;
  receive: number;
  balance: number;
  invoiceQty: number;
  invoiceUnitPrice: number;
  warehouseId?: number; // Selected warehouse for saving
  shelfNumber?: string;
  packList?: string;
  receivingDate?: Date;
}

export interface VendorLookupDto {
  label: string;
  value: string;
}

export interface PurchaseOrderRequest {
  startDate?: Date;
  endDate?: Date;
  page: number;
  pageSize: number;
  sortField?: string;
  sortOrder?: number;
  filterOrderRefNo?: string;
  filterCustomerSvc?: string;
}

@Injectable({
  providedIn: 'root',
})
export class PurchaseOrderService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/PurchaseOrders`;

  getOrders(request: PurchaseOrderRequest): Observable<Result<any>> {
    let params = new HttpParams()
      .set('page', request.page.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.startDate) params = params.set('startDate', request.startDate.toISOString());
    if (request.endDate) params = params.set('endDate', request.endDate.toISOString());
    if (request.sortField) params = params.set('sortField', request.sortField);
    if (request.sortOrder) params = params.set('sortOrder', request.sortOrder.toString());
    if (request.filterOrderRefNo) params = params.set('filterOrderRefNo', request.filterOrderRefNo);
    if (request.filterCustomerSvc)
      params = params.set('filterCustomerSvc', request.filterCustomerSvc);

    return this.http.get<Result<any>>(this.apiUrl, { params });
  }

  getOrderDetails(id: string): Observable<Result<PurchaseOrderDetailDto>> {
    return this.http.get<Result<PurchaseOrderDetailDto>>(`${this.apiUrl}/${id}`);
  }

  updateStatus(id: string, status: string): Observable<Result<boolean>> {
    return this.http.put<Result<boolean>>(`${this.apiUrl}/${id}/status`, { id, status });
  }

  saveInvoice(
    orderId: string,
    invoiceNumber: string,
    invoiceDate: Date,
    lines: any[],
    invoiceFile?: File | null,
  ): Observable<Result<boolean>> {
    const formData = new FormData();
    formData.append('orderId', orderId);
    formData.append('invoiceNumber', invoiceNumber);
    formData.append('invoiceDate', invoiceDate.toISOString());

    lines.forEach((l, index) => {
      formData.append(`lines[${index}].itemId`, l.id?.toString() ?? '');
      formData.append(`lines[${index}].invoiceQty`, (l.invoiceQty ?? 0).toString());
      formData.append(`lines[${index}].invoiceUnitPrice`, (l.invoiceUnitPrice ?? 0).toString());
      if (l.warehouseId !== undefined && l.warehouseId !== null) {
        formData.append(`lines[${index}].warehouseId`, l.warehouseId.toString());
      }
      if (l.shelfNumber) {
        formData.append(`lines[${index}].shelfNumber`, l.shelfNumber);
      }
      if (l.packList) {
        formData.append(`lines[${index}].packList`, l.packList);
      }
      if (l.receivingDate) {
        const receivingDate =
          l.receivingDate instanceof Date ? l.receivingDate.toISOString() : l.receivingDate;
        formData.append(`lines[${index}].receivingDate`, receivingDate);
      }
    });

    if (invoiceFile) {
      formData.append('invoiceFile', invoiceFile);
    }

    return this.http.post<Result<boolean>>(`${this.apiUrl}/${orderId}/invoice`, formData);
  }

  uploadExcel(
    request: CreatePurchaseOrderFromExcelRequest,
  ): Observable<Result<CreatePurchaseOrderFromExcelResponse>> {
    const formData = new FormData();
    formData.append('vendorCode', request.vendorCode);
    formData.append('orderDate', request.orderDate.toISOString());
    if (request.deliveryDate) {
      formData.append('deliveryDate', request.deliveryDate.toISOString());
    }
    formData.append('description', request.description);
    formData.append('file', request.file);

    return this.http.post<Result<CreatePurchaseOrderFromExcelResponse>>(
      `${this.apiUrl}/upload-excel`,
      formData,
    );
  }

  getVendors(onlyVendors: boolean = false): Observable<Result<VendorLookupDto[]>> {
    let params = new HttpParams();
    if (onlyVendors) {
      params = params.set('onlyVendors', 'true');
    }
    return this.http.get<Result<VendorLookupDto[]>>(`${environment.apiUrl}/Reports/customers`, {
      params,
    });
  }

  downloadTemplate(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/template`, { responseType: 'blob' });
  }
}
