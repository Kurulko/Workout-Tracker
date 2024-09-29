using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Repositories;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Repositories.WorkoutRepositories;

public class BaseWorkoutRepository_Tests<T> : DbModelRepository_Tests<T> where T : WorkoutModel
{
    protected async Task GetModelByName_ShouldReturnModelByName(BaseWorkoutRepository<T> baseRepository, string name)
    {
        //Act
        var modelByName = await baseRepository.GetByNameAsync(name);

        //Assert
        Assert.NotNull(modelByName);
        Assert.Equal(name, modelByName.Name);
    }

    protected async Task GetModelByName_ShouldReturnNull(BaseWorkoutRepository<T> baseRepository, string notFoundName)
    {
        //Act
        var modelByName = await baseRepository.GetByNameAsync(notFoundName);

        //Assert
        Assert.Null(modelByName);
    }

    protected async Task ModelExistsByName_ShouldReturnTrue(BaseWorkoutRepository<T> baseRepository, string name)
    {
        //Act
        var exists = await baseRepository.ExistsByNameAsync(name);

        //Assert
        Assert.True(exists);
    }

    protected async Task ModelExistsByName_ShouldReturnFalse(BaseWorkoutRepository<T> baseRepository, string notFoundName)
    {
        //Act
        var exists = await baseRepository.ExistsByNameAsync(notFoundName);

        //Assert
        Assert.False(exists);
    }
}
