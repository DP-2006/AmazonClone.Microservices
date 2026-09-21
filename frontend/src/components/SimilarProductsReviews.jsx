import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api, { reviewsApi } from '../api/client';
import StarRating from './StarRating';

export default function SimilarProductsReviews({ categoryId, excludeProductId }) {
  const [items, setItems] = useState([]);

  useEffect(() => {
    if (!categoryId) return;
    (async () => {
      try {
        const prods = await api.get('/api/products');
        const similar = prods.data
          .filter(p => p.categoryId === categoryId && p.id !== excludeProductId)
          .slice(0, 6);

        if (similar.length === 0) return;

        const ids = similar.map(p => p.id);
        const topRes = await reviewsApi.topByProducts(ids);
        const topMap = topRes.data;

        setItems(similar.map(p => ({ product: p, topReviews: topMap[p.id] || [] })));
      } catch (e) { console.error(e); }
    })();
  }, [categoryId, excludeProductId]);

  if (items.length === 0) return null;

  return (
    <div style={{ marginTop: 50 }}>
      <h2 style={{ marginBottom: 16 }}>نظرات محصولات مشابه</h2>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: 16 }}>
        {items.map(({ product, topReviews }) => (
          <div key={product.id} style={{ background: 'white', padding: 16, borderRadius: 8, border: '1px solid #eee' }}>
            <Link to={`/product/${product.id}`} style={{ textDecoration: 'none', color: '#007185' }}>
              <b style={{ fontSize: 14 }}>{product.name}</b>
            </Link>
            <p style={{ color: '#B12704', fontWeight: 'bold', margin: '6px 0' }}>${product.price}</p>
            {topReviews.length === 0 ? (
              <p style={{ fontSize: 13, color: '#999', fontStyle: 'italic' }}>بدون نظر</p>
            ) : (
              <div>
                {topReviews.map(r => (
                  <div key={r.id} style={{ borderTop: '1px solid #eee', paddingTop: 8, marginTop: 8 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 12 }}>
                      <b>{r.userName}</b>
                      <StarRating value={r.rating} showCount={false} size={12} />
                    </div>
                    <p style={{ fontSize: 13, margin: '4px 0', color: '#444' }}>
                      {r.comment.length > 80 ? r.comment.slice(0, 80) + '...' : r.comment}
                    </p>
                  </div>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
