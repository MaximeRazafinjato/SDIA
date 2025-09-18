import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Alert,
  CircularProgress,
  Container,
  Grid,
  Chip,
  Card,
  CardContent,
  Divider,
  Stack,
  Snackbar,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
} from '@mui/material';
import {
  Save as SaveIcon,
  Send as SendIcon,
  Person as PersonIcon,
  Email as EmailIcon,
  Phone as PhoneIcon,
  CalendarToday as CalendarIcon,
  Assignment as AssignmentIcon,
  CheckCircle as CheckCircleIcon,
  Cancel as CancelIcon,
  HourglassEmpty as PendingIcon,
  Description as DraftIcon,
  Comment as CommentIcon,
} from '@mui/icons-material';
import axios from '@/api/axios';
import { format } from 'date-fns';
import { fr } from 'date-fns/locale';

interface RegistrationData {
  id: string;
  registrationNumber: string;
  status: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  birthDate: string;
  formData?: string;
  createdAt: string;
  submittedAt?: string;
  validatedAt?: string;
  rejectedAt?: string;
  rejectionReason?: string;
  organizationName: string;
  formTemplateName?: string;
  isMinor: boolean;
  comments?: Comment[];
  documents?: Document[];
}

interface Comment {
  id: string;
  content: string;
  createdAt: string;
  authorName: string;
  isInternal: boolean;
}

interface Document {
  id: string;
  fileName: string;
  documentType: string;
  fileSize: number;
  uploadedAt: string;
}

