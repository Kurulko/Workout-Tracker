import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../app/shared/helpers/auth.guard';
import { LoginComponent } from '../app/auth/components/login/login.component';
import { RegisterComponent } from '../app/auth/components/register/register.component';
import { LogoutComponent } from '../app/auth/components/logout';
import { NotFoundComponent } from '../app/not-found/not-found.component';
import { HomeComponent } from '../app/home/home.component';
import { MusclesComponent } from '../app/muscles/muscles.component';
import { MuscleEditComponent } from '../app/muscles/edit-muscle.component';
import { BodyWeightsComponent } from '../app/body-weights/body-weights.component';
import { BodyWeightEditComponent } from '../app/body-weights/edit-body-weight.component';

const routes: Routes = [
    { path: '', redirectTo: '/home', pathMatch: 'full' },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'logout', component: LogoutComponent, canActivate: [AuthGuard] },
    { path: 'home', component: HomeComponent, canActivate: [AuthGuard] },
    { path: 'muscles', component: MusclesComponent, canActivate: [AuthGuard] },
    { path: 'muscle', component: MuscleEditComponent, canActivate: [AuthGuard] },
    { path: 'muscle/:id', component: MuscleEditComponent, canActivate: [AuthGuard] },
    { path: 'body-weights', component: BodyWeightsComponent, canActivate: [AuthGuard] },
    { path: 'body-weight', component: BodyWeightEditComponent, canActivate: [AuthGuard] },
    { path: 'body-weight/:id', component: BodyWeightEditComponent, canActivate: [AuthGuard] },
    { path: '**', component: NotFoundComponent },
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule {
}