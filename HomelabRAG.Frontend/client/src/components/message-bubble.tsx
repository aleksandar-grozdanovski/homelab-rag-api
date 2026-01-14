import { Bot, User, Copy, Check, FileText } from "lucide-react";
import { useState } from "react";
import { cn } from "@/lib/utils";

interface Source {
  fileName: string;
  chunkIndex: number;
  preview: string;
}

interface MessageBubbleProps {
  role: "user" | "assistant";
  content: string;
  timestamp?: Date;
  isStreaming?: boolean;
  sources?: Source[];
}

export function MessageBubble({ role, content, timestamp, isStreaming, sources }: MessageBubbleProps) {
  const isUser = role === "user";
  const [copied, setCopied] = useState(false);
  const [showSources, setShowSources] = useState(false);

  const handleCopy = async () => {
    await navigator.clipboard.writeText(content);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const formatTime = (date: Date) => {
    return date.toLocaleTimeString("en-US", {
      hour: "numeric",
      minute: "2-digit",
      hour12: true,
    });
  };

  return (
    <div
      className={cn(
        "flex gap-3 w-full",
        isUser ? "justify-end" : "justify-start"
      )}
      data-testid={`message-${role}`}
    >
      {!isUser && (
        <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0 mt-1">
          <Bot className="w-4 h-4 text-primary" />
        </div>
      )}

      <div
        className={cn(
          "relative group max-w-2xl rounded-2xl shadow-sm",
          isUser
            ? "bg-primary text-primary-foreground px-4 py-3"
            : "bg-card border border-border px-5 py-4"
        )}
      >
        <div
          className={cn(
            "prose prose-sm max-w-none",
            isUser ? "prose-invert" : "dark:prose-invert"
          )}
        >
          <MessageContent content={content} isUser={isUser} />
        </div>

        {!isUser && !isStreaming && (
          <button
            onClick={handleCopy}
            className="absolute top-3 right-3 p-1.5 rounded-md opacity-0 group-hover:opacity-100 hover-elevate transition-opacity"
            data-testid="button-copy-message"
          >
            {copied ? (
              <Check className="w-4 h-4 text-green-500" />
            ) : (
              <Copy className="w-4 h-4 text-muted-foreground" />
            )}
          </button>
        )}

        {timestamp && (
          <div
            className={cn(
              "text-xs mt-2",
              isUser ? "text-primary-foreground/70" : "text-muted-foreground"
            )}
          >
            {formatTime(timestamp)}
          </div>
        )}

        {/* Sources section */}
        {!isUser && sources && sources.length > 0 && (
          <div className="mt-3 pt-3 border-t border-border">
            <button
              onClick={() => setShowSources(!showSources)}
              className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              <FileText className="w-4 h-4" />
              <span>{sources.length} source{sources.length > 1 ? 's' : ''}</span>
            </button>
            
            {showSources && (
              <div className="mt-2 space-y-2">
                {sources.map((source, idx) => (
                  <div
                    key={idx}
                    className="text-xs p-2 rounded bg-muted/50 border border-border/50"
                  >
                    <div className="font-medium text-foreground mb-1">
                      {source.fileName} (chunk {source.chunkIndex})
                    </div>
                    <div className="text-muted-foreground line-clamp-2">
                      {source.preview}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      {isUser && (
        <div className="w-8 h-8 rounded-full bg-muted flex items-center justify-center shrink-0 mt-1">
          <User className="w-4 h-4 text-muted-foreground" />
        </div>
      )}
    </div>
  );
}

function MessageContent({ content, isUser }: { content: string; isUser: boolean }) {
  const processContent = (text: string) => {
    const parts: Array<{ type: "text" | "code"; content: string; language?: string }> = [];
    const codeBlockRegex = /```(\w+)?\n([\s\S]*?)```/g;
    let lastIndex = 0;
    let match;

    while ((match = codeBlockRegex.exec(text)) !== null) {
      if (match.index > lastIndex) {
        parts.push({ type: "text", content: text.slice(lastIndex, match.index) });
      }
      parts.push({ type: "code", content: match[2], language: match[1] || "plaintext" });
      lastIndex = match.index + match[0].length;
    }

    if (lastIndex < text.length) {
      parts.push({ type: "text", content: text.slice(lastIndex) });
    }

    return parts.length > 0 ? parts : [{ type: "text" as const, content: text }];
  };

  const processInlineCode = (text: string) => {
    const inlineCodeRegex = /`([^`]+)`/g;
    const parts: Array<{ type: "inline-code" | "text"; content: string }> = [];
    let lastIndex = 0;
    let match;

    while ((match = inlineCodeRegex.exec(text)) !== null) {
      if (match.index > lastIndex) {
        parts.push({ type: "text", content: text.slice(lastIndex, match.index) });
      }
      parts.push({ type: "inline-code", content: match[1] });
      lastIndex = match.index + match[0].length;
    }

    if (lastIndex < text.length) {
      parts.push({ type: "text", content: text.slice(lastIndex) });
    }

    return parts.length > 0 ? parts : [{ type: "text" as const, content: text }];
  };

  const renderText = (text: string) => {
    return text.split("\n").map((line, i) => {
      const inlineParts = processInlineCode(line);
      return (
        <span key={i}>
          {inlineParts.map((part, j) =>
            part.type === "inline-code" ? (
              <code
                key={j}
                className={cn(
                  "px-1.5 py-0.5 rounded font-mono text-sm",
                  isUser ? "bg-primary-foreground/20" : "bg-muted"
                )}
              >
                {part.content}
              </code>
            ) : (
              <span key={j}>{part.content}</span>
            )
          )}
          {i < text.split("\n").length - 1 && <br />}
        </span>
      );
    });
  };

  const parts = processContent(content);

  return (
    <>
      {parts.map((part, i) =>
        part.type === "code" ? (
          <CodeBlock key={i} code={part.content} language={part.language} />
        ) : (
          <p key={i} className="leading-relaxed">
            {renderText(part.content)}
          </p>
        )
      )}
    </>
  );
}

function CodeBlock({ code, language }: { code: string; language?: string }) {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    await navigator.clipboard.writeText(code);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="relative group my-3 rounded-lg bg-slate-900 dark:bg-slate-950 overflow-hidden">
      <div className="flex items-center justify-between px-4 py-2 bg-slate-800 dark:bg-slate-900 border-b border-slate-700">
        <span className="text-xs text-slate-400 font-mono">{language}</span>
        <button
          onClick={handleCopy}
          className="p-1 rounded hover:bg-slate-700 transition-colors"
          data-testid="button-copy-code"
        >
          {copied ? (
            <Check className="w-4 h-4 text-green-400" />
          ) : (
            <Copy className="w-4 h-4 text-slate-400" />
          )}
        </button>
      </div>
      <pre className="p-4 overflow-x-auto text-sm">
        <code className="text-slate-200 font-mono">{code.trim()}</code>
      </pre>
    </div>
  );
}