const RegistrationPublic: React.FC = () => {
  const { token } = useParams<{ token: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [registration, setRegistration] = useState<RegistrationData | null>(null);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    birthDate: '',
    formData: '',
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  useEffect(() => {
    const sessionToken = sessionStorage.getItem('registration_session_token');
    const storedToken = sessionStorage.getItem('registration_token');

    if (!token || !sessionToken || storedToken !== token) {
      navigate(`/registration-access/${token}`);
      return;
    }

    fetchRegistration();
  }, [token, navigate]);

  const fetchRegistration = async () => {
    try {
      const sessionToken = sessionStorage.getItem('registration_session_token');
      const response = await axios.get(`/api/registrations-public/${token}`, {
        headers: {
          'X-Session-Token': sessionToken,
        },
      });

      setRegistration(response.data);
      setFormData({
        firstName: response.data.firstName,
        lastName: response.data.lastName,
        email: response.data.email,
        phone: response.data.phone || '',
        birthDate: response.data.birthDate.split('T')[0],
        formData: response.data.formData || '',
      });
    } catch (err: any) {
      if (err.response?.status === 401) {
        navigate(`/registration-access/${token}`);
      } else {
        setError('Erreur lors du chargement du dossier');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (submit: boolean = false) => {
    setSaving(true);
    setError('');

    try {
      const sessionToken = sessionStorage.getItem('registration_session_token');
      const response = await axios.put(
        `/api/registrations-public/${token}`,
        {
          ...formData,
          submit,
        },
        {
          headers: {
            'X-Session-Token': sessionToken,
          },
        }
      );

      if (response.data.success) {
        setSnackbar({
          open: true,
          message: response.data.message,
          severity: 'success',
        });

        // Refresh data if submitted
        if (submit) {
          await fetchRegistration();
        }
      }
    } catch (err: any) {
      if (err.response?.status === 401) {
        navigate(`/registration-access/${token}`);
      } else {
        setSnackbar({
          open: true,
          message: err.response?.data?.error || 'Erreur lors de la sauvegarde',
          severity: 'error',
        });
      }
    } finally {
      setSaving(false);
    }
  };

  const getStatusIcon = (status: number) => {
    switch (status) {
      case 0: return <DraftIcon />;
      case 1: return <PendingIcon />;
      case 2: return <CheckCircleIcon color="success" />;
      case 3: return <CancelIcon color="error" />;
      default: return <DraftIcon />;
    }
  };

  const getStatusLabel = (status: number) => {
    switch (status) {
      case 0: return 'Brouillon';
      case 1: return 'En attente';
      case 2: return 'Validé';
      case 3: return 'Rejeté';
      default: return 'Inconnu';
    }
  };

  const getStatusColor = (status: number) => {
    switch (status) {
      case 0: return 'default';
      case 1: return 'warning';
      case 2: return 'success';
      case 3: return 'error';
      default: return 'default';
    }
  };

  if (loading) {
    return (
      <Container maxWidth="lg">
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (!registration) {
    return (
      <Container maxWidth="lg">
        <Alert severity="error">Impossible de charger le dossier</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 4 }}>
        <Paper sx={{ p: 4, mb: 3 }}>
          <Grid container alignItems="center" spacing={2} sx={{ mb: 3 }}>
            <Grid item xs>
              <Typography variant="h4" component="h1">
                Mon dossier d'inscription
              </Typography>
              <Typography variant="body1" color="text.secondary">
                N° {registration.registrationNumber}
              </Typography>
            </Grid>
            <Grid item>
              <Chip
                icon={getStatusIcon(registration.status)}
                label={getStatusLabel(registration.status)}
                color={getStatusColor(registration.status) as any}
                size="large"
              />
            </Grid>
          </Grid>

          {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

          {registration.rejectionReason && (
            <Alert severity="error" sx={{ mb: 3 }}>
              <Typography variant="subtitle2">Motif de rejet :</Typography>
              <Typography variant="body2">{registration.rejectionReason}</Typography>
            </Alert>
          )}

          <Grid container spacing={3}>
            <Grid item xs={12} md={8}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    <PersonIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
                    Informations personnelles
                  </Typography>
                  <Divider sx={{ mb: 3 }} />

                  <Grid container spacing={2}>
                    <Grid item xs={12} sm={6}>
                      <TextField
                        fullWidth
                        label="Prénom"
                        value={formData.firstName}
                        onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                        disabled={saving || registration.status === 2 || registration.status === 3}
                      />
                    </Grid>
                    <Grid item xs={12} sm={6}>
                      <TextField
                        fullWidth
                        label="Nom"
                        value={formData.lastName}
                        onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                        disabled={saving || registration.status === 2 || registration.status === 3}
                      />
                    </Grid>
                    <Grid item xs={12} sm={6}>
                      <TextField
                        fullWidth
                        label="Email"
                        type="email"
                        value={formData.email}
                        onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                        disabled={saving || registration.status === 2 || registration.status === 3}
                        InputProps={{
                          startAdornment: <EmailIcon sx={{ mr: 1, color: 'action.active' }} />,
                        }}
                      />
                    </Grid>
                    <Grid item xs={12} sm={6}>
                      <TextField
                        fullWidth
                        label="Téléphone"
                        value={formData.phone}
                        onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                        disabled={saving || registration.status === 2 || registration.status === 3}
                        InputProps={{
                          startAdornment: <PhoneIcon sx={{ mr: 1, color: 'action.active' }} />,
                        }}
                      />
                    </Grid>
                    <Grid item xs={12} sm={6}>
                      <TextField
                        fullWidth
                        label="Date de naissance"
                        type="date"
                        value={formData.birthDate}
                        onChange={(e) => setFormData({ ...formData, birthDate: e.target.value })}
                        disabled={saving || registration.status === 2 || registration.status === 3}
                        InputLabelProps={{ shrink: true }}
                        InputProps={{
                          startAdornment: <CalendarIcon sx={{ mr: 1, color: 'action.active' }} />,
                        }}
                      />
                    </Grid>
                  </Grid>

                  {registration.formTemplateName && (
                    <>
                      <Typography variant="h6" sx={{ mt: 4, mb: 2 }}>
                        <AssignmentIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
                        Formulaire : {registration.formTemplateName}
                      </Typography>
                      <Divider sx={{ mb: 3 }} />
                      <TextField
                        fullWidth
                        multiline
                        rows={6}
                        label="Données du formulaire"
                        value={formData.formData}
                        onChange={(e) => setFormData({ ...formData, formData: e.target.value })}
                        disabled={saving || registration.status === 2 || registration.status === 3}
                        placeholder="Entrez les informations demandées dans le formulaire..."
                      />
                    </>
                  )}

                  {(registration.status === 0 || registration.status === 3) && (
                    <Box sx={{ mt: 4, display: 'flex', gap: 2 }}>
                      <Button
                        variant="contained"
                        onClick={() => handleSave(false)}
                        disabled={saving}
                        startIcon={saving ? <CircularProgress size={20} /> : <SaveIcon />}
                      >
                        Enregistrer
                      </Button>
                      <Button
                        variant="contained"
                        color="primary"
                        onClick={() => handleSave(true)}
                        disabled={saving}
                        startIcon={saving ? <CircularProgress size={20} /> : <SendIcon />}
                      >
                        Soumettre le dossier
                      </Button>
                    </Box>
                  )}

                  {registration.status === 1 && (
                    <Alert severity="info" sx={{ mt: 3 }}>
                      Votre dossier est en cours d'examen. Vous ne pouvez plus le modifier.
                    </Alert>
                  )}

                  {registration.status === 2 && (
                    <Alert severity="success" sx={{ mt: 3 }}>
                      Votre dossier a été validé. Vous ne pouvez plus le modifier.
                    </Alert>
                  )}
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={4}>
              <Card sx={{ mb: 3 }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Informations du dossier
                  </Typography>
                  <List dense>
                    <ListItem>
                      <ListItemText
                        primary="Organisation"
                        secondary={registration.organizationName}
                      />
                    </ListItem>
                    <ListItem>
                      <ListItemText
                        primary="Date de création"
                        secondary={format(new Date(registration.createdAt), 'dd MMMM yyyy', { locale: fr })}
                      />
                    </ListItem>
                    {registration.submittedAt && (
                      <ListItem>
                        <ListItemText
                          primary="Date de soumission"
                          secondary={format(new Date(registration.submittedAt), 'dd MMMM yyyy', { locale: fr })}
                        />
                      </ListItem>
                    )}
                    {registration.validatedAt && (
                      <ListItem>
                        <ListItemText
                          primary="Date de validation"
                          secondary={format(new Date(registration.validatedAt), 'dd MMMM yyyy', { locale: fr })}
                        />
                      </ListItem>
                    )}
                    {registration.rejectedAt && (
                      <ListItem>
                        <ListItemText
                          primary="Date de rejet"
                          secondary={format(new Date(registration.rejectedAt), 'dd MMMM yyyy', { locale: fr })}
                        />
                      </ListItem>
                    )}
                  </List>
                </CardContent>
              </Card>

              {registration.comments && registration.comments.length > 0 && (
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      <CommentIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
                      Commentaires
                    </Typography>
                    <List>
                      {registration.comments.map((comment) => (
                        <ListItem key={comment.id} alignItems="flex-start">
                          <ListItemText
                            primary={comment.authorName}
                            secondary={
                              <>
                                <Typography component="span" variant="body2" color="text.primary">
                                  {comment.content}
                                </Typography>
                                <Typography variant="caption" display="block">
                                  {format(new Date(comment.createdAt), 'dd/MM/yyyy HH:mm')}
                                </Typography>
                              </>
                            }
                          />
                        </ListItem>
                      ))}
                    </List>
                  </CardContent>
                </Card>
              )}
            </Grid>
          </Grid>
        </Paper>
      </Box>

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
    </Container>
  );
};

export default RegistrationPublic;