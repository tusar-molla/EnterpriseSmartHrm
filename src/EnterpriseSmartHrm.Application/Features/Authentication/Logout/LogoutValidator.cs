using FluentValidation;

namespace EnterpriseSmartHrm.Application.Features.Authentication.Logout;

public sealed class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
