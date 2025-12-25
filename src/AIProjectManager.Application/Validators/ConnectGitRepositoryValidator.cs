using AIProjectManager.Application.DTOs.Integration;
using FluentValidation;

namespace AIProjectManager.Application.Validators;

public class ConnectGitRepositoryValidator : AbstractValidator<ConnectGitRepositoryDto>
{
    public ConnectGitRepositoryValidator()
    {
        RuleFor(x => x.RepositoryUrl)
            .NotEmpty().WithMessage("Repository URL is required")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == "git"))
            .WithMessage("Repository URL must be a valid URL");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .Must(p => new[] { "GitHub", "GitLab", "Bitbucket" }.Contains(p))
            .WithMessage("Provider must be one of: GitHub, GitLab, Bitbucket");

        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access Token is required")
            .MinimumLength(10).WithMessage("Access Token is too short");
    }
}

