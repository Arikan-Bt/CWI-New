import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import {
  Result,
  LoginResponse,
  UserDto,
  UpdateProfileRequest,
  ChangePasswordRequest,
} from '../models/auth.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient) {}

  login(userCode: string, password: string): Observable<Result<LoginResponse>> {
    return this.http
      .post<Result<LoginResponse>>(`${this.apiUrl}/login`, { userCode, password })
      .pipe(
        tap((result) => {
          if (result.success && result.data) {
            this.setSession(result.data);
          }
        }),
      );
  }

  private setSession(authResult: LoginResponse): void {
    localStorage.setItem('token', authResult.token);
    localStorage.setItem('refreshToken', authResult.refreshToken);
    localStorage.setItem('user', JSON.stringify(authResult.user));
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }

  refreshToken(): Observable<Result<LoginResponse>> {
    const refreshToken = localStorage.getItem('refreshToken');
    return this.http.post<Result<LoginResponse>>(`${this.apiUrl}/refresh`, { refreshToken }).pipe(
      tap((result) => {
        if (result.success && result.data) {
          this.setSession(result.data);
        }
      }),
    );
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  getUser(): UserDto | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }

  hasPermission(permission: string): boolean {
    const user = this.getUser();
    if (!user || !user.permissions) return false;
    return user.permissions.includes(permission);
  }

  hasAnyPermission(permissions: string[]): boolean {
    const user = this.getUser();
    if (!user || !user.permissions) return false;
    return permissions.some((p) => user.permissions.includes(p));
  }

  updateProfile(request: UpdateProfileRequest): Observable<Result<UserDto>> {
    return this.http.put<Result<UserDto>>(`${this.apiUrl}/profile`, request).pipe(
      tap((result) => {
        if (result.success && result.data) {
          localStorage.setItem('user', JSON.stringify(result.data));
        }
      }),
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<Result<void>> {
    return this.http.post<Result<void>>(`${this.apiUrl}/change-password`, request);
  }
}
