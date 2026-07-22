
import { useState } from 'react'
import type { ChangeEvent, SyntheticEvent } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import type { RegisterData, LocationState } from '../../types/auth.types'
import './Register.css'
import { FaUser, FaEnvelope, FaPhone, FaLock, FaEye, FaEyeSlash } from 'react-icons/fa'
import busImage from '../../assets/busImage.jpeg'
import logo from '../../assets/logo.png'

export const Register = () => {
  const navigate = useNavigate()

  const [formData, setFormData] = useState<RegisterData>({
    name: '',
    email: '',
    numberPhone: '',
    password: '',
    confirmPassword: ''
  })
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({ ...prev, [name]: value }))
  }

  const validateForm = (): boolean => {
    if (!formData.name || !formData.email || !formData.numberPhone || !formData.password) {
      setError('Please fill in all fields')
      return false
    }
    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match')
      return false
    }
    if (formData.password.length < 8) {
      setError('Password must be at least 8 characters')
      return false
    }
    return true
  }

  const handleSubmit = async (e: SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault()
    setError('')
    if (!validateForm()) return

    setIsLoading(true)

    try {
      await new Promise(resolve => setTimeout(resolve, 1500))
      console.log('Registration successful:', formData)

      const state: LocationState = {
        message: '✅ Registration successful! Please login with your credentials.',
        email: formData.email
      }

      navigate('/home', { replace: true, state })
    } catch {
      setError('Registration failed. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="register-page">
      <div className="left-section">
        <div className="logo">
          <img src={logo} className="logo-icon-img" alt="TshwaneLogo"/>
          <h2>TshwaneRide</h2>
        </div>
        <p className="tagline">CITY OF TSHWANE DIGITAL TICKETING</p>
        <h1>One account for your Connector, tickets and journeys.</h1>
        <p>Create an account, link your physical Connector and travel with a secure on-screen token.</p>

        <div className="steps">
          <div className="step">
            <div className="step-dot active"></div>
            <span className="step-label">Create account</span>
          </div>
          <div className="step">
            <div className="step-dot"></div>
            <span className="step-label">Link Connector</span>
          </div>
          <div className="step">
            <div className="step-dot"></div>
            <span className="step-label">Ready to travel</span>
          </div>
        </div>

        <img src={busImage} className="bus-image" alt="Tshwane buses" />
      </div>

      <div className="right-section">
        <div className="register-card">
          <p className="brand">TSHWANERIDE</p>
          <h2>Create Account</h2>
          <p className="subtitle">Register to manage your connector, wallet and journeys.</p>

          {error && <div className="error-message">{error}</div>}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="name">Full Name</label>
              <div className="input-container">
                <FaUser className="input-icon" />
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleChange}
                  placeholder="Enter your full name"
                  required
                  disabled={isLoading}
                />
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="email">Email</label>
              <div className="input-container">
                <FaEnvelope className="input-icon" />
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
            </div>

            <div className="form-group">
              <label htmlFor="numberPhone">Phone Number</label>
              <div className="input-container">
                <FaPhone className="input-icon" />
                <input
                  type="tel"
                  id="numberPhone"
                  name="numberPhone"
                  value={formData.numberPhone}
                  onChange={handleChange}
                  placeholder="Enter your phone number"
                  required
                  disabled={isLoading}
                />
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="password">Password</label>
              <div className="input-container">
                <FaLock className="input-icon" />
                <input
                  type={showPassword ? 'text' : 'password'}
                  id="password"
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  placeholder="Create a password (min 8 characters)"
                  required
                  disabled={isLoading}
                />
                <button
                  type="button"
                  className="password-toggle-btn"
                  onClick={() => setShowPassword(!showPassword)}
                  disabled={isLoading}
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                >
                  {showPassword ? <FaEyeSlash /> : <FaEye />}
                </button>
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="confirmPassword">Confirm Password</label>
              <div className="input-container">
                <FaLock className="input-icon" />
                <input
                  type={showConfirmPassword ? 'text' : 'password'}
                  id="confirmPassword"
                  name="confirmPassword"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  placeholder="Confirm your password"
                  required
                  disabled={isLoading}
                />
                <button
                  type="button"
                  className="password-toggle-btn"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  disabled={isLoading}
                  aria-label={showConfirmPassword ? 'Hide password' : 'Show password'}
                >
                  {showConfirmPassword ? <FaEyeSlash /> : <FaEye />}
                </button>
              </div>
            </div>

            <button type="submit" className="btn btn-primary" disabled={isLoading}>
              {isLoading ? 'Creating Account...' : 'Create Account'}
            </button>
          </form>

          <p className="auth-redirect">
            Already have an account? <Link to="/login">Log In</Link>
          </p>
        </div>
      </div>
    </div>
  )
}