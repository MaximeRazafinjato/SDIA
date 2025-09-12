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
  role: string;
  isActive: boolean;
  emailConfirmed: boolean;
  lastLoginAt?: string;
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
  items: UserList[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UserStats {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  confirmedEmails: number;
  unconfirmedEmails: number;
  byRole: { role: string; count: number }[];
}

export interface UserFormData extends CreateUser {}
export interface UpdateUserFormData extends UpdateUser {}