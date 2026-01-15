import { Cloud, Server } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
  DropdownMenuLabel,
} from "@/components/ui/dropdown-menu";
import { useLLMProvider } from "@/lib/use-llm-provider";

export function LLMProviderToggle() {
  const { provider, setProvider } = useLLMProvider();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          className="gap-2"
          data-testid="llm-provider-toggle"
        >
          {provider === "groq" ? (
            <>
              <Cloud className="w-4 h-4" />
              <span className="hidden sm:inline">Groq</span>
            </>
          ) : (
            <>
              <Server className="w-4 h-4" />
              <span className="hidden sm:inline">Ollama</span>
            </>
          )}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuLabel>LLM Provider</DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem
          onClick={() => setProvider("groq")}
          className="gap-2"
        >
          <Cloud className="w-4 h-4" />
          <div className="flex flex-col">
            <span>Groq (Cloud)</span>
            <span className="text-xs text-muted-foreground">
              Fast, 70B model
            </span>
          </div>
          {provider === "groq" && (
            <span className="ml-auto text-xs">✓</span>
          )}
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => setProvider("ollama")}
          className="gap-2"
        >
          <Server className="w-4 h-4" />
          <div className="flex flex-col">
            <span>Ollama (Local)</span>
            <span className="text-xs text-muted-foreground">
              Private, self-hosted
            </span>
          </div>
          {provider === "ollama" && (
            <span className="ml-auto text-xs">✓</span>
          )}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
