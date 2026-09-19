import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';

export default function Profile() {
  const { user } = useAuth();

  return (
    <div style={{ maxWidth: 700, margin: '0 auto', padding: 40 }}>
      <div style={{ background: 'white', padding: 30, borderRadius: 8 }}>
        <h1 style={{ marginBottom: 24 }}>My Account</h1>

        <div style={{ display: 'flex', alignItems: 'center', gap: 20, marginBottom: 30 }}>
          <div style={{
            width: 80, height: 80, borderRadius: '50%', background: '#232f3e',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: 'white', fontSize: 36, fontWeight: 'bold',
          }}>
            {user?.fullName?.[0]?.toUpperCase() || '?'}
          </div>
          <div>
            <h2 style={{ marginBottom: 4 }}>{user?.fullName}</h2>
            <p style={{ color: '#666' }}>{user?.email}</p>
          </div>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 16 }}>
          <Link to="/orders" style={{
            padding: 20, background: '#f8f8f8', borderRadius: 8,
            textDecoration: 'none', color: '#111', display: 'block',
          }}>
            <div style={{ fontSize: 32, marginBottom: 8 }}>📦</div>
            <b>My Orders</b>
            <p style={{ fontSize: 13, color: '#666', marginTop: 4 }}>Track and manage orders</p>
          </Link>

          <Link to="/basket" style={{
            padding: 20, background: '#f8f8f8', borderRadius: 8,
            textDecoration: 'none', color: '#111', display: 'block',
          }}>
            <div style={{ fontSize: 32, marginBottom: 8 }}>🛍️</div>
            <b>My Basket</b>
            <p style={{ fontSize: 13, color: '#666', marginTop: 4 }}>Items waiting to checkout</p>
          </Link>

          <Link to="/" style={{
            padding: 20, background: '#f8f8f8', borderRadius: 8,
            textDecoration: 'none', color: '#111', display: 'block',
          }}>
            <div style={{ fontSize: 32, marginBottom: 8 }}>🏠</div>
            <b>Continue Shopping</b>
            <p style={{ fontSize: 13, color: '#666', marginTop: 4 }}>Browse products</p>
          </Link>
        </div>

        {user?.expiresAt && (
          <p style={{ marginTop: 30, fontSize: 12, color: '#999' }}>
            Session expires: {new Date(user.expiresAt).toLocaleString()}
          </p>
        )}
      </div>
    </div>
  );
}
