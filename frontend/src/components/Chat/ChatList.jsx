export default function ChatList({ rooms, activeRoomId, onSelect }) {
  if (!rooms || rooms.length === 0) {
    return (
      <div style={{ padding: 40, textAlign: 'center', color: '#999' }}>
        <p style={{ fontSize: 40 }}>💬</p>
        <p>هنوز گفت‌وگویی نداری</p>
      </div>
    );
  }

  return (
    <div>
      {rooms.map(room => (
        <div key={room.id}
          onClick={() => onSelect(room)}
          style={{
            padding: 16, borderBottom: '1px solid #eee', cursor: 'pointer',
            background: room.id === activeRoomId ? '#f0f7ff' : 'white',
            display: 'flex', gap: 12, alignItems: 'center',
          }}>
          <div style={{
            width: 44, height: 44, borderRadius: '50%', background: '#232f3e',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: 'white', fontWeight: 'bold',
          }}>{(room.otherUserName || '?')[0].toUpperCase()}</div>
          <div style={{ flex: 1, minWidth: 0 }}>
            <b style={{ fontSize: 14 }}>{room.otherUserName || 'طرف گفت‌وگو'}</b>
            <p style={{
              fontSize: 13, color: '#666', marginTop: 2,
              whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis',
            }}>{room.lastMessagePreview || 'بدون پیام'}</p>
          </div>
          {room.unreadCount > 0 && (
            <span style={{
              background: '#FFD814', color: '#111', borderRadius: 10,
              padding: '2px 8px', fontSize: 12, fontWeight: 'bold',
            }}>{room.unreadCount}</span>
          )}
        </div>
      ))}
    </div>
  );
}
