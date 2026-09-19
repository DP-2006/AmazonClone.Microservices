import { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import api from '../api/client';

export default function Basket() {
  const [basket, setBasket] = useState(null);
  const navigate = useNavigate();

  const load = () => api.get('/api/basket').then(r => setBasket(r.data));
  useEffect(() => { load(); }, []);

  const updateQty = async (productId, quantity) => {
    if (quantity < 1) return;
    await api.put(`/api/basket/items/${productId}`, { quantity });
    load();
  };

  const remove = async (productId) => {
    await api.delete(`/api/basket/items/${productId}`);
    load();
  };

  if (!basket) return <div style={{ padding: 40 }}>Loading basket...</div>;

  return (
    <div style={{ maxWidth: 1000, margin: '0 auto', padding: 40, display: 'flex', gap: 24, flexWrap: 'wrap' }}>
      <div style={{ flex: '2 1 500px', background: 'white', padding: 24, borderRadius: 8 }}>
        <h1 style={{ marginBottom: 20 }}>Shopping Basket</h1>

        {basket.items.length === 0 ? (
          <div style={{ textAlign: 'center', padding: 40 }}>
            <p style={{ marginBottom: 16 }}>Your basket is empty.</p>
            <Link to="/" style={{ padding: '10px 24px', background: '#FFD814', borderRadius: 20, textDecoration: 'none', color: 'black', fontWeight: 'bold' }}>
              Continue shopping
            </Link>
          </div>
        ) : (
          basket.items.map(item => (
            <div key={item.productId} style={{
              display: 'flex', alignItems: 'center', gap: 20,
              borderBottom: '1px solid #eee', padding: '16px 0',
            }}>
              <div style={{
                width: 90, height: 90, background: '#f3f3f3', borderRadius: 4,
                display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 40,
              }}>
                📦
              </div>
              <div style={{ flex: 1 }}>
                <h3 style={{ fontSize: 16, marginBottom: 6 }}>{item.productName}</h3>
                <p style={{ color: '#B12704', fontWeight: 'bold' }}>${item.unitPrice}</p>
              </div>
              <select value={item.quantity} onChange={e => updateQty(item.productId, parseInt(e.target.value))}
                style={{ padding: 6, borderRadius: 4, border: '1px solid #888' }}>
                {[1,2,3,4,5,6,7,8,9,10].map(n => <option key={n} value={n}>{n}</option>)}
              </select>
              <button onClick={() => remove(item.productId)} style={{
                background: 'transparent', border: 'none', color: '#007185',
                cursor: 'pointer', fontSize: 14,
              }}>
                Delete
              </button>
            </div>
          ))
        )}
      </div>

      {basket.items.length > 0 && (
        <div style={{ flex: '1 1 260px', background: 'white', padding: 24, borderRadius: 8, height: 'fit-content' }}>
          <h2 style={{ marginBottom: 16 }}>Order Summary</h2>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <span>Items:</span>
            <span>{basket.items.reduce((s, i) => s + i.quantity, 0)}</span>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16, fontSize: 18, fontWeight: 'bold' }}>
            <span>Total:</span>
            <span style={{ color: '#B12704' }}>${basket.total.toFixed(2)}</span>
          </div>
          <button onClick={() => navigate('/checkout')} style={{
            width: '100%', padding: 12, background: '#FFD814', border: '1px solid #FCD200',
            borderRadius: 20, cursor: 'pointer', fontWeight: 'bold', fontSize: 15,
          }}>
            Proceed to Checkout
          </button>
        </div>
      )}
    </div>
  );
}
