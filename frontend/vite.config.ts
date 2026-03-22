import { defineConfig } from 'vite' 
import react from '@vitejs/plugin-react-swc'
import path from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],

  // Path alias: @ → ./src
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },

  // Development server configuration
  server: {
    port: 5173,
    strictPort: true,
    open: true, // Auto-open browser
    proxy: {
      // Proxy API requests to backend
      '/api': {
        target: 'http://localhost:5087',
        changeOrigin: true,
        secure: false,
      },
    },
  },

  // Build optimization
  build: {
    outDir: 'dist',
    sourcemap: false,
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules')) {
            if (id.includes('react') || id.includes('react-dom') || id.includes('react-router-dom')) {
              return 'vendor';
            }
            if (id.includes('@tanstack/react-query')) {
              return 'query';
            }
            if (id.includes('@radix-ui')) {
              return 'ui';
            }
          }
        },
      },
    },
  },
})