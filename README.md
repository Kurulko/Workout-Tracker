# WorkoutTracker

WorkoutTracker is a web application designed to help users track their fitness progress through workouts, exercises, body and muscle measurements, and more. It offers a seamless experience through an **Angular frontend** and a **.NET Core backend**, providing robust tracking and reporting features.

---

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Environment Configuration](#environment-configuration)
- [Running](#running)
- [API Documentation](#api-documentation)
- [Internal Pages](#internal-pages)

---

## Features

WorkoutTracker provides the following features:

1. **User Authentication and Authorization**
   - Secure user registration and login with JWT-based authentication.
   - Role-based access control (admin/user roles).
   - Password management (change/reset).

2. **Workout Tracking**
   - Log exercises with sets, reps, weight, and duration.
   - View workout history and track progress over time.
   - Add personal workout routines.

3. **Muscle Group Management**
   - View muscle categories and their related exercises.
   - Filter exercises by targeted muscle groups.

4. **Body and Muscle Measurements Tracking**
   - Record weight and muscle measurements over time.
   - Progress visualization via charts.

5. **User Profile Management**
   - Update profile details and preferences.
   - Upload profile photos.

6. **Admin Dashboard**
   - Manage users and roles.
   - Impersonate users.

7. **Responsive Design**
   - Mobile-friendly interface with Angular Material.

---

## Technologies Used

### **Backend (ASP.NET)**
- **ASP.NET Core 6** - Backend API framework.
- **Entity Framework Core 6** - Database interaction and ORM.
- **Microsoft SQL Server** - Relational database storage.
- **AutoMapper** - Object-to-object mapping.
- **xUnit** - Unit testing framework.
- **Newtonsoft.Json** - JSON processing
- **Swagger** - API documentation
- Logging: **Serilog**
- Authentication: **JWT (JSON Web Token)**

#### Backend Packages:
```
<ItemGroup>
  <PackageReference Include="AutoMapper" Version="13.0.1" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.0" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.2.3" />
</ItemGroup>
```
### **Frontend (Angular)**
- **Angular 16** - Component-based frontend framework.
- **Angular Material** - UI components and responsive design.
- **RxJS** - Reactive programming with observables.
- **SCSS** - CSS preprocessor for styling.
- **Angular Router** - Client-side navigation.
- Charts: **Chart.js with ng2-charts**
- Styling: **Bootstrap 5**

#### Frontend Dependencies:
```
"dependencies": {
  "@angular/material": "^16.2.14",
  "chart.js": "^3.9.1",
  "ng2-charts": "^4.1.1",
  "bootstrap": "^5.3.3"
}
```

---

## Prerequisites

To run the project, make sure you have the following installed:

- **Node.js (v18+)** - JavaScript runtime for running Angular.
- **Angular CLI (16+)** - CLI tool to manage Angular apps.
- **.NET 6 SDK** - Required to run the backend.
- **SQL Server** - Database for data persistence.
- **Git** - Version control.
- **npm** - Node Package Manager.

---

## Project Structure

- **Back-End (API)**: `WorkoutTrackerAPI` (ASP.NET Core 6.x)
- **Front-End**: `WorkoutTracker` (Angular 16.x)
- **Tests**: `WorkoutTrackerAPI.Tests` (xUnit)

All parts of the project are included in a single solution folder.

---

## Installation

### Step 1: **Clone the repository**
   ```
   git clone https://github.com/Kurulko/Workout-Tracker.git
   cd WorkoutTracker
  ```
### Step 2: **Install Back-End Dependencies**

#### 1. Navigate to the WorkoutTrackerAPI directory:
```
cd WorkoutTrackerAPI
```
#### 2. Restore the .NET dependencies:
```
dotnet restore
```
### Step 3. Install Front-End Dependencies:
#### 1. Navigate to the WorkoutTracker directory:
```
cd ../WorkoutTracker
```
#### 2. Install the necessary dependencies for the Angular project:
```
npm install
```

## Environment Configuration

### Backend (ASP.NET)
Modify settings in WorkoutTrackerAPI/appsettings.json

#### Configure appsettings.json

##### 1. In the WorkoutTrackerAPI project, locate or create the appsettings.json file in the root folder.
##### 2. Add your sensitive settings (e.g., connection strings, JWT settings) to appsettings.json:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb; Database=Workout; MultipleActiveResultSets=True;"
  },

  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "ExpirationDays": 7
  },

  "YourName": {
    "Name": "YourName",
    "Email": "youremail@email.com",
    "Password": "your-password"
  }
}

```

### Frontend (Angular)

#### Configure Environment Variables

##### 1. Modify WorkoutTracker/src/environments/environment.prod.ts:
```
export const environment = {
  production: true,
  apiUrl: 'https://your-production-api-url/api'
};
```

##### 2. Modify WorkoutTracker/src/proxy.conf.js:
```
const PROXY_CONFIG = [
  {
    context: [
      "/api",
      "/resources"
    ],
    target: "https://your-production-api-url/api",
    secure: false,
  }
]

module.exports = PROXY_CONFIG;
```

## Running

### Back-End (ASP.NET Core)

#### 1. Navigate to the WorkoutTrackerAPI directory and run the back-end:
```
cd WorkoutTrackerAPI
dotnet run
```
This will start your back-end server

### Front-End (Angular)

#### 1. Navigate to the WorkoutTracker directory and run the Angular application:
```
cd ../WorkoutTracker
ng serve
```
The Angular application will be available at http://localhost:4200.


## API Documentation
The backend API documentation is available via Swagger:

Swagger URL: `https://your-production-api-url/swagger`
Swagger provides endpoints documentation and testing capabilities.

## Internal Pages

### 1. Login Page (/login)
Allows users to sign in with email and password.
Shows validation messages for incorrect credentials.

### 2. Register Page (/register)
Provides user registration with field validation.

### 3. Home Page (/home)
Displays dashboard charts, stats, and recent activities.

### 4. Workout Page (/workout)
Lists logged workouts with options to add/edit/delete.

### 5. Profile Page (/profile)
Allows users to update their information.

### 6. Admin Dashboard (/admin)
Provides management tools for admin users.

