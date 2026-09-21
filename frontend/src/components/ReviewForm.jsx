import { useState, useEffect } from 'react';
import StarRating from './StarRating';

export default function ReviewForm({ onSubmit, initial = null, submitLabel = 'ثبت نظر' }) {
  const [rating, setRating] = useState(initial?.rating || 5);
  const [title, setTitle] = useState(initial?.title || '');
  const [comment, setComment] = useState(initial?.comment || '');
  const [loading, setLoading] = useState(false);
  const [msg, setMsg] = useState('');

  useEffect(() => {
    if (initial) {
      setRating(initial.rating);
      setTitle(initial.title || '');
      setComment(initial.comment || '');
    }
  }, [initial]);

  const submit = async (e) => {
    e.preventDefault();
    if (!comment.trim()) return;
    setLoading(true);
    try {
      await onSubmit({ rating, title, comment });
      setMsg('✅ ثبت شد');
      if (!initial) { setComment(''); setTitle(''); setRating(5); }
      setTimeout(() => setMsg(''), 2500);
    } catch (err) {
      setMsg('❌ ' + (err.response?.data?.message || err.response?.data || 'خطا'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={submit} style={{ background: 'white', padding: 20, borderRadius: 8, marginBottom: 20 }}>
      <h3 style={{ marginBottom: 12, fontSize: 16 }}>{initial ? 'ویرایش نظر' : 'ثبت نظر شما'}</h3>

      <div style={{ marginBottom: 12 }}>
        <label style={{ display: 'block', marginBottom: 6, fontSize: 14 }}>امتیاز:</label>
        <StarRating value={rating} showCount={false} size={28} onChange={setRating} />
      </div>

      <div style={{ marginBottom: 12 }}>
        <input
          value={title}
          onChange={e => setTitle(e.target.value)}
          placeholder="عنوان (اختیاری)"
          style={{ width: '100%', padding: 10, border: '1px solid #ccc', borderRadius: 4 }}
        />
      </div>

      <div style={{ marginBottom: 12 }}>
        <textarea
          value={comment}
          onChange={e => setComment(e.target.value)}
          placeholder="نظر شما..."
          rows={4}
          required
          style={{ width: '100%', padding: 10, border: '1px solid #ccc', borderRadius: 4, resize: 'vertical' }}
        />
      </div>

      <button type="submit" disabled={loading} style={{
        padding: '10px 24px', background: '#FFD814', border: '1px solid #FCD200',
        borderRadius: 20, cursor: 'pointer', fontWeight: 'bold',
      }}>
        {loading ? '...' : submitLabel}
      </button>

      {msg && <span style={{ marginLeft: 12, fontSize: 14 }}>{msg}</span>}
    </form>
  );
}
