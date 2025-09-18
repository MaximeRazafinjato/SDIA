import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
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
  IconButton,
  CircularProgress,
} from '@mui/material';
import {
  Lock as LockIcon,
  Visibility,
  VisibilityOff,
  CheckCircle as CheckCircleIcon,
} from '@mui/icons-material';
import apiClient from '@/api/client';

// Validation schema
const resetPasswordSchema = z
  .object({
    newPassword: z
      .string()
      .min(8, 'Le mot de passe doit contenir au moins 8 caractères')
      .regex(/[A-Z]/, 'Le mot de passe doit contenir au moins une majuscule')
      .regex(/[a-z]/, 'Le mot de passe doit contenir au moins une minuscule')
      .regex(/[0-9]/, 'Le mot de passe doit contenir au moins un chiffre')
      .regex(/[^a-zA-Z0-9]/, 'Le mot de passe doit contenir au moins un caractère spécial'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Les mots de passe ne correspondent pas',
    path: ['confirmPassword'],
  });

type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

const ResetPassword: React.FC = () => {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<boolean>(false);
  const [validatingToken, setValidatingToken] = useState(true);
  const [tokenValid, setTokenValid] = useState(false);
  const [userEmail, setUserEmail] = useState('');

  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');

  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      newPassword: '',
      confirmPassword: '',
    },
  });

  useEffect(() => {
    const validateToken = async () => {
      if (!token) {
        setError('Token manquant. Veuillez utiliser le lien envoyé par email.');
        setValidatingToken(false);
        return;
      }

      try {
        const response = await apiClient.get(`/api/auth/validate-reset-token?token=${token}`);
        if (response.data.isValid) {
          setTokenValid(true);
          setUserEmail(response.data.email || '');
        } else {
          setError('Le lien de réinitialisation est invalide ou a expiré.');
        }
      } catch {
        // unused error parameter
        setError('Erreur lors de la validation du lien.');
      } finally {
        setValidatingToken(false);
      }
    };

    validateToken();
  }, [token]);

  const onSubmit = async (data: ResetPasswordFormData) => {
    try {
      setError('');

      await apiClient.post('/api/auth/reset-password', {
        token,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword,
      });

      setSuccess(true);
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err: unknown) {
      const errorMessage =
        err && typeof err === 'object' && 'response' in err
          ? (err.response as { data?: { message?: string } })?.data?.message
          : undefined;
      setError(errorMessage || 'Une erreur est survenue lors de la réinitialisation.');
    }
  };

  if (validatingToken) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: '#f5f5f5',
          backgroundImage: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        }}
      >
        <CircularProgress size={60} />
      </Box>
    );
  }

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
              <CheckCircleIcon
                sx={{
                  fontSize: 80,
                  color: 'success.main',
                  mb: 2,
                }}
              />
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
                Mot de passe réinitialisé !
              </Typography>

              <Alert severity="success" sx={{ width: '100%', mb: 3 }}>
                Votre mot de passe a été réinitialisé avec succès. Vous allez être redirigé vers la
                page de connexion...
              </Alert>

              <CircularProgress size={30} />
            </Box>
          </Paper>
        </Container>
      </Box>
    );
  }

  if (!tokenValid) {
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
                  color: 'error.main',
                  textAlign: 'center',
                  mb: 3,
                }}
              >
                Lien invalide
              </Typography>

              <Alert severity="error" sx={{ width: '100%', mb: 3 }}>
                {error || 'Le lien de réinitialisation est invalide ou a expiré.'}
              </Alert>

              <Button
                fullWidth
                variant="contained"
                onClick={() => navigate('/forgot-password')}
                sx={{
                  mt: 2,
                  py: 1.5,
                }}
              >
                Demander un nouveau lien
              </Button>

              <Button
                fullWidth
                variant="text"
                onClick={() => navigate('/login')}
                sx={{
                  mt: 1,
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
              Nouveau mot de passe
            </Typography>

            {userEmail && (
              <Typography
                variant="body1"
                sx={{
                  textAlign: 'center',
                  mb: 4,
                  color: 'text.secondary',
                }}
              >
                Réinitialisation pour : <strong>{userEmail}</strong>
              </Typography>
            )}

            {error && (
              <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
                {error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ width: '100%' }}>
              <Controller
                name="newPassword"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    margin="normal"
                    required
                    fullWidth
                    label="Nouveau mot de passe"
                    type={showPassword ? 'text' : 'password'}
                    autoComplete="new-password"
                    autoFocus
                    error={!!errors.newPassword}
                    helperText={errors.newPassword?.message}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <LockIcon color="action" />
                        </InputAdornment>
                      ),
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                            {showPassword ? <VisibilityOff /> : <Visibility />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                  />
                )}
              />

              <Controller
                name="confirmPassword"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    margin="normal"
                    required
                    fullWidth
                    label="Confirmer le mot de passe"
                    type={showConfirmPassword ? 'text' : 'password'}
                    autoComplete="new-password"
                    error={!!errors.confirmPassword}
                    helperText={errors.confirmPassword?.message}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <LockIcon color="action" />
                        </InputAdornment>
                      ),
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                            edge="end"
                          >
                            {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                  />
                )}
              />

              <Typography
                variant="caption"
                sx={{
                  display: 'block',
                  mt: 2,
                  mb: 2,
                  color: 'text.secondary',
                }}
              >
                Le mot de passe doit contenir :
                <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
                  <li>Au moins 8 caractères</li>
                  <li>Au moins une majuscule</li>
                  <li>Au moins une minuscule</li>
                  <li>Au moins un chiffre</li>
                  <li>Au moins un caractère spécial</li>
                </ul>
              </Typography>

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
                  'Réinitialiser le mot de passe'
                )}
              </Button>

              <Button
                fullWidth
                variant="text"
                onClick={() => navigate('/login')}
                sx={{
                  mt: 1,
                }}
              >
                Annuler et retourner à la connexion
              </Button>
            </Box>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};

export default ResetPassword;
