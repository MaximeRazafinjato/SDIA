import { SxProps, Theme } from '@mui/material';

export const dataGridTheme: SxProps<Theme> = {
  minHeight: 400,
  '& .MuiDataGrid-root': {
    border: 'none',
  },
  '& .MuiDataGrid-cell': {
    borderBottom: '1px solid rgba(224, 224, 224, 0.4)',
    py: 1.5,
    display: 'flex',
    alignItems: 'center',
  },
  '& .MuiDataGrid-columnHeaders': {
    backgroundColor: 'primary.dark',
    color: 'white',
    borderBottom: 'none',
    '& .MuiDataGrid-columnHeader': {
      backgroundColor: 'primary.dark',
    },
    '& .MuiDataGrid-columnHeaderTitle': {
      fontWeight: 700,
      fontSize: '0.9rem',
      textTransform: 'uppercase',
      letterSpacing: 0.5,
    },
    '& .MuiIconButton-root': {
      color: 'white',
    },
  },
  '& .MuiDataGrid-row': {
    '&:hover': {
      backgroundColor: 'action.hover',
    },
    '&.Mui-selected': {
      backgroundColor: 'action.selected',
    },
  },
  '& .MuiDataGrid-footerContainer': {
    borderTop: '2px solid',
    borderColor: 'primary.main',
    backgroundColor: 'grey.50',
  },
  '& .MuiDataGrid-virtualScroller': {
    backgroundColor: 'background.paper',
  },
  '& .MuiDataGrid-toolbarContainer': {
    padding: 2,
    backgroundColor: 'grey.50',
    '& .MuiButton-text': {
      color: 'primary.main',
    },
  },
  '& .MuiDataGrid-columnSeparator': {
    color: 'white',
    opacity: 0.3,
  },
};