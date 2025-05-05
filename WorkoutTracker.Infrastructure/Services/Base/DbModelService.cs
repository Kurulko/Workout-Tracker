using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Infrastructure.Services.Base;

internal abstract class DbModelService<TModel> : BaseService<TModel>
    where TModel : class, IDbModel
{
    protected readonly IBaseRepository<TModel> baseRepository;
    public DbModelService(IBaseRepository<TModel> baseRepository)
        => this.baseRepository = baseRepository;
}
