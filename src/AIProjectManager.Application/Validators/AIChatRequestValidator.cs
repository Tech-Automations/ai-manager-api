using AIProjectManager.Application.DTOs;
using FluentValidation;

namespace AIProjectManager.Application.Validators;

public class AIChatRequestValidator : AbstractValidator<AIChatRequestDto>
{
    public AIChatRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(4000).WithMessage("Message must not exceed 4000 characters");
    }
}

