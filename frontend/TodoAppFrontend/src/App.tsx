import { useState, useEffect } from 'react';
import { Login } from './components/Login';
import { TodoListView } from './components/TodoListView';
import { api } from './api/client';
import type { User } from './types';
import './App.css';

function App() {
  const [user, setUser] = useState<User | null>(null);
  const [isValidatingUser, setIsValidatingUser] = useState(true);

  useEffect(() => {
    // Validate stored user on mount
    const storedUser = localStorage.getItem('currentUser');
    if (!storedUser) {
      setIsValidatingUser(false);
      return;
    }

    let isMounted = true;

    const validateUser = async () => {
      try {
        const parsedUser = JSON.parse(storedUser) as User;
        // Validate that the user still exists in the backend
        const { data, error } = await api.users.get(parsedUser.userId);

        if (!isMounted) return;

        if (data && !error) {
          // User exists in backend, use it
          setUser(parsedUser);
        } else {
          // User doesn't exist in backend, clear localStorage
          console.log('Cached user no longer exists in backend, clearing cache');
          console.log('Error details:', error);
          localStorage.removeItem('currentUser');
        }
      } catch (error) {
        if (!isMounted) return;
        console.error('Error validating stored user:', error);
        // Clear invalid data from localStorage
        localStorage.removeItem('currentUser');
      } finally {
        if (isMounted) {
          setIsValidatingUser(false);
        }
      }
    };

    validateUser();

    return () => {
      isMounted = false;
    };
  }, []);

  const handleLogin = (loggedInUser: User) => {
    setUser(loggedInUser);
    localStorage.setItem('currentUser', JSON.stringify(loggedInUser));
  };

  const handleLogout = () => {
    setUser(null);
    localStorage.removeItem('currentUser');
  };

  const handleUserInvalidated = () => {
    // Called when API calls fail due to invalid user
    console.log('User invalidated, logging out');
    handleLogout();
  };

  // Show loading spinner while validating user
  if (isValidatingUser) {
    return (
      <div className="app">
        <div
          style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            height: '100vh',
          }}
        >
          <div>Validating user...</div>
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      {!user ? (
        <Login onLogin={handleLogin} />
      ) : (
        <TodoListView
          user={user}
          onLogout={handleLogout}
          onUserInvalidated={handleUserInvalidated}
        />
      )}
    </div>
  );
}

export default App;
