import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  Alert,
  CircularProgress,
  Stepper,
  Step,
  StepLabel,
} from '@mui/material';
import { LockOutlined, CheckCircleOutline, SmsOutlined } from '@mui/icons-material';
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5206';

const RegistrationAccess: React.FC = () => {
  const { token } = useParams<{ token: string }>();
  const navigate = useNavigate();
  const hasRequestedCode = useRef(false);

  const [activeStep, setActiveStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<string>('');

  const [phoneNumber, setPhoneNumber] = useState<string>('');
  const [verificationCode, setVerificationCode] = useState('');
  const [attemptsRemaining, setAttemptsRemaining] = useState(5);
  // const [sessionData, setSessionData] = useState<unknown>(null);

  const steps = ['Vérification du lien', 'Code de vérification', 'Accès au dossier'];

  // Step 1: Request verification code when component mounts
  const requestVerificationCode = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const response = await axios.post(
        `${API_BASE_URL}/api/public/registration-access/${token}/request-code`,
        {},
        {
          headers: {
            'Content-Type': 'application/json',
          },
        },
      );

      console.log('Response:', response.data);

      if (response.data.success) {
        setPhoneNumber(response.data.phoneNumber);
        setSuccess(response.data.message);
        setActiveStep(1);
      } else {
        setError("Une erreur est survenue lors de l'envoi du code");
      }
    } catch (err: unknown) {
      const errorResponse =
        err && typeof err === 'object' && 'response' in err
          ? (err.response as {
              data?: { error?: string; requiresPhoneUpdate?: boolean };
              message?: string;
            })
          : null;
      const errorMessage =
        err && typeof err === 'object' && 'message' in err
          ? (err as { message: string }).message
          : null;
      console.error('Error:', err);

      if (errorResponse?.data?.error) {
        setError(errorResponse.data.error);
      } else if (errorResponse?.data?.requiresPhoneUpdate) {
        setError("Aucun numéro de téléphone associé. Veuillez contacter l'administration.");
      } else if (errorMessage) {
        setError(errorMessage);
      } else {
        setError('Impossible de vérifier le lien. Veuillez vérifier votre connexion.');
      }
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    if (token && !hasRequestedCode.current) {
      hasRequestedCode.current = true;
      requestVerificationCode();
    }
  }, [token, requestVerificationCode]);

  const verifyCode = async () => {
    if (!verificationCode || verificationCode.length !== 6) {
      setError('Veuillez entrer un code à 6 chiffres');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const response = await axios.post(
        `${API_BASE_URL}/api/public/registration-access/${token}/verify-code`,
        { code: verificationCode },
      );

      if (response.data.success) {
        setSuccess('Vérification réussie ! Redirection...');
        // setSessionData(response.data);
        setActiveStep(2);

        // Redirect to edit page after 2 seconds
        setTimeout(() => {
          navigate(`/registration-edit/${response.data.registrationId}`, {
            state: {
              sessionToken: response.data.sessionToken,
              verified: true,
            },
          });
        }, 2000);
      }
    } catch (err: unknown) {
      const errorData =
        err && typeof err === 'object' && 'response' in err
          ? (
              err.response as {
                data?: { error?: string; attemptsRemaining?: number; maxAttemptsReached?: boolean };
              }
            )?.data
          : undefined;
      setError(errorData?.error || 'Code incorrect');

      if (errorData?.attemptsRemaining !== undefined) {
        setAttemptsRemaining(errorData.attemptsRemaining);
      }

      if (errorData?.maxAttemptsReached) {
        setError('Trop de tentatives. Veuillez demander un nouveau lien.');
        setTimeout(() => {
          navigate('/');
        }, 3000);
      }
    } finally {
      setLoading(false);
    }
  };

  const resendCode = async () => {
    setVerificationCode('');
    setError('');
    setSuccess('');
    await requestVerificationCode();
  };

  const handleCodeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value.replace(/\D/g, ''); // Only allow digits
    if (value.length <= 6) {
      setVerificationCode(value);
    }
  };

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
                mb: 3,
              }}
            >
              Accès à votre inscription
            </Typography>

            <Stepper activeStep={activeStep} sx={{ width: '100%', mb: 4 }}>
              {steps.map((label) => (
                <Step key={label}>
                  <StepLabel>{label}</StepLabel>
                </Step>
              ))}
            </Stepper>

            {error && (
              <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
                {error}
              </Alert>
            )}

            {success && (
              <Alert severity="success" sx={{ width: '100%', mb: 2 }}>
                {success}
              </Alert>
            )}

            {loading && (
              <Box sx={{ display: 'flex', justifyContent: 'center', my: 3 }}>
                <CircularProgress />
              </Box>
            )}

            {!loading && activeStep === 0 && (
              <Box sx={{ textAlign: 'center', mt: 2 }}>
                <LockOutlined sx={{ fontSize: 48, color: 'primary.main', mb: 2 }} />
                <Typography variant="body1" color="text.secondary">
                  Vérification du lien en cours...
                </Typography>
              </Box>
            )}

            {!loading && activeStep === 1 && (
              <Box sx={{ width: '100%', mt: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                  <SmsOutlined sx={{ mr: 1, color: 'primary.main' }} />
                  <Typography variant="body1">Un code a été envoyé au {phoneNumber}</Typography>
                </Box>

                <TextField
                  fullWidth
                  label="Code de vérification"
                  value={verificationCode}
                  onChange={handleCodeChange}
                  placeholder="000000"
                  variant="outlined"
                  inputProps={{
                    style: {
                      fontSize: '1.5rem',
                      letterSpacing: '0.5rem',
                      textAlign: 'center',
                    },
                    maxLength: 6,
                  }}
                  sx={{ mb: 2 }}
                  autoFocus
                />

                {attemptsRemaining < 5 && (
                  <Typography variant="body2" color="error" sx={{ mb: 2 }}>
                    Tentatives restantes : {attemptsRemaining}
                  </Typography>
                )}

                <Button
                  fullWidth
                  variant="contained"
                  onClick={verifyCode}
                  disabled={verificationCode.length !== 6}
                  sx={{ mb: 2 }}
                >
                  Vérifier le code
                </Button>

                <Button
                  fullWidth
                  variant="text"
                  onClick={resendCode}
                  sx={{ textTransform: 'none' }}
                >
                  Renvoyer le code
                </Button>
              </Box>
            )}

            {!loading && activeStep === 2 && (
              <Box sx={{ textAlign: 'center', mt: 2 }}>
                <CheckCircleOutline sx={{ fontSize: 64, color: 'success.main', mb: 2 }} />
                <Typography variant="h6" gutterBottom>
                  Vérification réussie !
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  Redirection vers votre dossier...
                </Typography>
              </Box>
            )}
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};

export default RegistrationAccess;
