import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Result } from '../models/auth.models';
import { environment } from '../../../environments/environment';

export interface CreateVendorInvoiceResponse {
  message: string;
  id?: number;
}

export interface CreateVendorPaymentResponse {
  message: string;
  id?: number;
}

@Injectable({
  providedIn: 'root',
})
export class PurchasingService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/purchasing`;
  private reportsUrl = `${environment.apiUrl}/reports`;

  createInvoice(data: FormData): Observable<Result<CreateVendorInvoiceResponse>> {
    return this.http.post<Result<CreateVendorInvoiceResponse>>(`${this.apiUrl}/invoices`, data);
  }

  createPayment(data: FormData): Observable<Result<CreateVendorPaymentResponse>> {
    return this.http.post<Result<CreateVendorPaymentResponse>>(`${this.apiUrl}/payments`, data);
  }

  getVendorBalanceReport(request: any): Observable<Result<any>> {
    return this.http.post<Result<any>>(`${this.apiUrl}/balance-report`, request);
  }

  getVendors(): Observable<Result<{ label: string; value: string }[]>> {
    // Re-using the existing customers endpoint from ReportsController
    return this.http.get<Result<{ label: string; value: string }[]>>(
      `${this.reportsUrl}/customers`,
    );
  }
}
