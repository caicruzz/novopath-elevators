import { useState } from 'react';
import { useBuildingState } from './hooks/useBuildingState';
import { ElevatorCar } from './components/ElevatorCar';
import { FloorCallPanel } from './components/FloorCallPanel';
import { ConfigurationForm } from './components/ConfigurationForm';
import { EmergencyPanel } from './components/EmergencyPanel';
import { SimulationReport } from './components/SimulationReport';
import './App.css';

function App() {
  const { buildingState, error } = useBuildingState();
  const [showReport, setShowReport] = useState(false);

  return (
    <div className="app">
      {/* Emergency Banner */}
      {buildingState?.isEmergencyMode && (
        <div className="emergency-banner">
          <span className="emergency-banner-icon">🔥</span>
          FIRE EMERGENCY — ALL ELEVATORS RETURNING TO GROUND FLOOR
          <span className="emergency-banner-icon">🔥</span>
        </div>
      )}

      {/* Header */}
      <header className="app-header">
        <div className="header-glow" />
        <h1 className="app-title">
          <span className="title-icon">⬡</span>
          Elevator Simulator
        </h1>
        <p className="app-subtitle">High-Rise Building Control System</p>
      </header>

      {/* Main Content */}
      <main className="app-main">
        {error && (
          <div className="error-banner">
            <span className="error-icon">⚡</span>
            {error}
            <span className="error-hint">— Is the backend running?</span>
          </div>
        )}

        {!buildingState && !error && (
          <div className="loading-state">
            <div className="loading-spinner" />
            <p>Connecting to simulation engine…</p>
          </div>
        )}

        {buildingState && (
          <div className="simulation-layout">
            {/* Left side: Config + Emergency + Report */}
            <div className="left-panel">
              <ConfigurationForm
                currentConfig={{
                  numberOfFloors: buildingState.numberOfFloors,
                  numberOfElevators: buildingState.elevators.length,
                  weightLimitKg: buildingState.weightLimitKg,
                }}
              />
              <EmergencyPanel isEmergencyMode={buildingState.isEmergencyMode} />
              <button
                className="view-report-btn"
                onClick={() => setShowReport(true)}
              >
                📊 View Report
              </button>
            </div>

            {/* Elevator Shafts */}
            <div className="shafts-container">
              <div className="shafts-panel">
                <div className="shafts-header">
                  <span className={`live-dot${buildingState.isEmergencyMode ? ' emergency' : ''}`} />
                  <span>{buildingState.isEmergencyMode ? 'EMERGENCY' : 'LIVE'}</span>
                </div>
                <div className="shafts-grid">
                  {buildingState.elevators.map((elevator) => (
                    <ElevatorCar
                      key={elevator.id}
                      elevator={elevator}
                      numberOfFloors={buildingState.numberOfFloors}
                    />
                  ))}
                </div>
              </div>
            </div>

            {/* Call Panel */}
            <FloorCallPanel numberOfFloors={buildingState.numberOfFloors} />
          </div>
        )}

        {/* Compliance Log */}
        {buildingState && buildingState.complianceLog.length > 0 && (
          <div className="compliance-section">
            <h3 className="compliance-title">📋 Compliance Log</h3>
            <div className="compliance-table-container">
              <table className="compliance-table">
                <thead>
                  <tr>
                    <th>Time</th>
                    <th>Car</th>
                    <th>Event</th>
                    <th>Description</th>
                  </tr>
                </thead>
                <tbody>
                  {buildingState.complianceLog.slice().reverse().map((event, i) => (
                    <tr key={i} className={`event-${event.eventType.toLowerCase()}`}>
                      <td>{new Date(event.timestamp).toLocaleTimeString()}</td>
                      <td>{event.elevatorId === 0 ? 'System' : `Car ${event.elevatorId}`}</td>
                      <td><span className="event-badge">{event.eventType}</span></td>
                      <td>{event.description}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </main>

      {/* Report Overlay */}
      {showReport && <SimulationReport onClose={() => setShowReport(false)} />}
    </div>
  );
}

export default App;
