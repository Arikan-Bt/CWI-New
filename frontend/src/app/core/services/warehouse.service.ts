import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// Warehouse DTOs
export interface WarehouseDto {
  id: number;
  code: string;
  name: string;
  address?: string;
  isActive: boolean;
  isDefault: boolean;
}

export interface WarehouseDetailDto extends WarehouseDto {
  createdAt: Date;
  modifiedAt?: Date;
}

export interface CreateWarehouseDto {
  code: string;
  name: string;
  address?: string;
  isDefault: boolean;
}

export interface UpdateWarehouseDto {
  id: number;
  code: string;
  name: string;
  address?: string;
  isActive: boolean;
  isDefault: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiResult<T> {
  success: boolean;
  data: T;
  error?: string;
}

export interface WarehouseTableFilters {
  filterCode?: string;
  filterName?: string;
  filterAddress?: string;
  filterStatus?: string;
  filterDefault?: string;
}

@Injectable({
  providedIn: 'root',
})
export class WarehouseService {
  private apiUrl = `${environment.apiUrl}/inventory`;

  constructor(private http: HttpClient) {}

  /**
   * Tüm aktif depoları listele (dropdown için)
   */
  getAllWarehouses(): Observable<ApiResult<WarehouseDto[]>> {
    return this.http.get<ApiResult<WarehouseDto[]>>(`${this.apiUrl}/warehouses`);
  }

  /**
   * Depoları paginated listele (yönetim paneli için)
   */
  getWarehouses(
    page: number,
    pageSize: number,
    searchTerm?: string,
    sortField?: string,
    sortOrder: number = 1,
    filters?: WarehouseTableFilters,
  ): Observable<ApiResult<PagedResult<WarehouseDto>>> {
    const params: Record<string, string> = {
      page: page.toString(),
      pageSize: pageSize.toString(),
      searchTerm: searchTerm || '',
      sortOrder: sortOrder.toString(),
    };
    if (sortField) {
      params['sortField'] = sortField;
    }
    if (filters?.filterCode) params['filterCode'] = filters.filterCode;
    if (filters?.filterName) params['filterName'] = filters.filterName;
    if (filters?.filterAddress) params['filterAddress'] = filters.filterAddress;
    if (filters?.filterStatus) params['filterStatus'] = filters.filterStatus;
    if (filters?.filterDefault) params['filterDefault'] = filters.filterDefault;

    return this.http.get<ApiResult<PagedResult<WarehouseDto>>>(
      `${this.apiUrl}/warehouses/paginated`,
      { params },
    );
  }

  /**
   * ID'ye göre depo detayını getir
   */
  getWarehouseById(id: number): Observable<ApiResult<WarehouseDetailDto>> {
    return this.http.get<ApiResult<WarehouseDetailDto>>(`${this.apiUrl}/warehouses/${id}`);
  }

  /**
   * Yeni depo oluştur
   */
  createWarehouse(dto: CreateWarehouseDto): Observable<ApiResult<number>> {
    return this.http.post<ApiResult<number>>(`${this.apiUrl}/warehouses`, dto);
  }

  /**
   * Depo güncelle
   */
  updateWarehouse(id: number, dto: UpdateWarehouseDto): Observable<ApiResult<boolean>> {
    return this.http.put<ApiResult<boolean>>(`${this.apiUrl}/warehouses/${id}`, dto);
  }

  /**
   * Depo sil (soft delete)
   */
  deleteWarehouse(id: number): Observable<ApiResult<boolean>> {
    return this.http.delete<ApiResult<boolean>>(`${this.apiUrl}/warehouses/${id}`);
  }
}
