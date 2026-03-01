// ── Elevator Types (mirrors backend DTOs) ──

export type ElevatorState = 'Idle' | 'MovingUp' | 'MovingDown' | 'DoorsOpen' | 'Maintenance';

export interface ElevatorDto {
    id: number;
    currentFloor: number;
    targetFloor: number;
    state: ElevatorState;
    currentWeightKg: number;
    weightLimitKg: number;
}

export interface BuildingStateDto {
    numberOfFloors: number;
    weightLimitKg: number;
    elevators: ElevatorDto[];
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
