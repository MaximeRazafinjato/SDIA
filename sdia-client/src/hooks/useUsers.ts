import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '@/api/client';
import { 
  User, 
  UserList, 
  UsersResponse, 
  CreateUser, 
  UpdateUser, 
  UserStats 
} from '@/types/user';

const USERS_QUERY_KEY = 'users';

// Fetch all users with pagination
export const useUsers = (page: number = 1, pageSize: number = 20) => {
  return useQuery({
    queryKey: [USERS_QUERY_KEY, { page, pageSize }],
    queryFn: async (): Promise<UsersResponse> => {
      const response = await apiClient.get('/api/users', {
        params: { page, pageSize }
      });
      return response.data;
    },
  });
};

// Fetch a single user
export const useUser = (id: string) => {
  return useQuery({
    queryKey: [USERS_QUERY_KEY, id],
    queryFn: async (): Promise<User> => {
      const response = await apiClient.get(`/api/users/${id}`);
      return response.data;
    },
    enabled: !!id,
  });
};

// Fetch user statistics
export const useUserStats = () => {
  return useQuery({
    queryKey: [USERS_QUERY_KEY, 'stats'],
    queryFn: async (): Promise<UserStats> => {
      const response = await apiClient.get('/api/users/stats');
      return response.data;
    },
  });
};

// Create a new user
export const useCreateUser = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (user: CreateUser): Promise<{ id: string; message: string; email: string }> => {
      const response = await apiClient.post('/api/users', user);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [USERS_QUERY_KEY] });
    },
  });
};

// Update a user
export const useUpdateUser = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async ({ id, user }: { id: string; user: UpdateUser }): Promise<{ message: string }> => {
      const response = await apiClient.put(`/api/users/${id}`, user);
      return response.data;
    },
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: [USERS_QUERY_KEY] });
      queryClient.invalidateQueries({ queryKey: [USERS_QUERY_KEY, id] });
    },
  });
};

// Delete a user
export const useDeleteUser = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (id: string): Promise<{ message: string }> => {
      const response = await apiClient.delete(`/api/users/${id}`);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [USERS_QUERY_KEY] });
    },
  });
};

// Resend validation email
export const useResendValidationEmail = () => {
  return useMutation({
    mutationFn: async (id: string): Promise<{ message: string }> => {
      const response = await apiClient.post(`/api/users/${id}/resend-validation`);
      return response.data;
    },
  });
};

// Reset user password
export const useResetUserPassword = () => {
  return useMutation({
    mutationFn: async (id: string): Promise<{ message: string }> => {
      const response = await apiClient.post(`/api/users/${id}/reset-password`);
      return response.data;
    },
  });
};