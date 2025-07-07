using TaskHandler.Application.Commands.Tasks;

namespace TaskHandler.Application.Validators.Tasks;

public class AddTaskItemCommandValidator : AbstractValidator<AddTaskItemCommand>
{
    public AddTaskItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotNull()
            .WithMessage("Title is required")
            .NotEmpty()
            .WithMessage("Title is required");
        
        RuleFor(x => x.UserId)
            .NotNull()
            .WithMessage("User is required")
            .NotEmpty()
            .WithMessage("User is required")
            .Must(x => x != Guid.Empty);
    }
}