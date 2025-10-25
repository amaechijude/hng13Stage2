# HNG13 Stage 2 API Project

This is a .NET Web API project that provides endpoints for managing country information and exchange rates.

## Prerequisites

Before running this project, make sure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (version 12 or higher)
- Git (for version control)

## Dependencies

The project uses the following main packages:

- Microsoft.AspNetCore.OpenApi
- Microsoft.EntityFrameworkCore
- Npgsql.EntityFrameworkCore.PostgreSQL
- SixLabors.ImageSharp
- DotNetEnv

All dependencies are managed through NuGet and will be restored automatically when building the project.

## Setup Instructions

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd hng13Stage2
   ```

2. Update the database connection string:
   - Open `appsettings.json`
   - Modify the `DefaultConnection` string to match your PostgreSQL setup:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=hng13stage2;User=postgres;Password=postgres;Port=5432"
     }
     ```

3. Apply database migrations:
   ```bash
   cd hng13Stage2
   dotnet ef database update
   ```

4. Build and run the project:
   ```bash
   dotnet build
   dotnet run
   ```

The API will be available at `https://localhost:5001` and `http://localhost:5000` by default.

## Project Structure

- `Controllers/` - Contains API endpoint controllers
- `Data/` - Database context and configuration
- `DTOs/` - Data transfer objects
- `Entities/` - Database entity models
- `Services/` - Business logic and external service integrations
- `Migrations/` - Database migration files

## Environment Variables

The project uses both `appsettings.json` and `.env` file for configuration. To set up your environment variables:

1. Create a `.env` file in the root directory of the project
2. Add your configuration values in the following format:

```env
# Database Configuration
CONNECTION_STRING=Server=localhost;Database=hng13stage2;User=postgres;Password=postgres;Port=5432
```

The application will automatically load these environment variables on startup using the DotNetEnv package.

- Using .env file (recommended for development)
- Using system environment variables (recommended for production)

## API Documentation

The API documentation is available at `/scalar/v1` when running the application in development mode.

### Available Endpoints

- `GET /countries` - Get all countries
- `GET /countries/?region={region}&sort={sort}` - Get countries filtered by region and sorted
  - Example: `/countries/?region=Africa&sort=gdp_desc`
- `GET /countries/{countryName}` - Get details for a specific country
- `GET /countries/image` - Get country information as an image
- `POST /countries/refresh` - Refresh country data from external sources
- `DELETE /countries/{countryName}` - Delete a country entry
- `GET /status` - Get API status

### External API Integration
The application integrates with:
- REST Countries API (`https://restcountries.com/v2/all`) - For country information
- Exchange Rate API (`https://open.er-api.com/v6/latest/USD`) - For currency exchange rates

## License