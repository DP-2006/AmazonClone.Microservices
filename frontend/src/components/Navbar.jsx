import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import NotificationBell from './NotificationBell';

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  const linkStyle = {
    color: 'white',
    textDecoration: 'none',
    padding: '8px 12px',
    fontSize: 14,
    display: 'flex',
    alignItems: 'center',
    gap: 4,
  };

  return (
    <nav style={{
      background: '#131921',
      color: 'white',
      padding: '0 20px',
      display: 'flex',
      alignItems: 'center',
      gap: 8,
      height: 60,
      position: 'sticky',
      top: 0,
      zIndex: 100,
    }}>
      {/* Logo */}
      <Link to="/" style={{
        color: 'white', textDecoration: 'none', fontSize: 22,
        fontWeight: 'bold', padding: '0 12px',
      }}>🛒</Link>

      {/* Search bar (اختیاری) */}
      <Link to="/" style={linkStyle}>خانه</Link>

      {user && (
        <>
          <Link to="/orders" style={linkStyle}>📦 سفارش‌ها</Link>
          <Link to="/chat" style={linkStyle}>💬 چت</Link>
          <Link to="/storage" style={linkStyle}>📁 فضای ابری</Link>
        </>
      )}

      {/* Spacer */}
      <div style={{ flex: 1 }} />

      {/* Right side */}
      {user ? (
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <NotificationBell />

          <Link to="/basket" style={linkStyle}>🛍️</Link>

          <div style={{ position: 'relative', display: 'flex', alignItems: 'center', gap: 8, padding: '0 8px' }}>
            <Link to="/profile" style={{ ...linkStyle, padding: 0 }}>
              <div style={{
                width: 32, height: 32, borderRadius: '50%',
                background: '#FFD814', color: '#111',
                display: 'flex', alignItems: 'center',
                justifyContent: 'center', fontWeight: 'bold',
              }}>
                {(user.fullName || '?')[0].toUpperCase()}
              </div>
            </Link>
            <button onClick={handleLogout} style={{
              background: 'none', border: '1px solid #666',
              color: 'white', padding: '4px 10px', borderRadius: 4,
              cursor: 'pointer', fontSize: 12,
            }}>خروج</button>
          </div>
        </div>
      ) : (
        <div style={{ display: 'flex', gap: 8 }}>
          <Link to="/login" style={linkStyle}>ورود</Link>
          <Link to="/register" style={{
            ...linkStyle, background: '#FFD814', color: '#111',
            borderRadius: 20, fontWeight: 'bold',
          }}>ثبت‌نام</Link>
        </div>
      )}
    </nav>
  );
}
