import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

import { AppComponent } from './app.component';
import { RegisterComponent } from './auth/components/register/register.component';
import { LoginComponent } from './auth/components/login/login.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './shared/helpers/auth.interceptor';
import { AppRoutingModule } from './app-routing.module'; 
import { AngularMaterialModule } from './angular-material.module';
import { ReactiveFormsModule } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { MusclesComponent } from './muscles/components/muscles/muscles.component';
import { MuscleEditComponent } from './muscles/components/edit-muscle/edit-muscle.component';
import { BodyWeightsComponent } from './body-weights/components/body-weights/body-weights.component';
import { BodyWeightEditComponent } from './body-weights/components/edit-body-weight/edit-body-weight.component';
import { MuscleSizesComponent } from './muscles/components/muscle-sizes/muscle-sizes.component';
import { MuscleSizeEditComponent } from './muscles/components/edit-muscle-size/edit-muscle-size.component';
import { EquipmentsComponent } from './equipments/components/equipments/equipments.component';
import { ExerciseEditComponent } from './exercises/components/edit-exercise/edit-exercise.component';
import { AccountComponent } from './account/components/account/account.component';
import { WorkoutsComponent } from './workouts/components/workouts/workouts.component';
import { EditWorkoutComponent } from './workouts/components/edit-workout/edit-workout.component';
import { RolesComponent } from './roles/components/roles/roles.component';
import { EditUserComponent } from './users/components/edit-user/edit-user.component';
import { WorkoutDetailsComponent } from './workouts/components/workout-details/workout-details.component';
import { ProfileComponent } from './account/components/profile/profile.component';
import { WorkoutRecordsComponent } from './workouts/components/workout-records/workout-records.component';
import { EditWorkoutRecordComponent } from './workouts/components/edit-workout-record/edit-workout-record.component';
import { WorkoutRecordDetailsComponent } from './workouts/components/workout-record-details/workout-record-details.component';
import { ExerciseSetsEditorComponent } from './shared/components/editors/exercise-sets-editor/exercise-sets-editor.component';
import { ExerciseSelectorComponent } from './shared/components/selectors/exercise-selectors/exercise-selector/exercise-selector.component';
import { ExerciseSetGroupsEditorComponent } from './shared/components/editors/exercise-set-groups-editor/exercise-set-groups-editor.component';
import { DeleteButtonComponent } from './shared/components/helpers/delete-button/delete-button.component';
import { UserDetailsComponent } from './account/components/user-details/user-details.component';
import { MuscleSelectorComponent } from './shared/components/selectors/muscle-selectors/muscle-selector/muscle-selector.component';
import { RoleSelectorComponent } from './shared/components/selectors/role-selectors/role-selector/role-selector.component';
import { WorkoutSelectorComponent } from './shared/components/selectors/workout-selector/workout-selector.component';
import { MultipleMuscleSelectorComponent } from './shared/components/selectors/muscle-selectors/multiple-muscle-selector/multiple-muscle-selector.component';
import { MultipleEquipmentSelectorComponent } from './shared/components/selectors/equipment-selectors/multiple-equipment-selector/multiple-equipment-selector.component';
import { EquipmentSelectorComponent } from './shared/components/selectors/equipment-selectors/equipment-selector/equipment-selector.component';
import { MultipleExerciseSelectorComponent } from './shared/components/selectors/exercise-selectors/multiple-exercise-selector/multiple-exercise-selector.component';
import { MultipleRoleSelectorComponent } from './shared/components/selectors/role-selectors/multiple-role-selector/multiple-role-selector.component';
import { MainComponent } from './shared/components/base/main.component';
import { WeightTypeSelectorComponent } from './shared/components/selectors/weight-type-selector/weight-type-selector.component';
import { SizeTypeSelectorComponent } from './shared/components/selectors/size-type-selector/size-type-selector.component';
import { ExerciseTypeSelectorComponent } from './shared/components/selectors/exercise-type-selector/exercise-type-selector.component';
import { PasswordInputComponent } from './shared/components/inputs/password-input/password-input.component';
import { WeightInputComponent } from './shared/components/inputs/weight-input/weight-input.component';
import { SizeInputComponent } from './shared/components/inputs/size-input/size-input.component';
import { DateInputComponent } from './shared/components/inputs/date-input/date-input.component';
import { ModelWeightInputComponent } from './shared/components/inputs/model-weight-input/model-weight-input.component';
import { ModelSizeInputComponent } from './shared/components/inputs/model-size-input/model-size-input.component';
import { NameInputComponent } from './shared/components/inputs/name-input/name-input.component';
import { EmailInputComponent } from './shared/components/inputs/email-input/email-input.component';
import { GenderSelectorComponent } from './shared/components/selectors/gender-selector/gender-selector.component';
import { TimeSpanInputComponent } from './shared/components/inputs/time-span-input/time-span-input.component';
import { BodyFatPercentageInputComponent } from './shared/components/inputs/body-fat-percentage-input/body-fat-percentage-input.component';
import { ShowValidationErrorsComponent } from './shared/components/helpers/show-validation-errors/show-validation-errors.component';
import { ShowAuthResultErrorsComponent } from './auth/helpers/show-auth-result-errors/show-auth-result-errors.component';
import { DescriptionInputComponent } from './shared/components/inputs/description-input/description-input.component';
import { RepsInputComponent } from './shared/components/inputs/reps-input/reps-input.component';
import { ExerciseDetailsComponent } from './exercises/components/exercise-details/exercise-details.component';
import { ExercisesComponent } from './exercises/components/exercises/exercises.component';
import { EquipmentDetailsComponent } from './equipments/components/equipment-details/equipment-details.component';
import { MuscleDetailsComponent } from './muscles/components/muscle-details/muscle-details.component';
import { MuscleSizeTableComponent } from './shared/components/tables/muscle-size-table/muscle-size-table.component';
import { ExerciseTableComponent } from './shared/components/tables/exercise-table/exercise-table.component';
import { BodyWeightTableComponent } from './shared/components/tables/body-weight-table/body-weight-table.component';
import { EquipmentTableComponent } from './shared/components/tables/equipment-table/equipment-table.component';
import { MuscleTableComponent } from './shared/components/tables/muscle-table/muscle-table.component';
import { RoleTableComponent } from './shared/components/tables/role-table/role-table.component';
import { UserTableComponent } from './shared/components/tables/user-table/user-table.component';
import { WorkoutRecordTableComponent } from './shared/components/tables/workout-record-table/workout-record-table.component';
import { ExerciseRecordTableComponent } from './shared/components/tables/exercise-record-table/exercise-record-table.component';
import { WorkoutTableComponent } from './shared/components/tables/workout-table/workout-table.component';
import { ExerciseSetEditorComponent } from './shared/components/editors/exercise-set-editor/exercise-set-editor.component';
import { RoleDetailsComponent } from './roles/components/role-details/role-details.component';
import { PhotoInputComponent } from './shared/components/inputs/photo-input/photo-input.component';
import { ShortCardComponent } from './shared/components/helpers/short-card/short-card.component';
import { NgChartsModule } from 'ng2-charts';
import { BodyWeightChartComponent } from './shared/components/charts/body-weight-chart/body-weight-chart.component';
import { MuscleSizeChartComponent } from './shared/components/charts/muscle-size-chart/muscle-size-chart.component';
import { WorkoutRecordChartComponent } from './shared/components/charts/workout-record-chart/workout-record-chart.component';
import { ExerciseRecordChartComponent } from './shared/components/charts/exercise-record-chart/exercise-record-chart.component';
import { PhotoUploadDialogComponent } from './shared/components/dialogs/photo-upload-dialog/photo-upload-dialog.component';
import { NgxMultipleDatesModule } from 'ngx-multiple-dates';
import { WorkoutsVsRestChartComponent } from './shared/components/charts/workouts-vs-rest-chart/workouts-vs-rest-chart.component';
import { ReadonlyCalendarComponent } from './shared/components/calendars/readonly-calendar/readonly-calendar.component';
import { DatePipe } from '@angular/common';
import { ShortModelWeightInputComponent } from './shared/components/inputs/short-model-weight-input/short-model-weight-input.component';
import { ShortTimeSpanInputComponent } from './shared/components/inputs/short-time-span-input/short-time-span-input.component';
import { DateRangeInputComponent } from './shared/components/inputs/date-range-input/date-range-input.component';
import { PasswordComponent } from './account/components/password/password.component';
import { EditExerciseRecordComponent } from './exercises/components/edit-exercise-record/edit-exercise-record.component';
import { ExerciseRecordsComponent } from './exercises/components/exercise-records/exercise-records.component';
import { UsersComponent } from './users/components/users/users.component';
import { WorkoutProgressComponent } from './workout-progress/components/workout-progress.component';
import { ExerciseAliasesEditorComponent } from './shared/components/inputs/multi-string-input/multi-string-input.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    NotFoundComponent,
    NavMenuComponent,
    MainComponent,
    MusclesComponent,
    MuscleEditComponent,
    BodyWeightsComponent,
    BodyWeightEditComponent,
    MuscleSizesComponent,
    MuscleSizeEditComponent,
    EquipmentsComponent,
    ExercisesComponent,
    ExerciseEditComponent,
    WorkoutProgressComponent,
    UsersComponent,
    AccountComponent,
    PasswordComponent,
    WorkoutsComponent,
    EditWorkoutComponent,
    RolesComponent,
    UsersComponent,
    EditUserComponent,
    WorkoutDetailsComponent,
    ProfileComponent,
    WorkoutRecordsComponent,
    EditWorkoutRecordComponent,
    WorkoutRecordDetailsComponent,
    ExerciseSetsEditorComponent,
    ExerciseSelectorComponent,
    ExerciseSetGroupsEditorComponent,
    DeleteButtonComponent,
    UserDetailsComponent,
    MuscleSelectorComponent,
    EquipmentSelectorComponent,
    RoleSelectorComponent,
    WorkoutSelectorComponent,
    MultipleMuscleSelectorComponent,
    MultipleEquipmentSelectorComponent,
    MultipleExerciseSelectorComponent,
    MultipleRoleSelectorComponent,
    WeightTypeSelectorComponent,
    SizeTypeSelectorComponent,
    ExerciseTypeSelectorComponent,
    PasswordInputComponent,
    WeightInputComponent,
    SizeInputComponent,
    DateInputComponent,
    ModelWeightInputComponent,
    ModelSizeInputComponent,
    NameInputComponent,
    EmailInputComponent,
    GenderSelectorComponent,
    TimeSpanInputComponent,
    BodyFatPercentageInputComponent,
    ShowValidationErrorsComponent,
    ShowAuthResultErrorsComponent,
    DescriptionInputComponent,
    RepsInputComponent,
    ExerciseDetailsComponent,
    EquipmentDetailsComponent,
    MuscleDetailsComponent,
    MuscleSizeTableComponent,
    ExerciseTableComponent,
    BodyWeightTableComponent,
    EquipmentTableComponent,
    MuscleTableComponent,
    RoleTableComponent,
    UserTableComponent,
    WorkoutRecordTableComponent,
    ExerciseRecordTableComponent,
    WorkoutTableComponent,
    ExerciseRecordsComponent,
    EditExerciseRecordComponent,
    ExerciseSetEditorComponent,
    RoleDetailsComponent,
    PhotoInputComponent,
    ShortCardComponent,
    BodyWeightChartComponent,
    MuscleSizeChartComponent,
    WorkoutRecordChartComponent,
    ExerciseRecordChartComponent,
    PhotoUploadDialogComponent,
    WorkoutsVsRestChartComponent,
    ReadonlyCalendarComponent,
    ShortModelWeightInputComponent,
    ShortTimeSpanInputComponent,
    DateRangeInputComponent,
    ExerciseAliasesEditorComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    AppRoutingModule,
    AngularMaterialModule,
    ReactiveFormsModule,
    FormsModule,
    NgChartsModule,
    NgxMultipleDatesModule
  ],
  providers: [
    { 
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true 
    },
    DatePipe
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  bootstrap: [AppComponent]
})
export class AppModule { }
