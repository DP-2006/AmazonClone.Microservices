import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import storageApi from '../../api/storageApi';
import { useAuth } from '../../context/AuthContext';
import FileUpload from '../../components/storage/FileUpload';
import FileList from '../../components/storage/FileList';
import FolderList from '../../components/storage/FolderList';

export default function MyStorage() {
  const { user } = useAuth();
  const [tab, setTab] = useState('my');
  const [files, setFiles] = useState([]);
  const [shared, setShared] = useState([]);
  const [folders, setFolders] = useState([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    setLoading(true);
    try {
      const [filesRes, foldersRes, sharedRes] = await Promise.all([
        storageApi.myFiles(),
        storageApi.folders(),
        storageApi.sharedWithMe(),
      ]);
      setFiles(filesRes.data);
      setFolders(foldersRes.data);
      setShared(sharedRes.data);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const totalSize = files.reduce((s, f) => s + f.fileSize, 0);
  const formatSize = (bytes) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    if (bytes < 1024 * 1024 * 1024) return (bytes / 1024 / 1024).toFixed(1) + ' MB';
    return (bytes / 1024 / 1024 / 1024).toFixed(2) + ' GB';
  };

  return (
    <div style={{ maxWidth: 1100, margin: '0 auto', padding: 30 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <div>
          <h1 style={{ marginBottom: 4 }}>فضای ذخیره‌سازی من</h1>
          <p style={{ color: '#666', fontSize: 14 }}>
            {files.length} فایل • {formatSize(totalSize)} مصرف شده
          </p>
        </div>
        <Link to="/storage/admin" style={{
          padding: '10px 20px', background: '#232f3e', color: 'white',
          borderRadius: 20, textDecoration: 'none', fontWeight: 'bold', fontSize: 14,
        }}>
          🛠️ پنل ادمین
        </Link>
      </div>

      {/* Tabs */}
      <div style={{ display: 'flex', gap: 8, borderBottom: '1px solid #ddd', marginBottom: 20 }}>
        {[
          { id: 'my', label: `📄 فایل‌های من (${files.length})` },
          { id: 'folders', label: `📁 پوشه‌ها (${folders.length})` },
          { id: 'shared', label: `🔗 اشتراکی (${shared.length})` },
        ].map(t => (
          <button key={t.id} onClick={() => setTab(t.id)} style={{
            padding: '10px 16px', border: 'none', cursor: 'pointer',
            background: tab === t.id ? '#f0f0f0' : 'transparent',
            borderBottom: tab === t.id ? '2px solid #FF9900' : '2px solid transparent',
            fontWeight: tab === t.id ? 'bold' : 'normal', fontSize: 14,
          }}>{t.label}</button>
        ))}
      </div>

      {loading ? (
        <p style={{ textAlign: 'center', color: '#999', padding: 40 }}>در حال بارگذاری...</p>
      ) : (
        <>
          {tab === 'my' && (
            <>
              <FileUpload onUploaded={load} />
              <FileList files={files} onRefresh={load} />
            </>
          )}

          {tab === 'folders' && (
            <FolderList folders={folders} onRefresh={load} />
          )}

          {tab === 'shared' && (
            shared.length === 0 ? (
              <p style={{ padding: 40, textAlign: 'center', color: '#999' }}>
                هنوز فایلی با شما به اشتراک گذاشته نشده
              </p>
            ) : (
              <div style={{ background: 'white', borderRadius: 8 }}>
                {shared.map(s => (
                  <div key={s.id} style={{ padding: 16, borderBottom: '1px solid #eee' }}>
                    <div style={{ fontWeight: 'bold' }}>{s.fileName}</div>
                    {s.message && <p style={{ fontSize: 13, color: '#666', marginTop: 4 }}>{s.message}</p>}
                    <p style={{ fontSize: 12, color: '#999', marginTop: 4 }}>
                      اشتراک در {new Date(s.createdAt).toLocaleDateString('fa-IR')}
                    </p>
                  </div>
                ))}
              </div>
            )
          )}
        </>
      )}
    </div>
  );
}
