import type { BuildingStateDto, ElevatorDto } from '../types/elevator';

const API_BASE = 'http://localhost:5014';

export async function fetchBuildingState(): Promise<BuildingStateDto> {
    const res = await fetch(`${API_BASE}/api/building/state`);
    if (!res.ok) throw new Error(`Failed to fetch building state: ${res.statusText}`);
    return res.json();
}

export async function callElevator(floor: number): Promise<ElevatorDto> {
    const res = await fetch(`${API_BASE}/api/elevator/call`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ floor }),
    });
    if (!res.ok) throw new Error(`Failed to call elevator: ${res.statusText}`);
    return res.json();
}
