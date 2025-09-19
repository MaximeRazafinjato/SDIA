import React, { memo } from 'react';
import { DataGrid, GridColDef, GridValidRowModel } from '@mui/x-data-grid';
import { Paper, LinearProgress, Box } from '@mui/material';
import { dataGridTheme } from '@/styles/dataGridTheme';

interface DataGridRow extends GridValidRowModel {
  id: string;
}

interface RegistrationsDataGridProps {
  rows: DataGridRow[];
  columns: GridColDef[];
  loading: boolean;
  page: number;
  pageSize: number;
  total: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}

const RegistrationsDataGrid: React.FC<RegistrationsDataGridProps> = memo(
  ({ rows, columns, loading, page, pageSize, total, onPageChange, onPageSizeChange }) => {
    return (
      <Paper
        sx={{
          p: 0,
          backgroundColor: '#fff',
          border: 1,
          borderColor: '#E0E0E0',
          borderRadius: 2,
          overflow: 'hidden',
          position: 'relative',
        }}
      >
        {loading && (
          <Box sx={{ width: '100%', position: 'absolute', top: 0, zIndex: 1 }}>
            <LinearProgress />
          </Box>
        )}
        <DataGrid
          rows={rows}
          columns={columns}
          loading={loading}
          paginationModel={{ page, pageSize }}
          onPaginationModelChange={(model) => {
            if (model.page !== page) onPageChange(model.page);
            if (model.pageSize !== pageSize) onPageSizeChange(model.pageSize);
          }}
          pageSizeOptions={[10, 25, 50, 100]}
          paginationMode="server"
          rowCount={total}
          disableRowSelectionOnClick
          autoHeight
          getRowId={(row) => row.id}
          localeText={{
            // French translations
            noRowsLabel: 'Aucune inscription',
            MuiTablePagination: {
              labelDisplayedRows: ({ from, to, count }) =>
                `${from}-${to} sur ${count !== -1 ? count : `plus de ${to}`}`,
              labelRowsPerPage: 'Lignes par page:',
            },
          }}
          sx={{
            ...dataGridTheme,
            opacity: loading ? 0.5 : 1,
            transition: 'opacity 0.2s',
          }}
          slotProps={{
            loadingOverlay: {
              variant: 'linear-progress',
              noRowsVariant: 'linear-progress',
            },
          }}
        />
      </Paper>
    );
  },
  (prevProps, nextProps) => {
    // Custom comparison for memo - only re-render if these specific props change
    return (
      prevProps.rows === nextProps.rows &&
      prevProps.loading === nextProps.loading &&
      prevProps.page === nextProps.page &&
      prevProps.pageSize === nextProps.pageSize &&
      prevProps.total === nextProps.total
    );
  },
);

RegistrationsDataGrid.displayName = 'RegistrationsDataGrid';

export default RegistrationsDataGrid;
