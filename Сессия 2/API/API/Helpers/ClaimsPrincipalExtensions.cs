using System.Diagnostics;
using System.Security.Claims;

namespace API.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        // Ищем ВСЕ claim с типом NameIdentifier и берём тот, который можно распарсить как число
        var userIdClaim = user.Claims
            .Where(c => c.Type == ClaimTypes.NameIdentifier)
            .Select(c => c.Value)
            .FirstOrDefault(v => int.TryParse(v, out _));

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            throw new FormatException("User ID claim not found or not a valid integer");

        return userId;
    }
}