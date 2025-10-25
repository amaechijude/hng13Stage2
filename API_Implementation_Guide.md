# C# RESTful API: Country & Currency Data Service

## 1. High-Level Objective

Build a RESTful API using C# and ASP.NET Core that fetches country and currency exchange data from external sources, processes and combines this data, caches it in a database, and exposes it through a set of CRUD and utility endpoints.

## 2. Core Functionalities

- **Data Aggregation**: Fetch data from two separate external APIs.
- **Data Processing**: For each country, calculate an estimated GDP.
- **Database Caching**: Store the processed data in a MySQL database to act as a cache.
- **API Endpoints**: Provide endpoints for data refresh, retrieval (all, single, filtered), deletion, and status checks.
- **Image Generation**: Create and serve a summary image with key statistics.
- **Error Handling**: Implement robust error handling for both internal and external service failures.

## 3. Data Sources (External APIs)

- **Countries API**: `https://restcountries.com/v2/all?fields=name,capital,region,population,flag,currencies`
- **Exchange Rates API**: `https://open.er-api.com/v6/latest/USD`

## 4. Database Schema

Use a database (MySQL specified) for persistence. You will need a table to store country information.

### `Countries` Table

| Column             | Type          | Constraints / Notes                                            |
| ------------------ | ------------- | -------------------------------------------------------------- |
| `id`               | `INT`         | Auto-incrementing Primary Key.                                 |
| `name`             | `VARCHAR(255)`| Required, Unique.                                              |
| `capital`          | `VARCHAR(255)`| Optional.                                                      |
| `region`           | `VARCHAR(255)`| Optional.                                                      |
| `population`       | `BIGINT`      | Required.                                                      |
| `currency_code`    | `VARCHAR(10)` | Optional (`NULL` if not available).                            |
| `exchange_rate`    | `DECIMAL(18,6)`| Optional (`NULL` if not available).                            |
| `estimated_gdp`    | `DECIMAL(20,2)`| Computed. Set to `0` if `exchange_rate` is `NULL`.             |
| `flag_url`         | `VARCHAR(255)`| Optional.                                                      |
| `last_refreshed_at`| `DATETIME`    | Auto-updated timestamp on create/update.                       |

---

## 5. API Endpoint Specifications

### 5.1. Refresh Data

- **Endpoint**: `POST /countries/refresh`
- **Description**: Triggers a full refresh of the database cache. Fetches data from external APIs, processes it, and updates the `Countries` table. After a successful refresh, it generates a summary image.
- **Success Response**: `200 OK`
  ```json
  {
    "status": "success",
    "message": "Data refreshed successfully.",
    "countries_processed": 250,
    "refreshed_at": "2025-10-22T18:00:00Z"
  }
  ```
- **Error Response**: `503 Service Unavailable` (if an external API fails)
  ```json
  {
    "error": "External data source unavailable",
    "details": "Could not fetch data from [API name]"
  }
  ```

### 5.2. Get All Countries

- **Endpoint**: `GET /countries`
- **Description**: Retrieves a list of all countries from the database. Supports filtering and sorting.
- **Query Parameters**:
  - `region` (e.g., `?region=Africa`): Filter by country region (case-insensitive).
  - `currency` (e.g., `?currency=NGN`): Filter by currency code (case-insensitive).
  - `sort` (e.g., `?sort=gdp_desc`): Sort by `estimated_gdp`. Accepts `gdp_asc` or `gdp_desc`.
- **Success Response**: `200 OK`
  ```json
  [
    {
      "id": 1,
      "name": "Nigeria",
      "capital": "Abuja",
      "region": "Africa",
      "population": 206139589,
      "currency_code": "NGN",
      "exchange_rate": 1600.23,
      "estimated_gdp": 25767448125.2,
      "flag_url": "https://flagcdn.com/ng.svg",
      "last_refreshed_at": "2025-10-22T18:00:00Z"
    }
  ]
  ```

### 5.3. Get a Single Country

- **Endpoint**: `GET /countries/{name}`
- **Description**: Retrieves a single country by its name (case-insensitive).
- **Success Response**: `200 OK` (with a single country object as in the `GET /countries` response).
- **Error Response**: `404 Not Found`
  ```json
  { "error": "Country not found" }
  ```

### 5.4. Delete a Country

- **Endpoint**: `DELETE /countries/{name}`
- **Description**: Deletes a country record from the database by its name (case-insensitive).
- **Success Response**: `204 No Content`
- **Error Response**: `404 Not Found`
  ```json
  { "error": "Country not found" }
  ```

### 5.5. Get API Status

- **Endpoint**: `GET /status`
- **Description**: Shows the total number of countries in the database and the timestamp of the last successful refresh.
- **Success Response**: `200 OK`
  ```json
  {
    "total_countries": 250,
    "last_refreshed_at": "2025-10-22T18:00:00Z"
  }
  ```

### 5.6. Get Summary Image

- **Endpoint**: `GET /countries/image`
- **Description**: Serves the summary image file generated during the last refresh.
- **Success Response**: `200 OK` with `Content-Type: image/png`.
- **Error Response**: `404 Not Found`
  ```json
  { "error": "Summary image not found" }
  ```

---

## 6. Detailed Business Logic & Rules

### 6.1. Data Refresh Logic (`POST /countries/refresh`)

1.  **Fetch Data**: Concurrently fetch data from the Countries and Exchange Rate APIs. If either fails, abort and return `503`.
2.  **Iterate Countries**: For each country from the countries API:
    - **Match Existing**: Find a country in the database with the same name (case-insensitive).
    - **Currency Handling**:
        - If the `currencies` array is not empty, use the **first** currency's `code`.
        - If the `currencies` array is empty or missing, set `currency_code` and `exchange_rate` to `NULL`.
    - **Exchange Rate**:
        - If a `currency_code` exists, find its rate in the exchange rate data.
        - If the code is not found in the rates, set `exchange_rate` to `NULL`.
    - **GDP Calculation**:
        - `estimated_gdp = population * random(1000–2000) / exchange_rate`.
        - The random multiplier must be generated fresh for each country on every refresh.
        - If `exchange_rate` is `NULL` or zero, set `estimated_gdp` to `0`.
    - **Database Operation**:
        - If a match was found, **UPDATE** the existing record with all new data.
        - If no match was found, **INSERT** a new record.
3.  **Timestamp**: The `last_refreshed_at` field for each record and the global status should be updated to the current time.
4.  **Image Generation**: After all database operations are complete, generate the summary image.

### 6.2. Image Generation (`cache/summary.png`)

- **Trigger**: This runs at the end of a successful `POST /countries/refresh`.
- **Content**: The image should contain:
    1.  Total number of countries in the DB.
    2.  A list of the "Top 5 Countries by Estimated GDP".
    3.  The timestamp of the refresh.
- **Storage**: Save the generated image to the filesystem at `cache/summary.png`. The `cache` directory should be created if it doesn't exist.

### 6.3. Validation

- When creating/updating records, `name` and `population` are required.
- If validation fails (e.g., on a potential future `POST /countries` endpoint), return `400 Bad Request`.
  ```json
  {
    "error": "Validation failed",
    "details": { "name": "is required" }
  }
  ```

## 7. Technical Requirements

- **Framework**: C# / ASP.NET Core
- **Configuration**: Use a mechanism like `.env` or `appsettings.json` for database connection strings and other configurations.
- **Dependencies**: List all NuGet packages in the project file.
- **README**: Provide a `README.md` with clear setup and run instructions.