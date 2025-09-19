// Common filter types for data grids

export interface GridSort {
  sortBy?: string;
  sortDescending?: boolean;
}

export interface GridPagination {
  page: number;
  pageSize: number;
}

export interface GridFilters extends GridSort, GridPagination {
  searchTerm?: string;
}

// User-specific filters
export interface UserFilters extends GridFilters {
  role?: string;
  isActive?: boolean;
  emailConfirmed?: boolean;
  phoneConfirmed?: boolean;
  organizationId?: string;
  createdFrom?: string;
  createdTo?: string;
  lastLoginFrom?: string;
  lastLoginTo?: string;
}

// Registration-specific filters
export interface RegistrationFilters extends GridFilters {
  status?: 'Draft' | 'Pending' | 'Validated' | 'Rejected';
  organizationId?: string;
  formTemplateId?: string;
  assignedToUserId?: string;
  submittedFrom?: string;
  submittedTo?: string;
  createdFrom?: string;
  createdTo?: string;
  validatedFrom?: string;
  validatedTo?: string;
  rejectedFrom?: string;
  rejectedTo?: string;
  emailVerified?: boolean;
  phoneVerified?: boolean;
  isMinor?: boolean;
  ageFrom?: number;
  ageTo?: number;
}

// Form template-specific filters
export interface FormTemplateFilters extends GridFilters {
  isActive?: boolean;
  organizationId?: string;
}
