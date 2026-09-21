import { useState } from 'react';

export default function MessageBubble({ message, isMine, currentUserId, onReport }) {
  const [showActions, setShowActions] = useState(false);

  const roleColor = {
    Buyer: '#FFD814',
    Seller: '#4CAF50',
    Admin: '#F44336',
  };

  const formatTime = (d) => new Date(d).toLocaleTimeString('fa-IR', {
    hour: '2-digit', minute: '2-digit'
  });

  const formatSize = (bytes) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1024 / 1024).toFixed(1) + ' MB';
  };

  const renderAttachment = (att) => {
    const url = `http://localhost:5007${att.fileUrl}`;
    const mime = att.mimeType || '';

    if (mime.startsWith('image/')) {
      return (
        <img key={att.id} src={url} alt={att.fileName}
          style={{ maxWidth: 300, maxHeight: 300, borderRadius: 8, marginTop: 4, display: 'block' }}
          onClick={() => window.open(url, '_blank')} />
      );
    }
    if (mime.startsWith('video/')) {
      return (
        <video key={att.id} controls style={{ maxWidth: 300, borderRadius: 8, marginTop: 4 }}>
          <source src={url} type={mime} />
        </video>
      );
    }
    return (
      <a key={att.id} href={url} target="_blank" rel="noreferrer"
        style={{ display: 'block', marginTop: 4, color: isMine ? '#fff' : '#007185', fontSize: 13 }}>
        📎 {att.fileName} ({formatSize(att.fileSize)})
      </a>
    );
  };

  if (message.isDeleted) {
    return (
      <div style={{ textAlign: isMine ? 'right' : 'left', margin: '4px 0' }}>
        <span style={{ fontSize: 12, color: '#999', fontStyle: 'italic' }}>
          🚫 پیام حذف شد
        </span>
      </div>
    );
  }

  return (
    <div
      style={{ display: 'flex', justifyContent: isMine ? 'flex-end' : 'flex-start', margin: '6px 0' }}
      onMouseEnter={() => setShowActions(true)}
      onMouseLeave={() => setShowActions(false)}
    >
      <div style={{ maxWidth: '70%', position: 'relative' }}>
        {!isMine && (
          <div style={{ fontSize: 12, color: '#666', marginBottom: 2 }}>
            {message.senderName}
            <span style={{
              background: roleColor[message.senderRole] || '#ccc',
              color: 'white', padding: '1px 6px', borderRadius: 4,
              marginLeft: 6, fontSize: 10,
            }}>
              {message.senderRole === 'Seller' ? 'فروشنده' : message.senderRole === 'Buyer' ? 'خریدار' : 'ادمین'}
            </span>
          </div>
        )}

        <div style={{
          background: isMine ? '#FFD814' : 'white',
          color: '#111',
          padding: '8px 12px',
          borderRadius: 12,
          border: isMine ? 'none' : '1px solid #eee',
          wordBreak: 'break-word',
          fontSize: 14,
          lineHeight: 1.5,
        }}>
          {message.content && <div>{message.content}</div>}
          {message.attachments?.map(renderAttachment)}
          <div style={{ fontSize: 10, color: '#888', marginTop: 4, textAlign: 'right' }}>
            {formatTime(message.createdAt)}
            {isMine && message.readAt && <span style={{ marginLeft: 4 }}>✓✓</span>}
            {isMine && !message.readAt && <span style={{ marginLeft: 4, color: '#bbb' }}>✓</span>}
          </div>
        </div>

        {showActions && !isMine && onReport && (
          <button onClick={() => onReport(message.id)} style={{
            position: 'absolute', top: -20, [isMine ? 'left' : 'right']: 0,
            background: '#f44336', color: 'white', border: 'none',
            borderRadius: 4, padding: '2px 6px', fontSize: 10, cursor: 'pointer',
          }}>گزارش</button>
        )}
      </div>
    </div>
  );
}
