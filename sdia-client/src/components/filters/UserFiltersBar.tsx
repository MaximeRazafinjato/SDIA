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
} from '@mui/material';
import {
  Search as SearchIcon,
  FilterList as FilterIcon,
  Clear as ClearIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
} from '@mui/icons-material';
import { DatePicker } from '@mui/x-date-pickers';
import { UserFilters } from '@/types/filters';

interface UserFiltersBarProps {
  filters: Partial<UserFilters>;
  onFiltersChange: (filters: Partial<UserFilters>) => void;
}

const UserFiltersBar: React.FC<UserFiltersBarProps> = ({ filters, onFiltersChange }) => {
  const [expanded, setExpanded] = useState(false);
  const [localFilters, setLocalFilters] = useState<Partial<UserFilters>>(filters);
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

  const handleFilterChange = (key: keyof UserFilters, value: UserFilters[keyof UserFilters]) => {
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
      filters[key as keyof UserFilters],
  ).length;

  return (
    <Paper sx={{ p: 2, mb: 2 }}>
      <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: expanded ? 2 : 0 }}>
        {/* Search field */}
        <TextField
          size="small"
          placeholder="Rechercher par nom, email, téléphone..."
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
        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>Rôle</InputLabel>
          <Select
            value={localFilters.role || ''}
            label="Rôle"
            onChange={(e) => {
              const newValue = e.target.value;
              handleFilterChange('role', newValue);
              onFiltersChange({ ...filters, role: newValue || undefined, page: 1 });
            }}
          >
            <MenuItem value="">Tous</MenuItem>
            <MenuItem value="User">Utilisateur</MenuItem>
            <MenuItem value="Manager">Manager</MenuItem>
            <MenuItem value="Admin">Admin</MenuItem>
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
        <Stack spacing={2}>
          <Typography variant="subtitle2" color="text.secondary">
            Filtres avancés
          </Typography>

          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))',
              gap: 2,
            }}
          >
            {/* Status filters */}
            <FormControlLabel
              control={
                <Checkbox
                  checked={localFilters.isActive === true}
                  indeterminate={localFilters.isActive === undefined}
                  onChange={(e) => {
                    handleFilterChange(
                      'isActive',
                      e.target.checked ? true : localFilters.isActive === true ? false : undefined,
                    );
                  }}
                />
              }
              label="Actif uniquement"
            />

            <FormControlLabel
              control={
                <Checkbox
                  checked={localFilters.emailConfirmed === true}
                  indeterminate={localFilters.emailConfirmed === undefined}
                  onChange={(e) => {
                    handleFilterChange(
                      'emailConfirmed',
                      e.target.checked
                        ? true
                        : localFilters.emailConfirmed === true
                          ? false
                          : undefined,
                    );
                  }}
                />
              }
              label="Email confirmé"
            />

            <FormControlLabel
              control={
                <Checkbox
                  checked={localFilters.phoneConfirmed === true}
                  indeterminate={localFilters.phoneConfirmed === undefined}
                  onChange={(e) => {
                    handleFilterChange(
                      'phoneConfirmed',
                      e.target.checked
                        ? true
                        : localFilters.phoneConfirmed === true
                          ? false
                          : undefined,
                    );
                  }}
                />
              }
              label="Téléphone confirmé"
            />
          </Box>

          {/* Date filters */}
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))',
              gap: 2,
            }}
          >
            <DatePicker
              label="Créé depuis"
              value={localFilters.createdFrom ? new Date(localFilters.createdFrom) : null}
              onChange={(date) => handleFilterChange('createdFrom', date?.toISOString())}
              slotProps={{ textField: { size: 'small', fullWidth: true } }}
            />

            <DatePicker
              label="Créé jusqu'à"
              value={localFilters.createdTo ? new Date(localFilters.createdTo) : null}
              onChange={(date) => handleFilterChange('createdTo', date?.toISOString())}
              slotProps={{ textField: { size: 'small', fullWidth: true } }}
            />

            <DatePicker
              label="Dernière connexion depuis"
              value={localFilters.lastLoginFrom ? new Date(localFilters.lastLoginFrom) : null}
              onChange={(date) => handleFilterChange('lastLoginFrom', date?.toISOString())}
              slotProps={{ textField: { size: 'small', fullWidth: true } }}
            />

            <DatePicker
              label="Dernière connexion jusqu'à"
              value={localFilters.lastLoginTo ? new Date(localFilters.lastLoginTo) : null}
              onChange={(date) => handleFilterChange('lastLoginTo', date?.toISOString())}
              slotProps={{ textField: { size: 'small', fullWidth: true } }}
            />
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

export default UserFiltersBar;
