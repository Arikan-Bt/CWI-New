import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Result } from '../models/auth.models';
import {
  PaymentListDto,
  PaymentFilterDto,
  CreatePaymentDto,
  UploadReceiptDto,
  PaymentDetailDto,
} from '../models/payment.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  getPayments(filter: PaymentFilterDto): Observable<Result<any>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.startDate) params = params.set('startDate', filter.startDate);
    if (filter.endDate) params = params.set('endDate', filter.endDate);
    if (filter.status !== undefined && filter.status !== null)
      params = params.set('status', filter.status.toString());
    if (filter.currAccCode) params = params.set('currAccCode', filter.currAccCode);
    if (filter.projectType) params = params.set('projectType', filter.projectType);

    return this.http.get<Result<any>>(this.apiUrl, { params });
  }

  getPayment(id: number): Observable<Result<PaymentDetailDto>> {
    return this.http.get<Result<PaymentDetailDto>>(`${this.apiUrl}/${id}`);
  }

  createPayment(payment: CreatePaymentDto): Observable<Result<PaymentDetailDto>> {
    return this.http.post<Result<PaymentDetailDto>>(this.apiUrl, payment);
  }

  createPaymentWithFormData(formData: FormData): Observable<Result<any>> {
    return this.http.post<Result<any>>(this.apiUrl, formData);
  }

  uploadReceipt(id: number, file: UploadReceiptDto): Observable<Result<boolean>> {
    return this.http.post<Result<boolean>>(`${this.apiUrl}/${id}/receipt`, file);
  }

  approvePayment(id: number): Observable<Result<boolean>> {
    return this.http.post<Result<boolean>>(`${this.apiUrl}/${id}/approve`, {});
  }

  rejectPayment(id: number, reason: string): Observable<Result<boolean>> {
    return this.http.post<Result<boolean>>(`${this.apiUrl}/${id}/reject`, { reason });
  }

  getCurrencies(): Observable<Result<any[]>> {
    return this.http.get<Result<any[]>>(`${this.apiUrl}/currencies`);
  }
}
