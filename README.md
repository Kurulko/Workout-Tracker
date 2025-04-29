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

3. **Muscles, Exercises and Equipments Management**
   - View muscles, exercises and equipments and filter them.
   - Create your own exercises and equipments.

4. **Body and Muscle Measurements Tracking**
   - Record weight and muscle measurements over time.
   - Progress visualization via charts.

5. **User Profile Management**
   - Update your username and email.
   - Update profile details and preferences.

7. **Admin Dashboard**
   - Manage users and roles.
   - Impersonate users.

8. **Responsive Design**
   - Mobile-friendly interface with Angular Material.

---

## Technologies Used

### **Back-end (ASP.NET Core)**
- **ASP.NET Core 6** - Backend API framework.
- **Entity Framework Core 6** - Database interaction and ORM.
- **Microsoft SQL Server** - Relational database storage.
- **AutoMapper** - Object-to-object mapping.
- **xUnit** - Unit testing framework.
- **Newtonsoft.Json** - JSON processing
- **Swagger** - API documentation
- Logging: **Serilog**
- Authentication: **JWT (JSON Web Token)**

#### Back-end Packages:
```
 <ItemGroup>
    <PackageReference Include="AutoMapper" Version="13.0.1" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="6.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="7.0.0" />
    <PackageReference Include="Serilog.Sinks.MSSqlServer" Version="6.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.2.3" />
    <PackageReference Include="System.Linq.Dynamic.Core" Version="1.4.5" />
  </ItemGroup>
```
### **Front-end (Angular)**
- **Angular 16** - Component-based frontend framework.
- **Angular Material** - UI components and responsive design.
- **RxJS** - Reactive programming with observables.
- **SCSS** - CSS preprocessor for styling.
- **Angular Router** - Client-side navigation.
- Charts: **Chart.js with ng2-charts**
- Styling: **Bootstrap 5**

#### Front-end Dependencies:
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

---

## Environment Configuration

### Back-end (ASP.NET)
Modify settings in `WorkoutTrackerAPI/appsettings.json`

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

  "Admin": {
    "Name": "YourName",
    "Email": "youremail@email.com",
    "Password": "your-password"
  }
}

```

### Front-end (Angular)

#### Configure Environment Variables

##### 1. Modify `WorkoutTracker/src/environments/environment.prod.ts`:
```
export const environment = {
  production: true,
  apiUrl: 'https://your-production-api-url/api'
};
```

##### 2. Modify `WorkoutTracker/src/proxy.conf.js`:
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
The Angular application will be available at `http://localhost:4200`.


## API Documentation
The backend API documentation is available via Swagger:

Swagger URL: `https://your-production-api-url/swagger`
Swagger provides endpoints documentation and testing capabilities.

## Internal Pages

### 1. Workout Pages

