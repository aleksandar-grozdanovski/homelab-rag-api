import { useState, useRef, useEffect } from "react";
import { ChatHeader } from "@/components/chat-header";
import { ChatInput } from "@/components/chat-input";
import { EmptyState } from "@/components/empty-state";
import { MessageBubble } from "@/components/message-bubble";
import { TypingIndicator } from "@/components/typing-indicator";
import { ConversationStorage } from "@/lib/storage";
import { API_ENDPOINTS } from "@/lib/api-config";
import type { Message } from "@/lib/storage";

export default function ChatPage() {
  const [conversationId, setConversationId] = useState<string | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  useEffect(() => {
    // Load conversation on mount or when ID changes
    if (conversationId) {
      const conversation = ConversationStorage.getConversation(conversationId);
      if (conversation) {
        setMessages(conversation.messages);
      }
    } else {
      setMessages([]);
    }
  }, [conversationId]);

  const handleSend = async (content: string) => {
    setIsLoading(true);
    
    try {
      // Create conversation if needed
      let activeConversationId = conversationId;
      if (!activeConversationId) {
        const newConversation = ConversationStorage.createConversation();
        activeConversationId = newConversation.id;
        setConversationId(activeConversationId);
      }

      // Add user message
      const userMessage = ConversationStorage.addMessage(activeConversationId, {
        role: "user",
        content,
      });
      
      setMessages(prev => [...prev, userMessage]);

      // Call .NET RAG API
      const response = await fetch(API_ENDPOINTS.query, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          question: content,
          topK: 5,
        }),
      });

      if (!response.ok) {
        throw new Error(`API error: ${response.status}`);
      }

      const data = await response.json();
      
      // Add assistant message with sources
      const assistantMessage = ConversationStorage.addMessage(activeConversationId, {
        role: "assistant",
        content: data.answer,
        sources: data.sources,
      });
      
      setMessages(prev => [...prev, assistantMessage]);
    } catch (error) {
      console.error("Failed to send message:", error);
      
      // Add error message
      if (conversationId) {
        const errorMessage = ConversationStorage.addMessage(conversationId, {
          role: "assistant",
          content: "Sorry, I encountered an error processing your request. Please make sure the RAG API is running.",
        });
        setMessages(prev => [...prev, errorMessage]);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleNewChat = () => {
    setConversationId(null);
    setMessages([]);
  };

  const handleSuggestionClick = (suggestion: string) => {
    handleSend(suggestion);
  };

  const showEmptyState = messages.length === 0;

  return (
    <div className="flex flex-col h-screen bg-background">
      <ChatHeader onNewChat={handleNewChat} />

      <main className="flex-1 overflow-y-auto">
        <div className="max-w-4xl mx-auto px-4 py-8">
          {showEmptyState ? (
            <EmptyState onSuggestionClick={handleSuggestionClick} />
          ) : (
            <div className="flex flex-col gap-6">
              {messages.map((message) => (
                <MessageBubble
                  key={message.id}
                  role={message.role}
                  content={message.content}
                  timestamp={message.timestamp}
                  sources={message.sources}
                />
              ))}

              {isLoading && (
                <div className="flex gap-3">
                  <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0">
                    <div className="w-4 h-4 rounded-full bg-primary animate-pulse" />
                  </div>
                  <div className="bg-card border border-border rounded-2xl shadow-sm">
                    <TypingIndicator />
                  </div>
                </div>
              )}

              <div ref={messagesEndRef} />
            </div>
          )}
        </div>
      </main>

      <footer className="sticky bottom-0 bg-background/80 backdrop-blur-lg border-t border-border">
        <div className="max-w-4xl mx-auto px-4 py-4">
          <ChatInput
            onSend={handleSend}
            disabled={isLoading}
          />
        </div>
      </footer>
    </div>
  );
}
