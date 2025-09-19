using Ardalis.Result;

namespace SDIA.Application._Extensions;

public static class ValidationExtensions
{
    public static void AddErrorIfNullOrWhiteSpace(
        this List<ValidationError> validationErrors,
        string? value,
        string identifier,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = identifier,
                ErrorMessage = errorMessage
            });
        }
    }

    public static void AddErrorIfNull<T>(
        this List<ValidationError> validationErrors,
        T? value,
        string identifier,
        string errorMessage) where T : class
    {
        if (value == null)
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = identifier,
                ErrorMessage = errorMessage
            });
        }
    }

    public static void AddErrorIfEmpty(
        this List<ValidationError> validationErrors,
        Guid value,
        string identifier,
        string errorMessage)
    {
        if (value == Guid.Empty)
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = identifier,
                ErrorMessage = errorMessage
            });
        }
    }

    public static void AddErrorIfExceedsLength(
        this List<ValidationError> validationErrors,
        string? value,
        int maxLength,
        string identifier,
        string errorMessage)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = identifier,
                ErrorMessage = errorMessage
            });
        }
    }

    public static void AddErrorIfLessThan(
        this List<ValidationError> validationErrors,
        int value,
        int minValue,
        string identifier,
        string errorMessage)
    {
        if (value < minValue)
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = identifier,
                ErrorMessage = errorMessage
            });
        }
    }

    public static void AddErrorIfNotMatch(
        this List<ValidationError> validationErrors,
        string? value,
        string pattern,
        string identifier,
        string errorMessage)
    {
        if (!string.IsNullOrWhiteSpace(value) && !System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = identifier,
                ErrorMessage = errorMessage
            });
        }
    }

    public static void AddErrorIfNotEmail(
        this List<ValidationError> validationErrors,
        string? email,
        string identifier,
        string errorMessage)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                {
                    validationErrors.Add(new ValidationError
                    {
                        Identifier = identifier,
                        ErrorMessage = errorMessage
                    });
                }
            }
            catch
            {
                validationErrors.Add(new ValidationError
                {
                    Identifier = identifier,
                    ErrorMessage = errorMessage
                });
            }
        }
    }
}