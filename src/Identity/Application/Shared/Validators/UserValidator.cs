using FluentValidation;

namespace Identity.Application.Shared.Validators;

public static class UserValidator
{   
    public static IRuleBuilderOptions<T, string> BeValidFirstName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithMessage("First name is required.");
    }

    public static IRuleBuilderOptions<T, string> BeValidLastName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithMessage("Last name is required.");
    }

    public static IRuleBuilderOptions<T, string> BeValidUsername<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithMessage("Username is required.")
                .MinimumLength(6).WithMessage("Username must be at least 6 characters long.")
                .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");
    }

    public static IRuleBuilderOptions<T, string> BeValidEmail<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithMessage("Email address is required.")
                .EmailAddress().WithMessage("Email address is not valid.");
    }

    public static IRuleBuilderOptions<T, string> BeValidPhoneNumber<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\d{10}$").WithMessage("Phone number must contain exactly 10 digits.");
    }

    public static IRuleBuilderOptions<T, string> BeValidPassword<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(16).WithMessage("Password cannot exceed 16 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[\W_]").WithMessage("Password must contain at least one special character (!, @, #, $, %, &, etc.).")
                .Must(BeAValidPassword).WithMessage("Password cannot contain easily guessable words like 'password' or '123456'.");
    }

    private static bool BeAValidPassword(string password)
    {
        // Add logic to reject common passwords like 'password123', '123456', etc.
        var commonPasswords = new[] { "password123", "123456", "letmein", "qwerty", "admin", "welcome" };
        return !commonPasswords.Contains(password.ToLower());
    }
}