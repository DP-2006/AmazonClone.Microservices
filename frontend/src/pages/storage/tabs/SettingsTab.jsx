import { useEffect, useState } from 'react';
import storageApi from '../../../api/storageApi';

export default function SettingsTab() {
  const [settings, setSettings] = useState(null);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [msg, setMsg] = useState('');
  const [edit, setEdit] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const [s, st] = await Promise.all([
        storageApi.settings(),
        storageApi.stats(),
      ]);
      setSettings(s.data);
      setStats(st.data);
    } finally { setLoading(false); }
  };

  useEffect(() => { load(); }, []);

  const save = async () => {
    try {
      await storageApi.updateSettings({
        maxUploadSizeMB: settings.maxUploadSizeMB,
        maxDownloadSizeMB: settings.maxDownloadSizeMB,
        warningThresholdMB: settings.warningThresholdMB,
        maxUserStorageMB: settings.maxUserStorageMB,
        allowLargeFiles: settings.allowLargeFiles,
        allowedExtensions: settings.allowedExtensions,
        blockedExtensions: settings.blockedExtensions,
      });
      setMsg('✅ ذخیره شد');
      setEdit(false);
      load();
    } catch (e) {
      setMsg('❌ ' + (e.response?.data || 'خطا'));
    }
  };

  const cleanup = async () => {
    const days = prompt('حذف لاگ‌های قدیمی‌تر از چند روز؟', '30');
    if (!days) return;
    try {
      const r = await storageApi.cleanup(parseInt(days));
      alert(`✅ ${r.data.removedLogs} لاگ و ${r.data.removedNotifications} نوتیفیکیشن حذف شد`);
      load();
    } catch { alert('خطا'); }
  };

  if (loading) return <p>...</p>;

  const formatSize = (mb) => mb > 1024 ? (mb / 1024).toFixed(2) + ' GB' : mb + ' MB';

  return (
    <div>
      {msg && <div style={{ padding: 10, background: '#f0f7ff', marginBottom: 12, borderRadius: 4 }}>{msg}</div>}

      {/* آمار سیستم */}
      {stats && (
        <div style={{ background: 'white', padding: 20, borderRadius: 8, marginBottom: 20 }}>
          <h3 style={{ marginBottom: 16 }}>📊 آمار کلی سیستم</h3>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: 16 }}>
            <StatBox label="فایل‌ها" value={stats.files.total} icon="📄" />
            <StatBox label="حجم کل" value={stats.files.sizeMB + ' MB'} icon="💾" />
            <StatBox label="پوشه‌ها" value={stats.folders} icon="📁" />
            <StatBox label="اشتراک‌ها" value={stats.shares} icon="🔗" />
            <StatBox label="گروه‌ها" value={stats.groups} icon="👥" />
            <StatBox label="دسترسی‌ها" value={stats.permissions} icon="🔑" />
            <StatBox label="کاربران فعال" value={stats.activeUsers} icon="🧑" />
            <StatBox label="لاگ‌ها (کل)" value={stats.logs.total} icon="📋" />
            <StatBox label="لاگ‌های امروز" value={stats.logs.today} icon="🆕" />
          </div>
        </div>
      )}

      {/* تنظیمات */}
      <div style={{ background: 'white', padding: 20, borderRadius: 8 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
          <h3>⚙️ تنظیمات Storage</h3>
          {!edit ? (
            <button onClick={() => setEdit(true)} style={btnY}>✏️ ویرایش</button>
          ) : (
            <div style={{ display: 'flex', gap: 8 }}>
              <button onClick={() => { setEdit(false); load(); }} style={btnG}>لغو</button>
              <button onClick={save} style={btnY}>💾 ذخیره</button>
            </div>
          )}
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: 16 }}>
          <Field label="حداکثر حجم آپلود" value={settings.maxUploadSizeMB + ' MB'}
            editing={edit} type="number" onChange={v => setSettings({...settings, maxUploadSizeMB: parseInt(v)})} />
          <Field label="حداکثر حجم دانلود" value={settings.maxDownloadSizeMB + ' MB'}
            editing={edit} type="number" onChange={v => setSettings({...settings, maxDownloadSizeMB: parseInt(v)})} />
          <Field label="آستانه هشدار" value={settings.warningThresholdMB + ' MB'}
            editing={edit} type="number" onChange={v => setSettings({...settings, warningThresholdMB: parseInt(v)})} />
          <Field label="سهمیه هر کاربر" value={formatSize(settings.maxUserStorageMB)}
            editing={edit} type="number" onChange={v => setSettings({...settings, maxUserStorageMB: parseInt(v)})} />
        </div>

        <div style={{ marginTop: 20 }}>
          <p style={{ fontSize: 13, color: '#666' }}>پسوندهای مسدود:</p>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6, marginTop: 8 }}>
            {settings.blockedExtensions.map(e => (
              <span key={e} style={{ background: '#fff0f0', padding: '4px 10px', borderRadius: 4, fontSize: 13 }}>
                {e}
              </span>
            ))}
          </div>
        </div>

        <div style={{ marginTop: 20, paddingTop: 20, borderTop: '1px solid #eee' }}>
          <h4>🧹 پاکسازی</h4>
          <p style={{ fontSize: 13, color: '#666', marginTop: 4 }}>
            حذف لاگ‌ها و نوتیفیکیشن‌های قدیمی برای آزادسازی فضا
          </p>
          <button onClick={cleanup} style={{...btnG, marginTop: 12}}>🧹 پاکسازی</button>
        </div>
      </div>
    </div>
  );
}

function StatBox({ label, value, icon }) {
  return (
    <div style={{ background: '#f8f8f8', padding: 16, borderRadius: 8 }}>
      <div style={{ fontSize: 24 }}>{icon}</div>
      <div style={{ fontSize: 22, fontWeight: 'bold', marginTop: 4 }}>{value}</div>
      <div style={{ fontSize: 12, color: '#666', marginTop: 2 }}>{label}</div>
    </div>
  );
}

function Field({ label, value, editing, type = 'text', onChange }) {
  return (
    <div>
      <label style={{ fontSize: 13, color: '#666', display: 'block', marginBottom: 4 }}>{label}</label>
      {editing ? (
        <input type={type} value={value.replace(/ [A-Z]+$/, '')}
          onChange={e => onChange(e.target.value)} style={inp} />
      ) : (
        <div style={{ padding: 10, background: '#f8f8f8', borderRadius: 4, fontSize: 14 }}>{value}</div>
      )}
    </div>
  );
}

const inp = { width: '100%', padding: 10, border: '1px solid #ccc', borderRadius: 4 };
const btnY = { padding: '8px 16px', background: '#FFD814', border: '1px solid #FCD200', borderRadius: 4, cursor: 'pointer', fontWeight: 'bold' };
const btnG = { padding: '8px 16px', background: '#f0f0f0', border: 'none', borderRadius: 4, cursor: 'pointer' };
