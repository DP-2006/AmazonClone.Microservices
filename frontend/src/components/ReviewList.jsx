import { useState } from 'react';
import StarRating from './StarRating';
import { reviewsApi } from '../api/client';
import { useAuth } from '../context/AuthContext';

function ReviewItem({ review, onReply, onReact, level = 0 }) {
  const [showReply, setShowReply] = useState(false);
  const [replyText, setReplyText] = useState('');
  const { user } = useAuth();

  const submitReply = async () => {
    if (!replyText.trim()) return;
    await onReply(review.id, replyText);
    setReplyText('');
    setShowReply(false);
  };

  return (
    <div style={{
      marginLeft: level * 30,
      padding: 16,
      borderLeft: level > 0 ? '2px solid #eee' : 'none',
      marginBottom: 12,
      background: level > 0 ? '#fafafa' : 'white',
      borderRadius: 6,
    }}>
      <div style={{ display: 'flex', gap: 12, alignItems: 'flex-start' }}>
        <div style={{
          width: 40, height: 40, borderRadius: '50%', background: '#232f3e',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          color: 'white', fontWeight: 'bold',
        }}>
          {(review.userName || '?')[0].toUpperCase()}
        </div>
        <div style={{ flex: 1 }}>
          <div style={{ display: 'flex', gap: 10, alignItems: 'center', marginBottom: 4 }}>
            <b style={{ fontSize: 14 }}>{review.userName}</b>
            <StarRating value={review.rating} showCount={false} size={14} />
          </div>
          {review.title && <p style={{ fontWeight: 'bold', fontSize: 14, margin: '4px 0' }}>{review.title}</p>}
          <p style={{ fontSize: 14, lineHeight: 1.5, margin: '6px 0', color: '#333' }}>{review.comment}</p>
          <div style={{ display: 'flex', gap: 16, alignItems: 'center', marginTop: 8, fontSize: 13 }}>
            <button onClick={() => onReact(review.id, 'like')} style={{
              background: 'none', border: 'none', cursor: 'pointer', color: '#007185',
            }}>👍 {review.likeCount}</button>
            <button onClick={() => onReact(review.id, 'dislike')} style={{
              background: 'none', border: 'none', cursor: 'pointer', color: '#007185',
            }}>👎 {review.dislikeCount}</button>
            {user && level === 0 && (
              <button onClick={() => setShowReply(!showReply)} style={{
                background: 'none', border: 'none', cursor: 'pointer', color: '#007185',
              }}>پاسخ</button>
            )}
          </div>

          {showReply && (
            <div style={{ marginTop: 10, display: 'flex', gap: 8 }}>
              <input
                value={replyText}
                onChange={e => setReplyText(e.target.value)}
                placeholder="پاسخ شما..."
                style={{ flex: 1, padding: 8, border: '1px solid #ccc', borderRadius: 4 }}
              />
              <button onClick={submitReply} style={{
                padding: '8px 16px', background: '#FFD814', border: '1px solid #FCD200',
                borderRadius: 4, cursor: 'pointer', fontWeight: 'bold',
              }}>ارسال</button>
            </div>
          )}

          {review.replies && review.replies.length > 0 && (
            <div style={{ marginTop: 12 }}>
              {review.replies.map(r => (
                <ReviewItem key={r.id} review={r} onReply={onReply} onReact={onReact} level={level + 1} />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default function ReviewList({ reviews, onReply, onReact }) {
  if (!reviews || reviews.length === 0) {
    return <p style={{ color: '#666', padding: 20, textAlign: 'center' }}>هنوز نظری ثبت نشده است.</p>;
  }
  return (
    <div>
      {reviews.map(r => (
        <ReviewItem key={r.id} review={r} onReply={onReply} onReact={onReact} />
      ))}
    </div>
  );
}
