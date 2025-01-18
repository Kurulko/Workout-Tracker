import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './shared/helpers/guards/auth.guard';
import { LoginComponent } from '../app/auth/components/login/login.component';
import { RegisterComponent } from '../app/auth/components/register/register.component';
import { LogoutComponent } from './auth/components/logout.component';
import { NotFoundComponent } from '../app/not-found/not-found.component';
import { MusclesComponent } from '../app/muscles/muscles.component';
import { MuscleEditComponent } from '../app/muscles/edit-muscle.component';
import { BodyWeightsComponent } from '../app/body-weights/body-weights.component';
import { BodyWeightEditComponent } from '../app/body-weights/edit-body-weight.component';
import { MuscleSizesComponent } from '../app/muscle-sizes/muscle-sizes.component';
import { MuscleSizeEditComponent } from '../app/muscle-sizes/edit-muscle-size.component';
import { EquipmentsComponent } from '../app/equipments/equipments.component';
import { ExerciseEditComponent } from './exercises/components/edit-exercise/edit-exercise.component';
import { UsersComponent } from './users/users.component';
import { EditUserComponent } from './users/edit-user.component';
import { RolesComponent } from './roles/roles.component';
import { AccountComponent } from './account/components/account.component';
import { PasswordComponent } from './account/components/password.component';
import { UserProgressComponent } from './user-progress/user-progress.component';
import { WorkoutsComponent } from './workouts/workouts.component';
import { EditWorkoutComponent } from './workouts/edit-workout.component';
import { WorkoutDetailsComponent } from './workouts/workout-details.component';
import { ProfileComponent } from './account/components/profile.component';
import { WorkoutRecordsComponent } from './workout-records/workout-records.component';
import { EditWorkoutRecordComponent } from './workout-records/edit-workout-record.component';
import { WorkoutRecordDetailsComponent } from './workout-records/workout-record-details.component';
import { UserDetailsComponent } from './account/components/user-details.component';
import { AdminGuard } from './shared/helpers/guards/admin.guard';
import { ExercisesComponent } from './exercises/components/exercises/exercises.component';
import { ExerciseDetailsComponent } from './exercises/components/exercise-details/exercise-details.component';
import { EquipmentDetailsComponent } from './equipments/equipment-details.component';
import { MuscleDetailsComponent } from './muscles/muscle-details.component';
import { EditExerciseRecordComponent } from './exercise-records/edit-exercise-record.component';
import { ExerciseRecordsComponent } from './exercise-records/exercise-records.component';
import { RoleDetailsComponent } from './roles/role-details.component';

const routes: Routes = [
    { path: '', redirectTo: '/workouts', pathMatch: 'full' },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'logout', component: LogoutComponent, canActivate: [AuthGuard] },
    
    { path: 'muscles', component: MusclesComponent, canActivate: [AuthGuard] },
    { path: 'muscle', component: MuscleEditComponent, canActivate: [AdminGuard] },
    { path: 'muscle/:id', component: MuscleEditComponent, canActivate: [AdminGuard] },
    { path: 'muscle/:id/details', component: MuscleDetailsComponent, canActivate: [AuthGuard] },
    
    { path: 'body-weights', component: BodyWeightsComponent, canActivate: [AuthGuard] },
    { path: 'body-weight', component: BodyWeightEditComponent, canActivate: [AuthGuard] },
    { path: 'body-weight/:id', component: BodyWeightEditComponent, canActivate: [AuthGuard] },
    
    { path: 'muscle-sizes', component: MuscleSizesComponent, canActivate: [AuthGuard] },
    { path: 'muscle-size', component: MuscleSizeEditComponent, canActivate: [AuthGuard] },
    { path: 'muscle-size/:id', component: MuscleSizeEditComponent, canActivate: [AuthGuard] },
    
    { path: 'equipments', component: EquipmentsComponent, canActivate: [AuthGuard] },
    { path: 'your-equipment/:id/details', component: EquipmentDetailsComponent, canActivate: [AuthGuard] },
    { path: 'equipment/:id/details', component: EquipmentDetailsComponent, canActivate: [AuthGuard] },

    { path: 'exercises', component: ExercisesComponent, canActivate: [AuthGuard] },
    { path: 'your-exercise', component: ExerciseEditComponent, canActivate: [AuthGuard] },
    { path: 'your-exercise/:id', component: ExerciseEditComponent, canActivate: [AuthGuard] },
    { path: 'exercise', component: ExerciseEditComponent, canActivate: [AdminGuard] },
    { path: 'exercise/:id', component: ExerciseEditComponent, canActivate: [AdminGuard] },
    { path: 'your-exercise/:id/details', component: ExerciseDetailsComponent, canActivate: [AuthGuard] },
    { path: 'exercise/:id/details', component: ExerciseDetailsComponent, canActivate: [AuthGuard] },

    { path: 'exercise-records', component: ExerciseRecordsComponent, canActivate: [AuthGuard] },
    { path: 'exercise-record', component: EditExerciseRecordComponent, canActivate: [AuthGuard] },
    { path: 'exercise-record/:id', component: EditExerciseRecordComponent, canActivate: [AuthGuard] },

    { path: 'users', component: UsersComponent, canActivate: [AdminGuard] },
    { path: 'user', component: EditUserComponent, canActivate: [AdminGuard] },
    { path: 'user/:id', component: EditUserComponent, canActivate: [AdminGuard] },

    { path: 'roles', component: RolesComponent, canActivate: [AdminGuard] },
    { path: 'role/:id/details', component: RoleDetailsComponent, canActivate: [AdminGuard] },

    { path: 'workouts', component: WorkoutsComponent, canActivate: [AuthGuard] },
    { path: 'workout', component: EditWorkoutComponent, canActivate: [AuthGuard] },
    { path: 'workout/:id', component: EditWorkoutComponent, canActivate: [AuthGuard] },
    { path: 'workout-details/:id', component: WorkoutDetailsComponent, canActivate: [AuthGuard] },

    { path: 'workout-records', component: WorkoutRecordsComponent, canActivate: [AuthGuard] },
    { path: 'workouts/:workoutId/workout-records', component: WorkoutRecordsComponent, canActivate: [AuthGuard] },
    { path: 'workouts/:workoutId/workout-record', component: EditWorkoutRecordComponent, canActivate: [AuthGuard] },
    { path: 'workouts/workout-record', component: EditWorkoutRecordComponent, canActivate: [AuthGuard] },
    { path: 'workouts/:workoutId/workout-record/:id', component: EditWorkoutRecordComponent, canActivate: [AuthGuard] },
    { path: 'workouts/:workoutId/workout-record-details/:id', component: WorkoutRecordDetailsComponent, canActivate: [AuthGuard] },

    { path: 'account', component: AccountComponent, canActivate: [AuthGuard] },
    { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
    { path: 'password', component: PasswordComponent, canActivate: [AuthGuard] },
    { path: 'progress', component: UserProgressComponent, canActivate: [AuthGuard] },
    { path: 'personal-data', component: UserDetailsComponent, canActivate: [AuthGuard] },

    { path: '**', component: NotFoundComponent },
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule {
}