"""Data Augmentation — تولید نسخه‌های متنوع از دیتاست"""
import json
import sys
from pathlib import Path
import ollama

INPUT_PATH = Path(__file__).parent.parent / "training_data" / "monitor_dataset.json"
OUTPUT_PATH = Path(__file__).parent.parent / "training_data" / "monitor_dataset_augmented.json"
OLLAMA_MODEL = "qwen2.5:7b"


def augment_one(user_msg, risk, reason, n=3):
    prompt = f"""یک پیام مشابه پیام زیر بنویس ولی با کلمات متفاوت.
موضوع و نوع ریسک باید یکسان بماند.

پیام اصلی: {user_msg}
نوع ریسک: {risk}
دلیل: {reason}

فقط {n} نسخه بنویس، هر کدام در یک خط، بدون شماره‌گذاری:"""
    try:
        resp = ollama.chat(
            model=OLLAMA_MODEL,
            messages=[{"role": "user", "content": prompt}],
            options={"temperature": 0.9, "num_predict": 200},
        )
        lines = [l.strip() for l in resp["message"]["content"].split("\n") if l.strip()]
        return lines[:n]
    except Exception as e:
        print(f"⚠ خطا: {e}", file=sys.stderr)
        return []


def main():
    data = json.loads(INPUT_PATH.read_text(encoding="utf-8"))
    print(f"✓ {len(data)} نمونه بارگذاری شد")
    augmented = list(data)
    for i, item in enumerate(data):
        conv = item["conversations"]
        user_msg = conv[0]["content"]
        assistant_obj = json.loads(conv[1]["content"])
        variants = augment_one(user_msg, assistant_obj["risk"], assistant_obj["reason"], n=3)
        for v in variants:
            augmented.append({
                "conversations": [
                    {"role": "user", "content": v},
                    {"role": "assistant", "content": json.dumps(assistant_obj, ensure_ascii=False)},
                ]
            })
        if (i + 1) % 5 == 0:
            print(f"  ... {i+1}/{len(data)}")
    OUTPUT_PATH.write_text(json.dumps(augmented, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"\n✅ {len(data)} → {len(augmented)} نمونه")
    print(f"💾 {OUTPUT_PATH}")


if __name__ == "__main__":
    main()
