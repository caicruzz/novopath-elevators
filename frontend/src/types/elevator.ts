// ── Elevator Types (mirrors backend DTOs) ──

export type ElevatorState = 'Idle' | 'MovingUp' | 'MovingDown' | 'DoorsOpen' | 'Maintenance';

export interface ElevatorDto {
    id: number;
    currentFloor: number;
    targetFloor: number;
    state: ElevatorState;
}

export interface BuildingStateDto {
    numberOfFloors: number;
    elevators: ElevatorDto[];
}

export interface ElevatorCallRequest {
    floor: number;
}
