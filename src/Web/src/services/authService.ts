import { type PublicClientApplication, InteractionRequiredAuthError } from "@azure/msal-browser";
import { loginRequest } from "../config/msalConfig";

class AuthService {
  private msalInstance: PublicClientApplication | null = null;

  setInstance(instance: PublicClientApplication) {
    this.msalInstance = instance;
  }

  private getInstance(): PublicClientApplication {
    if (!this.msalInstance) throw new Error("MSAL instance not set. Call setInstance first.");
    return this.msalInstance;
  }

  async getToken(): Promise<string | null> {
    const msal = this.getInstance();
    const accounts = msal.getAllAccounts();
    if (accounts.length === 0) return null;

    try {
      const result = await msal.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });
      return result.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) return null;
      console.error("Token acquisition failed:", error);
      return null;
    }
  }

  getAccount() {
    return this.getInstance().getAllAccounts()[0] ?? null;
  }
}

export const authService = new AuthService();