import { Link } from 'react-router-dom';

export default function ProductCard({ product }) {
  return (
    <div style={{
      border: '1px solid #ddd', borderRadius: 8, padding: 16,
      width: 220, background: 'white', display: 'flex', flexDirection: 'column',
    }}>
      <div style={{
        height: 160, background: '#f3f3f3', borderRadius: 4, marginBottom: 12,
        display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 60,
      }}>
        📦
      </div>
      <h3 style={{ fontSize: 15, margin: '4px 0', height: 42, overflow: 'hidden' }}>
        {product.name}
      </h3>
      <p style={{ color: '#B12704', fontWeight: 'bold', fontSize: 20, margin: '8px 0' }}>
        ${product.price}
      </p>
      <Link to={`/product/${product.id}`} style={{
        display: 'block', textAlign: 'center', background: '#FFD814',
        padding: '10px', borderRadius: 20, textDecoration: 'none',
        color: 'black', fontWeight: 'bold', marginTop: 'auto',
      }}>
        View details
      </Link>
    </div>
  );
}
