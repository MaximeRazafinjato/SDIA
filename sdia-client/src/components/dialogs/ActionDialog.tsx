import React, { memo } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
} from '@mui/material';

interface ActionDialogProps {
  open: boolean;
  actionType: 'validate' | 'reject' | 'remind';
  actionComments: string;
  onCommentsChange: (value: string) => void;
  onClose: () => void;
  onConfirm: () => void;
}

const ActionDialog: React.FC<ActionDialogProps> = memo(
  ({ open, actionType, actionComments, onCommentsChange, onClose, onConfirm }) => {
    return (
      <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
        <DialogTitle>
          {actionType === 'validate'
            ? "Valider l'inscription"
            : actionType === 'reject'
              ? "Rejeter l'inscription"
              : 'Relancer le candidat'}
        </DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            multiline
            rows={4}
            label={
              actionType === 'reject' ? 'Raison du rejet (obligatoire)' : 'Commentaires (optionnel)'
            }
            value={actionComments}
            onChange={(e) => onCommentsChange(e.target.value)}
            margin="normal"
            required={actionType === 'reject'}
            autoFocus
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Annuler</Button>
          <Button
            onClick={onConfirm}
            variant="contained"
            color={
              actionType === 'validate' ? 'success' : actionType === 'reject' ? 'error' : 'primary'
            }
            disabled={actionType === 'reject' && !actionComments.trim()}
          >
            {actionType === 'validate'
              ? 'Valider'
              : actionType === 'reject'
                ? 'Rejeter'
                : 'Envoyer la relance'}
          </Button>
        </DialogActions>
      </Dialog>
    );
  },
);

ActionDialog.displayName = 'ActionDialog';

export default ActionDialog;
