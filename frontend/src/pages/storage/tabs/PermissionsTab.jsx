import { useEffect, useState } from 'react';
import storageApi from '../../../api/storageApi';

export default function PermissionsTab() {
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [assignForm, setAssignForm] = useState({ userId: '', permissionIds: [] });
  const [msg, setMsg] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const r = await storageApi.permissionsByCategory();
      setCategories(r.data);
    } finally { setLoading(false); }
  };

  useEffect(() => { load(); }, []);

  const assign = async () => {
    if (!assignForm.userId.trim() || assignForm.permissionIds.length === 0) {
      setMsg('❌ کاربر و حداقل یک دسترسی انتخاب کن');
      return;
    }
    try {
      await storageApi.assignPermissions(assignForm);
      setMsg('✅ دسترسی‌ها اختصاص یافت');
      setAssignForm({ userId: '', permissionIds: [] });
    } catch (e) {
      setMsg('❌ ' + (e.response?.data || 'خطا'));
    }
  };

  const togglePerm = (id) => {
    setAssignForm(f => ({
      ...f,
      permissionIds: f.permissionIds.includes(id)
        ? f.permissionIds.filter(x => x !== id)
        : [...f.permissionIds, id],
    }));
  };

  return (
    <div>
      <div style={{ background: 'white', padding: 16, borderRadius: 8, marginBottom: 20 }}>
        <h3 style={{ marginBottom: 12 }}>اختصاص مستقیم دسترسی به کاربر</h3>
        <input
          placeholder="User ID (Guid)"
          value={assignForm.userId}
          onChange={e => setAssignForm(f => ({...f, userId: e.target.value}))}
          style={{ ...inp, width: '100%', marginBottom: 12 }}
        />
        <p style={{ fontSize: 13, color: '#666', marginBottom: 8 }}>
          دسترسی‌های انتخاب‌شده: {assignForm.permissionIds.length}
        </p>
        <button onClick={assign} style={btnY}>🔑 اختصاص</button>
      </div>

      {msg && <div style={{ padding: 10, background: '#f0f7ff', marginBottom: 12, borderRadius: 4 }}>{msg}</div>}

      {loading ? <p>...</p> : (
        categories.map(cat => (
          <div key={cat.category} style={{ background: 'white', padding: 16, borderRadius: 8, marginBottom: 12 }}>
            <h4 style={{ marginBottom: 12, color: '#232f3e' }}>{cat.category}</h4>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 8 }}>
              {cat.permissions.map(p => (
                <label key={p.id} style={{
                  display: 'flex', alignItems: 'center', gap: 8,
                  padding: 8, background: assignForm.permissionIds.includes(p.id) ? '#fff8e0' : '#f8f8f8',
                  borderRadius: 4, cursor: 'pointer', fontSize: 13,
                }}>
                  <input type="checkbox" checked={assignForm.permissionIds.includes(p.id)}
                    onChange={() => togglePerm(p.id)} />
                  <div>
                    <div style={{ fontWeight: 'bold' }}>{p.name}</div>
                    <code style={{ fontSize: 11, color: '#888' }}>{p.code}</code>
                  </div>
                </label>
              ))}
            </div>
          </div>
        ))
      )}
    </div>
  );
}

const inp = { padding: 10, border: '1px solid #ccc', borderRadius: 4 };
const btnY = { padding: '10px 16px', background: '#FFD814', border: '1px solid #FCD200', borderRadius: 4, cursor: 'pointer', fontWeight: 'bold' };
