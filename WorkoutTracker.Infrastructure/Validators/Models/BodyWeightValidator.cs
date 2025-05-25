using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models;

public class BodyWeightValidator : DbModelValidator<BodyWeight>
{
    public BodyWeightValidator(IBodyWeightRepository bodyWeightRepository)
        : base("Body weight", bodyWeightRepository)
    {
    }

    public override void Validate(BodyWeight model)
    {
        ArgumentValidator.ThrowIfDateInFuture(model.Date, nameof(BodyWeight.Date));
        ArgumentValidator.ThrowIfModelWeightNegative(model.Weight, nameof(BodyWeight.Weight));
    }
}