#### 1. Workouts Page (`/workouts`)
- Lists logged workouts.
![image](https://github.com/user-attachments/assets/154a4d24-6432-4b04-a1f7-08c2fc327bbb)
- Filtered by name: 
![image](https://github.com/user-attachments/assets/d47d0238-d467-4f03-9c70-462bf3197228)

#### 2. Edit Workout Page (`/workout/{id}`)
![image](https://github.com/user-attachments/assets/c27f3a78-cb15-4ff9-a740-2bc0c9639f65)
![image](https://github.com/user-attachments/assets/17074ec1-c920-4683-bd80-7d8cd5523bcb)

#### 3. Create Workout Page (`/workout`)
![image](https://github.com/user-attachments/assets/8dcf589c-f730-42ec-9e6d-96097097a569)

#### 4. Workout Details Page (`/workout/{id}/details`)
General information, exercises and calendar with records.
![image](https://github.com/user-attachments/assets/b34abf6f-f2e5-4ae2-a9f7-9b936f2e4c3f)
![image](https://github.com/user-attachments/assets/82dd6a59-2903-4d6f-a453-12c1ef436c32)

### 2. Progress Page (`/progress`)
Shows your total progress in a table, chart and calendar.
![image](https://github.com/user-attachments/assets/a4d347b4-7a42-4ed8-9e6c-6d1aff9e3e7a)

### 3. Exercise Pages 

#### 1. Exercises Page (`/exercises`)
Lists logged exercises.
- #### All
Your and internal exercises
![image](https://github.com/user-attachments/assets/d26ad6d7-eb24-450b-94fd-dc69130db867)
![image](https://github.com/user-attachments/assets/c01b8834-b066-49e2-91dc-bf61cb26f5d2)

- #### Yours
Your exercises
![image](https://github.com/user-attachments/assets/3ee73150-b5ac-40f1-a6ab-f3aea25f76c0)

- #### Internal
Internal exercises
![image](https://github.com/user-attachments/assets/fc96e3ff-c203-4d01-8609-32c1794f0430)

#### 2. Edit Exercise Page (`/exercise/{id}`)

![image](https://github.com/user-attachments/assets/ea89d2bd-674e-4a3f-82a3-e7e2d8ddeb32)
![image](https://github.com/user-attachments/assets/1c809c4a-29ff-4ff7-a1ce-eda8e82d1231)

#### 3. Create Exercise Pages

#### Yours - `/your-exercise`
#### Internal - `/exercise`
![image](https://github.com/user-attachments/assets/aa8237c9-20f1-4e2c-81db-7deeb602dcf7)
![image](https://github.com/user-attachments/assets/17461e53-8812-46df-91ae-ef11982d73d5)

#### 4. Exercise Details Page (`/exercise/{id}/details`)
General information, workouts, records and calendar with records.

![image](https://github.com/user-attachments/assets/e9765795-dc99-492b-be7a-ec66b69d6691)
![image](https://github.com/user-attachments/assets/91335beb-e48c-410f-9d99-d8962f1eebcd)

### 4. Equipment Pages 

#### 1. Equipments Page (`/equipments`)
Lists logged equipments with edit and create options.

- #### All
Your and internal equipments
![image](https://github.com/user-attachments/assets/368312f6-3a3a-4348-8342-22a6e702e95b)

- #### Yours
Your equipments
![image](https://github.com/user-attachments/assets/5c5c0ed1-617b-422d-8fa4-d4b4c6b37768)

- #### Internal
Internal equipments
![image](https://github.com/user-attachments/assets/b3e8949f-f193-4ce4-8ea0-6762ccad772e)

#### 2. Equipment Details Page (`/equipment/{id}/details`)
Its muscles and exercises.

![image](https://github.com/user-attachments/assets/36d6f628-94c9-4a90-8c87-1f7c446fe834)

### 5. Muscle Pages

#### 1. Muscles Page (`/muscles`)
Lists logged muscles.
![image](https://github.com/user-attachments/assets/7fdb8ce7-a0a1-4835-817d-90f72ed697b3)

#### 2. Edit Muscle Page (`/muscle/{id}`)
![image](https://github.com/user-attachments/assets/11bd5979-09d5-4243-8674-ca7b244424e0)

#### 3. Create Muscle Page (`/muscle`)
![image](https://github.com/user-attachments/assets/07c9e6c6-a3df-4c43-84c0-42baa1854ccf)

#### 4. Muscle Details Page (`/muscle/{id}/details`)
Its parent and child muscles, exercises and muscle sizes.

![image](https://github.com/user-attachments/assets/324e0e09-1579-4de8-a0ed-ca55ce8ad65f)
![image](https://github.com/user-attachments/assets/a08afb9b-b02f-40d0-9f7f-240ff812ac4f)

### 6. Account (`/account`)
Shows your preferable measurements and links to other account related pages.
![image](https://github.com/user-attachments/assets/178f5b39-8985-4e7e-b8ff-84efa626c3e7)

### 7. Profile (`/profile`)
Change your name and/or email.
![image](https://github.com/user-attachments/assets/d9518695-9e01-4029-ae72-a6619d94c4ce)

### 8. Password (`/password`)
Change your current password.
![image](https://github.com/user-attachments/assets/a1ca7c53-ef3b-4181-9ee9-660c598e2d50)

### 9. Personal Data (`/personal-data`)
Change your personal data.
![image](https://github.com/user-attachments/assets/58b96adf-4583-4e60-a2f0-c9c0d96858e0)

### 10. Muscle Size Pages

#### 1. Muscle Sizes Page (`/muscle-sizes`)
Lists logged muscle sizes with charts.
![image](https://github.com/user-attachments/assets/d006a80e-13dc-46ad-94ce-559461d2b6a9)
![image](https://github.com/user-attachments/assets/b483a215-8dd4-48f9-9b2a-f0e8b351c6e3)

#### 2. Edit Muscle Size Page (`/muscle-size/{id}`)
![image](https://github.com/user-attachments/assets/60a765d5-6da7-403e-9c76-2c7b950318f3)

#### 3. Create Muscle Size Page (`/muscle-size`)
![image](https://github.com/user-attachments/assets/41d8eb47-e34b-4e60-b268-2d516d14a388)

### 11. Body Weight Pages

#### 1. Body Weights Page (`/body-weights`)
Lists logged body weights with charts.
![image](https://github.com/user-attachments/assets/6d0f045c-3082-4bec-a364-01860bdf3737)
![image](https://github.com/user-attachments/assets/34624b23-6175-4cc2-b6f3-52b4ff56b6da)

#### 2. Edit Body Weight Page (`/body-weight/{id}`)
![image](https://github.com/user-attachments/assets/524606d6-bed6-42dc-9c0b-0c4a956fa6bb)

#### 3. Create Body Weight Page (`/body-weight`)
![image](https://github.com/user-attachments/assets/22b0b67b-bbbe-4ae4-b75e-34cd041f3ba5)

### 12. Exercise Record Pages

#### 1. Exercise Records Page (`/exercise-records`)
Lists logged exercise records with charts.
![image](https://github.com/user-attachments/assets/8464b0e8-189c-4683-8128-4cb08a73ae6b)
![image](https://github.com/user-attachments/assets/1f15c9de-6ce1-425e-8683-20221444cce0)

#### 2. Edit Exercise Record Page (`/exercise-record/{id}`)
![image](https://github.com/user-attachments/assets/16088c4c-08e2-474d-acea-6d3c0bc39f8b)

### 14. Workout Record Pages

#### 1. Workout Records Page (`/workout-records`)
Lists logged workout records with charts.
![image](https://github.com/user-attachments/assets/19d22e71-5d09-4f07-87e0-f71a3a2d0135)
![image](https://github.com/user-attachments/assets/38e8bc0f-8804-402d-bfef-00e02f81a08a)

#### 2. Edit Workout Record Page (`workouts/{workoutId}/workout-record/{id}`)
![image](https://github.com/user-attachments/assets/33934cfa-27f7-48c4-a065-22e9279a1bd7)
![image](https://github.com/user-attachments/assets/31ace6a4-d71d-4ba0-8115-4861f58fd79c)


#### 3. Create Workout Record Page (`/workout-record` or `workouts/{workoutId}/workout-record`)
![image](https://github.com/user-attachments/assets/1339773a-0607-496f-8acf-3633275768a7)

#### 4. Workout Record Details Page (`/workouts/{workoutId}/workout-record-details/{id}`)
General information and records.
![image](https://github.com/user-attachments/assets/a17c91a8-bae5-4856-bfbd-a27bc4c3f4d4)


### 15. User Pages

#### 1. Users Page (`/users`)
Lists logged users with impersonate option.
![image](https://github.com/user-attachments/assets/5a9dc818-707a-4722-aaa9-d13296de72a3)
![image](https://github.com/user-attachments/assets/c36b889a-f66f-449a-9b6b-e3c7ebb206b5)
![image](https://github.com/user-attachments/assets/be3e6e6b-ad08-4e2a-b26f-4d5e83727934)

#### 2. Edit Users Page (`user/{id}`)
![image](https://github.com/user-attachments/assets/b05258a0-6626-4f2d-82d2-a8fa50f8f5bd)

#### 3. Create Users Page (`/user`)
![image](https://github.com/user-attachments/assets/cf16144e-7f59-46c5-b939-0e8f9b03462c)

### 16. Role Pages

#### 1. Roles Page (`/users`)
Lists logged roles with edit and add options.
![image](https://github.com/user-attachments/assets/f1bbc3e4-99e9-4e4a-a7a0-67070e008a41)

#### 2. Role Details Page (`/role/{id}/details`)
Users with this role
![image](https://github.com/user-attachments/assets/8294f419-5318-4154-9d73-5c6f829138bb)

### 17. Login Page (`/login`)
![image](https://github.com/user-attachments/assets/de028819-3ea9-4ccd-94a9-65462a60801e)

### 18. Register Page (`/register`)
![image](https://github.com/user-attachments/assets/f74c5844-6dc9-4ff3-af4d-263bf4f51f70)
![image](https://github.com/user-attachments/assets/b1eab721-d0b4-49cf-b47f-2cbcf6aaf2ae)
![image](https://github.com/user-attachments/assets/5487cb28-500b-49b1-b14a-050f967b2ebc)

### 19. Logout (`/logout`)
### 20. Not Found
![image](https://github.com/user-attachments/assets/5022339f-9fbb-4882-80aa-f5302e127cc3)


