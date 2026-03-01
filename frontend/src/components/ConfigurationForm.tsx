import { useState } from 'react';
import { configureBuilding } from '../services/api';
import type { ConfigureRequest } from '../types/elevator';
import './ConfigurationForm.css';

interface Props {
    currentConfig: {
        numberOfFloors: number;
        numberOfElevators: number;
        weightLimitKg: number;
    };
}

export function ConfigurationForm({ currentConfig }: Props) {
    const [floors, setFloors] = useState(currentConfig.numberOfFloors);
    const [cars, setCars] = useState(currentConfig.numberOfElevators);
    const [weightLimit, setWeightLimit] = useState(currentConfig.weightLimitKg);
    const [submitting, setSubmitting] = useState(false);
    const [feedback, setFeedback] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        setFeedback(null);
        try {
            const req: ConfigureRequest = {
                numberOfFloors: floors,
                numberOfElevators: cars,
                weightLimitKg: weightLimit,
            };
            await configureBuilding(req);
            setFeedback('Configuration applied.');
        } catch (err) {
            setFeedback(err instanceof Error ? err.message : 'Configuration failed.');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <form className="config-form" onSubmit={handleSubmit}>
            <div className="config-form-header">
                <span className="config-icon">⚙</span>
                <h3>Building Config</h3>
            </div>
            <label className="config-field">
                <span className="config-label">Floors</span>
                <input
                    type="number"
                    min={2}
                    max={100}
                    value={floors}
                    onChange={e => setFloors(Number(e.target.value))}
                />
            </label>
            <label className="config-field">
                <span className="config-label">Elevator Cars</span>
                <input
                    type="number"
                    min={1}
                    max={20}
                    value={cars}
                    onChange={e => setCars(Number(e.target.value))}
                />
            </label>
            <label className="config-field">
                <span className="config-label">Weight Limit (kg)</span>
                <input
                    type="number"
                    min={100}
                    max={5000}
                    step={50}
                    value={weightLimit}
                    onChange={e => setWeightLimit(Number(e.target.value))}
                />
            </label>
            <button type="submit" className="config-submit" disabled={submitting}>
                {submitting ? 'Applying…' : 'Apply'}
            </button>
            {feedback && <p className="config-feedback">{feedback}</p>}
        </form>
    );
}
