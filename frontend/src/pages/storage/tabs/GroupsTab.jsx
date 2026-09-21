import { useEffect, useState } from 'react';
import storageApi from '../../../api/storageApi';

export default function GroupsTab() {
  const [groups, setGroups] = useState([]);
  const [allPerms, setAllPerms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [msg, setMsg] = useState('');
  const [form, setForm] = useState({ name: '', description: '' });
  const [selected, setSelected] = useState(null);
  const [selectedPerms, setSelectedPerms] = useState([]);
  const [newMemberId, setNewMemberId] = useState('');
  const [policy, setPolicy] = useState(null);
  const [activeTab, setActiveTab] = useState('members');

  const load = async () => {
    setLoading(true);
    try {
      const [g, p] = await Promise.all([
        storageApi.groups(),
        storageApi.permissionsByCategory(),
      ]);
      setGroups(g.data);
      const flat = p.data.flatMap(c => c.permissions.map(x => ({ ...x, category: c.category })));
      setAllPerms(flat);
    } finally { setLoading(false); }
  };

  useEffect(() => { load(); }, []);

  const create = async () => {
    if (!form.name.trim()) return;
    try {
      await storageApi.createGroup(form);
      setForm({ name: '', description: '' });
      setMsg('✅ گروه ساخته شد');
      load();
    } catch (e) {
      setMsg('❌ ' + (e.response?.data || 'خطا'));
    } finally { setTimeout(() => setMsg(''), 3000); }
  };

  const remove = async (id, name) => {
    if (!confirm(`حذف گروه «${name}»؟`)) return;
    try {
      await storageApi.deleteGroup(id);
      setMsg('✅ حذف شد');
      load();
    } catch (e) {
      setMsg('❌ ' + (e.response?.data || 'خطا'));
    }
  };

  const openGroup = async (id) => {
    try {
      const [g, p] = await Promise.all([
        storageApi.group(id),
        storageApi.getPasswordPolicy(id),
      ]);
      setSelected(g.data);
      setSelectedPerms(g.data.permissions.map(p => p.id));
      setPolicy(p.data);
      setActiveTab('members');
    } catch { alert('خطا در بارگذاری'); }
  };

  const savePerms = async () => {
    try {
      await storageApi.setGroupPermissions(selected.id, selectedPerms);
      setMsg('✅ دسترسی‌ها ذخیره شد');
      setSelected(null);
      load();
    } catch (e) {
      setMsg('❌ خطا');
    }
  };

  const savePolicy = async () => {
    try {
      await storageApi.updatePasswordPolicy(selected.id, policy);
      setMsg('✅ Password Policy ذخیره شد');
    } catch (e) {
      setMsg('❌ خطا');
    }
  };

  const addMember = async () => {
    if (!newMemberId.trim()) return;
    try {
      await storageApi.addGroupMembers(selected.id, [newMemberId.trim()]);
      setNewMemberId('');
      openGroup(selected.id);
      setMsg('✅ عضو اضافه شد');
    } catch (e) {
      setMsg('❌ ' + (e.response?.data || 'خطا'));
    }
  };

  const removeMember = async (userId) => {
    try {
      await storageApi.removeGroupMember(selected.id, userId);
      openGroup(selected.id);
    } catch {
      setMsg('❌ خطا');
    }
  };

  const togglePerm = (id) => {
    setSelectedPerms(prev =>
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
    );
  };

  const permsByCategory = allPerms.reduce((acc, p) => {
    if (!acc[p.category]) acc[p.category] = [];
    acc[p.category].push(p);
    return acc;
  }, {});

  return (
    <div>
      <div style={{ background: 'white', padding: 16, borderRadius: 8, marginBottom: 20 }}>
        <h3 style={{ marginBottom: 12 }}>گروه جدید</h3>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 2fr auto', gap: 8 }}>
          <input value={form.name} onChange={e => setForm({...form, name: e.target.value})}
            placeholder="نام گروه" style={inp} />
          <input value={form.description} onChange={e => setForm({...form, description: e.target.value})}
            placeholder="توضیح" style={inp} />
          <button onClick={create} style={btnY}>ساخت</button>
        </div>
      </div>

      {msg && <div style={{ padding: 10, background: '#f0f7ff', marginBottom: 12, borderRadius: 4 }}>{msg}</div>}

      {loading ? <p>...</p> : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 12 }}>
          {groups.map(g => (
            <div key={g.id} style={{ background: 'white', padding: 16, borderRadius: 8, border: '1px solid #eee' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <b>{g.name}</b>
                {!g.isSystemGroup && (
                  <button onClick={() => remove(g.id, g.name)} style={btnSm}>🗑️</button>
                )}
              </div>
              {g.description && <p style={{ fontSize: 13, color: '#666', marginTop: 4 }}>{g.description}</p>}
              <p style={{ fontSize: 12, color: '#999', marginTop: 8 }}>
                👥 {g.memberCount} عضو • 🔑 {g.permissionCount} دسترسی
              </p>
              <button onClick={() => openGroup(g.id)} style={{...btnY, marginTop: 12, width: '100%'}}>
                مدیریت
              </button>
            </div>
          ))}
        </div>
      )}

      {selected && (
        <div style={overlay} onClick={() => setSelected(null)}>
          <div style={modal} onClick={e => e.stopPropagation()}>
            <h3>{selected.name}</h3>
            <p style={{ fontSize: 13, color: '#666', marginBottom: 16 }}>{selected.description}</p>

            {/* Tabs */}
            <div style={{ display: 'flex', gap: 8, borderBottom: '1px solid #eee', marginBottom: 16 }}>
              {[
                { id: 'members', label: `👥 اعضا (${selected.members.length})` },
                { id: 'permissions', label: `🔑 دسترسی‌ها (${selectedPerms.length})` },
                { id: 'policy', label: '🔒 Password Policy' },
              ].map(t => (
                <button key={t.id} onClick={() => setActiveTab(t.id)} style={{
                  padding: '8px 16px', border: 'none', cursor: 'pointer',
                  background: activeTab === t.id ? '#f0f0f0' : 'transparent',
                  borderBottom: activeTab === t.id ? '2px solid #FF9900' : '2px solid transparent',
                  fontWeight: activeTab === t.id ? 'bold' : 'normal', fontSize: 13,
                }}>{t.label}</button>
              ))}
            </div>

            {/* Members Tab */}
            {activeTab === 'members' && (
              <div>
                <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
                  <input placeholder="User ID (Guid)" value={newMemberId}
                    onChange={e => setNewMemberId(e.target.value)} style={{...inp, flex: 1}} />
                  <button onClick={addMember} style={btnY}>افزودن</button>
                </div>
                <div style={{ maxHeight: 300, overflow: 'auto', background: '#f8f8f8', padding: 8, borderRadius: 4 }}>
                  {selected.members.length === 0 && <p style={{ fontSize: 12, color: '#999', padding: 8 }}>هیچ عضوی نیست</p>}
                  {selected.members.map(m => (
                    <div key={m.id} style={{ display: 'flex', justifyContent: 'space-between', padding: 6, fontSize: 13 }}>
                      <span>{m.username}</span>
                      <button onClick={() => removeMember(m.userId)} style={btnSm}>✖</button>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Permissions Tab */}
            {activeTab === 'permissions' && (
              <div>
                <div style={{ maxHeight: 400, overflow: 'auto', border: '1px solid #eee', borderRadius: 4, padding: 12 }}>
                  {Object.entries(permsByCategory).map(([cat, perms]) => (
                    <div key={cat} style={{ marginBottom: 16 }}>
                      <b style={{ fontSize: 13, color: '#232f3e', display: 'block', marginBottom: 6 }}>{cat}</b>
                      {perms.map(p => (
                        <label key={p.id} style={{ display: 'flex', alignItems: 'center', gap: 8, padding: 4, fontSize: 13 }}>
                          <input type="checkbox" checked={selectedPerms.includes(p.id)}
                            onChange={() => togglePerm(p.id)} />
                          <span>{p.name} <code style={{ fontSize: 11, color: '#888' }}>{p.code}</code></span>
                        </label>
                      ))}
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Password Policy Tab */}
            {activeTab === 'policy' && policy && (
              <div>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                  <div>
                    <label style={{ fontSize: 12, color: '#666', display: 'block', marginBottom: 4 }}>
                      حداقل طول رمز
                    </label>
                    <input type="number" min="4" max="64" value={policy.minPasswordLength}
                      onChange={e => setPolicy({...policy, minPasswordLength: parseInt(e.target.value)})}
                      style={inp} />
                  </div>
                  <div>
                    <label style={{ fontSize: 12, color: '#666', display: 'block', marginBottom: 4 }}>
                      انقضا (روز) — خالی = بدون انقضا
                    </label>
                    <input type="number" value={policy.passwordExpiryDays || ''}
                      onChange={e => setPolicy({...policy, passwordExpiryDays: e.target.value ? parseInt(e.target.value) : null})}
                      style={inp} />
                  </div>
                  <div>
                    <label style={{ fontSize: 12, color: '#666', display: 'block', marginBottom: 4 }}>
                      حداکثر تلاش ناموفق
                    </label>
                    <input type="number" value={policy.maxLoginAttempts || ''}
                      onChange={e => setPolicy({...policy, maxLoginAttempts: e.target.value ? parseInt(e.target.value) : null})}
                      style={inp} />
                  </div>
                </div>

                <div style={{ marginTop: 16 }}>
                  {[
                    { key: 'requireUppercase', label: 'حداقل یک حرف بزرگ (A-Z)' },
                    { key: 'requireLowercase', label: 'حداقل یک حرف کوچک (a-z)' },
                    { key: 'requireDigit', label: 'حداقل یک عدد (0-9)' },
                    { key: 'requireSpecialChar', label: 'حداقل یک کاراکتر خاص (!@#$)' },
                  ].map(item => (
                    <label key={item.key} style={{
                      display: 'flex', alignItems: 'center', gap: 10,
                      padding: 10, background: '#f8f8f8', borderRadius: 6, marginBottom: 8,
                      cursor: 'pointer', fontSize: 13,
                    }}>
                      <input type="checkbox" checked={policy[item.key]}
                        onChange={() => setPolicy({...policy, [item.key]: !policy[item.key]})} />
                      {item.label}
                    </label>
                  ))}
                </div>

                <button onClick={savePolicy} style={{...btnY, marginTop: 12, width: '100%'}}>
                  💾 ذخیره Password Policy
                </button>
              </div>
            )}

            <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 20 }}>
              <button onClick={() => setSelected(null)} style={btnG}>بستن</button>
              {activeTab === 'permissions' && (
                <button onClick={savePerms} style={btnY}>💾 ذخیره دسترسی‌ها</button>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

const inp = { padding: 10, border: '1px solid #ccc', borderRadius: 4, width: '100%' };
const btnY = { padding: '10px 16px', background: '#FFD814', border: '1px solid #FCD200', borderRadius: 4, cursor: 'pointer', fontWeight: 'bold' };
const btnG = { padding: '10px 16px', background: '#f0f0f0', border: 'none', borderRadius: 4, cursor: 'pointer' };
const btnSm = { background: 'none', border: 'none', cursor: 'pointer', fontSize: 14 };
const overlay = { position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 };
const modal = { background: 'white', padding: 24, borderRadius: 8, maxWidth: 700, width: '90%', maxHeight: '90vh', overflow: 'auto' };
