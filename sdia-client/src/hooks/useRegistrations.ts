import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '@/api/client';
import { Registration, RegistrationStatus } from '@/types/registration';

const REGISTRATIONS_QUERY_KEY = 'registrations';

// Fetch all registrations
export const useRegistrations = () => {
  return useQuery({
    queryKey: [REGISTRATIONS_QUERY_KEY],
    queryFn: async (): Promise<Registration[]> => {
      const response = await apiClient.get('/api/registrations');
      return response.data;
    },
  });
};

// Fetch a single registration
export const useRegistration = (id: string) => {
  return useQuery({
    queryKey: [REGISTRATIONS_QUERY_KEY, id],
    queryFn: async (): Promise<Registration> => {
      const response = await apiClient.get(`/api/registrations/${id}`);
      return response.data;
    },
    enabled: !!id,
  });
};

// Create a new registration
export const useCreateRegistration = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (registration: Omit<Registration, 'id'>): Promise<Registration> => {
      const response = await apiClient.post('/api/registrations', registration);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [REGISTRATIONS_QUERY_KEY] });
    },
  });
};

// Update a registration
export const useUpdateRegistration = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async ({ id, registration }: { id: string; registration: Partial<Registration> }): Promise<Registration> => {
      const response = await apiClient.put(`/api/registrations/${id}`, registration);
      return response.data;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: [REGISTRATIONS_QUERY_KEY] });
      queryClient.invalidateQueries({ queryKey: [REGISTRATIONS_QUERY_KEY, data.id] });
    },
  });
};

// Update registration status
export const useUpdateRegistrationStatus = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async ({ id, status }: { id: string; status: RegistrationStatus }): Promise<Registration> => {
      const response = await apiClient.patch(`/api/registrations/${id}/status`, { status });
      return response.data;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: [REGISTRATIONS_QUERY_KEY] });
      queryClient.invalidateQueries({ queryKey: [REGISTRATIONS_QUERY_KEY, data.id] });
    },
  });
};

// Delete a registration
export const useDeleteRegistration = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (id: string): Promise<void> => {
      await apiClient.delete(`/api/registrations/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [REGISTRATIONS_QUERY_KEY] });
    },
  });
};