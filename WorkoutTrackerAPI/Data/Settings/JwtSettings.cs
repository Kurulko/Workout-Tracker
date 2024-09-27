using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WorkoutTrackerAPI.Data.Settings;

public class JwtSettings
{
    public string SecretKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpirationDays { get; set; }


    public static explicit operator TokenValidationParameters(JwtSettings jwtSettings)
       => new TokenValidationParameters
       {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateIssuerSigningKey = true,
           ValidIssuer = jwtSettings.Issuer,
           ValidAudience = jwtSettings.Audience,
           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
       };
}