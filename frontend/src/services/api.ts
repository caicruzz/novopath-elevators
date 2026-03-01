import type { BuildingStateDto, ElevatorDto, ConfigureRequest } from '../types/elevator';

const API_BASE = 'http://localhost:5014';

export async function fetchBuildingState(): Promise<BuildingStateDto> {
    const res = await fetch(`${API_BASE}/api/building/state`);
    if (!res.ok) throw new Error(`Failed to fetch building state: ${res.statusText}`);
    return res.json();
}

export async function callElevator(
    floor: number,
    destinationFloor: number,
    weightKg: number
): Promise<ElevatorDto> {
    const res = await fetch(`${API_BASE}/api/elevator/call`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ floor, destinationFloor, weightKg }),
    });
    if (!res.ok) throw new Error(`Failed to call elevator: ${res.statusText}`);
    return res.json();
}

export async function configureBuilding(req: ConfigureRequest): Promise<BuildingStateDto> {
    const res = await fetch(`${API_BASE}/api/building/configure`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(req),
    });
    if (!res.ok) throw new Error(`Failed to configure building: ${res.statusText}`);
    return res.json();
}
