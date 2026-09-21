import { useEffect, useState } from 'react';
import api, { productsApi } from '../api/client';

export default function SellerProducts() {
  const [items, setItems] = useState([]);
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({ categoryId: '', name: '', slug: '', description: '', price: 0, currency: 'USD' });
  const [msg, setMsg] = useState('');

  const load = async () => {
    const [p, c] = await Promise.all([
      productsApi.myProducts(),
      api.get('/api/categories'),
    ]);
    setItems(p.data);
    setCategories(c.data);
  };

  useEffect(() => { load(); }, []);

  const submit = async (e) => {
    e.preventDefault();
    try {
      await productsApi.create(form);
      setForm({ categoryId: '', name: '', slug: '', description: '', price: 0, currency: 'USD' });
      setMsg('✅ محصول اضافه شد');
      load();
      setTimeout(() => setMsg(''), 2500);
    } catch (err) { setMsg('❌ ' + JSON.stringify(err.response?.data)); }
  };

  const remove = async (id) => {
    if (!confirm('حذف شود؟')) return;
    await productsApi.remove(id);
    load();
  };

  const inputStyle = { width: '100%', padding: 10, border: '1px solid #ccc', borderRadius: 4, marginBottom: 10 };

  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: 40 }}>
      <h1 style={{ marginBottom: 20 }}>محصولات من</h1>

      {msg && <p style={{ marginBottom: 16, padding: 10, background: '#f0f7ff', borderRadius: 4 }}>{msg}</p>}

      <form onSubmit={submit} style={{ background: 'white', padding: 20, borderRadius: 8, marginBottom: 30 }}>
        <h3 style={{ marginBottom: 16 }}>افزودن محصول جدید</h3>
        <input placeholder="نام محصول" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} style={inputStyle} required />
        <input placeholder="slug (فقط حروف کوچک و خط تیره)" value={form.slug} onChange={e => setForm({ ...form, slug: e.target.value })} style={inputStyle} required />
        <select value={form.categoryId} onChange={e => setForm({ ...form, categoryId: e.target.value })} style={inputStyle} required>
          <option value="">انتخاب دسته</option>
          {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <textarea placeholder="توضیحات" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} rows={3} style={inputStyle} />
        <input type="number" placeholder="قیمت" value={form.price} onChange={e => setForm({ ...form, price: parseFloat(e.target.value) || 0 })} style={inputStyle} required />
        <button type="submit" style={{ padding: '10px 24px', background: '#FFD814', border: '1px solid #FCD200', borderRadius: 20, cursor: 'pointer', fontWeight: 'bold' }}>افزودن</button>
      </form>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: 16 }}>
        {items.map(p => (
          <div key={p.id} style={{ background: 'white', padding: 16, borderRadius: 8 }}>
            <b>{p.name}</b>
            <p style={{ color: '#B12704', fontWeight: 'bold', margin: '6px 0' }}>${p.price}</p>
            <button onClick={() => remove(p.id)} style={{
              padding: '6px 12px', background: '#fff', border: '1px solid #ccc', borderRadius: 4, cursor: 'pointer', fontSize: 13,
            }}>حذف</button>
          </div>
        ))}
      </div>
    </div>
  );
}
