import api from './client';

export const chatApi = {
  // Rooms
  listRooms: () => api.get('/api/chat/rooms'),
  createRoom: (sellerId) => api.post('/api/chat/rooms', { sellerId }),
  getRoom: (id) => api.get(`/api/chat/rooms/${id}`),

  // Messages
  getMessages: (roomId, page = 1, pageSize = 50) =>
    api.get(`/api/chat/rooms/${roomId}/messages?page=${page}&pageSize=${pageSize}`),
  sendMessage: (roomId, data) => api.post(`/api/chat/rooms/${roomId}/messages`, data),
  uploadFile: (roomId, file) => {
    const form = new FormData();
    form.append('file', file);
    return api.post(`/api/chat/rooms/${roomId}/upload`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },

  // Actions
  toggleChat: (roomId) => api.post(`/api/chat/rooms/${roomId}/toggle`),
  reportMessage: (messageId, reason) =>
    api.post(`/api/chat/messages/${messageId}/report`, { reason }),
  deleteMessage: (messageId) => api.delete(`/api/chat/messages/${messageId}`),
};

export default chatApi;
