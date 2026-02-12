import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Result } from '../models/result.model';

export interface ProductPurchasePriceDto {
  id: number;
  productId: number;
  productSku: string;
  productName: string;
  vendorId: number;
  vendorName: string;
  price: number;
  currencyId: number;
  currencyCode: string;
  validFrom: string;
  validTo?: string;
  isActive: boolean;
}

export interface CreateProductPurchasePriceDto {
  productId: number;
  vendorId: number;
  price: number;
  currencyId: number;
  validFrom: string;
  validTo?: string;
  isActive: boolean;
}

export interface UpdateProductPurchasePriceDto {
  id: number;
  productId: number;
  vendorId: number;
  price: number;
  currencyId: number;
  validFrom: string;
  validTo?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class PurchasePriceService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/ProductPurchasePrices`;

  getPrices(vendorId?: number, productId?: number): Observable<Result<ProductPurchasePriceDto[]>> {
    let params = new HttpParams();
    if (vendorId) params = params.set('vendorId', vendorId);
    if (productId) params = params.set('productId', productId);

    return this.http.get<Result<ProductPurchasePriceDto[]>>(this.apiUrl, { params });
  }

  createPrice(data: CreateProductPurchasePriceDto): Observable<Result<number>> {
    return this.http.post<Result<number>>(this.apiUrl, data);
  }

  updatePrice(id: number, data: UpdateProductPurchasePriceDto): Observable<Result<number>> {
    return this.http.put<Result<number>>(`${this.apiUrl}/${id}`, data);
  }

  deletePrice(id: number): Observable<Result<number>> {
    return this.http.delete<Result<number>>(`${this.apiUrl}/${id}`);
  }
}
