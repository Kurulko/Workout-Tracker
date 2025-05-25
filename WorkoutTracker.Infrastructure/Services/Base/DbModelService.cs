using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Infrastructure.Services.Base;

internal abstract class DbModelService<TService, TModel> : BaseService<TService, TModel>
    where TModel : class, IDbModel
    where TService : IBaseService
{
    protected readonly IBaseRepository<TModel> baseRepository;
    public DbModelService(IBaseRepository<TModel> baseRepository, ILogger<TService> logger) : base(logger)
        => this.baseRepository = baseRepository;
}
