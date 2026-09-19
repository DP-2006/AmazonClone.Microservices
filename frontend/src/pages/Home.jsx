import { useEffect, useState } from 'react';
import api from '../api/client';
import ProductCard from '../components/ProductCard';

export default function Home() {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      api.get('/api/products'),
      api.get('/api/categories'),
    ]).then(([p, c]) => {
      setProducts(p.data);
      setCategories(c.data);
    }).catch(err => console.error(err))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div style={{ padding: 40 }}>Loading products...</div>;

  const filtered = selectedCategory
    ? products.filter(p => p.categoryId === selectedCategory)
    : products;

  return (
    <div>
      <div style={{
        background: 'linear-gradient(to bottom, #232f3e, #37475A)',
        color: 'white', padding: '40px 20px', textAlign: 'center',
      }}>
        <h1 style={{ fontSize: 32, marginBottom: 8 }}>Welcome to AmazonClone</h1>
        <p style={{ fontSize: 16, opacity: 0.9 }}>Millions of products at your fingertips</p>
      </div>

      <div style={{ maxWidth: 1200, margin: '0 auto', padding: 24 }}>
        <h2 style={{ marginBottom: 12 }}>Categories</h2>
        <div style={{ display: 'flex', gap: 8, marginBottom: 32, flexWrap: 'wrap' }}>
          <button onClick={() => setSelectedCategory(null)} style={{
            background: selectedCategory === null ? '#FFD814' : '#eee',
            padding: '8px 18px', border: 'none', borderRadius: 20, cursor: 'pointer',
            fontWeight: selectedCategory === null ? 'bold' : 'normal',
          }}>
            All
          </button>
          {categories.map(c => (
            <button key={c.id} onClick={() => setSelectedCategory(c.id)} style={{
              background: selectedCategory === c.id ? '#FFD814' : '#eee',
              padding: '8px 18px', border: 'none', borderRadius: 20, cursor: 'pointer',
              fontWeight: selectedCategory === c.id ? 'bold' : 'normal',
            }}>
              {c.name}
            </button>
          ))}
        </div>

        <h2 style={{ marginBottom: 16 }}>Products ({filtered.length})</h2>
        <div style={{ display: 'flex', gap: 20, flexWrap: 'wrap' }}>
          {filtered.map(p => <ProductCard key={p.id} product={p} />)}
        </div>

        {filtered.length === 0 && (
          <p style={{ color: '#666', padding: 40, textAlign: 'center' }}>
            No products yet. Add some via{' '}
            <a href="http://localhost:5001/swagger" target="_blank" rel="noreferrer">Swagger</a>
          </p>
        )}
      </div>
    </div>
  );
}
