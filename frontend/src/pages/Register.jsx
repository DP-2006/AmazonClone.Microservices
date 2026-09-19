import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Register() {
  const [form, setForm] = useState({ email: '', password: '', firstName: '', lastName: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await register(form.email, form.password, form.firstName, form.lastName);
      navigate('/');
    } catch (err) {
      const d = err.response?.data;
      setError(typeof d === 'string' ? d : (d?.title || 'Registration failed'));
    } finally {
      setLoading(false);
    }
  };

  const change = (k) => (e) => setForm({ ...form, [k]: e.target.value });
  const inputStyle = { width: '100%', padding: 10, marginBottom: 16, border: '1px solid #888', borderRadius: 4 };

  return (
    <div style={{ maxWidth: 400, margin: '60px auto', padding: 30, background: 'white', border: '1px solid #ddd', borderRadius: 8 }}>
      <h1 style={{ textAlign: 'center', marginBottom: 24 }}>Create account</h1>
      <form onSubmit={submit}>
        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: 4 }}>First name</label>
        <input value={form.firstName} onChange={change('firstName')} required style={inputStyle} />

        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: 4 }}>Last name</label>
        <input value={form.lastName} onChange={change('lastName')} required style={inputStyle} />

        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: 4 }}>Email</label>
        <input type="email" value={form.email} onChange={change('email')} required style={inputStyle} />

        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: 4 }}>Password (min 6 chars)</label>
        <input type="password" value={form.password} onChange={change('password')} required minLength={6} style={inputStyle} />

        {error && <p style={{ color: '#B12704', marginBottom: 12, fontSize: 13 }}>{String(error)}</p>}

        <button type="submit" disabled={loading} style={{
          width: '100%', padding: 12, background: '#FFD814', border: '1px solid #FCD200',
          borderRadius: 20, cursor: 'pointer', fontWeight: 'bold', fontSize: 14,
        }}>
          {loading ? 'Creating...' : 'Create account'}
        </button>
      </form>

      <p style={{ textAlign: 'center', marginTop: 20, fontSize: 13, color: '#666' }}>
        Already have an account? <Link to="/login">Sign in</Link>
      </p>
    </div>
  );
}
