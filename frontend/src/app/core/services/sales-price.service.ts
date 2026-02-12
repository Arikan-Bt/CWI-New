import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Result } from '../models/result.model';

export interface ProductSalesPriceDto {
  id: number;
  productId: number;
  productSku: string;
  productName: string;
  customerId: number;
  customerName: string;
  price: number;
  currencyId: number;
  currencyCode: string;
  validFrom: string;
  validTo?: string;
  isActive: boolean;
}

export interface CreateProductSalesPriceDto {
  productId: number;
  customerId: number;
  price: number;
  currencyId: number;
  validFrom: string;
  validTo?: string;
  isActive: boolean;
}

export interface UpdateProductSalesPriceDto {
  id: number;
  productId: number;
  customerId: number;
  price: number;
  currencyId: number;
  validFrom: string;
  validTo?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class SalesPriceService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/ProductSalesPrices`;

  getPrices(customerId?: number, productId?: number): Observable<Result<ProductSalesPriceDto[]>> {
    let params = new HttpParams();
    if (customerId) params = params.set('customerId', customerId);
    if (productId) params = params.set('productId', productId);

    return this.http.get<Result<ProductSalesPriceDto[]>>(this.apiUrl, { params });
  }

  createPrice(data: CreateProductSalesPriceDto): Observable<Result<number>> {
    return this.http.post<Result<number>>(this.apiUrl, data);
  }

  updatePrice(id: number, data: UpdateProductSalesPriceDto): Observable<Result<number>> {
    return this.http.put<Result<number>>(`${this.apiUrl}/${id}`, data);
  }

  deletePrice(id: number): Observable<Result<number>> {
    return this.http.delete<Result<number>>(`${this.apiUrl}/${id}`);
  }
}
