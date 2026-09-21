export default function StarRating({ value = 0, count = 0, size = 16, showCount = true, onChange = null }) {
  const stars = [1, 2, 3, 4, 5];

  const handleClick = (n) => {
    if (onChange) onChange(n);
  };

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
      <div style={{ display: 'flex', gap: 1 }}>
        {stars.map(n => {
          const filled = n <= Math.round(value);
          return (
            <span
              key={n}
              onClick={() => handleClick(n)}
              style={{
                fontSize: size,
                color: filled ? '#FFA41C' : '#DDD',
                cursor: onChange ? 'pointer' : 'default',
                lineHeight: 1,
              }}
            >
              ★
            </span>
          );
        })}
      </div>
      {showCount && (
        <span style={{ fontSize: 13, color: '#007185' }}>
          {value.toFixed(1)} ({count})
        </span>
      )}
    </div>
  );
}
