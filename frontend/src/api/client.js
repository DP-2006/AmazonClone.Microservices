import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(err);
  }
);

// ===== Reviews API =====
export const reviewsApi = {
  listByProduct: (productId) => api.get(`/api/reviews/product/${productId}`),
  summary: (productId) => api.get(`/api/reviews/product/${productId}/summary`),
  topByProducts: (productIds) => api.post('/api/reviews/top-by-products', productIds),
  create: (data) => api.post('/api/reviews', data),
  update: (id, data) => api.put(`/api/reviews/${id}`, data),
  remove: (id) => api.delete(`/api/reviews/${id}`),
  react: (id, type) => api.post(`/api/reviews/${id}/react`, { type }),
  report: (id, reason) => api.post(`/api/reviews/${id}/report`, { reason }),
};

// ===== Profile API =====
export const profileApi = {
  me: () => api.get('/api/profile/me'),
  update: (data) => api.put('/api/profile/me', data),
  changePassword: (data) => api.post('/api/profile/change-password', data),
  changeEmail: (data) => api.post('/api/profile/change-email', data),
  becomeSeller: (data) => api.post('/api/profile/become-seller', data),
};

// ===== Products API (فروشنده) =====
export const productsApi = {
  myProducts: () => api.get('/api/products/my-products'),
  create: (data) => api.post('/api/products', data),
  update: (id, data) => api.put(`/api/products/${id}`, data),
  remove: (id) => api.delete(`/api/products/${id}`),
};

export default api;
