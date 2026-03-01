import { useBuildingState } from './hooks/useBuildingState';
import { ElevatorCar } from './components/ElevatorCar';
import { FloorCallPanel } from './components/FloorCallPanel';
import './App.css';

function App() {
  const { buildingState, error } = useBuildingState();

  return (
    <div className="app">
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
            {/* Elevator Shafts */}
            <div className="shafts-container">
              <div className="shafts-panel">
                <div className="shafts-header">
                  <span className="live-dot" />
                  <span>LIVE</span>
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
      </main>
    </div>
  );
}

export default App;
