# High-Rise Elevator Simulator: Product Requirements & Architecture

## 1. Objective
An architect firm is designing a high-rise building and requires a simulation tool to design and implement an elevator system. The core requirement is that the resulting system must be **safe and compliant**.

This project delivers a configurable simulation engine that tests various building parameters and outputs a comprehensive "Simulation Report" to aid the architects in their design decisions, focusing on "did we build the right product" and "did we build it right".

## 2. Scope & Trade-offs (Architectural Disclaimer)
* **Out of Scope (Authentication & Authorization):** Identity management (OAuth, JWT, user roles) has been intentionally omitted from this submission. Because Identity is a well-solved domain with standard enterprise patterns, 100% of the development timebox was dedicated to the complex, domain-specific challenges of this prompt: the asynchronous dispatch algorithms, state management, and compliance rules engine.
* **Out of Scope (Visuals):** 3D visual rendering of the building and complex physics calculations (e.g., cable tension, acceleration curves) are omitted to focus on system logic and safety compliance.

## 3. Tech Stack & Architecture
As technologists focused on using the best tool for the solution, this system is designed with a decoupled client-server architecture. This choice of tools ensures scalability, testability, and a clear separation of concerns.

* **Backend:** .NET 10 Minimal API (C# 14). Chosen for state-of-the-art performance, utilizing the latest LTS release to ensure long-term maintainability. Its robust object-oriented capabilities are ideal for handling the complex state of the simulation engine.
* **Frontend:** React (TypeScript). A lightweight SPA (Single Page Application) responsible strictly for capturing architect configurations and rendering the final report.
* **Testing:** xUnit and Moq to ensure a rigorous software development lifecycle and verify that the final product closely meets requirements.


## 4. System Logic & Domain Design
To demonstrate domain mastery and the ability to adapt to changing scenarios, the core engine utilizes several structural design patterns:

* **The Dispatch Engine (Strategy Pattern):** The logic determining which elevator answers a call is abstracted into an `IElevatorDispatchStrategy`. This allows the system to easily swap between algorithms (e.g., *Nearest Car* vs. *Least Loaded Car*).
* **State Management:** Elevators are governed by a strict State Machine (`Idle`, `MovingUp`, `MovingDown`, `DoorsOpen`, `Maintenance`).
* **Safety Rules Engine:** The system calculates the aggregate weight of generated passengers. If a car exceeds the configured weight limit, it enters a `DoorsOpen` state and refuses to move until passengers disembark, logging a compliance violation.
* **Emergency Override:** Implementation of an `EmergencyFireMode` that instantly forces all cars to the ground floor and opens doors, rejecting all passenger calls.


## 5. Execution Phases (Vertical Slices)
The project was built in vertical full-stack slices to mitigate risks of unknowns and ensure a working, testable product at every stage of the lifecycle.

### Phase 1: The Foundation (State & Movement)
* **Backend:** Scaffolding of the .NET 10 Minimal API. Creation of an in-memory singleton to hold the `BuildingState`. Basic endpoint (`POST /api/elevator/call`) to update target/current floors.
* **Frontend:** React SPA scaffolding. A visual grid representing the elevator shaft with polling to visually update elevator positions.

### Phase 2: The Payload (Passengers & Dispatch)
* **Backend:** Implementation of the `IElevatorDispatchStrategy`. Introduction of multiple elevators and a `Passenger` record (destination floor, weight).
* **Frontend:** Addition of the Configuration Form (floors, cars, weight limits) mapped to the React UI, allowing architects to adapt the simulation dynamically without altering files.

### Phase 3: The Edge Cases (Safety & Compliance)
* **Backend:** Implementation of the rules engine (Weight limits -> `CapacityReached` state) and the `EmergencyFireMode` override.
* **Frontend:** UI integration for warning states (e.g., elevator turns red on weight limit) and an Emergency trigger button.

### Phase 4: The Deliverable (Metrics & The Report)
* **Backend:** Aggregation of simulation data: Average Wait Time, Max Wait Time, Total Trips, and a ledger of Compliance/Safety Events.
* **Frontend:** The final "Simulation Report" view. When the simulation finishes, the React app renders tables and metrics summarizing the run to fulfill the architect's business requirements.

## Quick Start

### Docker (Recommended)

```bash
docker compose up --build
```

Open **http://localhost:3000** — that's it. The frontend serves the React SPA and proxies all `/api` requests to the backend automatically.

### Manual Setup

**Backend** (.NET 10 SDK required):
```bash
cd backend && dotnet run
```

**Frontend** (Node 24+ required):
```bash
cd frontend && npm install && npm run dev
```

Open **http://localhost:5173** (frontend dev server connects to backend on port 5014).

## Future Scalability Enhancements

To transition this application from a robust single-node architecture to a highly scalable, cloud-native enterprise system, two primary enhancements would be introduced. First, replacing the in-memory collections for completed trips and compliance logs with a persistent relational database (e.g., PostgreSQL via Entity Framework Core) would prevent unbounded memory growth and allow for complex historical reporting without impacting application performance. Second, the architecture would decouple the Web API from the background simulation engine using a message broker (such as RabbitMQ or Azure Service Bus). By having the API publish asynchronous events (e.g., ElevatorRequestedEvent) for a separate worker process to consume, the core simulation engine is protected from sudden API traffic spikes, ensuring deterministic processing and true horizontal scalability.

---
*Built to reflect a disciplined ability to innovate and excel.*

