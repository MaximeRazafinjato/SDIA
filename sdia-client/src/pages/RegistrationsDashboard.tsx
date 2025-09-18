import React, { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Button,
  IconButton,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  Tooltip,
  Stack,
  InputAdornment,
  Snackbar,
  Card,
  CardContent,
  Tabs,
  Tab,
  Grid,
} from '@mui/material';
import PageLayout from '@/components/layout/PageLayout';
import {
  Search as SearchIcon,
  Refresh as RefreshIcon,
  Visibility as ViewIcon,
  Link as LinkIcon,
  Send as SendIcon,
  Check as CheckIcon,
  Close as CloseIcon,
  Download as DownloadIcon,
  Sort as SortIcon,
  Assignment as AssignmentIcon,
} from '@mui/icons-material';
import { GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import axios from '@/api/axios';
import ActionDialog from '@/components/dialogs/ActionDialog';
import RegistrationsDataGrid from '@/components/registrations/RegistrationsDataGrid';

interface Registration {
  id: string;
  registrationNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  status: string | number;
  createdAt: string;
  submittedAt?: string;
  assignedToUserName?: string;
  organizationName: string;
  isMinor: boolean;
  birthDate?: string;
  notificationType?: string;
  recipient?: string;
  documents?: Array<{
    id: string;
    fileName: string;
    fileType: string;
    fileSize: number;
  }>;
  comments?: Array<{
    id: string;
    content: string;
    createdAt: string;
    user?: {
      name: string;
    };
  }>;
  history?: Array<{
    id: string;
    action: string;
    details: string;
    createdAt: string;
    user?: {
      name: string;
    };
  }>;
}

interface RegistrationStats {
  total: number;
  pending: number;
  validated: number;
  rejected: number;
}

const RegistrationsDashboard: React.FC = () => {
  const navigate = useNavigate();
  const [registrations, setRegistrations] = useState<Registration[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [sortBy, setSortBy] = useState('CreatedAt');
  const [sortDesc, setSortDesc] = useState(true);
  const [stats, setStats] = useState<RegistrationStats | null>(null);
  const [selectedRegistration, setSelectedRegistration] = useState<Registration | null>(null);
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);
  const [publicLinkDialogOpen, setPublicLinkDialogOpen] = useState(false);
  const [publicLink, setPublicLink] = useState('');
  const [publicLinkCode, setPublicLinkCode] = useState('');
  const [actionDialogOpen, setActionDialogOpen] = useState(false);
  const [actionType, setActionType] = useState<'validate' | 'reject' | 'remind'>('validate');
  const [actionComments, setActionComments] = useState('');
  const actionCommentsRef = useRef('');
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: '',
    severity: 'success' as 'success' | 'error',
  });
  const [tabValue, setTabValue] = useState(0);

  const fetchRegistrations = useCallback(async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        page: (page + 1).toString(),
        pageSize: pageSize.toString(),
        sortBy,
        sortDesc: sortDesc.toString(),
      });

      if (searchTerm) params.append('search', searchTerm);
      if (statusFilter) params.append('status', statusFilter);

      const response = await axios.get(`/api/registrations?${params}`);
      setRegistrations(response.data.items);
      setTotalCount(response.data.totalCount);
    } catch (error) {
      console.error('Error fetching registrations:', error);
      setSnackbar({
        open: true,
        message: 'Erreur lors du chargement des inscriptions',
        severity: 'error',
      });
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, searchTerm, statusFilter, sortBy, sortDesc]);

  const fetchStats = async () => {
    try {
      const response = await axios.get('/api/registrations/stats');
      setStats(response.data);
    } catch (error) {
      console.error('Error fetching stats:', error);
    }
  };

  useEffect(() => {
    fetchRegistrations();
    fetchStats();
  }, [fetchRegistrations]);

  const handleViewDetails = useCallback(
    (id: string) => {
      navigate(`/registrations/${id}`);
    },
    [navigate],
  );

  const handleGeneratePublicLink = useCallback(
    async (id: string) => {
      try {
        const response = await axios.post(`/api/registrations/${id}/generate-access-link`, {
          sendNotification: true,
        });
        setPublicLink(response.data.accessLink);
        setPublicLinkCode(response.data.verificationCode);

        // Find the registration from the list and update it
        const registration = registrations.find((r) => r.id === id);
        if (registration) {
          setSelectedRegistration({
            ...registration,
            notificationType: response.data.notificationType,
            recipient: response.data.recipient,
          });
        }

        setPublicLinkDialogOpen(true);
        setSnackbar({ open: true, message: 'Lien généré avec succès', severity: 'success' });
      } catch (error) {
        console.error('Error generating public link:', error);
        const errorMessage =
          (error as { response?: { data?: { message?: string } }; message?: string })?.response
            ?.data?.message ||
          (error as Error)?.message ||
          'Erreur lors de la génération du lien';
        setSnackbar({ open: true, message: errorMessage, severity: 'error' });
      }
    },
    [registrations],
  );

  const handleAction = async () => {
    if (!selectedRegistration) return;

    try {
      const endpoint = `/api/registrations/${selectedRegistration.id}/${actionType}`;
      const body =
        actionType === 'reject'
          ? { reason: actionComments }
          : actionType === 'validate'
            ? { comments: actionComments }
            : {};

      await axios.post(endpoint, body);

      setSnackbar({
        open: true,
        message: `Inscription ${actionType === 'validate' ? 'validée' : actionType === 'reject' ? 'rejetée' : 'relancée'} avec succès`,
        severity: 'success',
      });

      setActionDialogOpen(false);
      setActionComments('');
      fetchRegistrations();
      fetchStats();
    } catch (error) {
      console.error(`Error performing ${actionType}:`, error);
      setSnackbar({ open: true, message: "Erreur lors de l'action", severity: 'error' });
    }
  };

  const openActionDialog = useCallback(
    (registration: Registration, type: 'validate' | 'reject' | 'remind') => {
      setSelectedRegistration(registration);
      setActionType(type);
      setActionDialogOpen(true);
    },
    [],
  );

  const getStatusColor = (status: string | number) => {
    const statusStr = typeof status === 'number' ? status.toString() : status?.toLowerCase() || '';
    switch (statusStr) {
      case '10':
      case '5':
      case 'validated':
        return 'success';
      case '1':
      case 'pending':
        return 'warning';
      case '6':
      case 'rejected':
        return 'error';
      case '0':
      case 'draft':
        return 'default';
      default:
        return 'info';
    }
  };

  const getStatusLabel = (status: string | number) => {
    const labels: Record<string, string> = {
      '0': 'Brouillon',
      '1': 'En attente',
      '2': 'En cours',
      '3': 'Documents manquants',
      '4': 'En validation',
      '5': 'Validée',
      '6': 'Rejetée',
      '7': 'Annulée',
      '8': 'Complétée',
      Draft: 'Brouillon',
      Pending: 'En attente',
      InProgress: 'En cours',
      WaitingForDocuments: 'Documents manquants',
      WaitingForValidation: 'En validation',
      Validated: 'Validée',
      Rejected: 'Rejetée',
      Cancelled: 'Annulée',
      Completed: 'Complétée',
    };
    const key = typeof status === 'number' ? status.toString() : status;
    return labels[key] || key;
  };

  const handleActionCommentsChange = useCallback((value: string) => {
    actionCommentsRef.current = value;
    setActionComments(value);
  }, []);

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'registrationNumber',
        headerName: "Numéro d'inscription",
        flex: 1,
        minWidth: 150,
        renderCell: (params) => (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography
              variant="body2"
              fontWeight="600"
              sx={{
                color: 'primary.main',
                cursor: 'pointer',
                '&:hover': { textDecoration: 'underline' },
              }}
              onClick={() => handleViewDetails(params.row.id)}
            >
              {params.value}
            </Typography>
          </Box>
        ),
      },
      {
        field: 'fullName',
        headerName: 'Candidat',
        flex: 1.5,
        minWidth: 200,
        valueGetter: (value, row) => {
          if (!row) return '';
          return `${row.firstName || ''} ${row.lastName || ''}`.trim();
        },
        renderCell: (params) => (
          <Box sx={{ display: 'flex', flexDirection: 'column', py: 0.5 }}>
            <Typography variant="body2" fontWeight="500">
              {params.value}
            </Typography>
            <Box sx={{ display: 'flex', gap: 0.5, mt: 0.5 }}>
              {params.row?.isMinor && (
                <Chip
                  label="Mineur"
                  size="small"
                  color="warning"
                  sx={{ height: 20, fontSize: '0.7rem' }}
                />
              )}
              {params.row?.assignedToUserName && (
                <Chip
                  label={params.row.assignedToUserName}
                  size="small"
                  variant="outlined"
                  sx={{ height: 20, fontSize: '0.7rem' }}
                />
              )}
            </Box>
          </Box>
        ),
      },
      {
        field: 'contact',
        headerName: 'Contact',
        flex: 2,
        minWidth: 250,
        valueGetter: (value, row) => {
          return `${row?.email || ''} | ${row?.phone || ''}`;
        },
        renderCell: (params) => (
          <Box sx={{ display: 'flex', flexDirection: 'column', py: 0.5 }}>
            <Typography variant="body2" sx={{ color: 'text.primary', fontSize: '0.875rem' }}>
              ✉️ {params.row.email}
            </Typography>
            <Typography variant="body2" sx={{ color: 'text.secondary', fontSize: '0.875rem' }}>
              📞 {params.row.phone}
            </Typography>
          </Box>
        ),
      },
      {
        field: 'organizationName',
        headerName: 'Organisation',
        flex: 1.2,
        minWidth: 150,
        renderCell: (params) => (
          <Typography variant="body2" sx={{ fontWeight: 500 }}>
            {params.value}
          </Typography>
        ),
      },
      {
        field: 'status',
        headerName: 'Statut',
        flex: 1,
        minWidth: 140,
        renderCell: (params) => (
          <Chip
            label={getStatusLabel(params.value)}
            color={getStatusColor(params.value)}
            size="small"
            sx={{
              fontWeight: 600,
              minWidth: 100,
            }}
          />
        ),
      },
      {
        field: 'dates',
        headerName: 'Dates',
        flex: 1.2,
        minWidth: 150,
        valueGetter: (value, row) => {
          return row?.createdAt;
        },
        renderCell: (params) => (
          <Box sx={{ display: 'flex', flexDirection: 'column', py: 0.5 }}>
            <Typography variant="caption" sx={{ color: 'text.secondary' }}>
              Créé: {new Date(params.row.createdAt).toLocaleDateString('fr-FR')}
            </Typography>
            {params.row.submittedAt && (
              <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                Soumis: {new Date(params.row.submittedAt).toLocaleDateString('fr-FR')}
              </Typography>
            )}
          </Box>
        ),
      },
      {
        field: 'actions',
        type: 'actions',
        headerName: 'Actions',
        flex: 1.5,
        minWidth: 200,
        getActions: (params) => [
          <GridActionsCellItem
            icon={
              <Tooltip title="Voir les détails">
                <ViewIcon />
              </Tooltip>
            }
            label="View"
            onClick={() => handleViewDetails(params.row.id)}
          />,
          <GridActionsCellItem
            icon={
              <Tooltip title="Générer lien public">
                <LinkIcon />
              </Tooltip>
            }
            label="Link"
            onClick={() => handleGeneratePublicLink(params.row.id)}
          />,
          <GridActionsCellItem
            icon={
              <Tooltip title="Relancer">
                <SendIcon />
              </Tooltip>
            }
            label="Remind"
            onClick={() => openActionDialog(params.row, 'remind')}
            disabled={
              params.row.status === 5 ||
              params.row.status === 6 ||
              params.row.status === 'Validated' ||
              params.row.status === 'Rejected'
            }
          />,
          <GridActionsCellItem
            icon={
              <Tooltip title="Valider">
                <CheckIcon color="success" />
              </Tooltip>
            }
            label="Validate"
            onClick={() => openActionDialog(params.row, 'validate')}
            disabled={
              params.row.status === 5 ||
              params.row.status === 6 ||
              params.row.status === 'Validated' ||
              params.row.status === 'Rejected'
            }
          />,
          <GridActionsCellItem
            icon={
              <Tooltip title="Rejeter">
                <CloseIcon color="error" />
              </Tooltip>
            }
            label="Reject"
            onClick={() => openActionDialog(params.row, 'reject')}
            disabled={
              params.row.status === 5 ||
              params.row.status === 6 ||
              params.row.status === 'Validated' ||
              params.row.status === 'Rejected'
            }
          />,
        ],
      },
    ],
    [handleViewDetails, handleGeneratePublicLink, openActionDialog],
  );

  return (
    <PageLayout
      title="Gestion des Inscriptions"
      description="Gérez les inscriptions et candidatures de la plateforme"
      icon={<AssignmentIcon />}
      actions={
        <Button
          variant="contained"
          startIcon={<RefreshIcon />}
          onClick={() => {
            fetchRegistrations();
            fetchStats();
          }}
        >
          Actualiser
        </Button>
      }
    >
      {/* Statistics Cards */}
      {stats && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid size={3}>
            <Card>
              <CardContent sx={{ py: 1.5, px: 2, '&:last-child': { pb: 1.5 } }}>
                <Typography variant="body2" color="textSecondary">
                  Total
                </Typography>
                <Typography variant="h4" component="div" sx={{ mt: 0.5 }}>
                  {stats.total}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid size={3}>
            <Card>
              <CardContent sx={{ py: 1.5, px: 2, '&:last-child': { pb: 1.5 } }}>
                <Typography variant="body2" color="textSecondary">
                  En attente
                </Typography>
                <Typography variant="h4" component="div" color="warning.main" sx={{ mt: 0.5 }}>
                  {stats.pending}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid size={3}>
            <Card>
              <CardContent sx={{ py: 1.5, px: 2, '&:last-child': { pb: 1.5 } }}>
                <Typography variant="body2" color="textSecondary">
                  Validées
                </Typography>
                <Typography variant="h4" component="div" color="success.main" sx={{ mt: 0.5 }}>
                  {stats.validated}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid size={3}>
            <Card>
              <CardContent sx={{ py: 1.5, px: 2, '&:last-child': { pb: 1.5 } }}>
                <Typography variant="body2" color="textSecondary">
                  Rejetées
                </Typography>
                <Typography variant="h4" component="div" color="error.main" sx={{ mt: 0.5 }}>
                  {stats.rejected}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Filters */}
      <Paper elevation={2} sx={{ p: 2, mb: 2 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            placeholder="Rechercher..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            }}
            sx={{ flex: 1 }}
          />
          <FormControl sx={{ minWidth: 150 }}>
            <InputLabel>Statut</InputLabel>
            <Select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              label="Statut"
            >
              <MenuItem value="">Tous</MenuItem>
              <MenuItem value="0">Brouillon</MenuItem>
              <MenuItem value="1">En attente</MenuItem>
              <MenuItem value="2">En cours</MenuItem>
              <MenuItem value="5">Validée</MenuItem>
              <MenuItem value="6">Rejetée</MenuItem>
            </Select>
          </FormControl>
          <FormControl sx={{ minWidth: 150 }}>
            <InputLabel>Trier par</InputLabel>
            <Select value={sortBy} onChange={(e) => setSortBy(e.target.value)} label="Trier par">
              <MenuItem value="CreatedAt">Date création</MenuItem>
              <MenuItem value="RegistrationNumber">Numéro</MenuItem>
              <MenuItem value="FirstName">Prénom</MenuItem>
              <MenuItem value="LastName">Nom</MenuItem>
              <MenuItem value="Status">Statut</MenuItem>
            </Select>
          </FormControl>
          <IconButton onClick={() => setSortDesc(!sortDesc)} color="primary">
            <SortIcon style={{ transform: sortDesc ? 'rotate(180deg)' : 'none' }} />
          </IconButton>
        </Stack>
      </Paper>

      {/* Data Grid - Isolated component for better performance */}
      <RegistrationsDataGrid
        rows={registrations}
        columns={columns}
        loading={loading}
        page={page}
        pageSize={pageSize}
        total={totalCount}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
      />

      {/* Detail Dialog */}
      <Dialog
        open={detailDialogOpen}
        onClose={() => setDetailDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          Détails de l'inscription
          {selectedRegistration && (
            <Chip
              label={getStatusLabel(selectedRegistration.status)}
              color={
                getStatusColor(selectedRegistration.status) as
                  | 'default'
                  | 'primary'
                  | 'secondary'
                  | 'error'
                  | 'info'
                  | 'success'
                  | 'warning'
              }
              size="small"
              sx={{ ml: 2 }}
            />
          )}
        </DialogTitle>
        <DialogContent>
          {selectedRegistration && (
            <Box>
              <Tabs value={tabValue} onChange={(e, v) => setTabValue(v)} sx={{ mb: 2 }}>
                <Tab label="Informations" />
                <Tab label="Documents" />
                <Tab label="Commentaires" />
                <Tab label="Historique" />
              </Tabs>

              {tabValue === 0 && (
                <Box>
                  <Typography variant="h6" gutterBottom>
                    Informations personnelles
                  </Typography>
                  <Grid container spacing={2}>
                    <Grid size={6}>
                      <Typography variant="body2" color="textSecondary">
                        Numéro d'inscription
                      </Typography>
                      <Typography variant="body1" gutterBottom>
                        {selectedRegistration.registrationNumber}
                      </Typography>
                    </Grid>
                    <Grid size={6}>
                      <Typography variant="body2" color="textSecondary">
                        Nom complet
                      </Typography>
                      <Typography variant="body1" gutterBottom>
                        {selectedRegistration.firstName} {selectedRegistration.lastName}
                      </Typography>
                    </Grid>
                    <Grid size={6}>
                      <Typography variant="body2" color="textSecondary">
                        Email
                      </Typography>
                      <Typography variant="body1" gutterBottom>
                        {selectedRegistration.email}
                      </Typography>
                    </Grid>
                    <Grid size={6}>
                      <Typography variant="body2" color="textSecondary">
                        Téléphone
                      </Typography>
                      <Typography variant="body1" gutterBottom>
                        {selectedRegistration.phone}
                      </Typography>
                    </Grid>
                    <Grid size={6}>
                      <Typography variant="body2" color="textSecondary">
                        Date de naissance
                      </Typography>
                      <Typography variant="body1" gutterBottom>
                        {selectedRegistration.birthDate
                          ? new Date(selectedRegistration.birthDate).toLocaleDateString('fr-FR')
                          : 'Non renseigné'}
                        {selectedRegistration.isMinor && (
                          <Chip label="Mineur" size="small" color="warning" sx={{ ml: 1 }} />
                        )}
                      </Typography>
                    </Grid>
                    <Grid size={6}>
                      <Typography variant="body2" color="textSecondary">
                        Organisation
                      </Typography>
                      <Typography variant="body1" gutterBottom>
                        {selectedRegistration.organizationName || 'Non renseigné'}
                      </Typography>
                    </Grid>
                  </Grid>
                </Box>
              )}

              {tabValue === 1 && (
                <Box>
                  <Typography variant="h6" gutterBottom>
                    Documents
                  </Typography>
                  {selectedRegistration.documents && selectedRegistration.documents.length > 0 ? (
                    <Stack spacing={1}>
                      {selectedRegistration.documents.map((doc) => (
                        <Paper key={doc.id} sx={{ p: 2 }}>
                          <Box display="flex" justifyContent="space-between" alignItems="center">
                            <Box>
                              <Typography variant="body1">{doc.fileName}</Typography>
                              <Typography variant="caption" color="textSecondary">
                                {doc.fileType} - {(doc.fileSize / 1024).toFixed(2)} KB
                              </Typography>
                            </Box>
                            <IconButton>
                              <DownloadIcon />
                            </IconButton>
                          </Box>
                        </Paper>
                      ))}
                    </Stack>
                  ) : (
                    <Typography color="textSecondary">Aucun document</Typography>
                  )}
                </Box>
              )}

              {tabValue === 2 && (
                <Box>
                  <Typography variant="h6" gutterBottom>
                    Commentaires
                  </Typography>
                  {selectedRegistration.comments && selectedRegistration.comments.length > 0 ? (
                    <Stack spacing={2}>
                      {selectedRegistration.comments.map((comment) => (
                        <Paper key={comment.id} sx={{ p: 2 }}>
                          <Typography variant="body1">{comment.content}</Typography>
                          <Typography variant="caption" color="textSecondary">
                            Par {comment.user?.name} -{' '}
                            {new Date(comment.createdAt).toLocaleString('fr-FR')}
                          </Typography>
                        </Paper>
                      ))}
                    </Stack>
                  ) : (
                    <Typography color="textSecondary">Aucun commentaire</Typography>
                  )}
                </Box>
              )}

              {tabValue === 3 && (
                <Box>
                  <Typography variant="h6" gutterBottom>
                    Historique
                  </Typography>
                  {selectedRegistration.history && selectedRegistration.history.length > 0 ? (
                    <Stack spacing={1}>
                      {selectedRegistration.history.map((event) => (
                        <Paper key={event.id} sx={{ p: 2 }}>
                          <Typography variant="body1" fontWeight="bold">
                            {event.action}
                          </Typography>
                          <Typography variant="body2">{event.details}</Typography>
                          <Typography variant="caption" color="textSecondary">
                            Par {event.user?.name} -{' '}
                            {new Date(event.createdAt).toLocaleString('fr-FR')}
                          </Typography>
                        </Paper>
                      ))}
                    </Stack>
                  ) : (
                    <Typography color="textSecondary">Aucun historique</Typography>
                  )}
                </Box>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailDialogOpen(false)}>Fermer</Button>
        </DialogActions>
      </Dialog>

      {/* Public Link Dialog */}
      <Dialog
        open={publicLinkDialogOpen}
        onClose={() => setPublicLinkDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Lien d'accès généré avec succès</DialogTitle>
        <DialogContent>
          <Alert severity="success" sx={{ mb: 2 }}>
            Un lien d'accès a été généré et la notification a été envoyée par{' '}
            {selectedRegistration?.notificationType} à {selectedRegistration?.recipient}
          </Alert>

          <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
            Code de vérification
          </Typography>
          <Paper
            variant="outlined"
            sx={{ p: 2, mb: 3, textAlign: 'center', backgroundColor: '#f5f5f5' }}
          >
            <Typography variant="h3" sx={{ fontFamily: 'monospace', letterSpacing: 4 }}>
              {publicLinkCode}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Ce code expire dans 10 minutes
            </Typography>
          </Paper>

          <Typography variant="h6" gutterBottom>
            Lien d'accès
          </Typography>
          <TextField
            fullWidth
            value={publicLink}
            margin="normal"
            InputProps={{
              readOnly: true,
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    onClick={() => {
                      navigator.clipboard.writeText(publicLink);
                      setSnackbar({ open: true, message: 'Lien copié!', severity: 'success' });
                    }}
                  >
                    <LinkIcon />
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />

          <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
            Le lien est valable 24 heures. L'inscrit devra saisir le code de vérification pour
            accéder à son dossier.
          </Typography>

          <Alert severity="info" sx={{ mt: 2 }}>
            <Typography variant="body2">
              Les détails de la notification ont été enregistrés dans le fichier de log :
              <strong>
                {' '}
                logs/notifications/{new Date().toISOString().split('T')[0]}_
                {selectedRegistration?.notificationType?.toLowerCase() || 'notification'}.log
              </strong>
            </Typography>
          </Alert>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPublicLinkDialogOpen(false)}>Fermer</Button>
        </DialogActions>
      </Dialog>

      {/* Action Dialog - Isolated component for better performance */}
      <ActionDialog
        open={actionDialogOpen}
        actionType={actionType}
        actionComments={actionComments}
        onCommentsChange={handleActionCommentsChange}
        onClose={() => setActionDialogOpen(false)}
        onConfirm={handleAction}
      />

      {/* Snackbar */}
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
    </PageLayout>
  );
};

export default RegistrationsDashboard;
