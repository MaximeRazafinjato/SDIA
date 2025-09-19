import React, { useState, useEffect, useRef } from 'react';
import {
  Box,
  TextField,
  Button,
  IconButton,
  Collapse,
  Stack,
  Chip,
  InputAdornment,
  FormControlLabel,
  Checkbox,
  Paper,
  Divider,
  Typography,
} from '@mui/material';
import {
  Search as SearchIcon,
  FilterList as FilterIcon,
  Clear as ClearIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
} from '@mui/icons-material';
import { FormTemplateFilters } from '@/types/filters';

interface FormTemplateFiltersBarProps {
  filters: Partial<FormTemplateFilters>;
  onFiltersChange: (filters: Partial<FormTemplateFilters>) => void;
}

const FormTemplateFiltersBar: React.FC<FormTemplateFiltersBarProps> = ({
  filters,
  onFiltersChange,
}) => {
  const [expanded, setExpanded] = useState(false);
  const [localFilters, setLocalFilters] = useState<Partial<FormTemplateFilters>>(filters);
  const [searchTerm, setSearchTerm] = useState(filters.searchTerm || '');
  const searchTimeoutRef = useRef<NodeJS.Timeout>();

  // Handle search with debounce - only for API calls, not for input display
  useEffect(() => {
    if (searchTimeoutRef.current) clearTimeout(searchTimeoutRef.current);

    searchTimeoutRef.current = setTimeout(() => {
      if (searchTerm !== filters.searchTerm) {
        onFiltersChange({ ...filters, searchTerm, page: 1 });
      }
    }, 500); // Debounce only for API calls

    return () => {
      if (searchTimeoutRef.current) clearTimeout(searchTimeoutRef.current);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchTerm]);

  const handleFilterChange = (
    key: keyof FormTemplateFilters,
    value: FormTemplateFilters[keyof FormTemplateFilters],
  ) => {
    const newFilters = { ...localFilters, [key]: value };
    if (value === '' || value === null || value === undefined) {
      delete newFilters[key];
    }
    setLocalFilters(newFilters);
  };

  const applyFilters = () => {
    onFiltersChange({ ...localFilters, searchTerm, page: 1 });
  };

  const clearFilters = () => {
    setLocalFilters({});
    setSearchTerm('');
    onFiltersChange({ page: filters.page, pageSize: filters.pageSize });
  };

  const activeFiltersCount = Object.keys(filters).filter(
    (key) =>
      !['page', 'pageSize', 'sortBy', 'sortDescending'].includes(key) &&
      filters[key as keyof FormTemplateFilters],
  ).length;

  return (
    <Paper sx={{ p: 2, mb: 2 }}>
      <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: expanded ? 2 : 0 }}>
        {/* Search field */}
        <TextField
          size="small"
          placeholder="Rechercher par nom, description, version..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          sx={{ flex: 1, maxWidth: 400 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
            endAdornment: searchTerm && (
              <InputAdornment position="end">
                <IconButton size="small" onClick={() => setSearchTerm('')}>
                  <ClearIcon fontSize="small" />
                </IconButton>
              </InputAdornment>
            ),
          }}
        />

        {/* Quick filter - Active status */}
        <FormControlLabel
          control={
            <Checkbox
              checked={localFilters.isActive === true}
              onChange={(e) => {
                const newValue = e.target.checked ? true : undefined;
                handleFilterChange('isActive', newValue);
                onFiltersChange({ ...filters, isActive: newValue, page: 1 });
              }}
            />
          }
          label="Actifs uniquement"
        />

        {/* Active filters indicator */}
        {activeFiltersCount > 0 && (
          <Chip
            label={`${activeFiltersCount} filtre${activeFiltersCount > 1 ? 's' : ''}`}
            size="small"
            color="primary"
            onDelete={clearFilters}
          />
        )}

        {/* Expand/Collapse button */}
        <Button
          size="small"
          startIcon={<FilterIcon />}
          endIcon={expanded ? <CollapseIcon /> : <ExpandIcon />}
          onClick={() => setExpanded(!expanded)}
          variant={expanded ? 'contained' : 'outlined'}
        >
          Filtres avancés
        </Button>
      </Box>

      {/* Advanced filters */}
      <Collapse in={expanded}>
        <Divider sx={{ my: 2 }} />
        <Stack spacing={2}>
          <Typography variant="subtitle2" color="text.secondary">
            Filtres avancés
          </Typography>

          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
            <Typography variant="body2">
              Utilisez les filtres ci-dessus pour affiner votre recherche. Les formulaires peuvent
              être filtrés par:
            </Typography>
            <Stack direction="row" spacing={1}>
              <Chip label="Nom" size="small" variant="outlined" />
              <Chip label="Description" size="small" variant="outlined" />
              <Chip label="Version" size="small" variant="outlined" />
              <Chip label="Statut actif" size="small" variant="outlined" />
            </Stack>
          </Box>

          {/* Note about organization filtering */}
          <Box sx={{ p: 2, bgcolor: 'background.default', borderRadius: 1 }}>
            <Typography variant="caption" color="text.secondary">
              💡 Note: Le filtrage par organisation sera disponible dans une prochaine version.
            </Typography>
          </Box>

          {/* Action buttons */}
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
            <Button variant="outlined" onClick={clearFilters}>
              Réinitialiser
            </Button>
            <Button variant="contained" onClick={applyFilters}>
              Appliquer les filtres
            </Button>
          </Box>
        </Stack>
      </Collapse>
    </Paper>
  );
};

export default FormTemplateFiltersBar;
