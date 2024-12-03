using ClientManagementAPI.Application.Features.Clients.Shared.Dtos;
using FluentValidation;

namespace ClientManagementAPI.Application.Features.Clients.Shared.Validators;

public class ClientImageRequestValidator : AbstractValidator<ClientImageRequest>
{
    public ClientImageRequestValidator() 
    {
        RuleFor(x => x.Filename)
            .NotEmpty().WithMessage("Filename is required.")
            .MaximumLength(255).WithMessage("Filename cannot exceed 255 characters.");

        RuleFor(x => x.URL)
            .NotEmpty().WithMessage("URL is required")
            .Must(BeValidUri).WithMessage("URL must be a valid");

        RuleFor(x => x.AltText)
            .NotEmpty().WithMessage("Alt text is required.")
            .MaximumLength(255).WithMessage("Alt text cannot exceed 255 characters.");

        RuleFor(x => x.Size)
            .GreaterThan(0).WithMessage("Size must be greater than 0 Kb.")
            .LessThan(1048576).WithMessage("Size must be less than 10 Mb.");
    }

    private bool BeValidUri(string uriString)
    {
        Uri outUri;
        return Uri.TryCreate(uriString, UriKind.Absolute, out outUri!)
               && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
    }
}