import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import storageApi from '../api/storageApi';

export default function NotificationBell() {
  const [count, setCount] = useState(0);
  const [open, setOpen] = useState(false);
  const [items, setItems] = useState([]);

  const load = async () => {
    try {
      const r = await storageApi.unreadCount();
      setCount(r.data.count);
    } catch { /* ignore */ }
  };

  const loadList = async () => {
    try {
      const r = await storageApi.notifications();
      setItems(r.data);
    } catch { /* ignore */ }
  };

  useEffect(() => {
    load();
    const t = setInterval(load, 30000); // هر ۳۰ ثانیه
    return () => clearInterval(t);
  }, []);

  const openMenu = () => {
    setOpen(!open);
    if (!open) loadList();
  };

  const markAllRead = async () => {
    await storageApi.markAllNotificationsRead();
    load();
    loadList();
  };

  return (
    <div style={{ position: 'relative' }}>
      <button onClick={openMenu} style={{
        background: 'none', border: 'none', cursor: 'pointer',
        color: 'white', fontSize: 20, position: 'relative', padding: 8,
      }}>
        🔔
        {count > 0 && (
          <span style={{
            position: 'absolute', top: 0, right: 0,
            background: '#f44336', color: 'white',
            borderRadius: '50%', minWidth: 18, height: 18,
            fontSize: 11, display: 'flex', alignItems: 'center',
            justifyContent: 'center', fontWeight: 'bold',
          }}>{count > 9 ? '9+' : count}</span>
        )}
      </button>

      {open && (
        <div style={{
          position: 'absolute', top: 45, right: 0,
          width: 360, maxHeight: 500, overflow: 'auto',
          background: 'white', borderRadius: 8,
          boxShadow: '0 4px 20px rgba(0,0,0,0.15)',
          zIndex: 1000, color: '#111',
        }}>
          <div style={{
            padding: 12, borderBottom: '1px solid #eee',
            display: 'flex', justifyContent: 'space-between',
          }}>
            <b>اعلان‌ها</b>
            {count > 0 && (
              <button onClick={markAllRead} style={{
                background: 'none', border: 'none', color: '#007185',
                cursor: 'pointer', fontSize: 12,
              }}>همه خوانده شد</button>
            )}
          </div>

          {items.length === 0 ? (
            <p style={{ padding: 20, textAlign: 'center', color: '#999', fontSize: 13 }}>
              اعلانی نداری
            </p>
          ) : (
            items.slice(0, 20).map(n => (
              <div key={n.id} style={{
                padding: 12, borderBottom: '1px solid #f0f0f0',
                background: n.isRead ? 'white' : '#fff8e0',
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <b style={{ fontSize: 13 }}>{n.title}</b>
                  <span style={{ fontSize: 10, color: '#999' }}>
                    {new Date(n.createdAt).toLocaleDateString('fa-IR')}
                  </span>
                </div>
                <p style={{ fontSize: 12, color: '#666', marginTop: 4, lineHeight: 1.5 }}>{n.message}</p>
              </div>
            ))
          )}

          <Link to="/storage/admin?tab=notifications" style={{
            display: 'block', padding: 12, textAlign: 'center',
            color: '#007185', textDecoration: 'none', fontSize: 13,
            borderTop: '1px solid #eee',
          }} onClick={() => setOpen(false)}>مشاهده همه</Link>
        </div>
      )}
    </div>
  );
}
