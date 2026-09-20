"""Check AI environment setup"""
import sys
from pathlib import Path
from dotenv import load_dotenv
import os

load_dotenv(Path(__file__).parent.parent / ".env")
print("=== AI Environment Check ===\n")
print(f"✓ Python: {sys.version.split()[0]}")

def check(label, fn):
    try:
        print(f"✓ {label}: {fn()}")
    except Exception as e:
        print(f"✗ {label}: {e}")

def check_ollama():
    import ollama
    models = ollama.list()
    names = [m["name"] for m in models.get("models", [])]
    return f"{len(names)} مدل: {names}"

def check_qdrant():
    from qdrant_client import QdrantClient
    c = QdrantClient(host=os.getenv("QDRANT_HOST", "localhost"),
                     port=int(os.getenv("QDRANT_PORT", "6333")))
    return f"{len(c.get_collections().collections)} collection"

check("Ollama", check_ollama)
check("Qdrant", check_qdrant)

for p in ["training_data", "rag_data", "scripts", "models", "logs"]:
    full = Path(__file__).parent.parent / p
    print(f"{'✓' if full.exists() else '✗'} {p}/")

print("\n=== پایان ===")
