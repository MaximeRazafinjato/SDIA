import React, { useState } from 'react';
import {
  Box,
  Button,
  Paper,
  Typography,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Tooltip,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as VisibilityIcon,
} from '@mui/icons-material';
import { DataGrid, type GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { Registration, RegistrationStatus, Course } from '@/types/registration';

// Mock data for registrations
const mockRegistrations: Registration[] = [
  {
    id: '1',
    studentId: 'STU001',
    student: {
      id: 'STU001',
      firstName: 'Jean',
      lastName: 'Dupont',
      email: 'jean.dupont@email.com',
      phone: '0123456789',
      dateOfBirth: '1995-05-15',
      nationality: 'Française',
    },
    academicYear: '2024-2025',
    semester: 'S1',
    status: RegistrationStatus.Approved,
    registrationDate: '2024-09-01',
    courses: [
      { id: '1', name: 'Mathématiques', code: 'MATH101', credits: 6, semester: 'S1' },
      { id: '2', name: 'Physique', code: 'PHYS101', credits: 6, semester: 'S1' },
    ],
  },
  {
    id: '2',
    studentId: 'STU002',
    student: {
      id: 'STU002',
      firstName: 'Marie',
      lastName: 'Martin',
      email: 'marie.martin@email.com',
      phone: '0123456790',
      dateOfBirth: '1996-03-20',
      nationality: 'Française',
    },
    academicYear: '2024-2025',
    semester: 'S1',
    status: RegistrationStatus.Pending,
    registrationDate: '2024-09-05',
    courses: [
      { id: '3', name: 'Informatique', code: 'INFO101', credits: 6, semester: 'S1' },
    ],
  },
  {
    id: '3',
    studentId: 'STU003',
    student: {
      id: 'STU003',
      firstName: 'Pierre',
      lastName: 'Bernard',
      email: 'pierre.bernard@email.com',
      phone: '0123456791',
      dateOfBirth: '1997-07-10',
      nationality: 'Belge',
    },
    academicYear: '2024-2025',
    semester: 'S1',
    status: RegistrationStatus.Rejected,
    registrationDate: '2024-08-28',
    courses: [],
  },
];

const Registrations: React.FC = () => {
  const [registrations, setRegistrations] = useState<Registration[]>(mockRegistrations);
  const [selectedRegistration, setSelectedRegistration] = useState<Registration | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [statusFilter, setStatusFilter] = useState<string>('all');

  const getStatusColor = (status: string) => {
    switch (status) {
      case RegistrationStatus.Approved:
        return 'success';
      case RegistrationStatus.Pending:
        return 'warning';
      case RegistrationStatus.Rejected:
        return 'error';
      default:
        return 'default';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case RegistrationStatus.Approved:
        return 'Approuvée';
      case RegistrationStatus.Pending:
        return 'En attente';
      case RegistrationStatus.Rejected:
        return 'Rejetée';
      default:
        return status;
    }
  };

  const handleView = (registration: Registration) => {
    setSelectedRegistration(registration);
    setDialogOpen(true);
  };

  const handleEdit = (id: string) => {
    console.log('Edit registration:', id);
  };

  const handleDelete = (id: string) => {
    setRegistrations(prev => prev.filter(reg => reg.id !== id));
  };

  const filteredRegistrations = statusFilter === 'all' 
    ? registrations 
    : registrations.filter(reg => reg.status === statusFilter);

  const columns: GridColDef[] = [
    {
      field: 'student',
      headerName: 'Étudiant',
      width: 200,
      renderCell: (params) => (
        <Box>
          <Typography variant="body2" fontWeight="bold">
            {params.row.student.firstName} {params.row.student.lastName}
          </Typography>
          <Typography variant="caption" color="textSecondary">
            {params.row.student.email}
          </Typography>
        </Box>
      ),
    },
    {
      field: 'academicYear',
      headerName: 'Année académique',
      width: 150,
    },
    {
      field: 'semester',
      headerName: 'Semestre',
      width: 100,
    },
    {
      field: 'registrationDate',
      headerName: 'Date d\'inscription',
      width: 150,
      renderCell: (params) => new Date(params.value).toLocaleDateString('fr-FR'),
    },
    {
      field: 'status',
      headerName: 'Statut',
      width: 120,
      renderCell: (params) => (
        <Chip
          label={getStatusLabel(params.value)}
          color={getStatusColor(params.value)}
          size="small"
        />
      ),
    },
    {
      field: 'coursesCount',
      headerName: 'Nombre de cours',
      width: 130,
      renderCell: (params) => params.row.courses.length,
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'Actions',
      width: 150,
      getActions: (params) => [
        <GridActionsCellItem
          icon={
            <Tooltip title="Voir les détails">
              <VisibilityIcon />
            </Tooltip>
          }
          label="View"
          onClick={() => handleView(params.row)}
        />,
        <GridActionsCellItem
          icon={
            <Tooltip title="Modifier">
              <EditIcon />
            </Tooltip>
          }
          label="Edit"
          onClick={() => handleEdit(params.row.id)}
        />,
        <GridActionsCellItem
          icon={
            <Tooltip title="Supprimer">
              <DeleteIcon />
            </Tooltip>
          }
          label="Delete"
          onClick={() => handleDelete(params.row.id)}
        />,
      ],
    },
  ];

  return (
    <Box>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1" sx={{ fontWeight: 'bold' }}>
          Gestion des inscriptions
        </Typography>
        <Box>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => console.log('Add new registration')}
          >
            Nouvelle inscription
          </Button>
        </Box>
      </Box>

      {/* Status Filter Chips */}
      <Box mb={2}>
        <Chip
          label={`Toutes (${registrations.length})`}
          onClick={() => setStatusFilter('all')}
          color={statusFilter === 'all' ? 'primary' : 'default'}
          sx={{ mr: 1 }}
        />
        <Chip
          label={`En attente (${registrations.filter(r => r.status === RegistrationStatus.Pending).length})`}
          onClick={() => setStatusFilter(RegistrationStatus.Pending)}
          color={statusFilter === RegistrationStatus.Pending ? 'primary' : 'default'}
          sx={{ mr: 1 }}
        />
        <Chip
          label={`Approuvées (${registrations.filter(r => r.status === RegistrationStatus.Approved).length})`}
          onClick={() => setStatusFilter(RegistrationStatus.Approved)}
          color={statusFilter === RegistrationStatus.Approved ? 'primary' : 'default'}
          sx={{ mr: 1 }}
        />
        <Chip
          label={`Rejetées (${registrations.filter(r => r.status === RegistrationStatus.Rejected).length})`}
          onClick={() => setStatusFilter(RegistrationStatus.Rejected)}
          color={statusFilter === RegistrationStatus.Rejected ? 'primary' : 'default'}
        />
      </Box>

      {/* Data Grid */}
      <Paper elevation={3}>
        <DataGrid
          rows={filteredRegistrations}
          columns={columns}
          initialState={{
            pagination: {
              paginationModel: { page: 0, pageSize: 10 },
            },
          }}
          pageSizeOptions={[5, 10, 25]}
          checkboxSelection
          disableRowSelectionOnClick
          autoHeight
          sx={{
            border: 'none',
            '& .MuiDataGrid-cell:hover': {
              color: 'primary.main',
            },
          }}
        />
      </Paper>

      {/* View Details Dialog */}
      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          Détails de l'inscription
        </DialogTitle>
        <DialogContent>
          {selectedRegistration && (
            <Box>
              <Typography variant="h6" gutterBottom>
                Informations de l'étudiant
              </Typography>
              <Typography><strong>Nom:</strong> {selectedRegistration.student.firstName} {selectedRegistration.student.lastName}</Typography>
              <Typography><strong>Email:</strong> {selectedRegistration.student.email}</Typography>
              <Typography><strong>Téléphone:</strong> {selectedRegistration.student.phone}</Typography>
              <Typography><strong>Date de naissance:</strong> {new Date(selectedRegistration.student.dateOfBirth).toLocaleDateString('fr-FR')}</Typography>
              <Typography><strong>Nationalité:</strong> {selectedRegistration.student.nationality}</Typography>
              
              <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
                Détails de l'inscription
              </Typography>
              <Typography><strong>Année académique:</strong> {selectedRegistration.academicYear}</Typography>
              <Typography><strong>Semestre:</strong> {selectedRegistration.semester}</Typography>
              <Typography><strong>Date d'inscription:</strong> {new Date(selectedRegistration.registrationDate).toLocaleDateString('fr-FR')}</Typography>
              <Typography><strong>Statut:</strong> {getStatusLabel(selectedRegistration.status)}</Typography>
              
              <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
                Cours inscrits ({selectedRegistration.courses.length})
              </Typography>
              {selectedRegistration.courses.length > 0 ? (
                selectedRegistration.courses.map((course: Course) => (
                  <Box key={course.id} sx={{ ml: 2, mb: 1 }}>
                    <Typography>• {course.name} ({course.code}) - {course.credits} crédits</Typography>
                  </Box>
                ))
              ) : (
                <Typography color="textSecondary">Aucun cours inscrit</Typography>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Fermer</Button>
          <Button variant="contained" onClick={() => console.log('Edit registration')}>
            Modifier
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Registrations;