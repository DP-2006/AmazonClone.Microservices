import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Login() {
  const [email, setEmail] = useState('test@test.com');
  const [password, setPassword] = useState('Test123');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(email, password);
      navigate('/');
    } catch (err) {
      const d = err.response?.data;
      setError(typeof d === 'string' ? d : (d?.title || 'Login failed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: 400, margin: '60px auto', padding: 30, background: 'white', border: '1px solid #ddd', borderRadius: 8 }}>
      <h1 style={{ textAlign: 'center', marginBottom: 24 }}>Sign in</h1>
      <form onSubmit={submit}>
        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: 4 }}>Email</label>
        <input type="email" value={email} onChange={e => setEmail(e.target.value)} required
          style={{ width: '100%', padding: 10, marginBottom: 16, border: '1px solid #888', borderRadius: 4 }} />

        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: 4 }}>Password</label>
        <input type="password" value={password} onChange={e => setPassword(e.target.value)} required
          style={{ width: '100%', padding: 10, marginBottom: 16, border: '1px solid #888', borderRadius: 4 }} />

        {error && <p style={{ color: '#B12704', marginBottom: 12 }}>{String(error)}</p>}

        <button type="submit" disabled={loading} style={{
          width: '100%', padding: 12, background: '#FFD814', border: '1px solid #FCD200',
          borderRadius: 20, cursor: 'pointer', fontWeight: 'bold', fontSize: 14,
        }}>
          {loading ? 'Signing in...' : 'Sign in'}
        </button>
      </form>

      <p style={{ textAlign: 'center', marginTop: 20, fontSize: 13, color: '#666' }}>
        New to AmazonClone? <Link to="/register">Create your account</Link>
      </p>
    </div>
  );
}
