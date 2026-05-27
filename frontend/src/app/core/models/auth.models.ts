export interface LoginResult {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  userId: string;
  email: string;
}

export interface CurrentUserDto {
  userId: string;
  tenantId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions: string[];
  scopeType: string;
  scopeIds: string[];
}

export interface UserPermissionsResult {
  userId: string;
  roles: string[];
  permissions: string[];
  scopeType: string;
  scopeIds: string[];
}
