using TaskHandler.Domain.Services;

namespace TaskHandler.Application.Commands.Passwords;

public record ResetPasswordCommand(Guid UserId, string ResetToken, string NewPassword) : ICommand<ResetPasswordCommandResponse>;
public record ResetPasswordCommandResponse(string message, bool Success);

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, ResetPasswordCommandResponse>
{
    private readonly IUserPasswordService _userPasswordService;
    
    public ResetPasswordCommandHandler(IUserPasswordService userPasswordService)
    {
        _userPasswordService = userPasswordService;
    }
    
    public async Task<ResetPasswordCommandResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _userPasswordService.ResetPasswordAsync(request.UserId, request.ResetToken, request.NewPassword);

            if (!result)
            {
                return new ResetPasswordCommandResponse("Can't restore password", false);
            }

            return new ResetPasswordCommandResponse("Password has been reset", true);
        }
        catch (Exception e)
        {
            return new ResetPasswordCommandResponse(e.Message, false);
        }
    }
}