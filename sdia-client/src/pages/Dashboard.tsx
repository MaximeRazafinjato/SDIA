import React from 'react';
import {
  Box,
  Paper,
  Typography,
  Card,
  CardContent,
  IconButton,
  Stack,
} from '@mui/material';
import {
  People as PeopleIcon,
  School as SchoolIcon,
  TrendingUp as TrendingUpIcon,
  Assignment as AssignmentIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
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

// Mock data for charts
const monthlyRegistrations = [
  { month: 'Jan', inscriptions: 120 },
  { month: 'Fév', inscriptions: 150 },
  { month: 'Mar', inscriptions: 180 },
  { month: 'Avr', inscriptions: 200 },
  { month: 'Mai', inscriptions: 160 },
  { month: 'Juin', inscriptions: 140 },
];

const statusData = [
  { name: 'Approuvées', value: 65, color: '#4caf50' },
  { name: 'En attente', value: 25, color: '#ff9800' },
  { name: 'Rejetées', value: 10, color: '#f44336' },
];

const evolutionData = [
  { year: '2020', total: 450 },
  { year: '2021', total: 620 },
  { year: '2022', total: 780 },
  { year: '2023', total: 950 },
  { year: '2024', total: 1200 },
];

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
  const handleRefresh = () => {
    // Implement refresh logic here
  };

  return (
    <Box>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1" sx={{ fontWeight: 'bold' }}>
          Tableau de bord
        </Typography>
        <IconButton onClick={handleRefresh} size="large">
          <RefreshIcon />
        </IconButton>
      </Box>

      {/* Statistics Cards */}
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} mb={4} sx={{ flexWrap: 'wrap' }}>
        <StatCard
          title="Total des étudiants"
          value={1245}
          icon={<PeopleIcon sx={{ fontSize: 40 }} />}
          color="#1976d2"
          subtitle="+12% ce mois"
        />
        <StatCard
          title="Inscriptions actives"
          value={892}
          icon={<AssignmentIcon sx={{ fontSize: 40 }} />}
          color="#4caf50"
          subtitle="En cours"
        />
        <StatCard
          title="Cours disponibles"
          value={48}
          icon={<SchoolIcon sx={{ fontSize: 40 }} />}
          color="#ff9800"
          subtitle="Ce semestre"
        />
        <StatCard
          title="Taux de réussite"
          value={87}
          icon={<TrendingUpIcon sx={{ fontSize: 40 }} />}
          color="#9c27b0"
          subtitle="%"
        />
      </Stack>

      {/* Charts Section */}
      <Stack spacing={3}>
        {/* First Row - Bar Chart and Pie Chart */}
        <Box display="flex" gap={3} flexDirection={{ xs: 'column', lg: 'row' }}>
          {/* Monthly Registrations Chart */}
          <Box flex={2}>
            <Paper elevation={3} sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Inscriptions par mois
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={monthlyRegistrations}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="month" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="inscriptions" fill="#1976d2" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </Paper>
          </Box>

          {/* Status Distribution Pie Chart */}
          <Box flex={1}>
            <Paper elevation={3} sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Répartition des statuts
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={statusData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={5}
                    dataKey="value"
                  >
                    {statusData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value) => `${value}%`} />
                </PieChart>
              </ResponsiveContainer>
              <Box mt={2}>
                {statusData.map((entry, index) => (
                  <Box key={index} display="flex" alignItems="center" mb={1}>
                    <Box
                      sx={{
                        width: 12,
                        height: 12,
                        backgroundColor: entry.color,
                        borderRadius: '50%',
                        mr: 1,
                      }}
                    />
                    <Typography variant="body2" sx={{ flexGrow: 1 }}>
                      {entry.name}
                    </Typography>
                    <Typography variant="body2" fontWeight="bold">
                      {entry.value}%
                    </Typography>
                  </Box>
                ))}
              </Box>
            </Paper>
          </Box>
        </Box>

        {/* Evolution Line Chart */}
        <Paper elevation={3} sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Évolution des inscriptions
          </Typography>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={evolutionData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="year" />
              <YAxis />
              <Tooltip />
              <Line
                type="monotone"
                dataKey="total"
                stroke="#4caf50"
                strokeWidth={3}
                dot={{ fill: '#4caf50', r: 6 }}
              />
            </LineChart>
          </ResponsiveContainer>
        </Paper>
      </Stack>
    </Box>
  );
};

export default Dashboard;