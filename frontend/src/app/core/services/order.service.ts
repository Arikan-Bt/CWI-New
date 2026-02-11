import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Result } from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private apiUrl = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  uploadSalesOrder(formData: FormData): Observable<Result<any>> {
    return this.http.post<Result<any>>(`${this.apiUrl}/upload-sales-order`, formData);
  }
}
