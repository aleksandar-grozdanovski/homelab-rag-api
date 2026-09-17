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

export const getApiKey = (): string => {
  const storageKey = 'homelab-rag-api-key';
  const existing = window.sessionStorage.getItem(storageKey);
  if (existing) return existing;

  const supplied = window.prompt('Enter the Homelab RAG API key');
  if (!supplied) throw new Error('An API key is required');
  window.sessionStorage.setItem(storageKey, supplied);
  return supplied;
};

export const clearApiKey = (): void =>
  window.sessionStorage.removeItem('homelab-rag-api-key');

export const API_ENDPOINTS = {
  query: `${API_BASE_URL}/api/query`,
  documents: `${API_BASE_URL}/api/documents`,
  health: `${API_BASE_URL}/healthz`,
} as const;
