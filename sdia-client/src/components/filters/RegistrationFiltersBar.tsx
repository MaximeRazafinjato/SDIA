import React, { useState, useEffect, useRef } from 'react';
import {
  Box,
  TextField,
  Button,
  IconButton,
  Collapse,
  Stack,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Chip,
  InputAdornment,
  FormControlLabel,
  Checkbox,
  Paper,
  Divider,
  Typography,
  Grid,
} from '@mui/material';
import {
  Search as SearchIcon,
  FilterList as FilterIcon,
  Clear as ClearIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
} from '@mui/icons-material';
import { DatePicker } from '@mui/x-date-pickers';
import { RegistrationFilters } from '@/types/filters';

interface RegistrationFiltersBarProps {
  filters: Partial<RegistrationFilters>;
  onFiltersChange: (filters: Partial<RegistrationFilters>) => void;
}

const RegistrationFiltersBar: React.FC<RegistrationFiltersBarProps> = ({
  filters,
  onFiltersChange,
}) => {
  const [expanded, setExpanded] = useState(false);
  const [localFilters, setLocalFilters] = useState<Partial<RegistrationFilters>>(filters);
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
    key: keyof RegistrationFilters,
    value: RegistrationFilters[keyof RegistrationFilters],
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
      filters[key as keyof RegistrationFilters],
  ).length;

  return (
    <Paper sx={{ p: 2, mb: 2 }}>
      <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: expanded ? 2 : 0 }}>
        {/* Search field */}
        <TextField
          size="small"
          placeholder="Rechercher par nom, email, téléphone, numéro d'inscription..."
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

        {/* Quick filters */}
        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel>Statut</InputLabel>
          <Select
            value={localFilters.status || ''}
            label="Statut"
            onChange={(e) => {
              const newValue = e.target.value;
              handleFilterChange('status', newValue);
              onFiltersChange({ ...filters, status: newValue || undefined, page: 1 });
            }}
          >
            <MenuItem value="">Tous</MenuItem>
            <MenuItem value="Draft">Brouillon</MenuItem>
            <MenuItem value="Pending">En attente</MenuItem>
            <MenuItem value="Validated">Validé</MenuItem>
            <MenuItem value="Rejected">Rejeté</MenuItem>
          </Select>
        </FormControl>

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
        <Stack spacing={3}>
          <Typography variant="subtitle2" color="text.secondary">
            Filtres avancés
          </Typography>

          {/* Verification filters */}
          <Box>
            <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
              Vérifications
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={localFilters.emailVerified === true}
                    indeterminate={localFilters.emailVerified === undefined}
                    onChange={(e) => {
                      handleFilterChange(
                        'emailVerified',
                        e.target.checked
                          ? true
                          : localFilters.emailVerified === true
                            ? false
                            : undefined,
                      );
                    }}
                  />
                }
                label="Email vérifié"
              />

              <FormControlLabel
                control={
                  <Checkbox
                    checked={localFilters.phoneVerified === true}
                    indeterminate={localFilters.phoneVerified === undefined}
                    onChange={(e) => {
                      handleFilterChange(
                        'phoneVerified',
                        e.target.checked
                          ? true
                          : localFilters.phoneVerified === true
                            ? false
                            : undefined,
                      );
                    }}
                  />
                }
                label="Téléphone vérifié"
              />

              <FormControlLabel
                control={
                  <Checkbox
                    checked={localFilters.isMinor === true}
                    indeterminate={localFilters.isMinor === undefined}
                    onChange={(e) => {
                      handleFilterChange(
                        'isMinor',
                        e.target.checked ? true : localFilters.isMinor === true ? false : undefined,
                      );
                    }}
                  />
                }
                label="Mineur"
              />
            </Box>
          </Box>

          {/* Age filters */}
          <Box>
            <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
              Âge
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
              <TextField
                size="small"
                type="number"
                label="Âge minimum"
                value={localFilters.ageFrom || ''}
                onChange={(e) =>
                  handleFilterChange('ageFrom', e.target.value ? Number(e.target.value) : undefined)
                }
                sx={{ width: 150 }}
              />
              <Typography>à</Typography>
              <TextField
                size="small"
                type="number"
                label="Âge maximum"
                value={localFilters.ageTo || ''}
                onChange={(e) =>
                  handleFilterChange('ageTo', e.target.value ? Number(e.target.value) : undefined)
                }
                sx={{ width: 150 }}
              />
            </Box>
          </Box>

          {/* Date filters */}
          <Box>
            <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
              Dates
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Créé depuis"
                  value={localFilters.createdFrom ? new Date(localFilters.createdFrom) : null}
                  onChange={(date) => handleFilterChange('createdFrom', date?.toISOString())}
                  slotProps={{ textField: { size: 'small', fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Créé jusqu'à"
                  value={localFilters.createdTo ? new Date(localFilters.createdTo) : null}
                  onChange={(date) => handleFilterChange('createdTo', date?.toISOString())}
                  slotProps={{ textField: { size: 'small', fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Soumis depuis"
                  value={localFilters.submittedFrom ? new Date(localFilters.submittedFrom) : null}
                  onChange={(date) => handleFilterChange('submittedFrom', date?.toISOString())}
                  slotProps={{ textField: { size: 'small', fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Soumis jusqu'à"
                  value={localFilters.submittedTo ? new Date(localFilters.submittedTo) : null}
                  onChange={(date) => handleFilterChange('submittedTo', date?.toISOString())}
                  slotProps={{ textField: { size: 'small', fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Validé depuis"
                  value={localFilters.validatedFrom ? new Date(localFilters.validatedFrom) : null}
                  onChange={(date) => handleFilterChange('validatedFrom', date?.toISOString())}
                  slotProps={{ textField: { size: 'small', fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Validé jusqu'à"
                  value={localFilters.validatedTo ? new Date(localFilters.validatedTo) : null}
                  onChange={(date) => handleFilterChange('validatedTo', date?.toISOString())}
                  slotProps={{ textField: { size: 'small', fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Rejeté depuis"
                  value={localFilters.rejectedFrom ? new Date(localFilters.rejectedFrom) : null}
                  onChange={(date) => handleFilterChange('rejectedFrom', date?.toISOString())}
                  slotProps={{ textField: { size: 'small', fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Rejeté jusqu'à"
                  value={localFilters.rejectedTo ? new Date(localFilters.rejectedTo) : null}
                  onChange={(date) => handleFilterChange('rejectedTo', date?.toISOString())}
                  slotProps={{ textField: { size: 'small', fullWidth: true } }}
                />
              </Grid>
            </Grid>
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

export default RegistrationFiltersBar;
