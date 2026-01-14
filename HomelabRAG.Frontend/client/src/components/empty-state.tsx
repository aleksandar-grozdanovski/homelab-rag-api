import { BookOpen, Code, FileText, HelpCircle, Server, Cpu } from "lucide-react";
import { SuggestionChip } from "./suggestion-chip";

interface EmptyStateProps {
  onSuggestionClick: (suggestion: string) => void;
}

const suggestions = [
  { icon: Server, text: "How do I install Flux CD?" },
  { icon: Code, text: "How do I deploy my API to Kubernetes?" },
  { icon: FileText, text: "How do I set up Prometheus and Grafana?" },
  { icon: HelpCircle, text: "How do I configure WireGuard VPN?" },
  { icon: Cpu, text: "How do I set up K3s on dellbox?" },
  { icon: BookOpen, text: "How do I use GitOps with Flux?" },
];

export function EmptyState({ onSuggestionClick }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center h-full px-4 py-12">
      <div className="w-16 h-16 rounded-2xl bg-primary/10 flex items-center justify-center mb-6">
        <BookOpen className="w-8 h-8 text-primary" />
      </div>
      
      <h1 className="text-3xl font-semibold text-foreground mb-3 text-center">
        Chat with your Homelab Documentation
      </h1>
      
      <p className="text-muted-foreground text-center max-w-md mb-10">
        I'm your homelab assistant powered by RAG and Llama 3.2. Ask me anything about your infrastructure, deployment guides, or configuration steps.
      </p>

      <div className="flex flex-wrap justify-center gap-3 max-w-2xl">
        {suggestions.map((suggestion) => (
          <SuggestionChip
            key={suggestion.text}
            icon={suggestion.icon}
            text={suggestion.text}
            onClick={() => onSuggestionClick(suggestion.text)}
          />
        ))}
      </div>
    </div>
  );
}
