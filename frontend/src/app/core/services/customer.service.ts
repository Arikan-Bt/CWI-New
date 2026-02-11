import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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

export interface CustomerDto {
  id: number;
  code: string;
  name: string;
  taxOfficeName?: string;
  taxNumber?: string;
  addressLine1?: string;
  city?: string;
  phone?: string;
  email?: string;
  status: string;
  isActive: boolean;
  isVendor: boolean;
}

export interface CreateCustomerDto {
  code: string;
  name: string;
  taxOfficeName?: string;
  taxNumber?: string;
  addressLine1?: string;
  city?: string;
  phone?: string;
  email?: string;
  status: string;
  isVendor: boolean;
}

export interface CustomerTableFilters {
  filterCode?: string;
  filterName?: string;
  filterCity?: string;
  filterPhone?: string;
  filterEmail?: string;
  filterType?: string;
  filterStatus?: string;
}

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  private apiUrl = `${environment.apiUrl}/customers`;

  constructor(private http: HttpClient) {}

  getCustomers(
    page: number,
    pageSize: number,
    searchTerm?: string,
    sortField?: string,
    sortOrder: number = 1,
    filters?: CustomerTableFilters,
  ): Observable<ApiResult<PagedResult<CustomerDto>>> {
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
    if (filters?.filterCity) params['filterCity'] = filters.filterCity;
    if (filters?.filterPhone) params['filterPhone'] = filters.filterPhone;
    if (filters?.filterEmail) params['filterEmail'] = filters.filterEmail;
    if (filters?.filterType) params['filterType'] = filters.filterType;
    if (filters?.filterStatus) params['filterStatus'] = filters.filterStatus;

    return this.http.get<ApiResult<PagedResult<CustomerDto>>>(this.apiUrl, { params });
  }

  getCustomerById(id: number): Observable<ApiResult<CustomerDto>> {
    return this.http.get<ApiResult<CustomerDto>>(`${this.apiUrl}/${id}`);
  }

  createCustomer(company: CreateCustomerDto): Observable<ApiResult<CustomerDto>> {
    return this.http.post<ApiResult<CustomerDto>>(this.apiUrl, company);
  }

  updateCustomer(id: number, company: CreateCustomerDto): Observable<ApiResult<CustomerDto>> {
    return this.http.put<ApiResult<CustomerDto>>(`${this.apiUrl}/${id}`, { ...company, id });
  }

  deleteCustomer(id: number): Observable<ApiResult<void>> {
    return this.http.delete<ApiResult<void>>(`${this.apiUrl}/${id}`);
  }
}
