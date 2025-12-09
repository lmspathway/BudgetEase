## BudgetEase

**BudgetEase** is a simple and intuitive personal finance app, focused on busy people who need to track their spending quickly, without unnecessary complexity.

### App goal

- **Track expenses and income** quickly.
- **Organize transactions into categories**.
- **Provide a clear view of the monthly budget**.
- **Show simple charts** to understand where money is going.

### Target audience

- **Students**
- **Young professionals**
- **People with a tight budget**
- **Families who want to control their spending**

### Project architecture

- **`Program.cs`**: application startup and HTTP pipeline configuration.
- **`Components/App.razor`**: main HTML shell (host) for the app.
- **`Components/Pages`**: interactive pages (Home, Dashboard, etc.).
- **`Components/Layout`**: main layout (`MainLayout`) and navigation (`NavMenu`).
- **`Models`**: domain models (transactions, categories, budgets).
- **`Data`**: data access (EF Core, `DbContext`, migrations).
- **`Services`**: business logic and use cases (e.g., transaction service).
- **`Shared`**: shared types and utilities across layers.
- **`wwwroot`**: static assets (CSS, JS, images, fonts).

### Data model (Phase 2)

- **User**
  - Identity-compatible shape: `Id`, `Email`, `DisplayName`, navigation to `UserSettings`, `Transactions`, `Categories`.
- **Transaction**
  - `Id`, `UserId`, `Amount`, `Date`, `Type` (income/expense), `CategoryId` (optional), `Description`, `CreatedAt`, `UpdatedAt`.
- **Category**
  - `Id`, `UserId` (nullable), `Name`, `ColorHex` (optional), `IsDefaultGlobal` (marks global default categories).
- **UserSettings**
  - `Id`, `UserId`, `Currency`, `Theme`, `TimeZone`.

### Authentication & authorization (Phase 3)

- **Identity**
  - Uses ASP.NET Core Identity with a custom `ApplicationUser` (GUID key, email, display name, password hash, etc.).
  - Identity data and app data share the same `BudgetEaseDbContext`.
- **Auth flows**
  - **Register**: `/register` – collects display name, email and password, creates an `ApplicationUser`, signs the user in.
  - **Login**: `/login` – email + password + “remember me”; friendly error messages for invalid credentials or lockout.
  - **Logout**: `/logout` – signs out and redirects to `/login`.
  - **Forgot password**: `/forgot-password` – UI is present; email-based reset will be wired in a later phase.
- **Route protection**
  - All Blazor routes are wrapped in `AuthorizeRouteView`, showing a “Sign in required” message when unauthenticated.
  - Auth pages (`/login`, `/register`, `/forgot-password`) are explicitly marked as `[AllowAnonymous]`.

### How to run in development

1. **Install .NET 9 SDK** (or the version configured in `BudgetEase.csproj`).
2. In the project directory, run:

   ```bash
   dotnet run
   ```

3. Open the URL shown in the console (by default, something like `https://localhost:5xxx`).

### Version control

- **`.gitignore`** is configured for .NET/Blazor projects (ignores `bin/`, `obj/`, IDE files, logs, local databases and `.env`).
- Recommended first commit: initial template state + folder organization + README + `.gitignore`.

### Next steps (high level)

- Model the main entities (Transaction, Category, MonthlyBudget).
- Configure EF Core with a local database (SQLite or another provider).
- Add a charting library for the financial dashboard.
- Build a responsive layout with fixed sidebar and header, optimized for desktop and mobile.

### Planned dependencies (to be installed later)

- **Data access (Entity Framework Core)**
  - `Microsoft.EntityFrameworkCore.Sqlite` – primary provider for local development.
  - `Microsoft.EntityFrameworkCore.Design` – tooling support for migrations.
  - (Optional) `Microsoft.EntityFrameworkCore.SqlServer` – if you later move to SQL Server in production.

- **Charting**
  - `ChartJs.Blazor.Fork` – Blazor wrapper around Chart.js, good fit for dashboard charts (spending per category, monthly trends, etc.).

- **Forms and validation**
  - Built‑in **Data Annotations** (`[Required]`, `[Range]`, etc.) for basic validation.
  - `FluentValidation` + `FluentValidation.DependencyInjectionExtensions` for richer, testable validation rules.
  - (Optional) a Blazor‑specific helper such as `Blazored.FluentValidation` to integrate FluentValidation into forms.

> Note: these dependencies are **only planned** at this stage and will be added to `BudgetEase.csproj` in a later phase.

### Databases

- **Development**
  - Uses **SQLite** via the `DefaultConnection` defined in `appsettings.Development.json` (file-based DB like `budgetease-dev.db`).
- **Production**
  - Intended for **SQL Server / Azure SQL**, using the `DefaultConnection` from `appsettings.json` or environment variables.

### Migrations (EF Core) – plan

Once EF Core packages are added to `BudgetEase.csproj`, the workflow will be:

- **Install EF Core CLI tooling (once per machine)**

  ```bash
  dotnet tool install --global dotnet-ef
  ```

- **Add the first migration**

  ```bash
  dotnet ef migrations add InitialCreate
  ```

- **Apply migrations to the development database**

  ```bash
  dotnet ef database update
  ```

- **Production updates**
  - Use the same migration files generated in the project.
  - Apply migrations during deployment (CI/CD) using `dotnet ef database update` pointing to the production connection string, or run the generated SQL script in a controlled release process.

### Data access policies

- All **CRUD operations must be scoped to the current user**:
  - Always filter by `UserId` (from the authenticated `ApplicationUser`) when querying `Transactions`, `Categories` and `UserSettings`.
  - Never trust a `UserId` coming from the client; instead, derive it from the authenticated principal.
- On the server side:
  - Services and repositories should accept a `userId` parameter (or a user context service) and apply it in `WHERE` clauses.
  - APIs or interactive components that modify data should require authentication and validate ownership before applying changes.

### Password reset – planned UX

- **Trigger**: link on the login screen (“Forgot your password?”) sends the user to `/forgot-password`.
- **Flow (to be implemented)**:
  - User submits email.
  - If the account exists, the system generates a secure token with Identity and sends an email containing a time-limited reset link.
  - The reset page lets the user set a new password without revealing whether the email exists (for privacy).
- For now, the **UI is present but disabled**; wiring email sending and reset token handling will happen in a later phase.




