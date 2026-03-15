import React from 'react';
import { LogIn, LogOut, User } from 'lucide-react';
import { useAuth } from '../../hooks/useAuth';

export const LoginButton: React.FC = () => {
  const { isAuthenticated, user, login, logout, isLoading } = useAuth();

  if (isLoading) {
    return <div className="animate-pulse bg-slate-800 h-10 w-32 rounded-lg" />;
  }

  if (isAuthenticated && user) {
    return (
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-2 bg-slate-800 px-3 py-1.5 rounded-full border border-slate-700">
          <User size={16} className="text-blue-400" />
          <span className="text-sm font-medium text-slate-200">{user.name || user.username}</span>
        </div>
        <button
          onClick={logout}
          className="flex items-center gap-2 text-slate-400 hover:text-white transition-colors duration-200"
          title="Logout"
        >
          <LogOut size={20} />
        </button>
      </div>
    );
  }

  return (
    <button
      onClick={login}
      className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium transition-all duration-200 shadow-lg shadow-blue-500/20"
    >
      <LogIn size={20} />
      <span>Sign in with Entra ID</span>
    </button>
  );
};