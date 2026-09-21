import { useEffect, useState } from 'react';
import { reviewsApi } from '../api/client';
import { useAuth } from '../context/AuthContext';
import ReviewList from './ReviewList';
import ReviewForm from './ReviewForm';
import StarRating from './StarRating';

export default function CommentsSection({ productId }) {
  const [reviews, setReviews] = useState([]);
  const [summary, setSummary] = useState(null);
  const [myReview, setMyReview] = useState(null);
  const [editing, setEditing] = useState(false);
  const { user } = useAuth();

  const load = async () => {
    try {
      const [r, s] = await Promise.all([
        reviewsApi.listByProduct(productId),
        reviewsApi.summary(productId),
      ]);
      setReviews(r.data);
      setSummary(s.data);

      if (user) {
        const me = r.data.find(x => x.userName === user.fullName);
        setMyReview(me || null);
      }
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => { if (productId) load(); }, [productId, user]);

  const handleCreate = async (data) => {
    await reviewsApi.create({ ...data, productId });
    await load();
  };

  const handleUpdate = async (data) => {
    await reviewsApi.update(myReview.id, data);
    setEditing(false);
    await load();
  };

  const handleReply = async (parentId, text) => {
    await reviewsApi.create({
      productId, rating: 5, title: null, comment: text, parentReviewId: parentId,
    });
    await load();
  };

  const handleReact = async (reviewId, type) => {
    if (!user) return;
    try {
      await reviewsApi.react(reviewId, type);
      await load();
    } catch (e) { console.error(e); }
  };

  return (
    <div style={{ marginTop: 40 }}>
      <h2 style={{ marginBottom: 16 }}>نظرات کاربران</h2>

      {summary && (
        <div style={{ background: 'white', padding: 20, borderRadius: 8, marginBottom: 20,
          display: 'flex', gap: 30, alignItems: 'center' }}>
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontSize: 36, fontWeight: 'bold' }}>{summary.averageRating.toFixed(1)}</div>
            <StarRating value={summary.averageRating} showCount={false} size={18} />
            <div style={{ fontSize: 13, color: '#666', marginTop: 4 }}>{summary.totalCount} نظر</div>
          </div>
          <div style={{ flex: 1 }}>
            {[5,4,3,2,1].map(n => (
              <div key={n} style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13, marginBottom: 2 }}>
                <span>{n} ★</span>
                <div style={{ flex: 1, height: 8, background: '#eee', borderRadius: 4, overflow: 'hidden' }}>
                  <div style={{
                    width: summary.totalCount ? `${(summary[`star${n}`] / summary.totalCount) * 100}%` : '0%',
                    height: '100%', background: '#FFA41C',
                  }} />
                </div>
                <span style={{ width: 30 }}>{summary[`star${n}`]}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {user && !myReview && <ReviewForm onSubmit={handleCreate} submitLabel="ثبت نظر" />}

      {user && myReview && !editing && (
        <div style={{ background: '#f0f7ff', padding: 16, borderRadius: 8, marginBottom: 20 }}>
          <p style={{ fontSize: 14 }}>شما قبلاً نظر داده‌اید.</p>
          <button onClick={() => setEditing(true)} style={{
            padding: '6px 14px', background: '#FFD814', border: '1px solid #FCD200',
            borderRadius: 4, cursor: 'pointer', fontSize: 13,
          }}>ویرایش نظر</button>
        </div>
      )}

      {user && myReview && editing && (
        <ReviewForm initial={myReview} onSubmit={handleUpdate} submitLabel="ذخیره تغییرات" />
      )}

      <ReviewList reviews={reviews} onReply={handleReply} onReact={handleReact} />
    </div>
  );
}
