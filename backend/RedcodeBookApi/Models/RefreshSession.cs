using System.ComponentModel.DataAnnotations;

namespace RedcodeBookApi.Models;

public class RefreshSession
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } = string.Empty;
    [Required] public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
}
