using TaskHandler.Shared.Tasks.DTOs.Tasks;

namespace TaskHandler.Application.Common.Validators.Tasks;

public class AddTaskDTOValidator : AbstractValidator<AddTaskDTO>
{
    public AddTaskDTOValidator()
    {
        RuleFor(x => x.Title)
            .NotNull()
            .WithMessage("Title is required")
            .NotEmpty()
            .WithMessage("Title is required");
    }
}