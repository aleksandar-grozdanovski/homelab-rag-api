/**
 * API Configuration
 * Automatically detects the correct API URL based on environment
 */

const getApiBaseUrl = (): string => {
  // Check if we're in development (localhost)
  if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    return 'http://localhost:5113';
  }
  
  // In production, use same origin with /api prefix
  // This assumes Caddy/nginx will proxy /api/* to the backend
  return window.location.origin;
};

export const API_BASE_URL = getApiBaseUrl();

export const API_ENDPOINTS = {
  query: `${API_BASE_URL}/api/query`,
  documents: `${API_BASE_URL}/api/documents`,
  health: `${API_BASE_URL}/healthz`,
} as const;
