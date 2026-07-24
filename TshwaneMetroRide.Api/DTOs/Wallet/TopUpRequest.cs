using System.ComponentModel.DataAnnotations;

namespace TshwaneMetroRide.Api.DTOs.Wallet;

public class TopUpRequest
{
    [Required]
    [Range(0.01, 10000)]
    public decimal Amount { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }
}
