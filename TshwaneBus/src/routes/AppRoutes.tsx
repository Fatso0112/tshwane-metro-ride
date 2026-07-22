import { Routes, Route, Navigate, useLocation } from 'react-router-dom'
import { AnimatePresence, motion } from 'framer-motion'
import { Home } from '../pages/Home/Home'
import { Login } from '../pages/Login/Login'
import { Register } from '../pages/Register/Register'
import { Wallet } from '../pages/Wallet/Wallet'   //

// ---------- Animation Variants (unchanged) ----------
const pageVariants = {
  initial: { x: '100%', opacity: 0, filter: 'blur(10px)' },
  animate: { x: 0, opacity: 1, filter: 'blur(0px)' },
  exit: { x: '-100%', opacity: 0, filter: 'blur(10px)' },
}

const pageTransition = {
  type: 'tween' as const,
  ease: 'easeInOut' as const,
  duration: 0.6,
}

const blockVariants = {
  initial: { x: '-120%', opacity: 0, filter: 'blur(8px)' },
  animate: (i: number) => ({
    x: 0,
    opacity: 1,
    filter: 'blur(0px)',
    transition: {
      type: 'tween' as const,
      ease: 'easeOut' as const,
      duration: 0.5,
      delay: i * 0.12,
    },
  }),
  exit: (i: number) => ({
    x: '120%',
    opacity: 0,
    filter: 'blur(8px)',
    transition: {
      type: 'tween' as const,
      ease: 'easeIn' as const,
      duration: 0.4,
      delay: (2 - i) * 0.1,
    },
  }),
}

const ShatterBlock = ({ children, index }: { children: React.ReactNode; index: number }) => {
  const clipPaths = [
    'inset(0 66.66% 0 0)',
    'inset(0 33.33% 0 33.33%)',
    'inset(0 0 0 66.66%)',
  ]

  return (
    <motion.div
      custom={index}
      variants={blockVariants}
      style={{
        position: 'absolute',
        top: 0,
        left: 0,
        width: '100%',
        height: '100%',
        clipPath: clipPaths[index],
        pointerEvents: 'none',
        userSelect: 'none',
        zIndex: 10,
        background: `rgba(255,255,255,${index === 1 ? 0.05 : 0})`,
      }}
    >
      <div style={{ pointerEvents: 'none', userSelect: 'none' }}>
        {children}
      </div>
    </motion.div>
  )
}

const AnimatedPage = ({ children }: { children: React.ReactNode }) => {
  return (
    <motion.div
      variants={pageVariants}
      initial="initial"
      animate="animate"
      exit="exit"
      transition={pageTransition}
      style={{
        position: 'relative',
        width: '100%',
        height: '100%',
        overflow: 'hidden',
      }}
    >
      <div style={{ position: 'relative', zIndex: 20, pointerEvents: 'auto' }}>
        {children}
      </div>
      {[0, 1, 2].map((i) => (
        <ShatterBlock key={i} index={i}>
          {children}
        </ShatterBlock>
      ))}
    </motion.div>
  )
}

export const AppRoutes = () => {
  const location = useLocation()

  return (
    <AnimatePresence mode="wait">
      <Routes location={location} key={location.pathname}>
        {/* ✅ Root redirects to Register (start page) */}
        <Route path="/" element={<Navigate to="/register" replace />} />

        {/* Auth pages */}
        <Route path="/register" element={<AnimatedPage><Register /></AnimatedPage>} />
        <Route path="/login" element={<AnimatedPage><Login /></AnimatedPage>} />

        {/* Main pages */}
        <Route path="/home" element={<AnimatedPage><Home /></AnimatedPage>} />
        <Route path="/wallet" element={<AnimatedPage><Wallet /></AnimatedPage>} />

        {/* Catch-all – redirect to register */}
        <Route path="*" element={<Navigate to="/register" replace />} />
      </Routes>
    </AnimatePresence>
  )
}