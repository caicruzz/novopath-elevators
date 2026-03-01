import { callElevator } from '../services/api';
import './FloorCallPanel.css';

interface Props {
    numberOfFloors: number;
}

export function FloorCallPanel({ numberOfFloors }: Props) {
    const floors = Array.from({ length: numberOfFloors }, (_, i) => numberOfFloors - i);

    const handleCall = async (floor: number) => {
        try {
            await callElevator(floor);
        } catch (err) {
            console.error('Failed to call elevator:', err);
        }
    };

    return (
        <div className="call-panel">
            <div className="panel-header">
                <span className="panel-icon">🏢</span>
                <h3>Floor Panel</h3>
            </div>
            <div className="floor-buttons">
                {floors.map((floor) => (
                    <button
                        key={floor}
                        className="floor-call-btn"
                        onClick={() => handleCall(floor)}
                        title={`Call elevator to floor ${floor}`}
                    >
                        <span className="btn-floor-num">{floor}</span>
                        <span className="btn-label">CALL</span>
                    </button>
                ))}
            </div>
        </div>
    );
}
