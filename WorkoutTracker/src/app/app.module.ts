import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

import { AppComponent } from './app.component';
import { BaseComponent } from './shared/components/base.component';
import { HomeComponent } from './home/home.component';
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
import { MusclesComponent } from './muscles/muscles.component';
import { MuscleEditComponent } from './muscles/edit-muscle.component';
import { BodyWeightsComponent } from './body-weights/body-weights.component';
import { BodyWeightEditComponent } from './body-weights/edit-body-weight.component';
import { MuscleSizesComponent } from './muscle-sizes/muscle-sizes.component';
import { MuscleSizeEditComponent } from './muscle-sizes/edit-muscle-size.component';
import { EquipmentsComponent } from './equipments/equipments.component';
import { EquipmentEditComponent } from './equipments/edit-equipment.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    LoginComponent,
    RegisterComponent,
    NotFoundComponent,
    NavMenuComponent,
    BaseComponent,
    MusclesComponent,
    MuscleEditComponent,
    BodyWeightsComponent,
    BodyWeightEditComponent,
    MuscleSizesComponent,
    MuscleSizeEditComponent,
    EquipmentsComponent,
    EquipmentEditComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    AppRoutingModule,
    AngularMaterialModule,
    ReactiveFormsModule,
    FormsModule
  ],
  providers: [
    { 
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true 
    }
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  bootstrap: [AppComponent]
})
export class AppModule { }
