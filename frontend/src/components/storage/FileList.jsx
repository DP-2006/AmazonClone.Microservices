import { useState } from 'react';
import storageApi from '../../api/storageApi';

export default function FileList({ files, onRefresh, showActions = true }) {
  const [shareFile, setShareFile] = useState(null);
  const [shareUserId, setShareUserId] = useState('');
  const [shareMessage, setShareMessage] = useState('');
  const [busy, setBusy] = useState(false);

  const formatSize = (bytes) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    if (bytes < 1024 * 1024 * 1024) return (bytes / 1024 / 1024).toFixed(1) + ' MB';
    return (bytes / 1024 / 1024 / 1024).toFixed(2) + ' GB';
  };

  const handleDownload = async (file) => {
    try {
      const token = localStorage.getItem('token');
      const res = await fetch(`http://localhost:5008${storageApi.download(file.id)}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      if (!res.ok) throw new Error('Download failed');
      const blob = await res.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = file.originalName || file.fileName;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch {
      alert('خطا در دانلود');
    }
  };

  const handleDelete = async (file) => {
    if (!confirm(`حذف «${file.originalName}»؟`)) return;
    try {
      await storageApi.deleteFile(file.id);
      onRefresh?.();
    } catch {
      alert('خطا در حذف');
    }
  };

  const handleShare = async () => {
    if (!shareUserId.trim()) return;
    setBusy(true);
    try {
      await storageApi.shareFile(shareFile.id, {
        userIds: [shareUserId.trim()],
        message: shareMessage || null,
        canDownload: true,
        canReshare: false,
      });
      alert('✅ فایل به اشتراک گذاشته شد');
      setShareFile(null);
      setShareUserId('');
      setShareMessage('');
    } catch (e) {
      alert('خطا: ' + (e.response?.data || 'نامشخص'));
    } finally {
      setBusy(false);
    }
  };

  const getIcon = (ext) => {
    if (!ext) return '📄';
    ext = ext.toLowerCase();
    if (['.jpg', '.jpeg', '.png', '.gif', '.webp'].includes(ext)) return '🖼️';
    if (['.mp4', '.webm', '.mov'].includes(ext)) return '🎬';
    if (['.pdf'].includes(ext)) return '📕';
    if (['.zip', '.rar', '.7z'].includes(ext)) return '🗜️';
    if (['.doc', '.docx'].includes(ext)) return '📝';
    if (['.xls', '.xlsx'].includes(ext)) return '📊';
    return '📄';
  };

  if (!files || files.length === 0) {
    return <p style={{ padding: 40, textAlign: 'center', color: '#999' }}>هنوز فایلی نداری</p>;
  }

  return (
    <>
      <div style={{ background: 'white', borderRadius: 8, overflow: 'hidden' }}>
        {files.map(f => (
          <div key={f.id} style={{
            padding: 16, borderBottom: '1px solid #eee',
            display: 'flex', alignItems: 'center', gap: 12,
          }}>
            <div style={{ fontSize: 28 }}>{getIcon(f.extension)}</div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ fontWeight: 'bold', fontSize: 14 }}>{f.originalName}</div>
              <div style={{ fontSize: 12, color: '#666', marginTop: 2 }}>
                {formatSize(f.fileSize)} • {new Date(f.createdAt).toLocaleDateString('fa-IR')}
                {f.isPublic && <span style={{ marginRight: 8, color: '#007185' }}>🌐 عمومی</span>}
              </div>
            </div>
            {showActions && (
              <div style={{ display: 'flex', gap: 6 }}>
                <button onClick={() => handleDownload(f)} title="دانلود"
                  style={{ padding: '6px 10px', background: '#f0f0f0', border: 'none', borderRadius: 4, cursor: 'pointer' }}>⬇️</button>
                <button onClick={() => setShareFile(f)} title="اشتراک"
                  style={{ padding: '6px 10px', background: '#f0f0f0', border: 'none', borderRadius: 4, cursor: 'pointer' }}>🔗</button>
                <button onClick={() => handleDelete(f)} title="حذف"
                  style={{ padding: '6px 10px', background: '#fff0f0', border: 'none', borderRadius: 4, cursor: 'pointer' }}>🗑️</button>
              </div>
            )}
          </div>
        ))}
      </div>

      {shareFile && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(0,0,0,0.5)', display: 'flex',
          alignItems: 'center', justifyContent: 'center', zIndex: 1000,
        }} onClick={() => setShareFile(null)}>
          <div onClick={e => e.stopPropagation()} style={{
            background: 'white', padding: 24, borderRadius: 8,
            width: 400, maxWidth: '90%',
          }}>
            <h3 style={{ marginBottom: 16 }}>اشتراک فایل: {shareFile.originalName}</h3>
            <input
              placeholder="User ID (Guid) کاربر مقصد"
              value={shareUserId}
              onChange={e => setShareUserId(e.target.value)}
              style={{ width: '100%', padding: 10, border: '1px solid #ccc', borderRadius: 4, marginBottom: 12 }}
            />
            <textarea
              placeholder="پیام (اختیاری)"
              value={shareMessage}
              onChange={e => setShareMessage(e.target.value)}
              rows={3}
              style={{ width: '100%', padding: 10, border: '1px solid #ccc', borderRadius: 4, marginBottom: 12 }}
            />
            <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
              <button onClick={() => setShareFile(null)} style={{ padding: '8px 16px', background: '#f0f0f0', border: 'none', borderRadius: 4, cursor: 'pointer' }}>لغو</button>
              <button onClick={handleShare} disabled={busy} style={{ padding: '8px 16px', background: '#FFD814', border: '1px solid #FCD200', borderRadius: 4, cursor: 'pointer', fontWeight: 'bold' }}>
                {busy ? '...' : 'اشتراک'}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
