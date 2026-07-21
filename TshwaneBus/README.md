# React + TypeScript + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

# Project Structure 
tshwanebus/
├── public/
│   └── vite.svg
│
├── src/
│   ├── assets/
│   │   └── bus-bg.jpg                 // Bus background image
│   │
│   ├── pages/
│   │   ├── Home/
│   │   │   ├── Home.tsx               // Home page component
│   │   │   └── Home.css               // Home page styles
│   │   │
│   │   ├── Login/
│   │   │   ├── Login.tsx              // Login page component
│   │   │   └── Login.css              // Login page styles
│   │   │
│   │   └── Register/
│   │       ├── Register.tsx           // Register page component
│   │       └── Register.css           // Register page styles
│   │
│   ├── routes/
│   │   └── AppRoutes.tsx              // All route definitions with animations
│   │
│   ├── types/
│   │   └── auth.types.ts              // TypeScript interfaces
│   │
│   ├── App.tsx                        // Root App component
│   ├── App.css                        // Global App styles
│   ├── main.tsx                       // Entry point with Router
│   └── index.css                      // Global styles
│
├── index.html                          // HTML template
├── package.json                        // Dependencies
├── package-lock.json                   // Locked dependencies
├── tsconfig.json                       // TypeScript config
├── tsconfig.app.json                   // TypeScript app config
├── tsconfig.node.json                  // TypeScript node config
├── vite.config.ts                      // Vite config
├── .gitignore                          // Git ignore file
├── .eslintrc.json                      // ESLint config
└── README.md                           // Project documentation
