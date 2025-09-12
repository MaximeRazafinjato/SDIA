import { format } from 'date-fns';
import { fr } from 'date-fns/locale';

/**
 * Format a date string to French locale
 */
export const formatDate = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    return format(date, 'dd/MM/yyyy', { locale: fr });
  } catch (error) {
    return dateString;
  }
};

/**
 * Format a date string to French locale with time
 */
export const formatDateTime = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    return format(date, 'dd/MM/yyyy HH:mm', { locale: fr });
  } catch (error) {
    return dateString;
  }
};

/**
 * Format a number to French currency
 */
export const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('fr-FR', {
    style: 'currency',
    currency: 'EUR',
  }).format(amount);
};

/**
 * Format a number with thousands separator
 */
export const formatNumber = (num: number): string => {
  return new Intl.NumberFormat('fr-FR').format(num);
};

/**
 * Format a percentage
 */
export const formatPercentage = (value: number, decimals: number = 1): string => {
  return `${value.toFixed(decimals)}%`;
};

/**
 * Capitalize first letter of a string
 */
export const capitalize = (str: string): string => {
  return str.charAt(0).toUpperCase() + str.slice(1).toLowerCase();
};

/**
 * Format full name from first and last name
 */
export const formatFullName = (firstName: string, lastName: string): string => {
  return `${firstName} ${lastName}`.trim();
};

/**
 * Get initials from a name
 */
export const getInitials = (firstName: string, lastName: string): string => {
  const firstInitial = firstName.charAt(0).toUpperCase();
  const lastInitial = lastName.charAt(0).toUpperCase();
  return `${firstInitial}${lastInitial}`;
};