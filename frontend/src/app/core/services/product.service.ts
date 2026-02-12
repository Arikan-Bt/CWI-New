import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BrandDto,
  ProductFilter,
  ProductListResponse,
  ProductDetailDto,
} from '../models/product.models';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/products`;

  getVendorProducts(filter: ProductFilter): Observable<ProductListResponse> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.searchTerm) {
      params = params.set('searchTerm', filter.searchTerm);
    }

    if (filter.brandIds && filter.brandIds.length > 0) {
      filter.brandIds.forEach((id) => {
        params = params.append('brandIds', id.toString());
      });
    }

    return this.http.get<ProductListResponse>(`${this.apiUrl}/vendor-products`, { params });
  }

  getBrands(): Observable<BrandDto[]> {
    return this.http.get<BrandDto[]>(`${this.apiUrl}/brands`);
  }

  getProductDetail(id: number): Observable<ProductDetailDto> {
    return this.http.get<ProductDetailDto>(`${this.apiUrl}/${id}`);
  }

  uploadImage(productId: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/${productId}/images`, formData);
  }

  deleteImage(productId: number, imageUrl: string): Observable<any> {
    // Extract ID or filename from URL if API expects that,
    // or send URL if API expects that.
    // Assuming API takes image URL or ID.
    // If backend expects ID, we need to extract it.
    // However, usually delete ops on resources use DELETE verb.
    // If we just need to delete by URL, we might need a specific endpoint.
    // For now, let's assume a DELETE endpoint with query param or body.
    return this.http.delete(`${this.apiUrl}/${productId}/images`, {
      body: { imageUrl: imageUrl },
    });
  }

  getProductsLookup(): Observable<ProductListResponse> {
    // Reusing getVendorProducts for now as it returns a list of products
    // We might need to adjust arguments if required
    return this.getVendorProducts({ pageNumber: 1, pageSize: 1000 });
  }
}
