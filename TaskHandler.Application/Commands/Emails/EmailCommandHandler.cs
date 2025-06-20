using TaskHandler.Domain.Services;

namespace TaskHandler.Application.Commands.Emails;

public record SendEmailCommand(
    string? Email,
    string? Subject,
    string? Message,
    string? HtmlMessage,
    string[]? Attachments) : ICommand<SendEmailCommandResponse>;

public record SendEmailCommandResponse(bool Success);

public class EmailCommandHandler : ICommandHandler<SendEmailCommand, SendEmailCommandResponse>
{
    private readonly IEmailSender _emailSender;

    public EmailCommandHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task<SendEmailCommandResponse> Handle(SendEmailCommand request,
        CancellationToken cancellationToken)
    {
        var command = new SendEmailCommand(request.Email, request.Subject, request.Message, request.HtmlMessage,
            request.Attachments);

        var response = command.Email != null && command.Subject != null && command.Message != null &&
                       await _emailSender.SendEmailAsync(command.Email, command.Subject, command.Message,
                           command.HtmlMessage, command.Attachments);

        return new SendEmailCommandResponse(response);
    }
}