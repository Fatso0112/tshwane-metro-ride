import { useState, useEffect } from 'react'
import type { ChangeEvent, SyntheticEvent } from 'react'
import { useNavigate, useLocation, Link } from 'react-router-dom'
import type { LoginCredentials, LocationState } from '../../types/auth.types'
import './Login.css'
/* Import the eye icons */
import { FaEye, FaEyeSlash } from 'react-icons/fa'

export const Login = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const locationState = location.state as LocationState | null

  const [formData, setFormData] = useState<LoginCredentials>({
    email: '',
    password: ''
  })
  const [error, setError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  
  /* State to control password visibility */
  const [showPassword, setShowPassword] = useState(false)

  // Read success message from registration
  useEffect(() => {
    if (locationState?.message) setSuccessMessage(locationState.message)
    if (locationState?.email) {
      setFormData(prev => ({ ...prev, email: locationState.email || '' }))
    }
  }, [locationState])

  // Handle input changes
  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({ ...prev, [name]: value }))
    console.log(`${name}:`, value)
  }

  // Handle form submission
  const handleSubmit = async (e: SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault()
    setError('')
    setSuccessMessage('')

    if (!formData.email || !formData.password) {
      setError('Please fill in all fields')
      return
    }

    setIsLoading(true)

    try {
      await new Promise(resolve => setTimeout(resolve, 1500))
      console.log('Login successful:', formData)

      navigate('/', { 
        replace: true, 
        state: { 
          message: '👋 Welcome back!',
          user: { id: '1', name: 'User', email: formData.email }
        }
      })
    } catch {
      setError('Invalid email or password. Check your details and try again.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="login-wrapper">
      {/* LEFT: Brand & Image */}
      <div className="login-left">
        <div className="brand-box">
          
          <h1 className="brand-title">TshwaneRide</h1>
          <p className="brand-subtitle">CONNECTING THE CAPITAL</p>
          <div className="brand-tagline">
            <span>Sign in to see your Connector,</span>
            <span>tickets and live A Re Yeng journeys.</span>
          </div>
          <div className="brand-features">
            
          </div>
        </div>
      </div>

      {/* RIGHT: Login Form */}
      <div className="login-right">
        <div className="login-form-container">
          <div className="form-header">
            <h2 className="welcome-title">Welcome back</h2>
            <p className="welcome-description">
              Log in to manage your tickets and Connector.
            </p>
          </div>

          {error && (
            <div className="error-box">
              <span className="error-icon">✕</span>
              <div className="error-text">
                <strong>Invalid email address or password.</strong>
                <br />
                Check your details and try again.
              </div>
            </div>
          )}

          {successMessage && (
            <div className="success-box">
              <span className="success-icon">✓</span>
              <div className="success-text">{successMessage}</div>
            </div>
          )}

          <form onSubmit={handleSubmit} noValidate>
            <div className="input-group">
              <label htmlFor="email">Email address</label>
              <input
                type="email"
                id="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                placeholder="Enter your email"
                required
                disabled={isLoading}
              />
            </div>

            <div className="input-group">
              <label htmlFor="password">Password</label>
              {/* Added a relative container box to position the eye layout */}
              <div className="password-input-wrapper">
                <input
                  type={showPassword ? 'text' : 'password'} /* Dynamic input switching */
                  id="password"
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  placeholder="Enter your password"
                  required
                  disabled={isLoading}
                />
                <button
                  type="button" /* Crucial: stops the form from submitting on toggle click */
                  className="password-toggle-btn"
                  onClick={() => setShowPassword(!showPassword)}
                  disabled={isLoading}
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                >
                  {showPassword ? <FaEyeSlash /> : <FaEye />}
                </button>
              </div>
            </div>

            <button type="submit" className="login-btn" disabled={isLoading}>
              {isLoading ? 'Logging in...' : 'Login'}
            </button>
          </form>

          <div className="form-footer">
            <Link to="/forgot-password" className="forgot-link">
              Forgot password?
            </Link>
            <p className="signup-link">
              Don't have an account? <Link to="/register">Register</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
