using System.ComponentModel.DataAnnotations;

namespace RailwayApp.Application.Models;

public class ChangePasswordRequest
{
    public string OldPassword { get; set; }

    [Required(ErrorMessage = "password is required")]
    [MaxLength(30, ErrorMessage = "max password length is 30")]
    [MinLength(4, ErrorMessage = "min length is 4")]
    public string NewPassword { get; set; }

    [Compare(nameof(NewPassword))] public string DuplicateNewPassword { get; set; }
}