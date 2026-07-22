
import { useState } from 'react'
import { FaEye, FaQrcode, FaCheckCircle, FaUser, FaChevronRight } from 'react-icons/fa'
import { Link } from 'react-router-dom'
import './Wallet.css'

export const Wallet = () => {
  const [connectorNumber, setConnectorNumber] = useState('•••• •••• •••• 4587')
  const [isLinked, setIsLinked] = useState(false)

  const handleLink = () => {
    setIsLinked(true)
  }

  return (
    <div className="wallet-page">
      {/* Navigation */}
      <nav className="top-nav">
        <div className="nav-logo">
          <img src="/assets/logo.png" alt="TshwaneRide" className="nav-logo-img" />
          <span>TshwaneRide</span>
        </div>
        <div className="nav-links">
          <Link to="/home">Home</Link>
          <Link to="/journeys">Journeys</Link>
          <Link to="/wallet" className="active">Wallet</Link>
          <Link to="/timetable">Timetable</Link>
        </div>
        <div className="nav-user">
          <div className="nav-icon">
            <FaUser />
          </div>
          <span>AM</span>
          <span className="user-name">Aubrey</span>
        </div>
      </nav>

      {/* Page Content */}
      <div className="page-content">
        <div className="page-header">
          <p className="section-label">CONNECTOR ENROLLMENT</p>
          <h1>Make your physical card travel-ready on this device.</h1>
          <p className="page-subtitle">
            The 16-digit Connector number is a transit credential, not a bank-card number.
          </p>
        </div>

        {/* Main Card */}
        <div className="connector-card">
          {/* Column 1: Physical Connector */}
          <div className="column">
            <p className="column-label">PHYSICAL CONNECTOR</p>
            
            <div className="physical-card">
              <div className="card-top">
                <div className="card-signal">
                  <div className="signal-bars">
                    <span></span>
                    <span></span>
                    <span></span>
                    <span></span>
                  </div>
                  <div className="signal-waves">
                    <span></span>
                    <span></span>
                  </div>
                </div>
              </div>
              <div className="card-line"></div>
              <div className="card-dots">
                <span className="dot filled"></span>
                <span className="dot"></span>
                <span className="dot filled"></span>
              </div>
              <p className="card-label">CONNECTOR</p>
              <p className="card-name">Aubrey Matala</p>
              <p className="card-number">•••• •••• •••• 4587</p>
              <div className="card-bottom">
                <span className="card-initial">R<span className="card-dash">—</span></span>
                <FaEye className="card-eye" />
                <span className="card-status">Ready to travel</span>
              </div>
            </div>

            <p className="input-hint">Enter or scan the number printed on the card.</p>
            
            <label className="input-label">Connector number</label>
            <div className="connector-input-wrapper">
              <input
                type="text"
                value={connectorNumber}
                onChange={(e) => setConnectorNumber(e.target.value)}
                className="connector-input"
              />
              <button className="scan-btn">
                <FaQrcode />
              </button>
            </div>

            <button 
              className={`link-btn ${isLinked ? 'linked' : ''}`}
              onClick={handleLink}
            >
              {isLinked ? 'Connector Linked' : 'Link Connector'}
            </button>
          </div>

          {/* Column 2: Secure Device Binding */}
          <div className="column column-binding">
            <p className="column-label">SECURE DEVICE BINDING</p>
            
            <div className="binding-stepper">
              <div className="step-item">
                <div className="step-dot green"></div>
                <span>Entered</span>
              </div>
              <div className="step-line green"></div>
              <div className="step-item">
                <div className="step-dot purple"></div>
                <span>Binding</span>
              </div>
              <div className="step-line gray"></div>
              <div className="step-item">
                <div className="step-dot gray"></div>
                <span>Token</span>
              </div>
            </div>

            <div className="binding-arrows">
              <FaChevronRight className="arrow-icon" />
              <span className="arrow-dot"></span>
              <FaChevronRight className="arrow-icon" />
              <span className="arrow-dot"></span>
              <FaChevronRight className="arrow-icon" />
              <span className="arrow-dot"></span>
              <FaChevronRight className="arrow-icon" />
              <span className="arrow-dot"></span>
            </div>

            <div className="binding-info">
              <p>The full number is masked immediately. The app binds an opaque transit token to a non-exportable device key.</p>
              <div className="binding-check">
                <FaCheckCircle className="check-icon" />
                <span>Protected device binding</span>
              </div>
            </div>
          </div>

          {/* Column 3: Digital Token */}
          <div className="column">
            <div className="column-header">
              <p className="column-label">DIGITAL TOKEN</p>
              <span className="secure-badge">
                <span className="secure-dot"></span>
                SECURE
              </span>
            </div>

            <div className="digital-card">
              <p className="digital-brand">TSHWANERIDE</p>
              <h3>Ready to travel</h3>
              <div className="digital-divider"></div>
              
              <div className="digital-steps">
                <div className="digital-step">
                  <div className="digital-step-indicator">
                    <div className="digital-dot green"></div>
                    <div className="digital-line green"></div>
                  </div>
                  <span>Card entered</span>
                </div>
                <div className="digital-step">
                  <div className="digital-step-indicator">
                    <div className="digital-dot purple"></div>
                    <div className="digital-line gray"></div>
                  </div>
                  <span>Bound to device</span>
                </div>
                <div className="digital-step">
                  <div className="digital-step-indicator">
                    <div className="digital-dot gray"></div>
                  </div>
                  <span>Ready</span>
                </div>
              </div>

              <button className="preview-btn">
                Preview •••• 4587
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}