import React from 'react';
import { Box, Typography, Button } from '@mui/material';

interface PageLayoutProps {
  title: string;
  description?: string;
  icon?: React.ReactNode;
  actions?: React.ReactNode;
  children: React.ReactNode;
}

const PageLayout: React.FC<PageLayoutProps> = ({
  title,
  description,
  icon,
  actions,
  children,
}) => {
  return (
    <Box sx={{ 
      width: '100%', 
      minHeight: '100%',
      display: 'flex',
      flexDirection: 'column'
    }}>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Box>
          <Typography variant="h4" component="h1" fontWeight="bold" color="primary.dark" gutterBottom>
            {icon && (
              <Box component="span" sx={{ mr: 1, verticalAlign: 'middle', fontSize: '2rem' }}>
                {icon}
              </Box>
            )}
            {title}
          </Typography>
          {description && (
            <Typography variant="body2" color="textSecondary">
              {description}
            </Typography>
          )}
        </Box>
        {actions && (
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
            {actions}
          </Box>
        )}
      </Box>

      {/* Content */}
      <Box sx={{ 
        flex: 1, 
        display: 'flex', 
        flexDirection: 'column'
      }}>
        {children}
      </Box>
    </Box>
  );
};

export default PageLayout;