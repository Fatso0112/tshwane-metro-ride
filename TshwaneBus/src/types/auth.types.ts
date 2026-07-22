// src/types/auth.types.ts
export interface User {
  id: string
  name: string
  email: string
  role?: 'user' | 'admin'
}

export interface LoginCredentials {
  email: string
  password: string
}

export interface RegisterData {
  name: string
  email: string
  password: string
  confirmPassword: string
  numberPhone: string
}

export interface LocationState {
  message?: string
  email?: string
  from?: string
  user?: User
}