import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api/client';

export default function Orders() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);

  const load = () => api.get('/api/orders').then(r => setOrders(r.data));

  useEffect(() => {
    load().finally(() => setLoading(false));
  }, []);

  const cancel = async (id) => {
    if (!confirm('Cancel this order?')) return;
    await api.post(`/api/orders/${id}/cancel`);
    load();
  };

  if (loading) return <div style={{ padding: 40 }}>Loading orders...</div>;

  const statusColor = (s) => {
    if (s === 'Cancelled') return { bg: '#fee', color: '#900' };
    if (s === 'Delivered') return { bg: '#efe', color: '#070' };
    if (s === 'Paid') return { bg: '#e6f2ff', color: '#036' };
    return { bg: '#fffbe6', color: '#960' };
  };

  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: 40 }}>
      <h1 style={{ marginBottom: 24 }}>My Orders</h1>
      {orders.length === 0 ? (
        <div style={{ background: 'white', padding: 40, borderRadius: 8, textAlign: 'center' }}>
          <p style={{ marginBottom: 16 }}>No orders yet.</p>
          <Link to="/" style={{
            padding: '10px 24px', background: '#FFD814', borderRadius: 20,
            textDecoration: 'none', color: 'black', fontWeight: 'bold',
          }}>
            Start shopping
          </Link>
        </div>
      ) : orders.map(o => {
        const colors = statusColor(o.status);
        return (
          <div key={o.id} style={{ background: 'white', padding: 24, borderRadius: 8, marginBottom: 16 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 16 }}>
              <div>
                <b>Order #{o.orderNumber}</b>
                <p style={{ color: '#666', fontSize: 13, marginTop: 4 }}>
                  {new Date(o.createdAt).toLocaleString()}
                </p>
              </div>
              <span style={{
                padding: '6px 14px', background: colors.bg, color: colors.color,
                borderRadius: 20, fontSize: 13, fontWeight: 'bold',
              }}>
                {o.status}
              </span>
            </div>
            {o.items.map(i => (
              <div key={i.productId} style={{ display: 'flex', justifyContent: 'space-between', padding: '6px 0' }}>
                <span>{i.productName} × {i.quantity}</span>
                <span>${(i.unitPrice * i.quantity).toFixed(2)}</span>
              </div>
            ))}
            <div style={{ textAlign: 'right', marginTop: 12, borderTop: '1px solid #eee', paddingTop: 12 }}>
              <b>Total: ${o.totalAmount.toFixed(2)}</b>
            </div>
            {o.status !== 'Cancelled' && o.status !== 'Delivered' && (
              <button onClick={() => cancel(o.id)} style={{
                marginTop: 12, background: 'transparent', border: '1px solid #ddd',
                padding: '6px 12px', cursor: 'pointer', borderRadius: 4,
              }}>
                Cancel Order
              </button>
            )}
          </div>
        );
      })}
    </div>
  );
}
