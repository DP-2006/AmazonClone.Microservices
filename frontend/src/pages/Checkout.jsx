import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/client';

export default function Checkout() {
  const [basket, setBasket] = useState(null);
  const [address, setAddress] = useState({
    street: '', city: '', state: '', postalCode: '', country: 'Iran'
  });
  const [msg, setMsg] = useState('');
  const [placing, setPlacing] = useState(false);
  const navigate = useNavigate();

  useEffect(() => { api.get('/api/basket').then(r => setBasket(r.data)); }, []);

  const placeOrder = async () => {
    if (!address.street || !address.city || !address.country) {
      setMsg('❌ Please fill required address fields');
      return;
    }
    setPlacing(true);
    try {
      const items = basket.items.map(i => ({
        productId: i.productId,
        variantId: null,
        productName: i.productName,
        unitPrice: i.unitPrice,
        quantity: i.quantity,
      }));

      const orderRes = await api.post('/api/orders', { items, address, currency: 'USD' });
      await api.post('/api/payments', { orderId: orderRes.data.id, provider: 'MockPay' });
      await api.delete('/api/basket');

      setMsg('✅ Order placed successfully! Redirecting...');
      setTimeout(() => navigate('/orders'), 1500);
    } catch (e) {
      setMsg('❌ ' + (e.response?.data?.title || 'Failed to place order'));
    } finally {
      setPlacing(false);
    }
  };

  if (!basket) return <div style={{ padding: 40 }}>Loading...</div>;

  if (basket.items.length === 0) {
    return (
      <div style={{ padding: 40, textAlign: 'center' }}>
        <h2>Your basket is empty</h2>
        <button onClick={() => navigate('/')} style={{
          marginTop: 16, padding: '10px 24px', background: '#FFD814',
          border: '1px solid #FCD200', borderRadius: 20, cursor: 'pointer', fontWeight: 'bold',
        }}>
          Go shopping
        </button>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: 700, margin: '0 auto', padding: 40 }}>
      <div style={{ background: 'white', padding: 30, borderRadius: 8, marginBottom: 20 }}>
        <h1 style={{ marginBottom: 24 }}>Checkout</h1>
        <h3 style={{ marginBottom: 16 }}>Shipping Address</h3>
        {[
          { key: 'street', label: 'Street Address', required: true },
          { key: 'city', label: 'City', required: true },
          { key: 'state', label: 'State/Province', required: false },
          { key: 'postalCode', label: 'Postal Code', required: false },
          { key: 'country', label: 'Country', required: true },
        ].map(({ key, label, required }) => (
          <div key={key} style={{ marginBottom: 16 }}>
            <label style={{ display: 'block', fontWeight: 'bold', marginBottom: 6 }}>
              {label} {required && <span style={{ color: 'red' }}>*</span>}
            </label>
            <input value={address[key]} onChange={e => setAddress({ ...address, [key]: e.target.value })}
              style={{ width: '100%', padding: 10, border: '1px solid #888', borderRadius: 4 }} />
          </div>
        ))}
      </div>

      <div style={{ background: 'white', padding: 30, borderRadius: 8 }}>
        <h3 style={{ marginBottom: 16 }}>Order Summary</h3>
        {basket.items.map(i => (
          <div key={i.productId} style={{ display: 'flex', justifyContent: 'space-between', padding: '8px 0', borderBottom: '1px solid #f0f0f0' }}>
            <span>{i.productName} × {i.quantity}</span>
            <span>${(i.unitPrice * i.quantity).toFixed(2)}</span>
          </div>
        ))}
        <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 16, fontSize: 20, fontWeight: 'bold' }}>
          <span>Total:</span>
          <span style={{ color: '#B12704' }}>${basket.total.toFixed(2)}</span>
        </div>
        <button onClick={placeOrder} disabled={placing} style={{
          width: '100%', padding: 14, background: '#FFD814', border: '1px solid #FCD200',
          borderRadius: 20, cursor: 'pointer', fontWeight: 'bold', fontSize: 16, marginTop: 20,
        }}>
          {placing ? 'Placing order...' : 'Place Order'}
        </button>
        {msg && <p style={{ marginTop: 16, textAlign: 'center' }}>{msg}</p>}
      </div>
    </div>
  );
}
