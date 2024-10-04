using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using Xunit;
using System.Linq.Expressions;

namespace WorkoutTrackerAPI.Tests.Repositories;

public class DbModelRepository_Tests<T> where T : class, IDbModel
{
    protected readonly WorkoutContextFactory contextFactory = new WorkoutContextFactory();

    protected async Task<User> GetDefaultUserAsync(WorkoutDbContext db)
    {
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        string name = "User";
        string email = "user@email.com";
        string password = "P@$$w0rd";

        var user = await userRepository.GetUserByUsernameAsync(name);

        if (user is null)
        {
            await WorkoutContextFactory.InitializeRolesAsync(db);
            user = await UsersInitializer.InitializeAsync(userRepository, name, email, password, new[] { Roles.UserRole });
        }

        return user;
    }

    protected async Task AddModel_ShouldReturnNewModel(DbModelRepository<T> baseRepository, T validModel)
    {
        //Act
        var newModel = await baseRepository.AddAsync(validModel);

        //Assert
        Assert.NotNull(newModel);

        var modelById = await baseRepository.GetByIdAsync(newModel.Id);
        Assert.NotNull(modelById);

        Assert.Equal(newModel, modelById);
    }

    protected async Task AddModel_ShouldThrowException(DbModelRepository<T> baseRepository, T invalidModel)
    {
        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await baseRepository.AddAsync(invalidModel));
        Assert.Equal($"Entity of type {typeof(T).Name} should not have an ID assigned.", ex.Message);
    }

    protected async Task AddRangeModels_ShouldAddModelsSuccessfully(DbModelRepository<T> baseRepository, IEnumerable<T> validModels)
    {
        //Act
        await baseRepository.AddRangeAsync(validModels);

        //Assert
        foreach (var validModel in validModels)
        {
            var addedModel = await baseRepository.GetByIdAsync(validModel.Id);
            Assert.NotNull(addedModel);
            Assert.Equal(validModel, addedModel);
        }
    }

    protected async Task AddRangeModels_ShouldThrowException(DbModelRepository<T> baseRepository, IEnumerable<T> validModels)
    {
        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await baseRepository.AddRangeAsync(validModels));
        Assert.Equal($"New entities of type {typeof(T).Name} should not have an ID assigned.", ex.Message);
    }

    protected async Task RemoveModel_ShouldRemoveModelSuccessfully(DbModelRepository<T> baseRepository, long modelId)
    {
        //Act
        await baseRepository.RemoveAsync(modelId);

        //Assert
        var modelById = await baseRepository.GetByIdAsync(modelId);
        Assert.Null(modelById);
    }

    protected async Task RemoveModel_IncorrectModelID_ShouldThrowException(DbModelRepository<T> baseRepository)
    {
        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await baseRepository.RemoveAsync(-1));
        Assert.Equal($"Entity of type {typeof(T).Name} must have a positive ID to be removed.", ex.Message);
    }

    protected async Task RemoveModels_ModelNotExist_ShouldThrowException(DbModelRepository<T> baseRepository, long notFoundId)
    {
        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await baseRepository.RemoveAsync(notFoundId));
        Assert.Equal($"{typeof(T).Name} not found.", ex.Message);
    }

    protected async Task RemoveRangeModels_ShouldRemoveModelsSuccessfully(DbModelRepository<T> baseRepository, IEnumerable<T> validModels)
    {
        //Act
        await baseRepository.RemoveRangeAsync(validModels);

        //Assert
        foreach (var validModel in validModels)
        {
            var removedModel = await baseRepository.GetByIdAsync(validModel.Id);
            Assert.Null(removedModel);
        }
    }

    protected async Task RemoveRangeModels_ShouldThrowException(DbModelRepository<T> baseRepository, IEnumerable<T> invalidModels)
    {
        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await baseRepository.RemoveRangeAsync(invalidModels));
        Assert.Equal($"Entities of type {typeof(T).Name} must have a positive ID to be removed.", ex.Message);
    }

    protected async Task GetAllModels_ShouldReturnModels(DbModelRepository<T> baseRepository, int mustCountOfModels)
    {
        //Act
        var models = await baseRepository.GetAllAsync();

        //Assert
        Assert.NotNull(models);
        Assert.NotEmpty(models);
        Assert.Equal(mustCountOfModels, models.Count());
    }

    protected async Task GetAllModels_ShouldReturnEmpty(DbModelRepository<T> baseRepository)
    {
        //Act
        var addedModels = await baseRepository.GetAllAsync();

        //Assert
        Assert.NotNull(addedModels);
        Assert.Empty(addedModels);
    }

    protected async Task GetModelById_ShouldReturnModelById(DbModelRepository<T> baseRepository, long modelId)
    {
        //Act
        var modelById = await baseRepository.GetByIdAsync(modelId);

        //Assert
        Assert.NotNull(modelById);
        Assert.Equal(modelId, modelById.Id);
    }

    protected async Task GetModelById_ShouldReturnNull(DbModelRepository<T> baseRepository, long notFoundId)
    {
        //Act
        var modelById = await baseRepository.GetByIdAsync(notFoundId);

        //Assert
        Assert.Null(modelById);
    }

    protected async Task FindModels_ShouldFindModels(DbModelRepository<T> baseRepository, Expression<Func<T, bool>> validExpression, int mustCountOfModels)
    {
        //Act
        var someModels = await baseRepository.FindAsync(validExpression);

        //Assert
        Assert.NotNull(someModels);
        Assert.Equal(mustCountOfModels, someModels.Count());

        var result = someModels.All(validExpression);
        Assert.True(result);
    }

    protected async Task FindModels_ShouldReturnEmpty(DbModelRepository<T> baseRepository, Expression<Func<T, bool>> invalidExpression)
    {
        //Act
        var someModels = await baseRepository.FindAsync(invalidExpression);

        //Assert
        Assert.NotNull(someModels);
        Assert.Empty(someModels);
    }

    protected async Task ModelExists_ShouldReturnTrue(DbModelRepository<T> baseRepository, long modelId)
    {
        //Act
        var exists = await baseRepository.ExistsAsync(modelId);

        //Assert
        Assert.True(exists);
    }

    protected async Task ModelExists_ShouldReturnFalse(DbModelRepository<T> baseRepository, long notFoundId)
    {
        //Act
        var exists = await baseRepository.ExistsAsync(notFoundId);

        //Assert
        Assert.False(exists);
    }

    protected async Task UpdateModel_ShouldUpdateSuccessfully(DbModelRepository<T> baseRepository, T validModel)
    {
        //Act
        await baseRepository.UpdateAsync(validModel);

        //Assert
        var updatedModel = await baseRepository.GetByIdAsync(validModel.Id);

        Assert.NotNull(updatedModel);
        Assert.Equal(updatedModel, validModel);
    }

    protected async Task UpdateModel_ShouldThrowException(DbModelRepository<T> baseRepository, T invalidModel)
    {
        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await baseRepository.UpdateAsync(invalidModel));
        Assert.Equal($"Modified entities of type {typeof(T).Name} must have a positive ID.", ex.Message);
    }

    protected async Task SaveModelChanges_SaveEntitySuccessfully(DbModelRepository<T> baseRepository, T validModel)
    {
        //Act
        await baseRepository.SaveChangesAsync();

        //Assert
        var updatedModel = (await baseRepository.GetByIdAsync(validModel.Id))!;
        Assert.NotNull(updatedModel);
        Assert.Equal(updatedModel, validModel);
    }

    protected async Task SaveModelChanges_ShouldThrowException(DbModelRepository<T> baseRepository)
    {
        //Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(baseRepository.SaveChangesAsync);
    }
}
