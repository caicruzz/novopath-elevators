import { useState } from 'react';
import { activateEmergency, deactivateEmergency } from '../services/api';
import './EmergencyPanel.css';

interface Props {
    isEmergencyMode: boolean;
}

export function EmergencyPanel({ isEmergencyMode }: Props) {
    const [loading, setLoading] = useState(false);

    const handleToggle = async () => {
        setLoading(true);
        try {
            if (isEmergencyMode) {
                await deactivateEmergency();
            } else {
                await activateEmergency();
            }
        } catch (err) {
            console.error('Emergency toggle failed:', err);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className={`emergency-panel${isEmergencyMode ? ' active' : ''}`}>
            <div className="emergency-header">
                <span className="emergency-icon">{isEmergencyMode ? '🔥' : '🛡️'}</span>
                <h3>Emergency Control</h3>
            </div>
            <p className="emergency-status">
                {isEmergencyMode
                    ? 'FIRE MODE ACTIVE — All elevators returning to ground floor'
                    : 'System operating normally'}
            </p>
            <button
                className={`emergency-btn${isEmergencyMode ? ' deactivate' : ' activate'}`}
                onClick={handleToggle}
                disabled={loading}
            >
                {loading
                    ? 'Processing…'
                    : isEmergencyMode
                        ? '✓ DEACTIVATE FIRE MODE'
                        : '🔥 ACTIVATE FIRE MODE'}
            </button>
        </div>
    );
}
