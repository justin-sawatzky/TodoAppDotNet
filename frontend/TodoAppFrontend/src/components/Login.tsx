import { useState } from 'react';
import { api } from '../api/client';
import type { User } from '../types';
import './Login.css';

interface LoginProps {
  onLogin: (user: User) => void;
}

export function Login({ onLogin }: LoginProps) {
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [isNewUser, setIsNewUser] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      if (isNewUser) {
        // Create new user
        const { data, error } = await api.users.create(email, username);
        if (error) {
          setError('Failed to create user. Email may already exist.');
          setLoading(false);
          return;
        }
        if (data) {
          onLogin(data as User);
        }
      } else {
        // Find existing user by email
        const { data, error } = await api.users.list();
        if (error) {
          setError('Failed to fetch users');
          setLoading(false);
          return;
        }
        
        const user = data?.users?.find((u) => u.email === email);
        if (user) {
          onLogin(user as User);
        } else {
          setError('User not found. Please create a new account.');
        }
      }
    } catch (err) {
      setError('An error occurred. Please try again.');
    } finally {
      setLoading(false);
    }
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
                placeholder="Your name"
              />
            </div>
          )}

          {error && <div className="error-message">{error}</div>}

          <button type="submit" disabled={loading} className="btn-primary">
            {loading ? 'Loading...' : isNewUser ? 'Create Account' : 'Login'}
          </button>

          <button
            type="button"
            onClick={() => setIsNewUser(!isNewUser)}
            className="btn-secondary"
          >
            {isNewUser ? 'Already have an account?' : 'Create new account'}
          </button>
        </form>
      </div>
    </div>
  );
}
