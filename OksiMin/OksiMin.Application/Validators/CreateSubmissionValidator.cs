using FluentValidation;
using OksiMin.Application.DTOs;
using OksiMin.Domain.Constants;

namespace OksiMin.Application.Validators
{
    public class CreateSubmissionValidator : AbstractValidator<CreateSubmissionRequest>
    {

        public CreateSubmissionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters")
                .Must(BeValidName).WithMessage("Name contains invalid characters");

            RuleFor(x => x.Municipality)
                .NotEmpty().WithMessage("Municipality is required")
                .MaximumLength(100).WithMessage("Municipality cannot exceed 100 characters")
                .Must(BeValidMunicipality).WithMessage("Invalid municipality. Must be one of the 11 municipalities in Occidental Mindoro.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Valid category is required");

            RuleFor(x => x.Address)
                .MaximumLength(300).WithMessage("Address cannot exceed 300 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Address));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.LandmarkDirections)
                .MaximumLength(1000).WithMessage("Landmark directions cannot exceed 1000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.LandmarkDirections));

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
                .When(x => x.Longitude.HasValue);

            // If latitude is provided, longitude should also be provided (and vice versa)
            RuleFor(x => x)
                .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) ||
                          (!x.Latitude.HasValue && !x.Longitude.HasValue))
                .WithMessage("Both latitude and longitude must be provided together")
                .When(x => x.Latitude.HasValue || x.Longitude.HasValue);

            RuleFor(x => x.Tags)
                .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Tags));

            RuleFor(x => x.SubmitterEmail)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.SubmitterEmail));
        }

        private bool BeValidMunicipality(string municipality)
        {
            return OccidentalMindoroConstants.Municipalities.Contains(municipality, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeValidName(string name)
        {
            // Basic validation: must contain at least one letter
            return !string.IsNullOrWhiteSpace(name) && name.Any(char.IsLetter);
        }
    }
}