using AIProjectManager.Application.DTOs.Integration;
using FluentValidation;

namespace AIProjectManager.Application.Validators;

public class ConnectAzureDevOpsValidator : AbstractValidator<ConnectAzureDevOpsDto>
{
    public ConnectAzureDevOpsValidator()
    {
        RuleFor(x => x.OrganizationUrl)
            .NotEmpty().WithMessage("Organization URL is required")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Organization URL must be a valid URL");

        RuleFor(x => x.PersonalAccessToken)
            .NotEmpty().WithMessage("Personal Access Token is required")
            .MinimumLength(10).WithMessage("Personal Access Token is too short");
    }
}

