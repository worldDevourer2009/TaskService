using TaskHandler.Application.Commands.Tasks;

namespace TaskHandler.Application.Validators.Tasks;

public class DeleteTaskItemCommandValidator : AbstractValidator<DeleteTaskItemCommand>
{
    public DeleteTaskItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage("Id is required")
            .NotEmpty();
    }
}