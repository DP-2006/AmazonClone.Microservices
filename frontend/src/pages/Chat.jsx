import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import chatApi from '../api/chatApi';
import useChatHub from '../hooks/useChatHub';
import ChatList from '../components/Chat/ChatList';
import ChatWindow from '../components/Chat/ChatWindow';

export default function Chat() {
  const { roomId } = useParams();
  const navigate = useNavigate();
  const [rooms, setRooms] = useState([]);
  const [activeRoom, setActiveRoom] = useState(null);
  const [loading, setLoading] = useState(true);
  const hub = useChatHub();

  useEffect(() => { window.__chatHub = hub; }, [hub]);

  const load = async () => {
    try {
      const r = await chatApi.listRooms();
      setRooms(r.data);
      if (roomId) {
        const active = r.data.find(x => x.id === roomId);
        if (active) setActiveRoom(active);
      }
    } finally { setLoading(false); }
  };

  useEffect(() => { load(); }, [roomId]);

  useEffect(() => {
    const off = hub.on('onNewMessage', () => load());
    return off;
  }, [hub]);

  const selectRoom = (room) => {
    setActiveRoom(room);
    navigate(`/chat/${room.id}`);
  };

  return (
    <div style={{ display: 'flex', height: 'calc(100vh - 60px)' }}>
      <div style={{ width: 340, background: 'white', borderRight: '1px solid #ddd', overflowY: 'auto' }}>
        <div style={{ padding: 16, borderBottom: '1px solid #eee' }}>
          <h2 style={{ fontSize: 18 }}>پیام‌ها</h2>
          {hub.connected && <span style={{ fontSize: 11, color: 'green' }}>● متصل</span>}
          {!hub.connected && <span style={{ fontSize: 11, color: 'red' }}>● قطع</span>}
        </div>
        {loading ? <p style={{ padding: 20, color: '#999' }}>...</p>
          : <ChatList rooms={rooms} activeRoomId={activeRoom?.id} onSelect={selectRoom} />}
      </div>

      <div style={{ flex: 1, background: '#f8f8f8' }}>
        {activeRoom ? <ChatWindow room={activeRoom} onBack={() => setActiveRoom(null)} /> : (
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%', color: '#999' }}>
            <div style={{ textAlign: 'center' }}>
              <p style={{ fontSize: 60 }}>💬</p>
              <p>یک گفت‌وگو انتخاب کنید</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
