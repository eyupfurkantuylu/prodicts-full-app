using System.ComponentModel.DataAnnotations;

namespace Application.Models.DTOs;

public class UpdateUserDto
{
    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public bool? EmailVerified { get; set; }

    public string? CurrentSubscriptionPlan { get; set; }

    public DateTime? SubscriptionExpiresAt { get; set; }

    public bool? IsActive { get; set; }
}
