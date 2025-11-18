using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginandRegisterMVC.Models;

public class User
{
    [Key]
    [Required]
    [DataType(DataType.EmailAddress)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [NotMapped]
    [Compare("Password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Confirm Password required")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;
}
