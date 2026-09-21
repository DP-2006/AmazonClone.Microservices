import { useEffect, useRef, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

const HUB_URL = 'http://localhost:5007/hubs/chat';

export default function useChatHub() {
  const [connected, setConnected] = useState(false);
  const [typingUsers, setTypingUsers] = useState({});
  const connRef = useRef(null);
  const handlersRef = useRef({ onNewMessage: [], onMessageRead: [], onUserTyping: [] });

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) return;

    const conn = new signalR.HubConnectionBuilder()
      .withUrl(`${HUB_URL}?access_token=${token}`)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    conn.on('NewMessage', (msg) => {
      handlersRef.current.onNewMessage.forEach(fn => fn(msg));
    });
    conn.on('MessageRead', (data) => {
      handlersRef.current.onMessageRead.forEach(fn => fn(data));
    });
    conn.on('UserTyping', (data) => {
      setTypingUsers(prev => ({ ...prev, [data.roomId]: data.userName }));
      setTimeout(() => {
        setTypingUsers(prev => {
          const n = { ...prev };
          delete n[data.roomId];
          return n;
        });
      }, 3000);
      handlersRef.current.onUserTyping.forEach(fn => fn(data));
    });

    conn.start()
      .then(() => setConnected(true))
      .catch(err => console.error('SignalR:', err));

    conn.onreconnected(() => setConnected(true));
    conn.onreconnecting(() => setConnected(false));
    conn.onclose(() => setConnected(false));

    connRef.current = conn;
    return () => { conn.stop(); };
  }, []);

  const on = useCallback((event, fn) => {
    if (!handlersRef.current[event]) handlersRef.current[event] = [];
    handlersRef.current[event].push(fn);
    return () => {
      handlersRef.current[event] = handlersRef.current[event].filter(f => f !== fn);
    };
  }, []);

  const joinRoom = useCallback((roomId) => {
    if (connRef.current?.state === signalR.HubConnectionState.Connected)
      return connRef.current.invoke('JoinRoom', roomId);
  }, []);

  const leaveRoom = useCallback((roomId) => {
    if (connRef.current?.state === signalR.HubConnectionState.Connected)
      return connRef.current.invoke('LeaveRoom', roomId);
  }, []);

  const typing = useCallback((roomId) => {
    if (connRef.current?.state === signalR.HubConnectionState.Connected)
      return connRef.current.invoke('Typing', roomId);
  }, []);

  const markRead = useCallback((messageId) => {
    if (connRef.current?.state === signalR.HubConnectionState.Connected)
      return connRef.current.invoke('MarkRead', messageId);
  }, []);

  return { connected, typingUsers, on, joinRoom, leaveRoom, typing, markRead };
}
