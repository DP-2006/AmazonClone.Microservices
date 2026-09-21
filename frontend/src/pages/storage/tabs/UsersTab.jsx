import { useEffect, useState } from 'react';
import storageApi from '../../../api/storageApi';

export default function UsersTab() {
  const [users, setUsers] = useState([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [msg, setMsg] = useState('');
  const [selectedUser, setSelectedUser] = useState(null);
  const [userPerms, setUserPerms] = useState(null);

  const load = async () => {
    setLoading(true);
    try {
      const r = await storageApi.users(search);
      setUsers(r.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const block = async (id, current) => {
    const action = current ? 'رفع مسدودی' : 'مسدود کردن';
    if (!confirm(`${action} این کاربر؟`)) return;
    try {
      if (current) await storageApi.unblockUser(id);
      else await storageApi.blockUser(id);
      setMsg(`✅ ${action} انجام شد`);
      load();
    } catch (e) {
      setMsg('❌ خطا: ' + (e.response?.data || ''));
    } finally { setTimeout(() => setMsg(''), 3000); }
  };

  const sendMessage = async (id) => {
    const title = prompt('عنوان پیام:');
    if (!title) return;
    const message = prompt('متن پیام:');
    if (!message) return;
    try {
      await storageApi.messageUser(id, { title, message, severity: 'Info' });
      setMsg('✅ پیام ارسال شد');
    } catch (e) {
      setMsg('❌ خطا');
    } finally { setTimeout(() => setMsg(''), 3000); }
  };

  const viewUser = async (user) => {
    setSelectedUser(user);
    try {
      const r = await storageApi.userPermissions(user.id);
      setUserPerms(r.data);
    } catch {
      setUserPerms(null);
    }
  };

  const formatSize = (b) => b < 1024 * 1024 ? (b / 1024).toFixed(1) + ' KB' : (b / 1024 / 1024).toFixed(1) + ' MB';

  return (
    <div>
      <div style={{ marginBottom: 16, display: 'flex', gap: 12 }}>
        <input
          placeholder="جستجو (نام یا ایمیل)..."
          value={search}
          onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && load()}
          style={{ flex: 1, padding: 10, border: '1px solid #ccc', borderRadius: 4 }}
        />
        <button onClick={load} style={{ padding: '10px 20px', background: '#FFD814', border: '1px solid #FCD200', borderRadius: 4, cursor: 'pointer', fontWeight: 'bold' }}>🔍 جستجو</button>
      </div>

      {msg && <div style={{ padding: 10, background: '#f0f7ff', marginBottom: 12, borderRadius: 4 }}>{msg}</div>}

      {loading ? <p>...</p> : (
        <div style={{ background: 'white', borderRadius: 8, overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead style={{ background: '#f0f0f0' }}>
              <tr>
                <th style={th}>نام</th>
                <th style={th}>ایمیل</th>
                <th style={th}>نقش‌ها</th>
                <th style={th}>گروه‌ها</th>
                <th style={th}>فایل‌ها</th>
                <th style={th}>حجم</th>
                <th style={th}>وضعیت</th>
                <th style={th}>عملیات</th>
              </tr>
            </thead>
            <tbody>
              {users.map(u => (
                <tr key={u.id} style={{ borderBottom: '1px solid #eee' }}>
                  <td style={td}>{u.fullName || '—'}</td>
                  <td style={td}>{u.email}</td>
                  <td style={td}>
                    {u.roles.map(r => (
                      <span key={r} style={{
                        background: r === 'Admin' ? '#FFD814' : '#e0e0e0',
                        padding: '2px 8px', borderRadius: 4, fontSize: 12, marginLeft: 4,
                      }}>{r}</span>
                    ))}
                  </td>
                  <td style={td}>
                    {u.groups?.length > 0 ? u.groups.map(g => g.groupName).join(', ') : '—'}
                  </td>
                  <td style={td}>{u.fileCount}</td>
                  <td style={td}>{formatSize(u.storageUsedBytes)}</td>
                  <td style={td}>
                    <span style={{ color: u.isActive ? 'green' : 'red', fontSize: 12 }}>
                      {u.isActive ? '✅ فعال' : '🚫 مسدود'}
                    </span>
                  </td>
                  <td style={td}>
                    <button onClick={() => viewUser(u)} style={btn}>👁️</button>
                    <button onClick={() => sendMessage(u.id)} style={btn}>✉️</button>
                    <button onClick={() => block(u.id, !u.isActive)} style={btn}>
                      {u.isActive ? '🚫' : '✅'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {selectedUser && (
        <div style={overlay} onClick={() => setSelectedUser(null)}>
          <div style={modal} onClick={e => e.stopPropagation()}>
            <h3>{selectedUser.fullName}</h3>
            <p style={{ fontSize: 13, color: '#666' }}>{selectedUser.email}</p>
            <h4 style={{ marginTop: 16 }}>دسترسی‌های مؤثر:</h4>
            {userPerms ? (
              <>
                <p style={{ fontSize: 13, marginTop: 8 }}>
                  <b>مستقیم:</b> {userPerms.directPermissions?.length || 0}
                </p>
                <p style={{ fontSize: 13 }}>
                  <b>از گروه‌ها:</b> {userPerms.groupPermissions?.length || 0}
                </p>
                <p style={{ fontSize: 13 }}>
                  <b>کل:</b> {userPerms.effectivePermissions?.length || 0}
                </p>
                <div style={{ maxHeight: 200, overflow: 'auto', marginTop: 12,
                  background: '#f8f8f8', padding: 12, borderRadius: 4 }}>
                  {userPerms.effectivePermissions?.map(p => (
                    <div key={p.id} style={{ fontSize: 12, padding: 2 }}>
                      • <code>{p.code}</code> — {p.name}
                    </div>
                  ))}
                </div>
              </>
            ) : <p>در حال بارگذاری...</p>}
            <button onClick={() => setSelectedUser(null)} style={{...btn, marginTop: 16, padding: '8px 16px'}}>بستن</button>
          </div>
        </div>
      )}
    </div>
  );
}

const th = { padding: 10, textAlign: 'right', fontSize: 13, fontWeight: 'bold' };
const td = { padding: 10, fontSize: 13 };
const btn = { marginLeft: 4, padding: '4px 8px', background: '#f0f0f0', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 14 };
const overlay = { position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 };
const modal = { background: 'white', padding: 24, borderRadius: 8, maxWidth: 600, width: '90%', maxHeight: '80vh', overflow: 'auto' };
