/**
 * Client-side storage for conversations and messages
 * Uses localStorage for persistence
 */

export interface Message {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
  sources?: Array<{
    fileName: string;
    chunkIndex: number;
    preview: string;
  }>;
}

export interface Conversation {
  id: string;
  title: string;
  messages: Message[];
  createdAt: Date;
  updatedAt: Date;
}

const STORAGE_KEY = 'homelab-rag-conversations';

export class ConversationStorage {
  private static getConversations(): Conversation[] {
    try {
      const data = localStorage.getItem(STORAGE_KEY);
      if (!data) return [];
      
      const conversations = JSON.parse(data);
      // Convert date strings back to Date objects
      return conversations.map((conv: any) => ({
        ...conv,
        createdAt: new Date(conv.createdAt),
        updatedAt: new Date(conv.updatedAt),
        messages: conv.messages.map((msg: any) => ({
          ...msg,
          timestamp: new Date(msg.timestamp),
        })),
      }));
    } catch (error) {
      console.error('Failed to load conversations:', error);
      return [];
    }
  }

  private static saveConversations(conversations: Conversation[]): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(conversations));
    } catch (error) {
      console.error('Failed to save conversations:', error);
    }
  }

  static createConversation(title: string = 'New Chat'): Conversation {
    const conversation: Conversation = {
      id: Date.now().toString(),
      title,
      messages: [],
      createdAt: new Date(),
      updatedAt: new Date(),
    };

    const conversations = this.getConversations();
    conversations.unshift(conversation);
    this.saveConversations(conversations);

    return conversation;
  }

  static getConversation(id: string): Conversation | null {
    const conversations = this.getConversations();
    return conversations.find(c => c.id === id) || null;
  }

  static getAllConversations(): Conversation[] {
    return this.getConversations();
  }

  static addMessage(conversationId: string, message: Omit<Message, 'id' | 'timestamp'>): Message {
    const conversations = this.getConversations();
    const conversation = conversations.find(c => c.id === conversationId);
    
    if (!conversation) {
      throw new Error('Conversation not found');
    }

    const newMessage: Message = {
      ...message,
      id: Date.now().toString(),
      timestamp: new Date(),
    };

    conversation.messages.push(newMessage);
    conversation.updatedAt = new Date();
    
    // Update title based on first user message
    if (conversation.messages.length === 1 && message.role === 'user') {
      conversation.title = message.content.slice(0, 50) + (message.content.length > 50 ? '...' : '');
    }

    this.saveConversations(conversations);
    return newMessage;
  }

  static deleteConversation(id: string): void {
    const conversations = this.getConversations().filter(c => c.id !== id);
    this.saveConversations(conversations);
  }

  static clearAll(): void {
    localStorage.removeItem(STORAGE_KEY);
  }
}
