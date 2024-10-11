using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Settings;
using WorkoutTrackerAPI.Exceptions;

namespace WorkoutTrackerAPI.Repositories;

public class JwtHandler
{
    readonly JwtSettings jwtSettings;
    readonly UserManager<User> userManager;
    public JwtHandler(JwtSettings jwtSettings, UserManager<User> userManager)
        => (this.jwtSettings, this.userManager) = (jwtSettings, userManager);

    public virtual async Task<TokenModel> GenerateJwtTokenAsync(User user)
    {
        JwtSecurityTokenHandler tokenHandler = new();

        var roles = await userManager.GetRolesAsync(user);

        var rolesClaims = GetRolesClaims(user, roles);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(rolesClaims),
            Expires = DateTime.UtcNow.AddDays(jwtSettings.ExpirationDays),
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            SigningCredentials = GetSigningCredentials()
        };

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        var tokenModel = new TokenModel()
        {
            TokenStr = tokenHandler.WriteToken(securityToken),
            ExpirationDays = jwtSettings.ExpirationDays,
            Roles = roles.ToArray()
        };

        return tokenModel;
    }

    SigningCredentials GetSigningCredentials()
    {
        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
        var secret = new SymmetricSecurityKey(key);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }

    List<Claim> GetRolesClaims(User user, IEnumerable<string> roles)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.UserName)
        };

        foreach (string role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return claims;
    }

    public virtual ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        JwtSecurityTokenHandler tokenHandler = new();
        var validationParameters = (TokenValidationParameters)jwtSettings;
        return tokenHandler.ValidateToken(token, validationParameters, out var _);
    }
}
