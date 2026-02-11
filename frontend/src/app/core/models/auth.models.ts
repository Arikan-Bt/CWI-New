export interface Result<T> {
  success: boolean;
  data?: T;
  error?: string;
  errors: string[];
}

export interface UserDto {
  id: number;
  userCode: string;
  userName: string;
  fullName: string;
  email: string;
  // ... (UserDto update)
  userType: string;
  firstName?: string;
  lastName?: string;
  currAccCode?: string;
  roleName?: string;
  linkedCustomerId?: number;
  isAdministrator: boolean;
  permissions: string[];
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresIn: number;
  user: UserDto;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  email: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}
