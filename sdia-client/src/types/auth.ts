export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  isLoading: boolean;
  isAuthenticated: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// The API returns the user directly, not a token
export interface LoginResponse extends User {
  organizationId?: string;
}