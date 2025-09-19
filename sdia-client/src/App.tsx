import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { CssBaseline } from '@mui/material';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { fr } from 'date-fns/locale';
import { AuthProvider } from '@/contexts/AuthContext';
import ProtectedRoute from '@/components/common/ProtectedRoute';
import MainLayout from '@/layouts/MainLayout';
import Login from '@/pages/Login';
import ForgotPassword from '@/pages/ForgotPassword';
import ResetPassword from '@/pages/ResetPassword';
import Dashboard from '@/pages/Dashboard';
import RegistrationsDashboard from '@/pages/RegistrationsDashboard';
import RegistrationDetail from '@/pages/RegistrationDetail';
import RegistrationAccess from '@/pages/RegistrationAccess';
import RegistrationEdit from '@/pages/RegistrationEdit';
import RegistrationPublic from '@/pages/RegistrationPublic';
import Users from '@/pages/Users';

// Create a Material-UI theme
const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

// Create a React Query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 1,
    },
  },
});

const App: React.FC = () => {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={fr}>
          <AuthProvider>
            <Router>
              <Routes>
                <Route path="/login" element={<Login />} />
                <Route path="/forgot-password" element={<ForgotPassword />} />
                <Route path="/reset-password" element={<ResetPassword />} />
                <Route path="/registration-access/:token" element={<RegistrationAccess />} />
                <Route path="/registration-edit/:id" element={<RegistrationEdit />} />
                <Route path="/registration-public/:token" element={<RegistrationPublic />} />
                <Route
                  path="/"
                  element={
                    <ProtectedRoute>
                      <MainLayout />
                    </ProtectedRoute>
                  }
                >
                  <Route index element={<Navigate to="/dashboard" replace />} />
                  <Route path="dashboard" element={<Dashboard />} />
                  <Route path="registrations-dashboard" element={<RegistrationsDashboard />} />
                  <Route path="registrations/:id" element={<RegistrationDetail />} />
                  <Route path="users" element={<Users />} />
                  <Route path="courses" element={<div>Cours - À venir</div>} />
                </Route>
                <Route path="*" element={<Navigate to="/dashboard" replace />} />
              </Routes>
            </Router>
          </AuthProvider>
        </LocalizationProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
};

export default App;
