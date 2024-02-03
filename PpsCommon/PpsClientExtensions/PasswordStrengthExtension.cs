using PpsCommon.Dtos;
using PpsCommon.Models.PpsModels;

namespace PpsCommon.PpsClientExtensions;

public static class PasswordStrengthExtension
{
    public static Task<PasswordStrength> PostPasswordStrength(this PpsClient ppsClient, string testPassword) 
        => ppsClient.GenericPost<PasswordStrengthRequestDto, PasswordStrength>(
            new PasswordStrengthRequestDto(testPassword), "/api/v5/rest/passwordstrength");
}