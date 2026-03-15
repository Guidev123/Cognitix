import { useState, useEffect } from 'react';
import { useMsal, useIsAuthenticated } from '@azure/msal-react';
import { loginRequest } from '../config/msalConfig';
import type { UserProfile } from '../types/auth';

export const useAuth = () => {
  const { instance, accounts } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const [user, setUser] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (isAuthenticated && accounts.length > 0) {
      const account = accounts[0];
      setUser({
        name: account.name,
        username: account.username,
        localAccountId: account.localAccountId,
      });
    } else {
      setUser(null);
    }
    setIsLoading(false);
  }, [isAuthenticated, accounts]);

  const login = async () => {
    try {
      await instance.loginPopup(loginRequest);
    } catch (error) {
      console.error('Login failed:', error);
    }
  };

  const logout = async () => {
    try {
      await instance.logoutPopup();
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  return {
    isAuthenticated,
    user,
    isLoading,
    login,
    logout,
  };
};