import { useRef, useState } from 'react';
import storageApi from '../../api/storageApi';

export default function FileUpload({ folderId, onUploaded }) {
  const [uploading, setUploading] = useState(false);
  const [progress, setProgress] = useState(0);
  const [msg, setMsg] = useState('');
  const inputRef = useRef(null);

  const handleFile = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 100 * 1024 * 1024) {
      alert('حجم فایل بیش از 100MB است');
      return;
    }

    setUploading(true);
    setProgress(0);
    setMsg('');

    try {
      await storageApi.upload(file, folderId, '');
      setMsg(`✅ ${file.name} آپلود شد`);
      onUploaded?.();
      setTimeout(() => setMsg(''), 3000);
    } catch (err) {
      setMsg(`❌ ${err.response?.data || 'خطا در آپلود'}`);
    } finally {
      setUploading(false);
      if (inputRef.current) inputRef.current.value = '';
    }
  };

  return (
    <div style={{ padding: 16, background: 'white', borderRadius: 8, marginBottom: 16 }}>
      <input
        type="file"
        ref={inputRef}
        onChange={handleFile}
        style={{ display: 'none' }}
      />
      <button
        onClick={() => inputRef.current?.click()}
        disabled={uploading}
        style={{
          padding: '10px 24px', background: '#FFD814',
          border: '1px solid #FCD200', borderRadius: 20,
          cursor: uploading ? 'wait' : 'pointer',
          fontWeight: 'bold', fontSize: 14,
        }}>
        {uploading ? 'در حال آپلود...' : '📤 آپلود فایل جدید'}
      </button>
      {msg && <span style={{ marginRight: 12, fontSize: 14 }}>{msg}</span>}
    </div>
  );
}
