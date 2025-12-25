using AIProjectManager.Application.DTOs.Chat;
using FluentValidation;

namespace AIProjectManager.Application.Validators;

public class ChatQueryValidator : AbstractValidator<ChatQueryDto>
{
    public ChatQueryValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required")
            .MaximumLength(2000).WithMessage("Question must not exceed 2000 characters");
    }
}

