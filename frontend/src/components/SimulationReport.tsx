import { useState, useCallback, useEffect } from 'react';
import type { SimulationReportDto } from '../types/elevator';
import { fetchReport } from '../services/api';
import './SimulationReport.css';

interface SimulationReportProps {
    onClose: () => void;
}

export function SimulationReport({ onClose }: SimulationReportProps) {
    const [report, setReport] = useState<SimulationReportDto | null>(null);
    const [loading, setLoading] = useState(true);

    const loadReport = useCallback(async () => {
        setLoading(true);
        try {
            const data = await fetchReport();
            setReport(data);
        } catch (err) {
            console.error('Failed to load report', err);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { loadReport(); }, [loadReport]);

    const formatTime = (seconds: number) => {
        if (seconds < 60) return `${seconds.toFixed(1)}s`;
        const mins = Math.floor(seconds / 60);
        const secs = (seconds % 60).toFixed(0);
        return `${mins}m ${secs}s`;
    };

    return (
        <div className="report-overlay">
            <div className="report-container">
                {/* Header */}
                <div className="report-header">
                    <div className="report-title-group">
                        <h2 className="report-title">📊 Simulation Report</h2>
                        <span className="report-timestamp">
                            {report ? `Generated ${new Date(report.generatedAt).toLocaleTimeString()}` : ''}
                        </span>
                    </div>
                    <div className="report-actions">
                        <button className="report-btn refresh" onClick={loadReport} disabled={loading}>
                            🔄 Refresh
                        </button>
                        <button className="report-btn close" onClick={onClose}>
                            ✕ Close
                        </button>
                    </div>
                </div>

                {loading && !report && (
                    <div className="report-loading">
                        <div className="report-spinner" />
                        <p>Generating report…</p>
                    </div>
                )}

                {report && (
                    <>
                        {/* Metric Cards */}
                        <div className="metrics-grid">
                            <div className="metric-card">
                                <span className="metric-icon">🚀</span>
                                <span className="metric-value">{report.totalTrips}</span>
                                <span className="metric-label">Total Trips</span>
                            </div>
                            <div className="metric-card">
                                <span className="metric-icon">👤</span>
                                <span className="metric-value">{report.totalPassengersServed}</span>
                                <span className="metric-label">Passengers Served</span>
                            </div>
                            <div className="metric-card highlight">
                                <span className="metric-icon">⏱️</span>
                                <span className="metric-value">{formatTime(report.avgWaitTimeSeconds)}</span>
                                <span className="metric-label">Avg Wait Time</span>
                            </div>
                            <div className="metric-card warn">
                                <span className="metric-icon">⏳</span>
                                <span className="metric-value">{formatTime(report.maxWaitTimeSeconds)}</span>
                                <span className="metric-label">Max Wait Time</span>
                            </div>
                            <div className="metric-card">
                                <span className="metric-icon">🏗️</span>
                                <span className="metric-value">{formatTime(report.avgTravelTimeSeconds)}</span>
                                <span className="metric-label">Avg Travel Time</span>
                            </div>
                            <div className="metric-card">
                                <span className="metric-icon">📐</span>
                                <span className="metric-value">{report.totalFloorsTraversed}</span>
                                <span className="metric-label">Floors Traversed</span>
                            </div>
                            <div className={`metric-card${report.complianceEventCount > 0 ? ' danger' : ''}`}>
                                <span className="metric-icon">🛡️</span>
                                <span className="metric-value">{report.complianceEventCount}</span>
                                <span className="metric-label">Safety Events</span>
                            </div>
                        </div>

                        {/* Per-Elevator Table */}
                        <div className="report-section">
                            <h3 className="section-title">🏢 Per-Elevator Breakdown</h3>
                            <div className="report-table-container">
                                <table className="report-table">
                                    <thead>
                                        <tr>
                                            <th>Car</th>
                                            <th>Trips</th>
                                            <th>Floors</th>
                                            <th>Avg Wait</th>
                                            <th>Max Wait</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {report.elevatorStats.map(stat => (
                                            <tr key={stat.elevatorId}>
                                                <td className="car-cell">Car {stat.elevatorId}</td>
                                                <td>{stat.tripsCompleted}</td>
                                                <td>{stat.floorsTraversed}</td>
                                                <td>{formatTime(stat.avgWaitTimeSeconds)}</td>
                                                <td>{formatTime(stat.maxWaitTimeSeconds)}</td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>

                        {/* Compliance Log */}
                        {report.complianceLog.length > 0 && (
                            <div className="report-section">
                                <h3 className="section-title">📋 Compliance Ledger</h3>
                                <div className="report-table-container">
                                    <table className="report-table compliance">
                                        <thead>
                                            <tr>
                                                <th>Time</th>
                                                <th>Car</th>
                                                <th>Event</th>
                                                <th>Description</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {report.complianceLog.slice().reverse().map((evt, i) => (
                                                <tr key={i} className={`event-${evt.eventType.toLowerCase()}`}>
                                                    <td>{new Date(evt.timestamp).toLocaleTimeString()}</td>
                                                    <td>{evt.elevatorId === 0 ? 'System' : `Car ${evt.elevatorId}`}</td>
                                                    <td><span className="report-event-badge">{evt.eventType}</span></td>
                                                    <td>{evt.description}</td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        )}

                        {report.totalTrips === 0 && (
                            <div className="report-empty">
                                <span className="empty-icon">📭</span>
                                <p>No trips completed yet. Call some elevators and check back!</p>
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    );
}
