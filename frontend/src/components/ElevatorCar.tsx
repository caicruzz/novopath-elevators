import type { ElevatorDto } from '../types/elevator';
import './ElevatorCar.css';

interface Props {
    elevator: ElevatorDto;
    numberOfFloors: number;
}

const stateIcons: Record<string, string> = {
    Idle: '⏸',
    MovingUp: '▲',
    MovingDown: '▼',
    DoorsOpen: '◉',
    Maintenance: '⚠',
    CapacityExceeded: '⛔',
    EmergencyStop: '🔥',
};

export function ElevatorCar({ elevator, numberOfFloors }: Props) {
    const bottomPercent = ((elevator.currentFloor - 1) / (numberOfFloors - 1)) * 100;
    const stateClass = elevator.state.toLowerCase().replace(/([a-z])([A-Z])/g, '$1-$2').toLowerCase();
    const weightPercent = elevator.weightLimitKg > 0
        ? Math.min((elevator.currentWeightKg / elevator.weightLimitKg) * 100, 100)
        : 0;
    const weightLevel = weightPercent > 100 ? 'danger' : weightPercent > 80 ? 'warning' : 'normal';

    return (
        <div className="elevator-shaft">
            <div className="shaft-label">Car {elevator.id}</div>
            <div className="shaft-track">
                {/* Floor markers */}
                {Array.from({ length: numberOfFloors }, (_, i) => {
                    const floor = numberOfFloors - i;
                    return (
                        <div
                            key={floor}
                            className="floor-marker"
                            style={{ bottom: `${((floor - 1) / (numberOfFloors - 1)) * 100}%` }}
                        >
                            <span className="floor-number">{floor}</span>
                        </div>
                    );
                })}

                {/* Elevator car */}
                <div
                    className={`elevator-car state-${stateClass}`}
                    style={{ bottom: `${bottomPercent}%` }}
                >
                    <span className="car-icon">{stateIcons[elevator.state] ?? '?'}</span>
                    <span className="car-floor">{elevator.currentFloor}</span>
                </div>
            </div>

            {/* Weight capacity bar */}
            <div className="weight-bar-container">
                <div className={`weight-bar weight-${weightLevel}`} style={{ width: `${weightPercent}%` }} />
            </div>
            <div className="shaft-info">
                <span className="passenger-count">👤 {elevator.passengerCount}</span>
                <span className="weight-label">{Math.round(elevator.currentWeightKg)}kg</span>
            </div>

            <div className={`shaft-status state-${stateClass}`}>
                {elevator.state === 'CapacityExceeded' ? 'OVERWEIGHT' : elevator.state}
                {elevator.currentFloor !== elevator.targetFloor && (
                    <span className="target-indicator"> → F{elevator.targetFloor}</span>
                )}
            </div>
        </div>
    );
}
