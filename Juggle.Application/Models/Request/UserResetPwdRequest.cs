namespace Juggle.Application.Models.Request;

public class UserResetPwdRequest
{
    public long Id { get; set; }
    public string NewPassword { get; set; } = "";
}