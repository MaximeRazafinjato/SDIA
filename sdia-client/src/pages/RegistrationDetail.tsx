import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Paper,
  Typography,
  Button,
  TextField,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  CircularProgress,
  Snackbar,
  Card,
  CardContent,
  Divider,
  Stack,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Breadcrumbs,
  Link,
  Grid,
} from '@mui/material';
import {
  Timeline,
  TimelineItem,
  TimelineSeparator,
  TimelineConnector,
  TimelineContent,
  TimelineDot,
  TimelineOppositeContent,
} from '@mui/lab';
import PageLayout from '@/components/layout/PageLayout';
import {
  Edit as EditIcon,
  Save as SaveIcon,
  Cancel as CancelIcon,
  Send as SendIcon,
  Check as CheckIcon,
  Close as CloseIcon,
  Person as PersonIcon,
  Email as EmailIcon,
  Phone as PhoneIcon,
  Business as BusinessIcon,
  Assignment as AssignmentIcon,
  History as HistoryIcon,
  NavigateNext as NavigateNextIcon,
} from '@mui/icons-material';
import axios from '@/api/axios';
import { format } from 'date-fns';
import { fr } from 'date-fns/locale';

interface Registration {
  id: string;
  registrationNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  status: number;
  createdAt: string;
  submittedAt?: string;
  assignedToUserName?: string;
  organizationName: string;
  isMinor: boolean;
  birthDate?: string;
  address?: string;
  city?: string;
  postalCode?: string;
  country?: string;
  parentFirstName?: string;
  parentLastName?: string;
  parentEmail?: string;
  parentPhone?: string;
  comments?: string;
}

interface ActionDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: (comment: string) => void;
  title: string;
  message: string;
  actionLabel: string;
  actionColor: 'primary' | 'success' | 'error';
}

