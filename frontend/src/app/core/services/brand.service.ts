import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// Proje tipi enum
export enum ProjectType {
  CWI = 0,
  AWC = 1,
}

// Marka DTO interface
export interface BrandDetailDto {
  id: number;
  code: string;
  name: string;
  logoUrl?: string;
  sortOrder: number;
  isActive: boolean;
  projectType: ProjectType;
  projectTypeName: string;
}

// Marka oluşturma DTO
export interface CreateBrandDto {
  code: string;
  name: string;
  logoUrl?: string;
  sortOrder: number;
  isActive: boolean;
  projectType: ProjectType;
}

// Marka güncelleme DTO
export interface UpdateBrandDto {
  id: number;
  code: string;
  name: string;
  logoUrl?: string;
  sortOrder: number;
  isActive: boolean;
  projectType: ProjectType;
}

// Marka listesi yanıtı
export interface BrandListResponse {
  data: BrandDetailDto[];
  totalCount: number;
}

// API yanıt wrapper
export interface ApiResult<T> {
  success: boolean;
  data: T;
  error?: string;
}

export interface BrandTableFilters {
  filterCode?: string;
  filterName?: string;
  filterProjectType?: string;
  filterSortOrder?: string;
  filterStatus?: string;
}

@Injectable({
  providedIn: 'root',
})
export class BrandService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/brands`;

  /**
   * Marka listesini getirir (sayfalama, arama ve ProjectType filtresi destekli)
   */
  getBrands(
    page: number = 1,
    pageSize: number = 10,
    searchText?: string,
    projectType?: ProjectType,
    sortField?: string,
    sortOrder: number = 1,
    filters?: BrandTableFilters,
  ): Observable<ApiResult<BrandListResponse>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('sortOrder', sortOrder.toString());

    if (searchText) {
      params = params.set('searchText', searchText);
    }

    if (projectType !== undefined && projectType !== null) {
      params = params.set('projectType', projectType.toString());
    }

    if (sortField) {
      params = params.set('sortField', sortField);
    }
    if (filters?.filterCode) params = params.set('filterCode', filters.filterCode);
    if (filters?.filterName) params = params.set('filterName', filters.filterName);
    if (filters?.filterProjectType)
      params = params.set('filterProjectType', filters.filterProjectType);
    if (filters?.filterSortOrder) params = params.set('filterSortOrder', filters.filterSortOrder);
    if (filters?.filterStatus) params = params.set('filterStatus', filters.filterStatus);

    return this.http.get<ApiResult<BrandListResponse>>(this.apiUrl, { params });
  }

  /**
   * Belirtilen Id'ye sahip markayı getirir
   */
  getBrandById(id: number): Observable<ApiResult<BrandDetailDto>> {
    return this.http.get<ApiResult<BrandDetailDto>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Yeni marka oluşturur
   */
  createBrand(brand: CreateBrandDto): Observable<ApiResult<BrandDetailDto>> {
    return this.http.post<ApiResult<BrandDetailDto>>(this.apiUrl, brand);
  }

  /**
   * Mevcut markayı günceller
   */
  updateBrand(id: number, brand: UpdateBrandDto): Observable<ApiResult<BrandDetailDto>> {
    return this.http.put<ApiResult<BrandDetailDto>>(`${this.apiUrl}/${id}`, brand);
  }

  /**
   * Markayı siler (soft delete)
   */
  deleteBrand(id: number): Observable<ApiResult<boolean>> {
    return this.http.delete<ApiResult<boolean>>(`${this.apiUrl}/${id}`);
  }
}
