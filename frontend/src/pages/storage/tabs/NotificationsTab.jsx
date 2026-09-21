import { useEffect, useState } from 'react';
import storageApi from '../../../api/storageApi';

export default function NotificationsTab() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [msg, setMsg] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const r = await storageApi.notifications();
      setItems(r.data);
    } finally { setLoading(false); }
  };

  useEffect(() => { load(); }, []);

  const markRead = async (id) => {
    await storageApi.markRead(id);
    load();
  };

  const markAllRead = async () => {
    const r = await storageApi.markAllNotificationsRead();
    setMsg(`✅ ${r.data.marked} اعلان خوانده شد`);
    load();
    setTimeout(() => setMsg(''), 3000);
  };

  const del = async (id) => {
    if (!confirm('حذف شود؟')) return;
    await storageApi.deleteNotification(id);
    load();
  };

  const sevColor = (s) => ({
    Info: '#2196f3', Warning: '#ff9800', Critical: '#f44336'
  }[s] || '#666');

  return (
    <div>
      <div style={{ background: 'white', padding: 16, borderRadius: 8, marginBottom: 12,
        display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <b>اعلان‌ها ({items.length})</b>
        <button onClick={markAllRead} style={btnY}>✅ همه خوانده شد</button>
      </div>

      {msg && <div style={{ padding: 10, background: '#f0f7ff', marginBottom: 12, borderRadius: 4 }}>{msg}</div>}

      {loading ? <p>...</p> : items.length === 0 ? (
        <p style={{ padding: 40, textAlign: 'center', color: '#999' }}>اعلانی نداری</p>
      ) : (
        <div style={{ background: 'white', borderRadius: 8 }}>
          {items.map(n => (
            <div key={n.id} style={{
              padding: 16, borderBottom: '1px solid #f0f0f0',
              background: n.isRead ? 'white' : '#fff8e0',
              display: 'flex', gap: 12, alignItems: 'flex-start',
            }}>
              <div style={{ width: 6, height: 40, borderRadius: 3, background: sevColor(n.severity) }} />
              <div style={{ flex: 1 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <b style={{ fontSize: 14 }}>{n.title}</b>
                  <span style={{ fontSize: 11, color: '#999' }}>
                    {new Date(n.createdAt).toLocaleString('fa-IR')}
                  </span>
                </div>
                <p style={{ fontSize: 13, color: '#555', marginTop: 6, lineHeight: 1.6 }}>{n.message}</p>
                <div style={{ marginTop: 8, display: 'flex', gap: 8 }}>
                  <span style={{ fontSize: 11, padding: '2px 8px', background: sevColor(n.severity),
                    color: 'white', borderRadius: 4 }}>{n.severity}</span>
                  {!n.isRead && (
                    <button onClick={() => markRead(n.id)} style={btnSm}>خوانده شد</button>
                  )}
                  <button onClick={() => del(n.id)} style={{...btnSm, color: '#c00'}}>حذف</button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

const btnY = { padding: '8px 16px', background: '#FFD814', border: '1px solid #FCD200', borderRadius: 4, cursor: 'pointer', fontWeight: 'bold' };
const btnSm = { padding: '2px 8px', background: 'none', border: '1px solid #ccc', borderRadius: 4, cursor: 'pointer', fontSize: 11 };
