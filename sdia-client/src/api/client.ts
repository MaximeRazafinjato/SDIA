import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5206';

// Create axios instance with default configuration
export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
  withCredentials: true, // Important for cookie-based auth
});

// Request interceptor (no need to add token for cookie-based auth)
apiClient.interceptors.request.use(
  (config) => {
    // Cookies are automatically sent with withCredentials: true
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Flag to prevent multiple redirections
let isRedirecting = false;

// Response interceptor to handle common errors
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      // Skip redirect logic for auth endpoints
      const isAuthEndpoint = error.config?.url?.includes('/api/auth/');

      // Only clear and redirect if:
      // 1. Not already redirecting
      // 2. Not on an auth page
      // 3. Not calling an auth endpoint (like /api/auth/me)
      const authPages = ['/login', '/forgot-password', '/reset-password', '/registration-access', '/registration-public'];
      const isAuthPage = authPages.some(page => window.location.pathname.includes(page));

      if (!isRedirecting && !isAuthPage && !isAuthEndpoint) {
        isRedirecting = true;
        localStorage.removeItem('user');

        // Use replace to avoid history issues
        window.location.replace('/login');

        // Reset flag after a delay (in case redirect fails)
        setTimeout(() => {
          isRedirecting = false;
        }, 1000);
      }
    }
    return Promise.reject(error);
  }
);

export default apiClient;