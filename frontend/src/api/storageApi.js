import api from './client';

export const storageApi = {
  // ===== Files =====
  myFiles: (folderId) => api.get('/api/storage/files/my', { params: { folderId } }),
  sharedWithMe: () => api.get('/api/storage/files/shared-with-me'),
  upload: (file, folderId, description) => {
    const form = new FormData();
    form.append('file', file);
    if (folderId) form.append('folderId', folderId);
    if (description) form.append('description', description);
    return api.post('/api/storage/files/upload', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  download: (id) => `/api/storage/files/${id}/download`,
  updateFile: (id, data) => api.put(`/api/storage/files/${id}`, data),
  deleteFile: (id) => api.delete(`/api/storage/files/${id}`),
  shareFile: (id, data) => api.post(`/api/storage/files/${id}/share`, data),
  unshare: (id, userId) => api.delete(`/api/storage/files/${id}/share/${userId}`),

  // ===== Folders =====
  folders: (parentId) => api.get('/api/storage/folders', { params: { parentId } }),
  createFolder: (data) => api.post('/api/storage/folders', data),
  renameFolder: (id, name) => api.put(`/api/storage/folders/${id}/rename`, { name }),
  deleteFolder: (id) => api.delete(`/api/storage/folders/${id}`),

  // ===== Groups =====
  groups: () => api.get('/api/storage/groups'),
  group: (id) => api.get(`/api/storage/groups/${id}`),
  createGroup: (data) => api.post('/api/storage/groups', data),
  updateGroup: (id, data) => api.put(`/api/storage/groups/${id}`, data),
  deleteGroup: (id) => api.delete(`/api/storage/groups/${id}`),
  addGroupMembers: (id, userIds) => api.post(`/api/storage/groups/${id}/members`, { userIds }),
  removeGroupMember: (id, userId) => api.delete(`/api/storage/groups/${id}/members/${userId}`),
  setGroupPermissions: (id, permissionIds) =>
    api.put(`/api/storage/groups/${id}/permissions`, { permissionIds }),

  // ===== Permissions =====
  permissions: () => api.get('/api/storage/permissions'),
  permissionsByCategory: () => api.get('/api/storage/permissions/by-category'),
  userPermissions: (userId) => api.get(`/api/storage/permissions/user/${userId}`),
  assignPermissions: (data) => api.post('/api/storage/permissions/assign', data),
  revokePermission: (userId, permId) =>
    api.delete(`/api/storage/permissions/user/${userId}/permission/${permId}`),
  myPermissions: () => api.get('/api/storage/permissions/me'),

  // ===== Users =====
  users: (search) => api.get('/api/storage/users', { params: { search } }),
  user: (id) => api.get(`/api/storage/users/${id}`),
  blockUser: (id) => api.post(`/api/storage/users/${id}/block`),
  unblockUser: (id) => api.post(`/api/storage/users/${id}/unblock`),
  messageUser: (id, data) => api.post(`/api/storage/users/${id}/message`, data),
  userActivity: (id) => api.get(`/api/storage/users/${id}/activity-summary`),

  // ===== Settings =====
  settings: () => api.get('/api/storage/settings'),
  updateSettings: (data) => api.put('/api/storage/settings', data),
  stats: () => api.get('/api/storage/settings/stats'),
  cleanup: (daysOld = 30) => api.post(`/api/storage/settings/cleanup?daysOld=${daysOld}`),
};

export default storageApi;
