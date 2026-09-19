import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const doLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <nav style={{
      background: '#131921', color: 'white', padding: '10px 20px',
      display: 'flex', alignItems: 'center', gap: 20, flexWrap: 'wrap',
    }}>
      <Link to="/" style={{ color: 'white', textDecoration: 'none', fontSize: 22, fontWeight: 'bold' }}>
        🛒 AmazonClone
      </Link>
      <Link to="/" style={{ color: 'white', textDecoration: 'none' }}>Home</Link>

      {user && (
        <>
          <Link to="/basket" style={{ color: 'white', textDecoration: 'none' }}>🛍️ Basket</Link>
          <Link to="/orders" style={{ color: 'white', textDecoration: 'none' }}>📦 Orders</Link>
        </>
      )}

      <div style={{ marginLeft: 'auto', display: 'flex', gap: 16, alignItems: 'center' }}>
        {user ? (
          <>
            <Link to="/profile" style={{ color: 'white', textDecoration: 'none' }}>
              👤 {user.fullName}
            </Link>
            <button onClick={doLogout} style={{
              padding: '6px 14px', cursor: 'pointer', borderRadius: 4,
              border: '1px solid #555', background: 'transparent', color: 'white',
            }}>
              Logout
            </button>
          </>
        ) : (
          <>
            <Link to="/login" style={{ color: 'white', textDecoration: 'none' }}>Sign in</Link>
            <Link to="/register" style={{ color: 'white', textDecoration: 'none' }}>Register</Link>
          </>
        )}
      </div>
    </nav>
  );
}
