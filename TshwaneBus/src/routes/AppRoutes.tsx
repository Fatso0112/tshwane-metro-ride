import { Routes, Route, Navigate } from 'react-router-dom'
import { Home } from '../pages/Home/Home'
import { Login } from '../pages/Login/Login'
import { Register } from '../pages/Register/Register'

export const AppRoutes = () => {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}