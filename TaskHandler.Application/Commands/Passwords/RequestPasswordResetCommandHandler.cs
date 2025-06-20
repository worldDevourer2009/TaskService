using System.Net;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;

namespace TaskHandler.Application.Commands.Passwords;

public record RequestPasswordResetCommand(string Email) : ICommand;

public class RequestPasswordResetCommandHandler(
    IUserPasswordService userPasswordService,
    IEmailSender emailSender,
    IUserRepository context)
    : ICommandHandler<RequestPasswordResetCommand>
{
    private const string BaseUrl = "https://localhost:6000";

    public async Task<Unit> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await context.GetUserByEmail(request.Email, cancellationToken);

        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var token = await userPasswordService.GeneratePasswordResetTokenAsync(user.Id);
        var endpoint = $"{BaseUrl}/api/auth/reset-password";
        var resetLink = $"{endpoint}?userId={user.Id}&token={WebUtility.UrlEncode(token)}";

        if (user.Email == null)
        {
            throw new Exception("User email is null");
        }
        
        await emailSender.SendEmailAsync(user.Email.Value, "Reset password",
            $"Please reset your password by clicking <a href='{resetLink}'>here</a>", null, null);
        
        return Unit.Value;
    }
}