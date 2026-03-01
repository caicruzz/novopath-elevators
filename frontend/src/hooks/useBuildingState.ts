import { useState, useEffect, useCallback } from 'react';
import type { BuildingStateDto } from '../types/elevator';
import { fetchBuildingState } from '../services/api';

const POLL_INTERVAL_MS = 400;

export function useBuildingState() {
    const [buildingState, setBuildingState] = useState<BuildingStateDto | null>(null);
    const [error, setError] = useState<string | null>(null);

    const poll = useCallback(async () => {
        try {
            const state = await fetchBuildingState();
            setBuildingState(state);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Connection lost');
        }
    }, []);

    useEffect(() => {
        poll(); // initial fetch
        const interval = setInterval(poll, POLL_INTERVAL_MS);
        return () => clearInterval(interval);
    }, [poll]);

    return { buildingState, error };
}
