namespace ASPNETCore2JwtAuthentication.Models;

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public required string OldPassword { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 4)]
    [DataType(DataType.Password)]
    public required string NewPassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword))]
    public required string ConfirmPassword { get; set; }
}