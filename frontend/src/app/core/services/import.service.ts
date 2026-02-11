import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Result } from '../models/auth.models';
import {
  ImportOrderRequest,
  ImportResultDto,
  ValidateExcelResultDto,
} from '../models/import.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ImportService {
  private apiUrl = `${environment.apiUrl}/import`;

  constructor(private http: HttpClient) {}

  validateOrderExcel(fileContent: string): Observable<Result<ValidateExcelResultDto>> {
    return this.http.post<Result<ValidateExcelResultDto>>(`${this.apiUrl}/validate-order`, {
      fileContent,
    });
  }

  importOrderFromExcel(request: ImportOrderRequest): Observable<Result<ImportResultDto>> {
    return this.http.post<Result<ImportResultDto>>(`${this.apiUrl}/orders`, request);
  }

  downloadOrderTemplate(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/orders/template`, { responseType: 'blob' });
  }
}
