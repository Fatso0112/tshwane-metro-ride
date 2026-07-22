import { useNavigate } from 'react-router-dom'
import './Home.css'

export const Home = () => {
  const navigate = useNavigate()

  const handleLoginClick = () => navigate('/login')
  const handleRegisterClick = () => navigate('/register') 
  const handleWalletClick = () => navigate('/wallet')


  return (
    <div className="home-container">
      <div className="home-content">
        <h1>Welcome to Our App</h1>
        <p>Please choose an option to continue</p>
        <div className="button-group">
          <button onClick={handleLoginClick} className="btn btn-login" type="button">
            Login
          </button>
          <button onClick={handleRegisterClick} className="btn btn-register" type="button">
            Register
          </button>
          <button onClick={handleWalletClick} className="btn btn-test" type="button">
            Wallet
          </button>
        </div>
      </div>
    </div>
  )
}
