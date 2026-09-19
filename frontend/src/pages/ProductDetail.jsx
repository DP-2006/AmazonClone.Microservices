import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import api from '../api/client';
import { useAuth } from '../context/AuthContext';

export default function ProductDetail() {
  const { id } = useParams();
  const [product, setProduct] = useState(null);
  const [qty, setQty] = useState(1);
  const [msg, setMsg] = useState('');
  const [adding, setAdding] = useState(false);
  const { user } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    api.get(`/api/products/${id}`).then(r => setProduct(r.data));
  }, [id]);

  const addToBasket = async () => {
    if (!user) return navigate('/login');
    setAdding(true);
    try {
      await api.post('/api/basket/items', {
        productId: product.id,
        productName: product.name,
        unitPrice: product.price,
        quantity: qty,
      });
      setMsg('✅ Added to basket!');
      setTimeout(() => setMsg(''), 2500);
    } catch {
      setMsg('❌ Failed to add');
    } finally {
      setAdding(false);
    }
  };

  if (!product) return <div style={{ padding: 40 }}>Loading...</div>;

  return (
    <div style={{ maxWidth: 1100, margin: '0 auto', padding: 40, display: 'flex', gap: 40, flexWrap: 'wrap' }}>
      <div style={{
        flex: '1 1 400px', background: '#f3f3f3', height: 400,
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        fontSize: 120, borderRadius: 8,
      }}>
        📦
      </div>

      <div style={{ flex: '1 1 400px', background: 'white', padding: 30, borderRadius: 8 }}>
        <Link to="/" style={{ fontSize: 13 }}>← Back to products</Link>
        <h1 style={{ marginTop: 12, marginBottom: 12 }}>{product.name}</h1>
        <p style={{ color: '#666', lineHeight: 1.6, marginBottom: 20 }}>{product.description}</p>
        <p style={{ fontSize: 32, color: '#B12704', fontWeight: 'bold', marginBottom: 24 }}>
          ${product.price} <span style={{ fontSize: 16, color: '#666' }}>{product.currency}</span>
        </p>

        <div style={{ marginBottom: 20 }}>
          <label style={{ marginRight: 8 }}>Quantity:</label>
          <select value={qty} onChange={e => setQty(parseInt(e.target.value))}
            style={{ padding: 8, borderRadius: 4, border: '1px solid #888' }}>
            {[1,2,3,4,5,6,7,8,9,10].map(n => <option key={n} value={n}>{n}</option>)}
          </select>
        </div>

        <button onClick={addToBasket} disabled={adding} style={{
          padding: '12px 40px', background: '#FFD814', border: '1px solid #FCD200',
          borderRadius: 20, cursor: 'pointer', fontWeight: 'bold', fontSize: 16,
        }}>
          {adding ? 'Adding...' : 'Add to Basket'}
        </button>

        {msg && <p style={{ marginTop: 16, fontSize: 15 }}>{msg}</p>}
      </div>
    </div>
  );
}
