using FluentValidation;
using WorkoutTracker.Application.DTOs.BodyWeights;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.BodyWeights;

internal class BodyWeightCreationDTOValidator : AbstractValidator<BodyWeightCreationDTO>
{
    public BodyWeightCreationDTOValidator()
    {
        RuleFor(m => m.Date)
            .DateNotInFuture();

        RuleFor(m => m.Weight)
            .ValidModelWeight();
    }
}

