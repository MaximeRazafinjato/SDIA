import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  Box,
  Paper,
  TextField,
  Button,
  Typography,
  Alert,
  Container,
  InputAdornment,
  CircularProgress,
} from '@mui/material';
import { Email as EmailIcon, ArrowBack as ArrowBackIcon } from '@mui/icons-material';
import apiClient from '@/api/client';

// Validation schema
const forgotPasswordSchema = z.object({
  email: z.string().min(1, "L'email est requis").email("Format d'email invalide"),
});

type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>;

const ForgotPassword: React.FC = () => {
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<boolean>(false);
  const navigate = useNavigate();

  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      email: '',
    },
  });

  const onSubmit = async (data: ForgotPasswordFormData) => {
    try {
      setError('');
      setSuccess(false);

      await apiClient.post('/api/auth/forgot-password', data);

      setSuccess(true);
    } catch (err: unknown) {
      const errorMessage =
        err && typeof err === 'object' && 'response' in err
          ? (err.response as { data?: { message?: string } })?.data?.message
          : undefined;
      setError(errorMessage || 'Une erreur est survenue. Veuillez réessayer.');
    }
  };

  if (success) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          backgroundColor: '#f5f5f5',
          backgroundImage: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        }}
      >
        <Container component="main" maxWidth="sm">
          <Paper
            elevation={24}
            sx={{
              padding: 4,
              borderRadius: 3,
              backdropFilter: 'blur(10px)',
              backgroundColor: 'rgba(255, 255, 255, 0.95)',
            }}
          >
            <Box
              sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
              }}
            >
              <Typography
                component="h1"
                variant="h5"
                gutterBottom
                sx={{
                  fontWeight: 'bold',
                  color: 'success.main',
                  textAlign: 'center',
                  mb: 3,
                }}
              >
                Email envoyé !
              </Typography>

              <Alert severity="success" sx={{ width: '100%', mb: 3 }}>
                Si l'adresse email existe dans notre système, vous recevrez un lien de
                réinitialisation dans quelques minutes. Pensez à vérifier votre dossier spam.
              </Alert>

              <Button
                fullWidth
                variant="contained"
                onClick={() => navigate('/login')}
                sx={{
                  mt: 2,
                  py: 1.5,
                }}
              >
                Retour à la connexion
              </Button>
            </Box>
          </Paper>
        </Container>
      </Box>
    );
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        backgroundColor: '#f5f5f5',
        backgroundImage: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      }}
    >
      <Container component="main" maxWidth="sm">
        <Paper
          elevation={24}
          sx={{
            padding: 4,
            borderRadius: 3,
            backdropFilter: 'blur(10px)',
            backgroundColor: 'rgba(255, 255, 255, 0.95)',
          }}
        >
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
            }}
          >
            <Typography
              component="h1"
              variant="h4"
              gutterBottom
              sx={{
                fontWeight: 'bold',
                color: 'primary.main',
                textAlign: 'center',
                mb: 2,
              }}
            >
              Mot de passe oublié
            </Typography>

            <Typography
              variant="body1"
              sx={{
                textAlign: 'center',
                mb: 4,
                color: 'text.secondary',
              }}
            >
              Entrez votre adresse email et nous vous enverrons un lien pour réinitialiser votre mot
              de passe.
            </Typography>

            {error && (
              <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
                {error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ width: '100%' }}>
              <Controller
                name="email"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    margin="normal"
                    required
                    fullWidth
                    id="email"
                    label="Adresse email"
                    autoComplete="email"
                    autoFocus
                    error={!!errors.email}
                    helperText={errors.email?.message}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <EmailIcon color="action" />
                        </InputAdornment>
                      ),
                    }}
                  />
                )}
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                disabled={isSubmitting}
                sx={{
                  mt: 3,
                  mb: 2,
                  py: 1.5,
                  fontSize: '1.1rem',
                  fontWeight: 'bold',
                }}
              >
                {isSubmitting ? (
                  <CircularProgress size={24} color="inherit" />
                ) : (
                  'Envoyer le lien de réinitialisation'
                )}
              </Button>

              <Button
                fullWidth
                variant="text"
                startIcon={<ArrowBackIcon />}
                onClick={() => navigate('/login')}
                sx={{
                  mt: 1,
                }}
              >
                Retour à la connexion
              </Button>
            </Box>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};

export default ForgotPassword;
