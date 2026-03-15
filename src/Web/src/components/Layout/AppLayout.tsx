import React from 'react';
import { Bot } from 'lucide-react';
import { LoginButton } from '../Auth/LoginButton';

interface AppLayoutProps {
  children: React.ReactNode;
}

export const AppLayout: React.FC<AppLayoutProps> = ({ children }) => {
  return (
    <div className="min-h-screen bg-slate-900 text-slate-100 flex flex-col">
      <header className="border-b border-slate-800 bg-slate-900/50 backdrop-blur-sm sticky top-0 z-10">
        <div className="max-w-5xl mx-auto px-4 h-16 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="bg-blue-600 p-2 rounded-xl">
              <Bot size={24} className="text-white" />
            </div>
            <div>
              <h1 className="text-lg font-bold tracking-tight">MCP Agent</h1>
              <p className="text-xs text-slate-400 font-medium uppercase tracking-wider">Modern Client</p>
            </div>
          </div>
          <LoginButton />
        </div>
      </header>

      <main className="flex-1 flex flex-col max-w-5xl mx-auto w-full px-4 py-6 overflow-hidden">
        {children}
      </main>

      <footer className="py-4 text-center text-slate-500 text-xs border-t border-slate-800/50">
        &copy; {new Date().getFullYear()} MCP Course &bull; Powered by .NET 10 & React
      </footer>
    </div>
  );
};