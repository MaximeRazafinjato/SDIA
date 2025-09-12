using System.ComponentModel.DataAnnotations;

namespace SDIA.API.Models.Users;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name must be less than 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name must be less than 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}