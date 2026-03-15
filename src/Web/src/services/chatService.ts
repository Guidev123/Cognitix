import type { ChatRequest, ChatResponse } from "../types/chat";
import { authService } from "./authService";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

class ChatService {
  async sendMessage(message: string, conversationId?: string): Promise<ChatResponse> {
    const token = await authService.getToken();

    if (!token) {
      throw new Error("Not authenticated or token expired. Please login.");
    }

    const request: ChatRequest = {
      message,
      conversationId,
    };

    const response = await fetch(`${API_BASE_URL}/api/chat`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`,
      },
      body: JSON.stringify(request),
    });

    if (response.status === 401) {
      throw new Error("Unauthorized. Your session may have expired.");
    }

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.detail || "Failed to send message to the agent.");
    }

    return await response.json();
  }
}

export const chatService = new ChatService();