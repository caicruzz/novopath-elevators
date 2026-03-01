import { useState } from 'react';
import { callElevator } from '../services/api';
import './FloorCallPanel.css';

interface Props {
    numberOfFloors: number;
}

export function FloorCallPanel({ numberOfFloors }: Props) {
    const floors = Array.from({ length: numberOfFloors }, (_, i) => numberOfFloors - i);
    const [pickupFloor, setPickupFloor] = useState<number | null>(null);
    const [destinationFloor, setDestinationFloor] = useState<number | null>(null);
    const [weightKg, setWeightKg] = useState<number>(70);

    const handleFloorClick = (floor: number) => {
        if (pickupFloor === null) {
            setPickupFloor(floor);
            setDestinationFloor(null);
        } else if (floor === pickupFloor) {
            // Clicking pickup again deselects
            setPickupFloor(null);
            setDestinationFloor(null);
        } else {
            setDestinationFloor(floor);
        }
    };

    const handleCall = async () => {
        if (pickupFloor === null || destinationFloor === null) return;
        try {
            await callElevator(pickupFloor, destinationFloor, weightKg);
            setPickupFloor(null);
            setDestinationFloor(null);
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
            <div className="call-config">
                <label className="call-field">
                    <span className="call-label">Passenger Weight (kg)</span>
                    <input
                        type="number"
                        min={1}
                        max={500}
                        value={weightKg}
                        onChange={e => setWeightKg(Number(e.target.value))}
                    />
                </label>
                <p className="call-instruction">
                    {pickupFloor === null
                        ? 'Select pickup floor'
                        : destinationFloor === null
                          ? `Select destination for pickup at F${pickupFloor}`
                          : `F${pickupFloor} → F${destinationFloor}`}
                </p>
            </div>
            <div className="floor-buttons">
                {floors.map((floor) => {
                    const isPickup = floor === pickupFloor;
                    const isDestination = floor === destinationFloor;
                    return (
                        <button
                            key={floor}
                            className={`floor-call-btn${isPickup ? ' selected-pickup' : ''}${isDestination ? ' selected-dest' : ''}`}
                            onClick={() => handleFloorClick(floor)}
                            title={`Floor ${floor}`}
                        >
                            <span className="btn-floor-num">{floor}</span>
                            <span className="btn-label">
                                {isPickup ? 'FROM' : isDestination ? 'TO' : 'FLOOR'}
                            </span>
                        </button>
                    );
                })}
            </div>
            <button
                className="call-submit-btn"
                disabled={pickupFloor === null || destinationFloor === null}
                onClick={handleCall}
            >
                Call Elevator
            </button>
        </div>
    );
}
