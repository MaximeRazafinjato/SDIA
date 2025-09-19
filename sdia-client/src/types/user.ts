export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone: string;
  role: string;
  isActive: boolean;
  emailConfirmed: boolean;
  phoneConfirmed: boolean;
  lastLoginAt?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface UserList {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone: string;
  role: string;
  isActive: boolean;
  emailConfirmed: boolean;
  phoneConfirmed: boolean;
  lastLoginAt?: string;
  organizationName?: string;
  createdAt: string;
}

export interface CreateUser {
  email: string;
  firstName: string;
  lastName: string;
  phone: string;
  role: string;
  organizationId?: string;
}

export interface UpdateUser {
  email: string;
  firstName: string;
  lastName: string;
  phone: string;
  role: string;
  isActive: boolean;
}

export interface UsersResponse {
  data: UserList[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface UserStats {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  confirmedEmails: number;
  unconfirmedEmails: number;
  byRole: { role: string; count: number }[];
}

export type UserFormData = CreateUser;
export type UpdateUserFormData = UpdateUser;
