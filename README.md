# Vehicle Insurance Solution

This repository contains a microservices-based solution for managing vehicle registrations and insurance policies. It consists of two main APIs:

- **VehicleRegistrationAPI**: Manages vehicle and customer data.
- **InsuranceAPI**: Manages insurance policies and interacts with VehicleRegistrationAPI to retrieve vehicle and customer information.
There are 3 types of Insurances:
1. Car Insurance
2. Pet Insurance
3. Health Insurance

If the person has CAR Insurance, while showing the details. The API endpoint should also show the car details. To get the car information, InsuranceAPI calls VehicleRegistrationAPI via REST method using HttpClient in C#

However, the Car details display can be enabled or disabled by a feature toggle called `ShowCarDetails`

**SOME EXTRA FEATURES HAVE BEEN ADDED TO INSURANCE API LIKE `DISCOUNTS` USING FEATURE TOGGLES**

While creating or adding insurance, Disounts can be applied on the insurance prices with a feature toggle called `ApplyDiscounts`

When `ApplyDiscounts : true`, then there will be discounts of `5% on CAR`, `10% on HEALTH` and `15% on PET` applied on the insurances.

---

## Table of Contents
- [Architecture](#architecture)
- [Endpoints](#endpoints)
  - [VehicleRegistrationAPI](#vehicleregistrationapi-endpoints)
  - [InsuranceAPI](#insuranceapi-endpoints)
  - [FeatureManagement](#featuremanagement-endpoints)
- [Inter-Service Communication](#inter-service-communication)
- [Testing](#testing)
- [Docker Compose Setup](#docker-compose-setup)
- [Running Locally](#running-locally)

---

## Architecture
- **.NET 9** microservices
- Minimal APIs with Results pattern (to handle the responses gracefully)
- PostgreSQL databases for each API
- Redis cache for performance
- Intercommunication between two APIs via HttpClient call using `Exponential Retries` and `Circuit Breaker Pattern` using `Polly`
- Docker Compose for local orchestration
- Integration and unit tests for both APIs

---

## Endpoints

### VehicleRegistrationAPI Endpoints
Base URL: `http://localhost:5059/api/v1`

#### Customers
- `GET /customers/{customerId}`: Get customer by ID
- `GET /customers/personal/{personalIdentificationNumber}`: Get customer by personal identification number
- `GET /customers`: Get all customers
- `POST /customers`: Add a new customer

#### Vehicles
- `GET /vehicles/{vehicleId}`: Get vehicle by ID
- `GET /vehicles/registration/{registrationNumber}`: Get vehicle by registration number
- `GET /vehicles/personal/{personalIdentificationNumber}`: Get vehicles by personal identification number
- `GET /vehicles`: Get all vehicles
- `POST /vehicles/add`: Add a new vehicle
- `DELETE /vehicles/{vehicleId}`: Delete vehicle by ID

### InsuranceAPI Endpoints
Base URL: `http://localhost:5096/api/v1`

#### Insurances
- `GET /insurances`: Get all insurances
- `GET /insurances/{insuranceId}`: Get insurance by ID
- `GET /insurances/personal/{personalIdentificationNumber}`: Get insurances by personal identification number
- `POST /insurances`: Add a new insurance

### FeatureManagement Endpoints
Base URL: `http://localhost:5096/api/v1`

#### Feature Toggles
- `GET /featuretoggles`: Get all feature toggles
- `GET /featuretoggles/{id}`: Get feature toggle by ID
- `GET /featuretoggles/{name}`: Get feature toggle by name
- `POST /featuretoggles/by-names`: Get multiple feature toggles by names
. `DELETE /featuretoggles/{id}`: Delete a feature toggle
- `POST /featuretoggles`: Create a new feature toggle
- `PATCH /featuretoggles/{name}`: Update description and enabled status of a feature toggle (partial update)

There are two feature toggles added by default `ApplyDiscounts: false` and `ShowCarDetails: true`

By setting the `ShowCarDetails` value, Car details display can be enabled or disabled in the Insurance GET (display) sections

By setting `ApplyDiscounts` value, Discounts can be enabled or disabled while adding or creating a new insurance.

---

## Feature Management

Feature management in this solution allows dynamic enabling or disabling of specific business features at runtime using feature toggles. This is implemented via a dedicated set of endpoints in the InsuranceAPI.

### Default Feature Toggles
- `ShowCarDetails` (default: `true`):
  - When enabled, car details are included in insurance responses for car insurance policies.
  - When disabled, car details are omitted from the response.
- `ApplyDiscounts` (default: `false`):
  - When enabled, discounts are applied to insurance prices:
    - Car Insurance: 5%
    - Health Insurance: 10%
    - Pet Insurance: 15%
  - When disabled, no discounts are applied.

### FeatureManagement Endpoints
Base URL: `http://localhost:5096/api/v1`

- `GET /featuretoggles`: Get all feature toggles
- `GET /featuretoggles/{id}`: Get feature toggle by ID
- `GET /featuretoggles/{name}`: Get feature toggle by name
- `POST /featuretoggles/by-names`: Get multiple feature toggles by names
- `POST /featuretoggles`: Create a new feature toggle
- `PATCH /featuretoggles/{name}`: Update description and enabled status of a feature toggle (partial update)
- `DELETE /featuretoggles/{id}`: Delete a feature toggle

#### Example: Update a Feature Toggle (PATCH)
```http
PATCH /api/v1/featuretoggles/ShowCarDetails
Content-Type: application/json

{
  "enabled": false,
  "description": "Temporarily hide car details from insurance responses."
}
```

#### Example: Retrieve a Feature Toggle
```http
GET /api/v1/featuretoggles/ApplyDiscounts
```

### Business Impact
- Feature toggles allow business stakeholders to control the rollout of features (such as discounts or car details display) without redeploying the service.
- Changes to toggles take effect immediately and are cached for performance.

### Testing
- **Unit Tests**: Cover repository, service, and validation logic for feature management, including cache interactions and nullability handling.
- **Integration Tests**: Cover all feature management endpoints (CRUD, PATCH), cache scenarios, and ensure correct API versioning. Integration tests are tagged with `[Trait("TestCategory", "Integration")]` for CI discovery.

---

## Inter-Service Communication
- **InsuranceAPI** uses `HttpClient` to call **VehicleRegistrationAPI** for:
  1. Fetching customer details by personal identification number
  2. Fetching vehicle details by registration number
  3. Fetching vehicles by personal identification number
- The base URL for VehicleRegistrationAPI is configured via environment variable or appsettings.

---

## Testing
- **Unit Tests**: Cover core business logic, validation, and repository methods for both APIs.
- **Integration Tests**: Cover API endpoints and inter-service communication, including:
  - InsuranceAPI integration with VehicleRegistrationAPI using a test server
  - FeatureManagement endpoints (feature toggle CRUD, PATCH, and cache scenarios)
  - Database operations with in-memory and real PostgreSQL providers
- Tests are located in `InsuranceAPI.Tests/` and `VehicleRegistrationAPI.Tests/`.

---

## Docker Compose Setup

The solution includes a `docker-compose.yml` file that sets up:
- `vehicleregistrationdb`: PostgreSQL for VehicleRegistrationAPI
- `insurancedb`: PostgreSQL for InsuranceAPI
- `redis`: Redis cache
- `vehicleregistrationapi`: VehicleRegistrationAPI service (port 5059)
- `insuranceapi`: InsuranceAPI service (port 5096)

**Example usage:**
```sh
docker-compose up --build
```

- Both APIs will be available at:
  - VehicleRegistrationAPI: [http://localhost:5059](http://localhost:5059)
  - InsuranceAPI: [http://localhost:5096](http://localhost:5096)
- Databases and Redis are automatically provisioned and networked.
- EF Core migrations and seed data are applied automatically at container startup.

---

## Running Locally
1. Clone the repository with command 
   `git clone https://github.com/Vivekkosare/VehicleInsurance.git`
2. After the repository is cloned, run `cd VehicleInsurance`
3. Ensure Docker Desktop is running
4. Run:
   ```sh
   docker-compose up --build
   ```
5. Once the build is succeeded run:
   ```sh
   docker-compose up
   ```
4. Use the provided Postman collection (`VehicleInsurance.postman_collection.json`) to test endpoints

---

## Running the GitHub Actions Workflow Locally
You can run the CI pipeline locally using [`act`](https://github.com/nektos/act):

1. Install Docker Desktop and ensure it is running.
2. Install `act` ([see instructions](https://github.com/nektos/act#installation)).
3. In the solution root, run:
   ```sh
   act -P ubuntu-latest=catthehacker/ubuntu:act-latest
   ```
   This will execute the workflow in a local Docker container, simulating GitHub Actions.

---

## Postman Collection
- The `VehicleInsurance.postman_collection.json` file contains ready-to-use requests for all endpoints in both APIs.
- Update the URLs if you change the default ports.

---

## Contact
For questions or contributions, please open an issue or submit a pull request.
