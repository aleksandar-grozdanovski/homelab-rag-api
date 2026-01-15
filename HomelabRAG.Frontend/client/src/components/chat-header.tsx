import { BookOpen, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ThemeToggle } from "./theme-toggle";
import { LLMProviderToggle } from "./llm-provider-toggle";

interface ChatHeaderProps {
  onNewChat: () => void;
}

export function ChatHeader({ onNewChat }: ChatHeaderProps) {
  return (
    <header className="h-16 border-b border-border bg-background/80 backdrop-blur-lg sticky top-0 z-50">
      <div className="h-full max-w-4xl mx-auto px-4 flex items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center">
            <BookOpen className="w-5 h-5 text-primary" />
          </div>
          <div>
            <h1 className="font-semibold text-foreground">DocuChat</h1>
            <p className="text-xs text-muted-foreground">AI Documentation Assistant</p>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={onNewChat}
            className="gap-2"
            data-testid="button-new-chat"
          >
            <Plus className="w-4 h-4" />
            <span className="hidden sm:inline">New Chat</span>
          </Button>
          <LLMProviderToggle />
          <ThemeToggle />
        </div>
      </div>
    </header>
  );
}
