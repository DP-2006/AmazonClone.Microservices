"""Chat Moderation Service - نظارت بر چت با LLM + RAG"""
import json
import sys
from pathlib import Path
from dotenv import load_dotenv
import os

import ollama
from rag_query import search

load_dotenv(Path(__file__).parent.parent / ".env")
OLLAMA_MODEL = os.getenv("OLLAMA_MODEL", "qwen2.5:7b")

MODERATION_PROMPT = """شما یک سیستم نظارت بر چت فروشگاه آنلاین هستید.
وظیفه: تحلیل پیام کاربر و تشخیص ریسک.

دسته‌بندی ریسک:
- "none" — پیام عادی
- "scam" — کلاهبرداری، درخواست معامله خارج از پلتفرم، لینک مشکوک
- "grooming" — درخواست عکس/ویدیو شخصی، ابراز علاقه خارج از حد، پیشنهاد ملاقات
- "substance" — اشاره به سیگار، مشروب، مواد مخدر
- "sexual" — محتوای جنسی صریح

قوانین:
1. فقط JSON برگردان، هیچ متن اضافه‌ای نده.
2. خروجی دقیقاً: {"risk": "نوع", "confidence": 0.0-1.0, "reason": "توضیح کوتاه"}
3. اگر مطمئن نیستی، risk="none" بگذار.
"""

USER_GUIDE_PROMPT = """شما یک دستیار خرید هستید.
کاربر در حال چت با فروشنده است ولی سؤالش مبهم است.
وظیفه: به کاربر پیشنهاد بده چه سؤال‌های مفیدی می‌تواند بپرسد.
پاسخ کوتاه و مفید بده (حداکثر ۳ خط).
"""


def moderate_message(message: str) -> dict:
    try:
        rag_results = search(message, top_k=2)
        rag_context = "\n".join([r[0] for r in rag_results])
    except Exception as e:
        rag_context = ""
        print(f"⚠ RAG error: {e}", file=sys.stderr)

    prompt = f"""دانش مرتبط از دیتابیس:
{rag_context}

پیام کاربر:
{message}

تحلیل ریسک (فقط JSON):"""

    try:
        response = ollama.chat(
            model=OLLAMA_MODEL,
            messages=[
                {"role": "system", "content": MODERATION_PROMPT},
                {"role": "user", "content": prompt},
            ],
            options={"temperature": 0.1, "num_predict": 100},
        )
        content = response["message"]["content"].strip()
        content = content.replace("```json", "").replace("```", "").strip()
        result = json.loads(content)
    except json.JSONDecodeError:
        result = {"risk": "parse_error", "confidence": 0.0, "raw": content}
    except Exception as e:
        result = {"risk": "error", "confidence": 0.0, "reason": str(e)}
    return result


def guide_user(message: str) -> str:
    try:
        response = ollama.chat(
            model=OLLAMA_MODEL,
            messages=[
                {"role": "system", "content": USER_GUIDE_PROMPT},
                {"role": "user", "content": message},
            ],
            options={"temperature": 0.3, "num_predict": 150},
        )
        return response["message"]["content"].strip()
    except Exception as e:
        return f"خطا: {e}"


if __name__ == "__main__":
    test_messages = [
        "این چند می‌فروشی؟",
        "بیا واتساپ ارزون‌تر بدم",
        "تو چند سالته؟ یه عکس بفرست",
        "سیگار داری؟",
    ]
    for msg in test_messages:
        print(f"\n{'='*60}")
        print(f"📩 پیام: {msg}")
        result = moderate_message(msg)
        print(f"🛡️  نتیجه: {json.dumps(result, ensure_ascii=False)}")
        if result.get("risk") == "none":
            print(f"💡 راهنما: {guide_user(msg)}")
        else:
            print(f"🚨 هشدار: این پیام به ادمین گزارش می‌شود")
