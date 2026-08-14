using FluentValidation;

namespace EnterpriseSmartHrm.Application.Features.Authentication.RefreshAccessToken;

public sealed class RefreshAccessTokenValidator : AbstractValidator<RefreshAccessTokenCommand>
{
    public RefreshAccessTokenValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
