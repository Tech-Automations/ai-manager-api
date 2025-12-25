using AIProjectManager.Application.DTOs;
using FluentValidation;

namespace AIProjectManager.Application.Validators;

public class UpdateTaskItemValidator : AbstractValidator<UpdateTaskItemDto>
{
    public UpdateTaskItemValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Status)
            .Must(s => s == "Todo" || s == "InProgress" || s == "Done" || s == "Blocked")
            .WithMessage("Status must be Todo, InProgress, Done, or Blocked");

        RuleFor(x => x.Priority)
            .Must(p => p == "Low" || p == "Medium" || p == "High" || p == "Critical")
            .WithMessage("Priority must be Low, Medium, High, or Critical");
    }
}

