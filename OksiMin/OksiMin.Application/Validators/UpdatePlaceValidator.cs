using FluentValidation;
using OksiMin.Application.DTOs;
using OksiMin.Domain.Constants;

namespace OksiMin.Application.Validators
{
    public class UpdatePlaceValidator : AbstractValidator<UpdatePlaceRequest>
    {
        public UpdatePlaceValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

            RuleFor(x => x.Municipality)
                .NotEmpty().WithMessage("Municipality is required")
                .MaximumLength(100)
                .Must(m => OccidentalMindoroConstants.Municipalities.Contains(m, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Invalid municipality");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Valid category is required");

            RuleFor(x => x.Address)
                .MaximumLength(300).When(x => !string.IsNullOrWhiteSpace(x.Address));

            RuleFor(x => x.Description)
                .MaximumLength(2000).When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.LandmarkDirections)
                .MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.LandmarkDirections));

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);

            RuleFor(x => x.Tags)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Tags));
        }
    }
}