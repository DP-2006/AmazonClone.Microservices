import { useEffect, useState } from 'react';
import storageApi from '../../../api/storageApi';

const TYPES = [
  'Login','Logout','Upload','Download','Delete','View','Edit','Share','Move',
  'CreateFolder','DeleteFolder','PermissionChange','UserBlocked','UserUnblocked',
  'PasswordChange','Report'
];

export default function ActivityTab() {
  const [stats, setStats] = useState(null);
  const [logs, setLogs] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [msg, setMsg] = useState('');

  const [filter, setFilter] = useState({
    username: '', type: '', search: '',
    fromDate: '', toDate: '', resourceType: '',
  });

  const loadStats = async () => {
    try {
      const r = await storageApi.activityStats();
      setStats(r.data);
    } catch { /* ignore */ }
  };

  const loadLogs = async (p = page) => {
    setLoading(true);
    try {
      const r = await storageApi.activitySearch({
        ...filter,
        fromDate: filter.fromDate || null,
        toDate: filter.toDate || null,
        type: filter.type || null,
        page: p,
        pageSize,
      });
      setLogs(r.data.items);
      setTotal(r.data.total);
      setTotalPages(r.data.totalPages);
      setPage(r.data.page);
    } finally { setLoading(false); }
  };

  useEffect(() => { loadStats(); loadLogs(1); }, []);

  const exportCsv = async () => {
    try {
      const res = await storageApi.activityExport(filter);
      const blob = new Blob([res.data], { type: 'text/csv' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `activity-${Date.now()}.csv`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch { alert('خطا در خروجی'); }
  };

  const clearOld = async () => {
    const days = prompt('حذف لاگ‌های قدیمی‌تر از چند روز؟', '30');
    if (!days) return;
    try {
      const r = await storageApi.activityClear(parseInt(days));
      setMsg(`✅ ${r.data.removed} لاگ حذف شد`);
      loadLogs(1);
    } catch { setMsg('❌ خطا'); }
  };

  const formatTime = (d) => new Date(d).toLocaleString('fa-IR');

  const typeColor = (t) => {
    if (['Delete', 'UserBlocked', 'PermissionChange'].includes(t)) return '#f44336';
    if (['Upload', 'Share'].includes(t)) return '#4caf50';
    if (['Login'].includes(t)) return '#2196f3';
    if (['Download'].includes(t)) return '#ff9800';
    return '#666';
  };

  return (
    <div>
      {msg && <div style={{ padding: 10, background: '#f0f7ff', marginBottom: 12, borderRadius: 4 }}>{msg}</div>}

      {/* آمار */}
      {stats && (
        <div style={{ background: 'white', padding: 16, borderRadius: 8, marginBottom: 16 }}>
          <h3 style={{ marginBottom: 12 }}>📊 آمار لاگ‌ها</h3>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(120px, 1fr))', gap: 12 }}>
            <Box label="کل" value={stats.total} />
            <Box label="امروز" value={stats.today} />
            <Box label="این هفته" value={stats.week} />
            <Box label="این ماه" value={stats.month} />
          </div>

          <div style={{ marginTop: 16, display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 20 }}>
            <div>
              <b style={{ fontSize: 13 }}>پرکاربردترین عملیات:</b>
              {stats.byType?.slice(0, 5).map(t => (
                <div key={t.type} style={{ fontSize: 12, marginTop: 4, display: 'flex', justifyContent: 'space-between' }}>
                  <span style={{ color: typeColor(t.type) }}>● {t.type}</span>
                  <span>{t.count}</span>
                </div>
              ))}
            </div>
            <div>
              <b style={{ fontSize: 13 }}>فعال‌ترین کاربران:</b>
              {stats.topUsers?.slice(0, 5).map((u, i) => (
                <div key={i} style={{ fontSize: 12, marginTop: 4, display: 'flex', justifyContent: 'space-between' }}>
                  <span>{u.username}</span>
                  <span>{u.count}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* فیلتر */}
      <div style={{ background: 'white', padding: 16, borderRadius: 8, marginBottom: 16 }}>
        <h4 style={{ marginBottom: 12 }}>🔍 فیلتر</h4>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: 8 }}>
          <input placeholder="نام کاربر" value={filter.username}
            onChange={e => setFilter({...filter, username: e.target.value})} style={inp} />
          <select value={filter.type} onChange={e => setFilter({...filter, type: e.target.value})} style={inp}>
            <option value="">همه انواع</option>
            {TYPES.map(t => <option key={t} value={t}>{t}</option>)}
          </select>
          <input placeholder="جستجو در متن" value={filter.search}
            onChange={e => setFilter({...filter, search: e.target.value})} style={inp} />
          <input type="date" value={filter.fromDate}
            onChange={e => setFilter({...filter, fromDate: e.target.value})} style={inp} />
          <input type="date" value={filter.toDate}
            onChange={e => setFilter({...filter, toDate: e.target.value})} style={inp} />
        </div>
        <div style={{ marginTop: 12, display: 'flex', gap: 8 }}>
          <button onClick={() => loadLogs(1)} style={btnY}>🔍 اعمال فیلتر</button>
          <button onClick={() => { setFilter({username:'',type:'',search:'',fromDate:'',toDate:'',resourceType:''}); setTimeout(() => loadLogs(1), 100); }} style={btnG}>پاک کردن</button>
          <button onClick={exportCsv} style={btnG}>📥 خروجی CSV</button>
          <button onClick={clearOld} style={{...btnG, color: '#c00'}}>🗑️ پاکسازی قدیمی</button>
        </div>
      </div>

      {/* لیست لاگ‌ها */}
      <div style={{ background: 'white', borderRadius: 8, overflow: 'hidden' }}>
        <div style={{ padding: 12, borderBottom: '1px solid #eee' }}>
          <b>📋 {total} لاگ</b>
        </div>
        {loading ? <p style={{ padding: 30, textAlign: 'center', color: '#999' }}>در حال بارگذاری...</p> : (
          logs.length === 0 ? <p style={{ padding: 30, textAlign: 'center', color: '#999' }}>لاگی یافت نشد</p> : (
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
              <thead style={{ background: '#f8f8f8' }}>
                <tr>
                  <th style={th}>زمان</th>
                  <th style={th}>کاربر</th>
                  <th style={th}>نوع</th>
                  <th style={th}>توضیح</th>
                  <th style={th}>IP</th>
                </tr>
              </thead>
              <tbody>
                {logs.map(l => (
                  <tr key={l.id} style={{ borderBottom: '1px solid #f0f0f0' }}>
                    <td style={td}>{formatTime(l.createdAt)}</td>
                    <td style={td}>{l.username}</td>
                    <td style={{ ...td, color: typeColor(l.type), fontWeight: 'bold' }}>{l.type}</td>
                    <td style={td}>{l.description}</td>
                    <td style={td}>{l.ipAddress || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )
        )}

        {/* صفحه‌بندی */}
        {totalPages > 1 && (
          <div style={{ padding: 12, display: 'flex', gap: 6, justifyContent: 'center' }}>
            <button disabled={page <= 1} onClick={() => loadLogs(page - 1)} style={btnG}>قبلی</button>
            <span style={{ padding: 8 }}>صفحه {page} از {totalPages}</span>
            <button disabled={page >= totalPages} onClick={() => loadLogs(page + 1)} style={btnG}>بعدی</button>
          </div>
        )}
      </div>
    </div>
  );
}

function Box({ label, value }) {
  return (
    <div style={{ padding: 12, background: '#f8f8f8', borderRadius: 8, textAlign: 'center' }}>
      <div style={{ fontSize: 20, fontWeight: 'bold' }}>{value}</div>
      <div style={{ fontSize: 12, color: '#666' }}>{label}</div>
    </div>
  );
}

const inp = { padding: 8, border: '1px solid #ccc', borderRadius: 4, fontSize: 13 };
const th = { padding: 10, textAlign: 'right', fontWeight: 'bold' };
const td = { padding: 10 };
const btnY = { padding: '8px 16px', background: '#FFD814', border: '1px solid #FCD200', borderRadius: 4, cursor: 'pointer', fontWeight: 'bold' };
const btnG = { padding: '8px 16px', background: '#f0f0f0', border: 'none', borderRadius: 4, cursor: 'pointer' };
