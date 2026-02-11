import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PermissionDto {
  key: string;
  name: string;
  groupName: string;
  description?: string;
}

export interface PermissionGroupDto {
  name: string;
  permissions: PermissionDto[];
}

export interface RoleDto {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  userCount: number;
}

export interface RoleDetailDto extends RoleDto {
  permissions: string[];
  users: RoleUserDto[];
}

export interface RoleUserDto {
  id: number;
  fullName: string;
  email: string;
  isActive: boolean;
}

export interface CreateRoleDto {
  name: string;
  description?: string;
  isActive: boolean;
  permissions: string[];
}

export interface RoleTableFilters {
  filterName?: string;
  filterDescription?: string;
  filterStatus?: string;
}

@Injectable({
  providedIn: 'root',
})
export class RoleService {
  private apiUrl = `${environment.apiUrl}/roles`;

  constructor(private http: HttpClient) {}

  getAllRoles(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(`${this.apiUrl}/all`);
  }

  getRoles(
    page: number,
    pageSize: number,
    search?: string,
    sortField?: string,
    sortOrder: number = 1,
    filters?: RoleTableFilters,
  ): Observable<any> {
    const params: Record<string, string> = {
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortOrder: sortOrder.toString(),
    };
    if (search) params['search'] = search;
    if (sortField) params['sortField'] = sortField;
    if (filters?.filterName) params['filterName'] = filters.filterName;
    if (filters?.filterDescription) params['filterDescription'] = filters.filterDescription;
    if (filters?.filterStatus) params['filterStatus'] = filters.filterStatus;
    return this.http.get<any>(this.apiUrl, { params });
  }

  getRoleById(id: number): Observable<RoleDetailDto> {
    return this.http.get<RoleDetailDto>(`${this.apiUrl}/${id}`);
  }

  getPermissions(): Observable<PermissionGroupDto[]> {
    return this.http.get<PermissionGroupDto[]>(`${this.apiUrl}/permissions`);
  }

  createRole(role: CreateRoleDto): Observable<number> {
    return this.http.post<number>(this.apiUrl, role);
  }

  updateRole(id: number, role: CreateRoleDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, { ...role, id });
  }

  deleteRole(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
