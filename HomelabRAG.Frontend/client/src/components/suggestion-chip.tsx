import { type LucideIcon } from "lucide-react";

interface SuggestionChipProps {
  icon: LucideIcon;
  text: string;
  onClick: () => void;
}

export function SuggestionChip({ icon: Icon, text, onClick }: SuggestionChipProps) {
  return (
    <button
      onClick={onClick}
      className="inline-flex items-center gap-2 px-4 py-2.5 rounded-full bg-card border border-border hover-elevate active-elevate-2 transition-all duration-200 cursor-pointer whitespace-nowrap"
      data-testid={`chip-${text.toLowerCase().replace(/\s+/g, '-')}`}
    >
      <Icon className="w-4 h-4 text-primary" />
      <span className="text-sm text-foreground">{text}</span>
    </button>
  );
}
