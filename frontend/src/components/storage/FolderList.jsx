import { useState } from 'react';
import storageApi from '../../api/storageApi';

export default function FolderList({ folders, onOpen, onRefresh }) {
  const [creating, setCreating] = useState(false);
  const [name, setName] = useState('');

  const create = async () => {
    if (!name.trim()) return;
    try {
      await storageApi.createFolder({ name: name.trim(), parentFolderId: null });
      setName('');
      setCreating(false);
      onRefresh?.();
    } catch {
      alert('خطا در ساخت پوشه');
    }
  };

  const del = async (id, folderName) => {
    if (!confirm(`حذف پوشه «${folderName}»؟`)) return;
    try {
      await storageApi.deleteFolder(id);
      onRefresh?.();
    } catch {
      alert('خطا');
    }
  };

  return (
    <div style={{ marginBottom: 20 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
        <h3 style={{ fontSize: 16 }}>پوشه‌ها</h3>
        <button onClick={() => setCreating(!creating)} style={{
          padding: '6px 14px', background: '#f0f0f0', border: 'none',
          borderRadius: 4, cursor: 'pointer', fontSize: 13,
        }}>+ پوشه جدید</button>
      </div>

      {creating && (
        <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
          <input value={name} onChange={e => setName(e.target.value)}
            placeholder="نام پوشه"
            onKeyDown={e => e.key === 'Enter' && create()}
            style={{ flex: 1, padding: 8, border: '1px solid #ccc', borderRadius: 4 }} />
          <button onClick={create} style={{ padding: '8px 16px', background: '#FFD814', border: 'none', borderRadius: 4, cursor: 'pointer' }}>ساخت</button>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: 12 }}>
        {folders.map(f => (
          <div key={f.id} onClick={() => onOpen?.(f)} style={{
            padding: 16, background: 'white', border: '1px solid #eee',
            borderRadius: 8, cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 12,
          }}>
            <div style={{ fontSize: 28 }}>📁</div>
            <div style={{ flex: 1 }}>
              <div style={{ fontWeight: 'bold', fontSize: 14 }}>{f.name}</div>
              <div style={{ fontSize: 12, color: '#666' }}>{f.fileCount} فایل</div>
            </div>
            <button onClick={(e) => { e.stopPropagation(); del(f.id, f.name); }}
              style={{ background: 'none', border: 'none', cursor: 'pointer' }}>🗑️</button>
          </div>
        ))}
      </div>

      {folders.length === 0 && !creating && (
        <p style={{ color: '#999', fontSize: 14 }}>هنوز پوشه‌ای نداری</p>
      )}
    </div>
  );
}
