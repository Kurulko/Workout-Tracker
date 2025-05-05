using Microsoft.IdentityModel.Tokens;
using System.Text;
using WorkoutTracker.Application.Common.Settings;

namespace WorkoutTracker.Infrastructure.Extensions;

public static class JwtSettingsExtensions
{
    public static TokenValidationParameters ToTokenValidationParameters(this JwtSettings jwtSettings)
    {
        ArgumentNullException.ThrowIfNull(jwtSettings);

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    }
}
