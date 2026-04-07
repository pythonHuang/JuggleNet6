namespace Juggle.Application.Models.Request;

public class UserChangePwdRequest
{
    public string OldPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}