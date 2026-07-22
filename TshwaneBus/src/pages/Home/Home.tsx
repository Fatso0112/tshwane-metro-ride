import { useNavigate, Link } from 'react-router-dom'
import './Home.css'

export const Home = () => {
  const navigate = useNavigate()

  return (
    <div className="home-wrapper">
      {/* Header */}
      <header className="home-header">
        <h1 className="app-title">TshwaneRide</h1>
      </header>

      {/* Main Content */}
      <main className="home-content">
        {/* Route Section */}
        <section className="route-section">
          <div className="route-header">
          
          </div>

          <div className="route-card">
            <div className="route-card-left">
              <div className="route-info">
               
              </div>
            </div>
            <div className="departure-time">
              
            </div>
          </div>
        </section>

        {/* Quick Actions */}
        <section className="quick-actions">
          <h3 className="actions-title">Quick actions</h3>
          <div className="actions-grid">
            <Link to="/buy-tickets" className="action-card">
              <span className="action-icon"></span>
              <span className="action-label">Buy tickets</span>
            </Link>
            {/* Track ride removed */}
            <Link to="/timetable" className="action-card">
              <span className="action-icon"></span>
              <span className="action-label">View timetable</span>
            </Link>
            <Link to="/top-up" className="action-card">
              <span className="action-icon"></span>
              <span className="action-label">Top up Connector</span>
            </Link>
          </div>
        </section>

        {/* Bottom: Alerts (left) + Image (right) */}
        <section className="bottom-section">
          <div className="alerts-section">
            <p className="alerts-text">No critical services alerts</p>
          </div>
          <div className="bottom-image">
            <img
              src="/src/assets/bus-bg.jpeg"
              alt="Bus"
              className="bottom-bus-image"
            />
          </div>
        </section>
      </main>
    </div>
  )
}