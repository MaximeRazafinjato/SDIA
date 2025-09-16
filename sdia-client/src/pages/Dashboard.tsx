import React, { useEffect, useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  Card,
  CardContent,
  IconButton,
  Stack,
  CircularProgress,
  Alert,
} from '@mui/material';
import {
  People as PeopleIcon,
  School as SchoolIcon,
  TrendingUp as TrendingUpIcon,
  Assignment as AssignmentIcon,
  Refresh as RefreshIcon,
  Dashboard as DashboardIcon,
} from '@mui/icons-material';
import PageLayout from '@/components/layout/PageLayout';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  LineChart,
  Line,
} from 'recharts';
import api from '@/api/axios';

interface StatCardProps {
  title: string;
  value: number;
  icon: React.ReactNode;
  color: string;
  subtitle?: string;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, icon, color, subtitle }) => (
  <Card elevation={3} sx={{ minWidth: 250 }}>
    <CardContent>
      <Box display="flex" alignItems="center" justifyContent="space-between">
        <Box>
          <Typography color="textSecondary" gutterBottom variant="body2">
            {title}
          </Typography>
          <Typography variant="h4" component="h2" sx={{ fontWeight: 'bold' }}>
            {value.toLocaleString()}
          </Typography>
          {subtitle && (
            <Typography color="textSecondary" variant="body2">
              {subtitle}
            </Typography>
          )}
        </Box>
        <Box
          sx={{
            backgroundColor: `${color}20`,
            borderRadius: 2,
            p: 1.5,
            color: color,
          }}
        >
          {icon}
        </Box>
      </Box>
    </CardContent>
  </Card>
);

const Dashboard: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [stats, setStats] = useState({
    totalUsers: 0,
    totalRegistrations: 0,
    pendingRegistrations: 0,
    validatedRegistrations: 0,
    rejectedRegistrations: 0,
    totalFormTemplates: 0,
  });

  const [registrationsByStatus, setRegistrationsByStatus] = useState<any[]>([]);

  const fetchDashboardData = async () => {
    setLoading(true);
    setError(null);
    try {
      // Fetch users count
      const usersResponse = await api.get('/api/users?pageSize=1');
      const totalUsers = usersResponse.data.totalCount || 0;

      // Fetch registration statistics
      const registrationStatsResponse = await api.get('/api/registrations/stats');
      const registrationStats = registrationStatsResponse.data;

      // Fetch form templates count
      const formTemplatesResponse = await api.get('/api/form-templates?pageSize=1');
      const totalFormTemplates = formTemplatesResponse.data.totalCount || 0;

      setStats({
        totalUsers,
        totalRegistrations: registrationStats.total || 0,
        pendingRegistrations: registrationStats.pending || 0,
        validatedRegistrations: registrationStats.validated || 0,
        rejectedRegistrations: registrationStats.rejected || 0,
        totalFormTemplates,
      });

      // Set status data for pie chart
      const statusData = [
        { name: 'Validées', value: registrationStats.validated || 0, color: '#4caf50' },
        { name: 'En attente', value: registrationStats.pending || 0, color: '#ff9800' },
        { name: 'Rejetées', value: registrationStats.rejected || 0, color: '#f44336' },
        { name: 'Brouillons', value: registrationStats.byStatus?.find((s: any) => s.status === 'Draft')?.count || 0, color: '#9e9e9e' },
      ].filter(item => item.value > 0);

      setRegistrationsByStatus(statusData);
    } catch (err: any) {
      console.error('Error fetching dashboard data:', err);
      setError('Erreur lors du chargement des données du tableau de bord');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const handleRefresh = () => {
    fetchDashboardData();
  };

  // Calculate success rate
  const successRate = stats.totalRegistrations > 0 
    ? Math.round((stats.validatedRegistrations / stats.totalRegistrations) * 100)
    : 0;

  return (
    <PageLayout 
      title="Tableau de bord" 
      description="Vue d'ensemble des activités et statistiques de la plateforme"
      icon={<DashboardIcon />}
      actions={
        <IconButton onClick={handleRefresh} size="large" color="primary" disabled={loading}>
          <RefreshIcon />
        </IconButton>
      }
    >
      {loading ? (
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
          <CircularProgress />
        </Box>
      ) : error ? (
        <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>
      ) : (
        <>
          {/* Statistics Cards */}
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} mb={4} sx={{ flexWrap: 'wrap' }}>
            <StatCard
              title="Total des utilisateurs"
              value={stats.totalUsers}
              icon={<PeopleIcon sx={{ fontSize: 40 }} />}
              color="#1976d2"
            />
            <StatCard
              title="Inscriptions totales"
              value={stats.totalRegistrations}
              icon={<AssignmentIcon sx={{ fontSize: 40 }} />}
              color="#4caf50"
            />
            <StatCard
              title="Modèles de formulaire"
              value={stats.totalFormTemplates}
              icon={<SchoolIcon sx={{ fontSize: 40 }} />}
              color="#ff9800"
            />
            <StatCard
              title="Taux de validation"
              value={successRate}
              icon={<TrendingUpIcon sx={{ fontSize: 40 }} />}
              color="#9c27b0"
              subtitle="%"
            />
          </Stack>

          {/* Charts Section */}
          <Stack spacing={3}>
            {/* Status Distribution */}
            {registrationsByStatus.length > 0 && (
              <Paper elevation={3} sx={{ p: 3 }}>
                <Typography variant="h6" gutterBottom>
                  Répartition des inscriptions par statut
                </Typography>
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={registrationsByStatus}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={(entry) => `${entry.name}: ${entry.value}`}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {registrationsByStatus.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              </Paper>
            )}

            {/* Summary Stats */}
            <Paper elevation={3} sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Résumé des statistiques
              </Typography>
              <Stack spacing={2}>
                <Box display="flex" justifyContent="space-between">
                  <Typography>Inscriptions en attente:</Typography>
                  <Typography fontWeight="bold" color="warning.main">
                    {stats.pendingRegistrations}
                  </Typography>
                </Box>
                <Box display="flex" justifyContent="space-between">
                  <Typography>Inscriptions validées:</Typography>
                  <Typography fontWeight="bold" color="success.main">
                    {stats.validatedRegistrations}
                  </Typography>
                </Box>
                <Box display="flex" justifyContent="space-between">
                  <Typography>Inscriptions rejetées:</Typography>
                  <Typography fontWeight="bold" color="error.main">
                    {stats.rejectedRegistrations}
                  </Typography>
                </Box>
              </Stack>
            </Paper>
          </Stack>
        </>
      )}
    </PageLayout>
  );
};

export default Dashboard;