import React, { useState, useEffect } from 'react';
import { useParams, useLocation, useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  Alert,
  CircularProgress,
  Grid,
  Divider,
} from '@mui/material';
import { Save, Cancel, CheckCircle } from '@mui/icons-material';
import axios from 'axios';
import { useForm, Controller } from 'react-hook-form';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5206';

interface RegistrationData {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  address: string;
  city: string;
  postalCode: string;
  country: string;
  additionalData?: string;
  status: string;
  canEdit: boolean;
}

const RegistrationEdit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const location = useLocation();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<string>('');
  const [registration, setRegistration] = useState<RegistrationData | null>(null);

  const sessionToken = location.state?.sessionToken || '';
  const isVerified = location.state?.verified || false;

  const { control, handleSubmit, reset, formState: { errors, isDirty } } = useForm<RegistrationData>();

  useEffect(() => {
    if (!sessionToken || !isVerified) {
      setError('Accès non autorisé. Veuillez utiliser le lien envoyé par email.');
      setTimeout(() => navigate('/'), 3000);
      return;
    }

    fetchRegistration();
  }, [id, sessionToken]);

  const fetchRegistration = async () => {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/api/public/registration/${id}/details`,
        { params: { sessionToken } }
      );

      setRegistration(response.data);
      reset(response.data);
      setLoading(false);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Impossible de charger l\'inscription');
      setLoading(false);
    }
  };

  const onSubmit = async (data: RegistrationData) => {
    setSaving(true);
    setError('');
    setSuccess('');

    try {
      const response = await axios.put(
        `${API_BASE_URL}/api/public/registration/${id}`,
        data,
        { params: { sessionToken } }
      );

      if (response.data.success) {
        setSuccess('Inscription mise à jour avec succès !');
        reset(data);
        
        // Optionally redirect after success
        setTimeout(() => {
          setSuccess('Merci pour votre mise à jour. Vous pouvez fermer cette page.');
        }, 2000);
      }
    } catch (err: any) {
      setError(err.response?.data?.error || 'Erreur lors de la mise à jour');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
      >
        <CircularProgress />
      </Box>
    );
  }

  if (!registration) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
      >
        <Alert severity="error">
          {error || 'Inscription non trouvée'}
        </Alert>
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f5', py: 4 }}>
      <Container maxWidth="md">
        <Paper elevation={3} sx={{ p: 4 }}>
          <Box sx={{ mb: 3, textAlign: 'center' }}>
            <Typography variant="h4" component="h1" gutterBottom>
              Modifier votre inscription
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Statut actuel : {registration.status}
            </Typography>
          </Box>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          {success && (
            <Alert
              severity="success"
              sx={{ mb: 2 }}
              icon={<CheckCircle />}
            >
              {success}
            </Alert>
          )}

          {!registration.canEdit && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              Cette inscription ne peut plus être modifiée en raison de son statut.
            </Alert>
          )}

          <form onSubmit={handleSubmit(onSubmit)}>
            <Grid container spacing={3}>
              <Grid item xs={12} sm={6}>
                <Controller
                  name="firstName"
                  control={control}
                  rules={{ required: 'Le prénom est requis' }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Prénom"
                      error={!!errors.firstName}
                      helperText={errors.firstName?.message}
                      disabled={!registration.canEdit}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name="lastName"
                  control={control}
                  rules={{ required: 'Le nom est requis' }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Nom"
                      error={!!errors.lastName}
                      helperText={errors.lastName?.message}
                      disabled={!registration.canEdit}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name="email"
                  control={control}
                  rules={{
                    required: 'L\'email est requis',
                    pattern: {
                      value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                      message: 'Email invalide'
                    }
                  }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Email"
                      type="email"
                      error={!!errors.email}
                      helperText={errors.email?.message}
                      disabled={!registration.canEdit}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name="phone"
                  control={control}
                  rules={{ required: 'Le téléphone est requis' }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Téléphone"
                      error={!!errors.phone}
                      helperText={errors.phone?.message}
                      disabled={!registration.canEdit}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name="dateOfBirth"
                  control={control}
                  rules={{ required: 'La date de naissance est requise' }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Date de naissance"
                      type="date"
                      InputLabelProps={{ shrink: true }}
                      error={!!errors.dateOfBirth}
                      helperText={errors.dateOfBirth?.message}
                      disabled={!registration.canEdit}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12}>
                <Divider sx={{ my: 2 }} />
                <Typography variant="h6" gutterBottom>
                  Adresse
                </Typography>
              </Grid>

              <Grid item xs={12}>
                <Controller
                  name="address"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Adresse"
                      multiline
                      rows={2}
                      disabled={!registration.canEdit}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name="city"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Ville"
                      disabled={!registration.canEdit}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={3}>
                <Controller
                  name="postalCode"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Code postal"
                      disabled={!registration.canEdit}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={3}>
                <Controller
                  name="country"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Pays"
                      disabled={!registration.canEdit}
                    />
                  )}
                />
              </Grid>

              {registration.additionalData && (
                <Grid item xs={12}>
                  <Controller
                    name="additionalData"
                    control={control}
                    render={({ field }) => (
                      <TextField
                        {...field}
                        fullWidth
                        label="Informations complémentaires"
                        multiline
                        rows={4}
                        disabled={!registration.canEdit}
                      />
                    )}
                  />
                </Grid>
              )}
            </Grid>

            {registration.canEdit && (
              <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                <Button
                  variant="outlined"
                  startIcon={<Cancel />}
                  onClick={() => reset()}
                  disabled={saving || !isDirty}
                >
                  Annuler les modifications
                </Button>
                <Button
                  type="submit"
                  variant="contained"
                  startIcon={<Save />}
                  disabled={saving || !isDirty}
                >
                  {saving ? 'Enregistrement...' : 'Enregistrer'}
                </Button>
              </Box>
            )}
          </form>
        </Paper>
      </Container>
    </Box>
  );
};

export default RegistrationEdit;