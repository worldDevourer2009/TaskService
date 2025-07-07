using TaskHandler.Shared.Tasks.DTOs.Tasks;

namespace TaskHandler.Application.Common.Validators.Tasks;

public class UpdateTaskDTOValidator : AbstractValidator<UpdateTaskDTO>
{
    public UpdateTaskDTOValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage("Id is required")
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotNull()
            .WithMessage("Title is required");
    }
}