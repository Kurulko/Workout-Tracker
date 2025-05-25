using System.Text.Json.Serialization;

namespace WorkoutTracker.Application.DTOs.Account;

public class RegisterModel : AccountModel
{
    public string? Email { get; set; }

    [JsonPropertyName("passwordconfirm")]
    public string PasswordConfirm { get; set; } = null!;


    public static explicit operator LoginModel(RegisterModel registerModel)
    {
        var loginModel = new LoginModel
        {
            Name = registerModel.Name,
            Password = registerModel.Password,
            RememberMe = registerModel.RememberMe
        };

        return loginModel;
    }
}
