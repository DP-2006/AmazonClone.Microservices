import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import UsersTab from './tabs/UsersTab';
import GroupsTab from './tabs/GroupsTab';
import PermissionsTab from './tabs/PermissionsTab';
import SettingsTab from './tabs/SettingsTab';
import ActivityTab from './tabs/ActivityTab';
import NotificationsTab from './tabs/NotificationsTab';

export default function AdminPanel() {
  const [params] = useSearchParams();
  const [tab, setTab] = useState(params.get('tab') || 'users');

  const tabs = [
    { id: 'users', label: '👥 کاربران' },
    { id: 'groups', label: '📁 گروه‌ها' },
    { id: 'permissions', label: '🔑 دسترسی‌ها' },
    { id: 'activity', label: '📋 فعالیت‌ها' },
    { id: 'notifications', label: '🔔 اعلان‌ها' },
    { id: 'settings', label: '⚙️ تنظیمات' },
  ];

  return (
    <div style={{ maxWidth: 1300, margin: '0 auto', padding: 20 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h1>🛠️ پنل ادمین Storage</h1>
        <Link to="/storage" style={{ color: '#007185', textDecoration: 'none' }}>← بازگشت به فایل‌ها</Link>
      </div>

      <div style={{ display: 'flex', gap: 6, borderBottom: '2px solid #232f3e', marginBottom: 24, flexWrap: 'wrap' }}>
        {tabs.map(t => (
          <button key={t.id} onClick={() => setTab(t.id)} style={{
            padding: '10px 18px', border: 'none', cursor: 'pointer',
            background: tab === t.id ? '#232f3e' : '#f0f0f0',
            color: tab === t.id ? 'white' : '#333',
            borderTopLeftRadius: 8, borderTopRightRadius: 8,
            fontWeight: 'bold', fontSize: 13,
          }}>{t.label}</button>
        ))}
      </div>

      <div>
        {tab === 'users' && <UsersTab />}
        {tab === 'groups' && <GroupsTab />}
        {tab === 'permissions' && <PermissionsTab />}
        {tab === 'activity' && <ActivityTab />}
        {tab === 'notifications' && <NotificationsTab />}
        {tab === 'settings' && <SettingsTab />}
      </div>
    </div>
  );
}
