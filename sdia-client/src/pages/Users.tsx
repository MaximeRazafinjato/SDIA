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
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  FormControlLabel,
  Switch,
  Alert,
  Snackbar,
  CircularProgress,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Email as EmailIcon,
  Lock as LockIcon,
  Person as PersonIcon,
} from '@mui/icons-material';
import { DataGrid, type GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { useForm, Controller } from 'react-hook-form';
import { 
  useUsers, 
  useCreateUser, 
  useUpdateUser, 
  useDeleteUser, 
  useResendValidationEmail, 
  useResetUserPassword 
} from '@/hooks/useUsers';
import { CreateUser, UpdateUser, UserList } from '@/types/user';

const Users: React.FC = () => {
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserList | null>(null);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  // Queries and mutations
  const { data: usersData, isLoading, refetch } = useUsers(page + 1, pageSize);
  const createUserMutation = useCreateUser();
  const updateUserMutation = useUpdateUser();
  const deleteUserMutation = useDeleteUser();
  const resendEmailMutation = useResendValidationEmail();
  const resetPasswordMutation = useResetUserPassword();

  // Forms
  const createForm = useForm<CreateUser>({
    defaultValues: {
      email: '',
      firstName: '',
      lastName: '',
      phone: '',
      role: 'User',
    }
  });

  const editForm = useForm<UpdateUser>({
    defaultValues: {
      email: '',
      firstName: '',
      lastName: '',
      phone: '',
      role: 'User',
      isActive: true,
    }
  });

  const showSnackbar = (message: string, severity: 'success' | 'error' = 'success') => {
    setSnackbar({ open: true, message, severity });
  };

  const handleCreateUser = async (data: CreateUser) => {
    try {
      await createUserMutation.mutateAsync(data);
      setCreateDialogOpen(false);
      createForm.reset();
      showSnackbar('Utilisateur créé avec succès. Un email de validation a été envoyé.');
      refetch();
    } catch (error: any) {
      showSnackbar(error?.response?.data?.message || 'Erreur lors de la création', 'error');
    }
  };

  const handleUpdateUser = async (data: UpdateUser) => {
    if (!selectedUser) return;
    
    try {
      await updateUserMutation.mutateAsync({ id: selectedUser.id, user: data });
      setEditDialogOpen(false);
      setSelectedUser(null);
      editForm.reset();
      showSnackbar('Utilisateur modifié avec succès');
      refetch();
    } catch (error: any) {
      showSnackbar(error?.response?.data?.message || 'Erreur lors de la modification', 'error');
    }
  };

  const handleDeleteUser = async () => {
    if (!selectedUser) return;

    try {
      await deleteUserMutation.mutateAsync(selectedUser.id);
      setDeleteDialogOpen(false);
      setSelectedUser(null);
      showSnackbar('Utilisateur supprimé avec succès');
      refetch();
    } catch (error: any) {
      showSnackbar(error?.response?.data?.message || 'Erreur lors de la suppression', 'error');
    }
  };

  const handleResendEmail = async (userId: string) => {
    try {
      await resendEmailMutation.mutateAsync(userId);
      showSnackbar('Email de validation renvoyé avec succès');
    } catch (error: any) {
      showSnackbar(error?.response?.data?.message || 'Erreur lors de l\'envoi de l\'email', 'error');
    }
  };

  const handleResetPassword = async (userId: string) => {
    try {
      await resetPasswordMutation.mutateAsync(userId);
      showSnackbar('Mot de passe réinitialisé avec succès');
    } catch (error: any) {
      showSnackbar(error?.response?.data?.message || 'Erreur lors de la réinitialisation', 'error');
    }
  };

  const openEditDialog = (user: UserList) => {
    setSelectedUser(user);
    editForm.reset({
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      phone: '', // Phone is not in UserList, will be loaded when editing
      role: user.role,
      isActive: user.isActive,
    });
    setEditDialogOpen(true);
  };

  const openDeleteDialog = (user: UserList) => {
    setSelectedUser(user);
    setDeleteDialogOpen(true);
  };

  const getRoleColor = (role: string) => {
    switch (role.toLowerCase()) {
      case 'admin':
        return 'error';
      case 'manager':
        return 'warning';
      case 'user':
        return 'primary';
      default:
        return 'default';
    }
  };

  const columns: GridColDef[] = [
    {
      field: 'user',
      headerName: 'Utilisateur',
      width: 250,
      renderCell: (params) => (
        <Box>
          <Typography variant="body2" fontWeight="bold">
            {params.row.firstName} {params.row.lastName}
          </Typography>
          <Typography variant="caption" color="textSecondary">
            {params.row.email}
          </Typography>
        </Box>
      ),
    },
    {
      field: 'role',
      headerName: 'Rôle',
      width: 120,
      renderCell: (params) => (
        <Chip
          label={params.value}
          color={getRoleColor(params.value)}
          size="small"
          variant="outlined"
        />
      ),
    },
    {
      field: 'isActive',
      headerName: 'Statut',
      width: 100,
      renderCell: (params) => (
        <Chip
          label={params.value ? 'Actif' : 'Inactif'}
          color={params.value ? 'success' : 'default'}
          size="small"
          variant="outlined"
        />
      ),
    },
    {
      field: 'emailConfirmed',
      headerName: 'Email Confirmé',
      width: 120,
      renderCell: (params) => (
        <Chip
          label={params.value ? 'Confirmé' : 'En attente'}
          color={params.value ? 'success' : 'warning'}
          size="small"
          variant="outlined"
        />
      ),
    },
    {
      field: 'createdAt',
      headerName: 'Créé le',
      width: 120,
      renderCell: (params) => new Date(params.value).toLocaleDateString('fr-FR'),
    },
    {
      field: 'lastLoginAt',
      headerName: 'Dernière connexion',
      width: 150,
      renderCell: (params) => 
        params.value ? new Date(params.value).toLocaleDateString('fr-FR') : 'Jamais',
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'Actions',
      width: 200,
      getActions: (params) => [
        <GridActionsCellItem
          icon={
            <Tooltip title="Modifier">
              <EditIcon />
            </Tooltip>
          }
          label="Modifier"
          onClick={() => openEditDialog(params.row)}
        />,
        <GridActionsCellItem
          icon={
            <Tooltip title="Supprimer">
              <DeleteIcon />
            </Tooltip>
          }
          label="Supprimer"
          onClick={() => openDeleteDialog(params.row)}
          disabled={params.row.role === 'Admin'} // Prevent deleting admin users
        />,
        <GridActionsCellItem
          icon={
            <Tooltip title="Renvoyer email de validation">
              <EmailIcon />
            </Tooltip>
          }
          label="Renvoyer email"
          onClick={() => handleResendEmail(params.row.id)}
          disabled={params.row.emailConfirmed}
        />,
        <GridActionsCellItem
          icon={
            <Tooltip title="Réinitialiser le mot de passe">
              <LockIcon />
            </Tooltip>
          }
          label="Réinitialiser mot de passe"
          onClick={() => handleResetPassword(params.row.id)}
        />,
      ],
    },
  ];

  return (
    <Box>
      <Paper sx={{ p: 2, mb: 2 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
          <Box>
            <Typography variant="h4" gutterBottom>
              <PersonIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
              Gestion des Utilisateurs
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Gérez les comptes utilisateurs de la plateforme
            </Typography>
          </Box>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setCreateDialogOpen(true)}
          >
            Nouvel Utilisateur
          </Button>
        </Box>

        <DataGrid
          rows={usersData?.items || []}
          columns={columns}
          loading={isLoading}
          paginationMode="server"
          rowCount={usersData?.totalCount || 0}
          paginationModel={{ page, pageSize }}
          onPaginationModelChange={(model) => {
            setPage(model.page);
            setPageSize(model.pageSize);
          }}
          pageSizeOptions={[5, 10, 25, 50]}
          disableRowSelectionOnClick
          autoHeight
          sx={{ minHeight: 400 }}
        />
      </Paper>

      {/* Create User Dialog */}
      <Dialog 
        open={createDialogOpen} 
        onClose={() => setCreateDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <form onSubmit={createForm.handleSubmit(handleCreateUser)}>
          <DialogTitle>Créer un Utilisateur</DialogTitle>
          <DialogContent>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
              <Controller
                name="email"
                control={createForm.control}
                rules={{ 
                  required: 'Email requis',
                  pattern: {
                    value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                    message: 'Email invalide'
                  }
                }}
                render={({ field, fieldState }) => (
                  <TextField
                    {...field}
                    label="Email"
                    type="email"
                    error={!!fieldState.error}
                    helperText={fieldState.error?.message}
                    fullWidth
                  />
                )}
              />
              
              <Controller
                name="firstName"
                control={createForm.control}
                rules={{ required: 'Prénom requis' }}
                render={({ field, fieldState }) => (
                  <TextField
                    {...field}
                    label="Prénom"
                    error={!!fieldState.error}
                    helperText={fieldState.error?.message}
                    fullWidth
                  />
                )}
              />
              
              <Controller
                name="lastName"
                control={createForm.control}
                rules={{ required: 'Nom requis' }}
                render={({ field, fieldState }) => (
                  <TextField
                    {...field}
                    label="Nom"
                    error={!!fieldState.error}
                    helperText={fieldState.error?.message}
                    fullWidth
                  />
                )}
              />
              
              <Controller
                name="phone"
                control={createForm.control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Téléphone"
                    type="tel"
                    fullWidth
                  />
                )}
              />
              
              <Controller
                name="role"
                control={createForm.control}
                rules={{ required: 'Rôle requis' }}
                render={({ field, fieldState }) => (
                  <FormControl fullWidth error={!!fieldState.error}>
                    <InputLabel>Rôle</InputLabel>
                    <Select {...field} label="Rôle">
                      <MenuItem value="User">Utilisateur</MenuItem>
                      <MenuItem value="Manager">Manager</MenuItem>
                      <MenuItem value="Admin">Administrateur</MenuItem>
                    </Select>
                    {fieldState.error && (
                      <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                        {fieldState.error.message}
                      </Typography>
                    )}
                  </FormControl>
                )}
              />
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setCreateDialogOpen(false)}>Annuler</Button>
            <Button 
              type="submit" 
              variant="contained"
              disabled={createUserMutation.isPending}
            >
              {createUserMutation.isPending ? <CircularProgress size={20} /> : 'Créer'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>

      {/* Edit User Dialog */}
      <Dialog 
        open={editDialogOpen} 
        onClose={() => setEditDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <form onSubmit={editForm.handleSubmit(handleUpdateUser)}>
          <DialogTitle>Modifier l'Utilisateur</DialogTitle>
          <DialogContent>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
              <Controller
                name="email"
                control={editForm.control}
                rules={{ 
                  required: 'Email requis',
                  pattern: {
                    value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                    message: 'Email invalide'
                  }
                }}
                render={({ field, fieldState }) => (
                  <TextField
                    {...field}
                    label="Email"
                    type="email"
                    error={!!fieldState.error}
                    helperText={fieldState.error?.message}
                    fullWidth
                  />
                )}
              />
              
              <Controller
                name="firstName"
                control={editForm.control}
                rules={{ required: 'Prénom requis' }}
                render={({ field, fieldState }) => (
                  <TextField
                    {...field}
                    label="Prénom"
                    error={!!fieldState.error}
                    helperText={fieldState.error?.message}
                    fullWidth
                  />
                )}
              />
              
              <Controller
                name="lastName"
                control={editForm.control}
                rules={{ required: 'Nom requis' }}
                render={({ field, fieldState }) => (
                  <TextField
                    {...field}
                    label="Nom"
                    error={!!fieldState.error}
                    helperText={fieldState.error?.message}
                    fullWidth
                  />
                )}
              />
              
              <Controller
                name="phone"
                control={editForm.control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Téléphone"
                    type="tel"
                    fullWidth
                  />
                )}
              />
              
              <Controller
                name="role"
                control={editForm.control}
                rules={{ required: 'Rôle requis' }}
                render={({ field, fieldState }) => (
                  <FormControl fullWidth error={!!fieldState.error}>
                    <InputLabel>Rôle</InputLabel>
                    <Select {...field} label="Rôle">
                      <MenuItem value="User">Utilisateur</MenuItem>
                      <MenuItem value="Manager">Manager</MenuItem>
                      <MenuItem value="Admin">Administrateur</MenuItem>
                    </Select>
                    {fieldState.error && (
                      <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                        {fieldState.error.message}
                      </Typography>
                    )}
                  </FormControl>
                )}
              />
              
              <Controller
                name="isActive"
                control={editForm.control}
                render={({ field }) => (
                  <FormControlLabel
                    control={
                      <Switch
                        checked={field.value}
                        onChange={field.onChange}
                      />
                    }
                    label="Compte actif"
                  />
                )}
              />
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setEditDialogOpen(false)}>Annuler</Button>
            <Button 
              type="submit" 
              variant="contained"
              disabled={updateUserMutation.isPending}
            >
              {updateUserMutation.isPending ? <CircularProgress size={20} /> : 'Sauvegarder'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
        maxWidth="sm"
      >
        <DialogTitle>Confirmer la Suppression</DialogTitle>
        <DialogContent>
          <Typography>
            Êtes-vous sûr de vouloir supprimer l'utilisateur{' '}
            <strong>
              {selectedUser?.firstName} {selectedUser?.lastName}
            </strong> ?
          </Typography>
          <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
            Cette action est irréversible.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Annuler</Button>
          <Button
            onClick={handleDeleteUser}
            color="error"
            variant="contained"
            disabled={deleteUserMutation.isPending}
          >
            {deleteUserMutation.isPending ? <CircularProgress size={20} /> : 'Supprimer'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar for notifications */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
      >
        <Alert
          onClose={() => setSnackbar({ ...snackbar, open: false })}
          severity={snackbar.severity}
          sx={{ width: '100%' }}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default Users;