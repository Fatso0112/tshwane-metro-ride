import { useState, useEffect } from 'react'
import type { ChangeEvent, SyntheticEvent}  from 'react'
import { useNavigate, useLocation, Link } from 'react-router-dom'
import type { LoginCredentials, LocationState } from '../../types/auth.types'
import './Login.css'

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

  useEffect(() => {
    if (locationState?.message) setSuccessMessage(locationState.message)
    if (locationState?.email) {
      setFormData(prev => ({ ...prev, email: locationState.email || '' }))
    }
  }, [locationState])

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({ ...prev, [name]: value }))
  }

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

      const state: LocationState = {
        message: '👋 Welcome back!',
        user: { id: '1', name: 'User', email: formData.email }
      }

      navigate('/', { replace: true, state })
    } catch {
      setError('Invalid email or password. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="login-container">
      <div className="login-card">
        <h2>Login</h2>
        <p className="subtitle">Welcome back!</p>
        {successMessage && <div className="success-message">{successMessage}</div>}
        {error && <div className="error-message">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="email">Email</label>
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
          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Enter your password"
              required
              disabled={isLoading}
            />
          </div>
          <button type="submit" className="btn btn-primary" disabled={isLoading}>
            {isLoading ? 'Logging in...' : 'Login'}
          </button>
        </form>
        <p className="auth-redirect">
          Don't have an account? <Link to="/register">Register</Link>
        </p>
      </div>
    </div>
  )
}