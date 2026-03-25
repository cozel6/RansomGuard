/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        'terminal-black': '#0d1117',
        'terminal-dark': '#161b22',
        'terminal-gray': '#21262d',
        'terminal-light': '#30363d',
        'terminal-green': '#3fb950',
        'terminal-cyan': '#58d1eb',
        'terminal-red': '#ff6b6b',
        'terminal-yellow': '#f0c74e',
        'terminal-orange': '#ff9f43',
        'terminal-muted': '#8b949e',
      },
      fontFamily: {
        mono: ['Cascadia Code', 'Fira Code', 'Consolas', 'monospace'],
      },
      boxShadow: {
        'glow-green': '0 0 20px rgba(63, 185, 80, 0.4)',
        'glow-cyan': '0 0 20px rgba(88, 209, 235, 0.4)',
        'glow-red': '0 0 20px rgba(255, 107, 107, 0.4)',
      },
      keyframes: {
        scan: {
          '0%': { transform: 'translateY(0px)' },
          '100%': { transform: 'translateY(4px)' },
        },
        'glow-pulse': {
          '0%, 100%': { opacity: '1' },
          '50%': { opacity: '0.5' },
        },
        'cursor-blink': {
          '0%, 100%': { opacity: '1' },
          '50%': { opacity: '0' },
        },
        flicker: {
          '0%, 100%': { opacity: '1' },
          '50%': { opacity: '0.9' },
        },
      },
      animation: {
        scan: 'scan 8s linear infinite',
        'glow-pulse': 'glow-pulse 2s ease-in-out infinite',
        'cursor-blink': 'cursor-blink 1s step-end infinite',
        flicker: 'flicker 150ms ease-in-out infinite',
      },
    },
  },
  plugins: [],
}

