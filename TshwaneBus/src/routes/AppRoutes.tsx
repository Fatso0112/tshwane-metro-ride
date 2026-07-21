import { Routes, Route, Navigate, useLocation } from 'react-router-dom'
import { AnimatePresence, motion } from 'framer-motion'
import { Home } from '../pages/Home/Home'
import { Login } from '../pages/Login/Login'
import { Register } from '../pages/Register/Register'

// ---------- Animation Variants for the Page (single container) ----------
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

// ---------- Shatter Blocks (3 vertical blocks) ----------
// Each block is a motion.div that covers the page but is clipped to a vertical third.
// They will stagger their slide and blur to create the "crash" effect.

const blockVariants = {
  initial: {
    x: '-120%',       // far left (off-screen)
    opacity: 0,
    filter: 'blur(8px)',
  },
  animate: (i: number) => ({
    x: 0,
    opacity: 1,
    filter: 'blur(0px)',
    transition: {
      type: 'tween' as const,
      ease: 'easeOut' as const,
      duration: 0.5,
      delay: i * 0.12,      // left block first, center next, right last
    },
  }),
  exit: (i: number) => ({
    x: '120%',           // fly out to the right
    opacity: 0,
    filter: 'blur(8px)',
    transition: {
      type: 'tween' as const,
      ease: 'easeIn' as const,
      duration: 0.4,
      delay: (2 - i) * 0.1, // reverse order: right block exits first
    },
  }),
}

// A block that covers a third of the page using clip-path
const ShatterBlock = ({ children, index }: { children: React.ReactNode; index: number }) => {
  // Calculate clip-path for each third: left, center, right
  const clipPaths = [
    'inset(0 66.66% 0 0)',   // left third
    'inset(0 33.33% 0 33.33%)', // center third
    'inset(0 0 0 66.66%)',   // right third
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
        // To avoid overlapping issues, we set pointer-events: none during transition
        pointerEvents: 'none',
        zIndex: 10,
        // Slight background tint to enhance the shatter effect (optional)
        background: `rgba(255,255,255,${index === 1 ? 0.05 : 0})`,
      }}
    >
      {/* We render the children (page content) inside each block, but clipped */}
      {/* To avoid duplication of interactive elements, we only render content on the center block? */}
      {/* For simplicity, we'll render children in all, but they are clipped, so it's okay */}
      {children}
    </motion.div>
  )
}

// ---------- Wrapper component that applies the effect ----------
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
      {/* The actual page content (will be duplicated inside blocks) */}
      <div style={{ position: 'relative', zIndex: 1 }}>{children}</div>

      {/* Three shatter blocks overlay – they will cover the same area but with clip-path */}
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
        <Route path="/" element={<AnimatedPage><Home /></AnimatedPage>} />
        <Route path="/login" element={<AnimatedPage><Login /></AnimatedPage>} />
        <Route path="/register" element={<AnimatedPage><Register /></AnimatedPage>} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AnimatePresence>
  )
}