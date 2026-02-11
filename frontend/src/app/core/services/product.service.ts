import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BrandDto, ProductFilter, ProductListResponse, ProductDetailDto } from '../models/product.models';

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
}