const ActionDialog: React.FC<ActionDialogProps> = ({
  open,
  onClose,
  onConfirm,
  title,
  message,
  actionLabel,
  actionColor,
}) => {
  const [comment, setComment] = useState('');

  const handleConfirm = () => {
    onConfirm(comment);
    setComment('');
  };

  const handleClose = () => {
    setComment('');
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <Typography variant="body2" sx={{ mb: 2 }}>
          {message}
        </Typography>
        <TextField
          autoFocus
          margin="dense"
          label="Commentaire / Message"
          fullWidth
          multiline
          rows={4}
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          placeholder="Entrez votre message ici..."
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Annuler</Button>
        <Button onClick={handleConfirm} color={actionColor} variant="contained">
          {actionLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

const getStatusLabel = (status: number) => {
  const statusMap: { [key: number]: string } = {
    0: 'Brouillon',
    1: 'En attente',
    2: 'En cours',
    3: 'En attente de documents',
    4: 'En attente de validation',
    5: 'Validé',
    6: 'Rejeté',
    7: 'Annulé',
    8: 'Terminé',
  };
  return statusMap[status] || 'Inconnu';
};

const getStatusColor = (status: number) => {
  const colorMap: {
    [key: number]: 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';
  } = {
    0: 'default',
    1: 'warning',
    2: 'info',
    3: 'warning',
    4: 'primary',
    5: 'success',
    6: 'error',
    7: 'default',
    8: 'success',
  };
  return colorMap[status] || 'default';
};

const RegistrationDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [registration, setRegistration] = useState<Registration | null>(null);
  const [loading, setLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const [editedData, setEditedData] = useState<Registration | null>(null);
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: '',
    severity: 'success' as 'success' | 'error',
  });

  // Action dialogs
  const [remindDialog, setRemindDialog] = useState(false);
  const [validateDialog, setValidateDialog] = useState(false);
  const [rejectDialog, setRejectDialog] = useState(false);

  const fetchRegistration = useCallback(async () => {
    setLoading(true);
    try {
      const response = await axios.get(`/api/registrations/${id}`);
      setRegistration(response.data);
      setEditedData(response.data);
    } catch (error) {
      console.error('Error fetching registration:', error);
      setSnackbar({
        open: true,
        message: 'Erreur lors du chargement du dossier',
        severity: 'error',
      });
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchRegistration();
  }, [fetchRegistration]);

  const handleEdit = () => {
    setIsEditing(true);
  };

  const handleCancelEdit = () => {
    setIsEditing(false);
    setEditedData(registration);
  };

  const handleSave = async () => {
    if (!editedData) return;

    try {
      await axios.put(`/api/registrations/${id}`, editedData);
      setRegistration(editedData);
      setIsEditing(false);
      setSnackbar({ open: true, message: 'Dossier mis à jour avec succès', severity: 'success' });
    } catch (error) {
      console.error('Error updating registration:', error);
      setSnackbar({ open: true, message: 'Erreur lors de la mise à jour', severity: 'error' });
    }
  };

  const handleFieldChange = (field: keyof Registration, value: string | number | boolean) => {
    if (editedData) {
      setEditedData({ ...editedData, [field]: value });
    }
  };

  const handleRemind = async (comment: string) => {
    try {
      await axios.post(`/api/registrations/${id}/remind`, { comment });
      setSnackbar({ open: true, message: 'Relance envoyée avec succès', severity: 'success' });
      setRemindDialog(false);
    } catch (error) {
      console.error('Error sending reminder:', error);
      setSnackbar({
        open: true,
        message: "Erreur lors de l'envoi de la relance",
        severity: 'error',
      });
    }
  };

  const handleValidate = async (comment: string) => {
    try {
      await axios.post(`/api/registrations/${id}/validate`, { comment });
      setSnackbar({ open: true, message: 'Dossier validé avec succès', severity: 'success' });
      setValidateDialog(false);
      fetchRegistration(); // Refresh to get updated status
    } catch (error) {
      console.error('Error validating registration:', error);
      setSnackbar({ open: true, message: 'Erreur lors de la validation', severity: 'error' });
    }
  };

  const handleReject = async (comment: string) => {
    try {
      await axios.post(`/api/registrations/${id}/reject`, { comment });
      setSnackbar({ open: true, message: 'Dossier rejeté', severity: 'success' });
      setRejectDialog(false);
      fetchRegistration(); // Refresh to get updated status
    } catch (error) {
      console.error('Error rejecting registration:', error);
      setSnackbar({ open: true, message: 'Erreur lors du rejet', severity: 'error' });
    }
  };

  if (loading) {
    return (
      <PageLayout title="Détail du dossier" icon={<AssignmentIcon />}>
        <Box
          sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '400px' }}
        >
          <CircularProgress />
        </Box>
      </PageLayout>
    );
  }

  if (!registration || !editedData) {
    return (
      <PageLayout title="Détail du dossier" icon={<AssignmentIcon />}>
        <Alert severity="error">Dossier non trouvé</Alert>
      </PageLayout>
    );
  }

  return (
    <PageLayout
      title={`Dossier ${registration.registrationNumber}`}
      icon={<AssignmentIcon />}
      actions={
        <Stack direction="row" spacing={2}>
          {!isEditing ? (
            <>
              <Button
                variant="outlined"
                startIcon={<SendIcon />}
                onClick={() => setRemindDialog(true)}
                disabled={registration.status === 5 || registration.status === 6}
              >
                Relancer
              </Button>
              <Button
                variant="contained"
                color="success"
                startIcon={<CheckIcon />}
                onClick={() => setValidateDialog(true)}
                disabled={registration.status === 5 || registration.status === 6}
              >
                Valider
              </Button>
              <Button
                variant="contained"
                color="error"
                startIcon={<CloseIcon />}
                onClick={() => setRejectDialog(true)}
                disabled={registration.status === 5 || registration.status === 6}
              >
                Rejeter
              </Button>
              <Button variant="contained" startIcon={<EditIcon />} onClick={handleEdit}>
                Modifier
              </Button>
            </>
          ) : (
            <>
              <Button variant="outlined" startIcon={<CancelIcon />} onClick={handleCancelEdit}>
                Annuler
              </Button>
              <Button
                variant="contained"
                color="primary"
                startIcon={<SaveIcon />}
                onClick={handleSave}
              >
                Enregistrer
              </Button>
            </>
          )}
        </Stack>
      }
    >
      <Box sx={{ mb: 2 }}>
        <Breadcrumbs separator={<NavigateNextIcon fontSize="small" />}>
          <Link
            underline="hover"
            color="inherit"
            href="#"
            onClick={(e) => {
              e.preventDefault();
              navigate('/registrations-dashboard');
            }}
          >
            Dossiers
          </Link>
          <Typography color="text.primary">{registration.registrationNumber}</Typography>
        </Breadcrumbs>
      </Box>

      <Grid container spacing={3}>
        {/* Main Information */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              <PersonIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
              Informations personnelles
            </Typography>
            <Divider sx={{ mb: 2 }} />
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  label="Prénom"
                  value={editedData.firstName}
                  onChange={(e) => handleFieldChange('firstName', e.target.value)}
                  fullWidth
                  disabled={!isEditing}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  label="Nom"
                  value={editedData.lastName}
                  onChange={(e) => handleFieldChange('lastName', e.target.value)}
                  fullWidth
                  disabled={!isEditing}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  label="Email"
                  value={editedData.email}
                  onChange={(e) => handleFieldChange('email', e.target.value)}
                  fullWidth
                  disabled={!isEditing}
                  InputProps={{
                    startAdornment: <EmailIcon sx={{ mr: 1, color: 'action.active' }} />,
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  label="Téléphone"
                  value={editedData.phone}
                  onChange={(e) => handleFieldChange('phone', e.target.value)}
                  fullWidth
                  disabled={!isEditing}
                  InputProps={{
                    startAdornment: <PhoneIcon sx={{ mr: 1, color: 'action.active' }} />,
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  label="Date de naissance"
                  type="date"
                  value={editedData.birthDate || ''}
                  onChange={(e) => handleFieldChange('birthDate', e.target.value)}
                  fullWidth
                  disabled={!isEditing}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <FormControl fullWidth disabled={!isEditing}>
                  <InputLabel>Statut mineur</InputLabel>
                  <Select
                    value={editedData.isMinor ? 'true' : 'false'}
                    onChange={(e) => handleFieldChange('isMinor', e.target.value === 'true')}
                    label="Statut mineur"
                  >
                    <MenuItem value="false">Majeur</MenuItem>
                    <MenuItem value="true">Mineur</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          </Paper>

          {/* Address Information */}
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              <BusinessIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
              Adresse
            </Typography>
            <Divider sx={{ mb: 2 }} />
            <Grid container spacing={2}>
              <Grid size={12}>
                <TextField
                  label="Adresse"
                  value={editedData.address || ''}
                  onChange={(e) => handleFieldChange('address', e.target.value)}
                  fullWidth
                  disabled={!isEditing}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  label="Ville"
                  value={editedData.city || ''}
                  onChange={(e) => handleFieldChange('city', e.target.value)}
                  fullWidth
                  disabled={!isEditing}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  label="Code postal"
                  value={editedData.postalCode || ''}
                  onChange={(e) => handleFieldChange('postalCode', e.target.value)}
                  fullWidth
                  disabled={!isEditing}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField
                  label="Pays"
                  value={editedData.country || 'France'}
                  onChange={(e) => handleFieldChange('country', e.target.value)}
                  fullWidth
                  disabled={!isEditing}
                />
              </Grid>
            </Grid>
          </Paper>

          {/* Parent Information (if minor) */}
          {editedData.isMinor && (
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" gutterBottom>
                <PersonIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                Représentant légal
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    label="Prénom du parent"
                    value={editedData.parentFirstName || ''}
                    onChange={(e) => handleFieldChange('parentFirstName', e.target.value)}
                    fullWidth
                    disabled={!isEditing}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    label="Nom du parent"
                    value={editedData.parentLastName || ''}
                    onChange={(e) => handleFieldChange('parentLastName', e.target.value)}
                    fullWidth
                    disabled={!isEditing}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    label="Email du parent"
                    value={editedData.parentEmail || ''}
                    onChange={(e) => handleFieldChange('parentEmail', e.target.value)}
                    fullWidth
                    disabled={!isEditing}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    label="Téléphone du parent"
                    value={editedData.parentPhone || ''}
                    onChange={(e) => handleFieldChange('parentPhone', e.target.value)}
                    fullWidth
                    disabled={!isEditing}
                  />
                </Grid>
              </Grid>
            </Paper>
          )}

          {/* Comments */}
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Commentaires
            </Typography>
            <Divider sx={{ mb: 2 }} />
            <TextField
              label="Commentaires internes"
              value={editedData.comments || ''}
              onChange={(e) => handleFieldChange('comments', e.target.value)}
              fullWidth
              multiline
              rows={4}
              disabled={!isEditing}
            />
          </Paper>
        </Grid>

        {/* Side Information */}
        <Grid size={{ xs: 12, md: 4 }}>
          {/* Status Card */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Statut
              </Typography>
              <Chip
                label={getStatusLabel(registration.status)}
                color={getStatusColor(registration.status)}
                sx={{ mb: 2 }}
              />
              {isEditing && (
                <FormControl fullWidth>
                  <InputLabel>Statut</InputLabel>
                  <Select
                    value={editedData.status}
                    onChange={(e) => handleFieldChange('status', e.target.value)}
                    label="Statut"
                  >
                    <MenuItem value={0}>Brouillon</MenuItem>
                    <MenuItem value={1}>En attente</MenuItem>
                    <MenuItem value={2}>En cours</MenuItem>
                    <MenuItem value={3}>En attente de documents</MenuItem>
                    <MenuItem value={4}>En attente de validation</MenuItem>
                    <MenuItem value={5}>Validé</MenuItem>
                    <MenuItem value={6}>Rejeté</MenuItem>
                    <MenuItem value={7}>Annulé</MenuItem>
                    <MenuItem value={8}>Terminé</MenuItem>
                  </Select>
                </FormControl>
              )}
            </CardContent>
          </Card>

          {/* Organization Card */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Organisation
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {registration.organizationName}
              </Typography>
              {registration.assignedToUserName && (
                <>
                  <Typography variant="subtitle2" sx={{ mt: 2 }}>
                    Assigné à
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {registration.assignedToUserName}
                  </Typography>
                </>
              )}
            </CardContent>
          </Card>

          {/* Timeline Card */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <HistoryIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                Historique
              </Typography>
              <Timeline position="right" sx={{ p: 0 }}>
                <TimelineItem>
                  <TimelineOppositeContent sx={{ display: 'none' }} />
                  <TimelineSeparator>
                    <TimelineDot color="primary" />
                    <TimelineConnector />
                  </TimelineSeparator>
                  <TimelineContent>
                    <Typography variant="subtitle2">Créé</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {format(new Date(registration.createdAt), 'dd/MM/yyyy HH:mm', { locale: fr })}
                    </Typography>
                  </TimelineContent>
                </TimelineItem>
                {registration.submittedAt && (
                  <TimelineItem>
                    <TimelineOppositeContent sx={{ display: 'none' }} />
                    <TimelineSeparator>
                      <TimelineDot color="success" />
                      <TimelineConnector />
                    </TimelineSeparator>
                    <TimelineContent>
                      <Typography variant="subtitle2">Soumis</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {format(new Date(registration.submittedAt), 'dd/MM/yyyy HH:mm', {
                          locale: fr,
                        })}
                      </Typography>
                    </TimelineContent>
                  </TimelineItem>
                )}
              </Timeline>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Action Dialogs */}
      <ActionDialog
        open={remindDialog}
        onClose={() => setRemindDialog(false)}
        onConfirm={handleRemind}
        title="Relancer l'utilisateur"
        message="Un email et/ou SMS sera envoyé à l'utilisateur pour lui rappeler de compléter son dossier."
        actionLabel="Envoyer la relance"
        actionColor="primary"
      />

      <ActionDialog
        open={validateDialog}
        onClose={() => setValidateDialog(false)}
        onConfirm={handleValidate}
        title="Valider le dossier"
        message="Le dossier sera marqué comme validé et l'utilisateur sera notifié par email et/ou SMS."
        actionLabel="Valider"
        actionColor="success"
      />

      <ActionDialog
        open={rejectDialog}
        onClose={() => setRejectDialog(false)}
        onConfirm={handleReject}
        title="Rejeter le dossier"
        message="Le dossier sera marqué comme rejeté. Veuillez indiquer la raison du rejet qui sera communiquée à l'utilisateur."
        actionLabel="Rejeter"
        actionColor="error"
      />

      {/* Snackbar */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
      >
        <Alert
          severity={snackbar.severity}
          onClose={() => setSnackbar({ ...snackbar, open: false })}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </PageLayout>
  );
};

export default RegistrationDetail;
