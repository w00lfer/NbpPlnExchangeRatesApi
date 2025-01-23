# NbpPlnExchangeRatesApi

## About The Project

This is a simple WebAPI that gathers exchange rates from the public API of **NBP**. The fetched exchange rates are saved in the database to minimize unnecessary requests to the external server. If a requested exchange rate (with a specific date and currency) is already stored in the database, the API skips the call to the NBP API.

The API includes a pre-filled list of currency ISO codes (**ISO 4217**) to validate incoming requests effectively.

---

### Built With

This project is built using the following technologies and frameworks:

- **[.NET 9]** - Core framework for building the API
- **[EntityFrameworkCore]** - As ORM
- **[MediatR]** - For implementing the CQRS pattern
- **[FluentValidation]** - To validate incoming requests
- **[Microsoft.Extensions.Http.Resilience]** - For resilient and error-prone communication between the API and the NBP API
- **[NodaTime]** - Enhanced support for working with dates and times
- **[Scalar]** - For creating styled and interactive API documentation
- **[TestContainers]** - Simplifies and speeds up integration testing
- **[Clean Architecture]** - Ensures the project is well-structured, facilitating future feature implementation
- **Result Pattern** - While not a design pattern, this helps developers handle potential errors explicitly in methods

---

## Installation & Usage

### Using Docker and Docker Compose

1. Ensure Docker and Docker Compose are installed on your system.
2. In the project directory, run the following command:

```bash
docker compose up
```

3. Head into and start sending requests!

```
http://localhost:5000/scalar/v1
```

### Without Docker

1. Update the connection string:

- Open the file `.\src\NbpPlnExchangeRates.API\appSettings.Development.json`.

- Update the key with your database's connection string.

```bash
DatabaseConnectionString__DatabaseConnectionString
```

2. Run the application:

```bash
dotnet run --project .\src\NbpPlnExchangeRates.API\NbpPlnExchangeRates.API.csproj
```

Open your browser and navigate to this site to start sending requests!.

```bash
https://localhost:7115/scalar/v1
```
