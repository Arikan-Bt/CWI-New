import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserDto {
  id: any;
  name: string;
  surname: string;
  email: string;
  role: string;
  status: string;
  clientCode?: string;
  mobilePhone?: string;
  currentAccount?: string;
  allowedBrands?: number[];
  restrictedBrands?: number[];
  allowedProducts?: string[];
  blockedProducts?: string[];
  roleId?: number;
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

export interface UserTableFilters {
  filterName?: string;
  filterEmail?: string;
  filterRole?: string;
  filterStatus?: string;
}

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getUsers(
    page: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    sortField?: string,
    sortOrder: number = 1,
    filters?: UserTableFilters,
  ): Observable<ApiResult<PagedResult<UserDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('sortOrder', sortOrder.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (sortField) {
      params = params.set('sortField', sortField);
    }
    if (filters?.filterName) params = params.set('filterName', filters.filterName);
    if (filters?.filterEmail) params = params.set('filterEmail', filters.filterEmail);
    if (filters?.filterRole) params = params.set('filterRole', filters.filterRole);
    if (filters?.filterStatus) params = params.set('filterStatus', filters.filterStatus);

    return this.http.get<ApiResult<PagedResult<UserDto>>>(this.apiUrl, { params });
  }

  getUserById(id: any): Observable<ApiResult<UserDto>> {
    return this.http.get<ApiResult<UserDto>>(`${this.apiUrl}/${id}`);
  }

  createUser(user: any): Observable<UserDto> {
    return this.http.post<UserDto>(this.apiUrl, user);
  }

  updateUser(id: any, user: any): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.apiUrl}/${id}`, { ...user, id });
  }

  deleteUser(id: any): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getUserActivities(userId: any): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${userId}/activities`);
  }
}
