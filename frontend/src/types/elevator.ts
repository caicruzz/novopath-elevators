// ── Elevator Types (mirrors backend DTOs) ──

export type ElevatorState = 'Idle' | 'MovingUp' | 'MovingDown' | 'DoorsOpen' | 'Maintenance' | 'CapacityExceeded' | 'EmergencyStop';

export interface ElevatorDto {
    id: number;
    currentFloor: number;
    targetFloor: number;
    state: ElevatorState;
    currentWeightKg: number;
    weightLimitKg: number;
    passengerCount: number;
}

export interface ComplianceEvent {
    timestamp: string;
    elevatorId: number;
    eventType: string;
    description: string;
}

export interface BuildingStateDto {
    numberOfFloors: number;
    weightLimitKg: number;
    elevators: ElevatorDto[];
    isEmergencyMode: boolean;
    complianceLog: ComplianceEvent[];
}

export interface ElevatorCallRequest {
    floor: number;
    destinationFloor: number;
    weightKg: number;
}

export interface ConfigureRequest {
    numberOfFloors: number;
    numberOfElevators: number;
    weightLimitKg: number;
}

// ── Report Types ──

export interface ElevatorStatsDto {
    elevatorId: number;
    tripsCompleted: number;
    floorsTraversed: number;
    avgWaitTimeSeconds: number;
    maxWaitTimeSeconds: number;
}

export interface SimulationReportDto {
    totalTrips: number;
    totalPassengersServed: number;
    avgWaitTimeSeconds: number;
    maxWaitTimeSeconds: number;
    avgTravelTimeSeconds: number;
    totalFloorsTraversed: number;
    complianceEventCount: number;
    elevatorStats: ElevatorStatsDto[];
    complianceLog: ComplianceEvent[];
    generatedAt: string;
}
