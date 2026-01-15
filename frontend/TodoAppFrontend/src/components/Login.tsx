import { useState } from 'react';
import { api } from '../api/client';
import type { User } from '../types';
import { useApiError } from '../hooks/useApiError';
import { ErrorDisplay } from './ErrorDisplay';
import './Login.css';

interface LoginProps {
  onLogin: (user: User) => void;
}

export function Login({ onLogin }: LoginProps) {
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [isNewUser, setIsNewUser] = useState(false);
  const [loading, setLoading] = useState(false);
  const { error, handleApiCall, handleError, clearError } = useApiError();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    if (isNewUser) {
      // Create new user
      const { data, error: apiError } = await handleApiCall(
        () => api.users.create(email, username),
        'create user'
      );
      
      if (apiError) {
        setLoading(false);
        return;
      }
      
      if (data) {
        onLogin(data as User);
      }
    } else {
      // Find existing user by email
      const { data, error: apiError } = await handleApiCall(
        () => api.users.list(),
        'fetch users'
      );
      
      if (apiError) {
        setLoading(false);
        return;
      }
      
      const user = data?.users?.find((u) => u.email === email);
      if (user) {
        onLogin(user as User);
      } else {
        // User not found - create a helpful error message
        handleError({
          message: `No account found with email ${email}. Please create a new account or check your email address.`
        }, 'find user');
      }
    }
    
    setLoading(false);
  };

  const handleToggleMode = () => {
    setIsNewUser(!isNewUser);
    clearError(); // Clear any existing errors when switching modes
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h1>Todo App</h1>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              placeholder="your@email.com"
            />
          </div>

          {isNewUser && (
            <div className="form-group">
              <label htmlFor="username">Username</label>
              <input
                id="username"
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                placeholder="Your username"
              />
            </div>
          )}

          <ErrorDisplay error={error} onDismiss={clearError} />

          <button type="submit" disabled={loading} className="btn-primary">
            {loading ? 'Loading...' : isNewUser ? 'Create Account' : 'Login'}
          </button>

          <button
            type="button"
            onClick={handleToggleMode}
            className="btn-secondary"
          >
            {isNewUser ? 'Already have an account?' : 'Create new account'}
          </button>
        </form>
      </div>
    </div>
  );
}
