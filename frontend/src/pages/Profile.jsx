import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { profileApi } from '../api/client';
import { useAuth } from '../context/AuthContext';

export default function Profile() {
  const { user } = useAuth();
  const [profile, setProfile] = useState(null);
  const [tab, setTab] = useState('info');
  const [msg, setMsg] = useState('');

  // فرم‌ها
  const [form, setForm] = useState({});
  const [pwd, setPwd] = useState({ currentPassword: '', newPassword: '' });
  const [eml, setEml] = useState({ newEmail: '', password: '' });
  const [seller, setSeller] = useState({ businessName: '', businessType: 'Product', description: '', website: '' });

  const load = async () => {
    try {
      const r = await profileApi.me();
      setProfile(r.data);
      setForm(r.data);
    } catch (e) { console.error(e); }
  };

  useEffect(() => { load(); }, []);

  const saveInfo = async (e) => {
    e.preventDefault();
    try {
      await profileApi.update(form);
      setMsg('✅ ذخیره شد');
      load();
      setTimeout(() => setMsg(''), 2000);
    } catch { setMsg('❌ خطا'); }
  };

  const changePwd = async (e) => {
    e.preventDefault();
    try {
      await profileApi.changePassword(pwd);
      setPwd({ currentPassword: '', newPassword: '' });
      setMsg('✅ رمز عوض شد');
      setTimeout(() => setMsg(''), 2000);
    } catch (err) { setMsg('❌ ' + JSON.stringify(err.response?.data)); }
  };

  const changeEmail = async (e) => {
    e.preventDefault();
    try {
      await profileApi.changeEmail(eml);
      setEml({ newEmail: '', password: '' });
      setMsg('✅ ایمیل عوض شد');
      load();
      setTimeout(() => setMsg(''), 2000);
    } catch (err) { setMsg('❌ ' + JSON.stringify(err.response?.data)); }
  };

  const becomeSeller = async (e) => {
    e.preventDefault();
    try {
      await profileApi.becomeSeller(seller);
      setMsg('✅ فروشنده شدید');
      load();
      setTimeout(() => setMsg(''), 3000);
    } catch (err) { setMsg('❌ ' + JSON.stringify(err.response?.data)); }
  };

  if (!profile) return <div style={{ padding: 40 }}>Loading...</div>;

  const inputStyle = { width: '100%', padding: 10, border: '1px solid #ccc', borderRadius: 4, marginBottom: 10 };
  const btn = { padding: '10px 24px', background: '#FFD814', border: '1px solid #FCD200', borderRadius: 20, cursor: 'pointer', fontWeight: 'bold' };

  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: 40 }}>
      <div style={{ background: 'white', padding: 30, borderRadius: 8 }}>
        <h1 style={{ marginBottom: 24 }}>حساب من</h1>

        <div style={{ display: 'flex', alignItems: 'center', gap: 20, marginBottom: 30 }}>
          <div style={{
            width: 80, height: 80, borderRadius: '50%', background: '#232f3e',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: 'white', fontSize: 36, fontWeight: 'bold',
          }}>{(profile.firstName || '?')[0].toUpperCase()}</div>
          <div>
            <h2 style={{ marginBottom: 4 }}>{profile.firstName} {profile.lastName}</h2>
            <p style={{ color: '#666' }}>{profile.email}</p>
            {profile.isSeller && <span style={{ background: '#FFD814', padding: '2px 8px', borderRadius: 4, fontSize: 12 }}>فروشنده</span>}
          </div>
        </div>

        {/* تب‌ها */}
        <div style={{ display: 'flex', gap: 8, borderBottom: '1px solid #eee', marginBottom: 24 }}>
          {[
            { id: 'info', label: 'اطلاعات' },
            { id: 'address', label: 'آدرس' },
            { id: 'password', label: 'رمز عبور' },
            { id: 'email', label: 'ایمیل' },
            ...(!profile.isSeller ? [{ id: 'seller', label: 'فروشنده شدن' }] : []),
          ].map(t => (
            <button key={t.id} onClick={() => setTab(t.id)} style={{
              padding: '10px 16px', border: 'none', cursor: 'pointer',
              background: tab === t.id ? '#f0f0f0' : 'transparent',
              borderBottom: tab === t.id ? '2px solid #FF9900' : '2px solid transparent',
              fontWeight: tab === t.id ? 'bold' : 'normal',
            }}>{t.label}</button>
          ))}
        </div>

        {msg && <p style={{ marginBottom: 16, padding: 10, background: '#f0f7ff', borderRadius: 4 }}>{msg}</p>}

        {tab === 'info' && (
          <form onSubmit={saveInfo}>
            <input placeholder="نام" value={form.firstName || ''} onChange={e => setForm({ ...form, firstName: e.target.value })} style={inputStyle} />
            <input placeholder="نام خانوادگی" value={form.lastName || ''} onChange={e => setForm({ ...form, lastName: e.target.value })} style={inputStyle} />
            <input placeholder="شماره تلفن" value={form.phoneNumber || ''} onChange={e => setForm({ ...form, phoneNumber: e.target.value })} style={inputStyle} />
            <button style={btn} type="submit">ذخیره</button>
          </form>
        )}

        {tab === 'address' && (
          <form onSubmit={saveInfo}>
            <input placeholder="کد پستی" value={form.postalCode || ''} onChange={e => setForm({ ...form, postalCode: e.target.value })} style={inputStyle} />
            <input placeholder="آدرس" value={form.address || ''} onChange={e => setForm({ ...form, address: e.target.value })} style={inputStyle} />
            <input placeholder="شهر" value={form.city || ''} onChange={e => setForm({ ...form, city: e.target.value })} style={inputStyle} />
            <input placeholder="استان" value={form.state || ''} onChange={e => setForm({ ...form, state: e.target.value })} style={inputStyle} />
            <input placeholder="کشور" value={form.country || ''} onChange={e => setForm({ ...form, country: e.target.value })} style={inputStyle} />
            <button style={btn} type="submit">ذخیره</button>
          </form>
        )}

        {tab === 'password' && (
          <form onSubmit={changePwd}>
            <input type="password" placeholder="رمز فعلی" value={pwd.currentPassword} onChange={e => setPwd({ ...pwd, currentPassword: e.target.value })} style={inputStyle} />
            <input type="password" placeholder="رمز جدید" value={pwd.newPassword} onChange={e => setPwd({ ...pwd, newPassword: e.target.value })} style={inputStyle} />
            <button style={btn} type="submit">تغییر رمز</button>
          </form>
        )}

        {tab === 'email' && (
          <form onSubmit={changeEmail}>
            <input type="email" placeholder="ایمیل جدید" value={eml.newEmail} onChange={e => setEml({ ...eml, newEmail: e.target.value })} style={inputStyle} />
            <input type="password" placeholder="رمز عبور برای تایید" value={eml.password} onChange={e => setEml({ ...eml, password: e.target.value })} style={inputStyle} />
            <button style={btn} type="submit">تغییر ایمیل</button>
          </form>
        )}

        {tab === 'seller' && !profile.isSeller && (
          <form onSubmit={becomeSeller}>
            <input placeholder="نام کسب‌وکار" value={seller.businessName} onChange={e => setSeller({ ...seller, businessName: e.target.value })} style={inputStyle} />
            <select value={seller.businessType} onChange={e => setSeller({ ...seller, businessType: e.target.value })} style={inputStyle}>
              <option value="Product">محصول‌محور</option>
              <option value="Scientific">علمی</option>
            </select>
            <textarea placeholder="توضیحات کسب‌وکار" value={seller.description} onChange={e => setSeller({ ...seller, description: e.target.value })} rows={4} style={inputStyle} />
            <input placeholder="وب‌سایت (اختیاری)" value={seller.website} onChange={e => setSeller({ ...seller, website: e.target.value })} style={inputStyle} />
            <button style={btn} type="submit">ثبت درخواست</button>
          </form>
        )}

        {/* لینک‌ها */}
        <div style={{ marginTop: 30, display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 12 }}>
          <Link to="/orders" style={{ padding: 16, background: '#f8f8f8', borderRadius: 8, textDecoration: 'none', color: '#111' }}>📦 سفارش‌ها</Link>
          <Link to="/basket" style={{ padding: 16, background: '#f8f8f8', borderRadius: 8, textDecoration: 'none', color: '#111' }}>🛍️ سبد</Link>
          {profile.isSeller && <Link to="/seller/products" style={{ padding: 16, background: '#f8f8f8', borderRadius: 8, textDecoration: 'none', color: '#111' }}>🏪 محصولات من</Link>}
        </div>
      </div>
    </div>
  );
}
