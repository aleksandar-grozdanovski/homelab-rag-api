import { useState, useEffect } from "react";

type LLMProvider = "ollama" | "groq";

const STORAGE_KEY = "llm-provider";

export function useLLMProvider() {
  const [provider, setProviderState] = useState<LLMProvider>(() => {
    // Initialize from localStorage or default to groq (faster for testing)
    const stored = localStorage.getItem(STORAGE_KEY);
    return (stored as LLMProvider) || "groq";
  });

  const setProvider = (newProvider: LLMProvider) => {
    setProviderState(newProvider);
    localStorage.setItem(STORAGE_KEY, newProvider);
  };

  return { provider, setProvider };
}
