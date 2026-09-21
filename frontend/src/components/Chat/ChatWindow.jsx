import { useEffect, useRef, useState } from 'react';
import chatApi from '../../api/chatApi';
import { useAuth } from '../../context/AuthContext';
import MessageBubble from './MessageBubble';

export default function ChatWindow({ room, onBack }) {
  const [messages, setMessages] = useState([]);
  const [text, setText] = useState('');
  const [loading, setLoading] = useState(false);
  const [sending, setSending] = useState(false);
  const { user } = useAuth();
  const scrollRef = useRef(null);
  const fileInputRef = useRef(null);

  const currentUserId = user?.id || user?.userId;

  const load = async () => {
    if (!room?.id) return;
    setLoading(true);
    try {
      const r = await chatApi.getMessages(room.id);
      setMessages(r.data.items.reverse());
    } finally { setLoading(false); }
  };

  useEffect(() => {
    load();
    if (window.__chatHub) {
      window.__chatHub.joinRoom(room.id);
      return () => window.__chatHub.leaveRoom(room.id);
    }
  }, [room?.id]);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages]);

  const handleSend = async (e) => {
    e?.preventDefault();
    if (!text.trim()) return;
    setSending(true);
    try {
      await chatApi.sendMessage(room.id, { content: text, type: 'Text' });
      setText('');
      await load();
    } finally { setSending(false); }
  };

  const handleFile = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 10 * 1024 * 1024) { alert('حجم فایل بیش از 10MB است'); return; }
    setSending(true);
    try {
      await chatApi.uploadFile(room.id, file);
      await load();
    } catch (err) {
      alert('خطا: ' + (err.response?.data || 'نامشخص'));
    } finally {
      setSending(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const handleReport = async (messageId) => {
    const reason = prompt('دلیل گزارش:');
    if (!reason?.trim()) return;
    try {
      await chatApi.reportMessage(messageId, reason);
      alert('گزارش ثبت شد');
    } catch (err) {
      alert('خطا: ' + (err.response?.data || 'نامشخص'));
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100vh', background: '#f8f8f8' }}>
      <div style={{ padding: 16, background: '#232f3e', color: 'white', display: 'flex', alignItems: 'center', gap: 12 }}>
        <button onClick={onBack} style={{ background: 'none', border: 'none', color: 'white', fontSize: 20, cursor: 'pointer' }}>←</button>
        <b>{room.otherUserName || 'طرف گفت‌وگو'}</b>
      </div>

      <div ref={scrollRef} style={{ flex: 1, overflowY: 'auto', padding: 16 }}>
        {loading && <p style={{ textAlign: 'center', color: '#999' }}>در حال بارگذاری...</p>}
        {messages.length === 0 && !loading && <p style={{ textAlign: 'center', color: '#999', marginTop: 40 }}>شروع گفت‌وگو...</p>}
        {messages.map(m => (
          <MessageBubble key={m.id} message={m}
            isMine={m.senderId === currentUserId}
            currentUserId={currentUserId}
            onReport={handleReport} />
        ))}
      </div>

      <form onSubmit={handleSend} style={{ padding: 12, background: 'white', borderTop: '1px solid #ddd', display: 'flex', gap: 8, alignItems: 'center' }}>
        <input type="file" ref={fileInputRef} onChange={handleFile}
          style={{ display: 'none' }}
          accept="image/*,video/*,.pdf,.zip,.docx,.txt" />
        <button type="button" onClick={() => fileInputRef.current?.click()}
          style={{ background: '#f0f0f0', border: '1px solid #ccc', borderRadius: '50%', width: 40, height: 40, cursor: 'pointer', fontSize: 18 }}>📎</button>
        <input value={text} onChange={e => setText(e.target.value)}
          placeholder="پیام خود را بنویسید..."
          style={{ flex: 1, padding: 10, border: '1px solid #ccc', borderRadius: 20 }} />
        <button type="submit" disabled={sending || !text.trim()} style={{
          padding: '10px 20px', background: '#FFD814', border: '1px solid #FCD200',
          borderRadius: 20, cursor: 'pointer', fontWeight: 'bold',
        }}>{sending ? '...' : 'ارسال'}</button>
      </form>
    </div>
  );
}
