"""RAG Query - ایندکس و بازیابی دانش از Qdrant"""
import sys
import uuid
from pathlib import Path
from dotenv import load_dotenv
import os

from qdrant_client import QdrantClient
from qdrant_client.models import Distance, VectorParams, PointStruct
from langchain_huggingface import HuggingFaceEmbeddings
from langchain_text_splitters import RecursiveCharacterTextSplitter

load_dotenv(Path(__file__).parent.parent / ".env")

QDRANT_HOST = os.getenv("QDRANT_HOST", "localhost")
QDRANT_PORT = int(os.getenv("QDRANT_PORT", "6333"))
COLLECTION = os.getenv("QDRANT_COLLECTION", "safety_knowledge")
EMBEDDING_MODEL = os.getenv("EMBEDDING_MODEL", "BAAI/bge-m3")

client = QdrantClient(host=QDRANT_HOST, port=QDRANT_PORT)
embeddings = HuggingFaceEmbeddings(
    model_name=EMBEDDING_MODEL,
    model_kwargs={"device": "cpu"},
    encode_kwargs={"normalize_embeddings": True},
)


def ensure_collection():
    try:
        client.get_collection(COLLECTION)
        print(f"✓ Collection '{COLLECTION}' موجود است")
    except Exception:
        client.create_collection(
            collection_name=COLLECTION,
            vectors_config=VectorParams(size=1024, distance=Distance.COSINE),
        )
        print(f"✓ Collection '{COLLECTION}' ساخته شد")


def index_knowledge(filepath):
    path = Path(filepath)
    if not path.exists():
        print(f"✗ فایل پیدا نشد: {filepath}")
        return
    text = path.read_text(encoding="utf-8")
    splitter = RecursiveCharacterTextSplitter(
        chunk_size=500, chunk_overlap=50,
        separators=["\n\n", "\n", "。", "！", "？", ".", " "],
    )
    chunks = splitter.split_text(text)
    print(f"✓ {len(chunks)} قطعه ساخته شد")
    points = []
    for i, chunk in enumerate(chunks):
        vector = embeddings.embed_query(chunk)
        points.append(PointStruct(
            id=str(uuid.uuid4()), vector=vector,
            payload={"text": chunk, "source": str(path), "chunk_idx": i},
        ))
    client.upsert(collection_name=COLLECTION, points=points)
    print(f"✓ {len(points)} قطعه ایندکس شد")


def search(query, top_k=3):
    query_vector = embeddings.embed_query(query)
    results = client.search(
        collection_name=COLLECTION,
        query_vector=query_vector,
        limit=top_k, with_payload=True,
    )
    return [(r.payload["text"], r.score) for r in results]


if __name__ == "__main__":
    ensure_collection()
    if len(sys.argv) > 1 and sys.argv[1] == "index":
        knowledge_path = Path(__file__).parent.parent / "rag_data" / "knowledge.txt"
        index_knowledge(str(knowledge_path))
    else:
        query = sys.argv[1] if len(sys.argv) > 1 else "الگوی کلاهبرداری"
        print(f"\n🔍 جستجو: {query}\n")
        results = search(query)
        for i, (text, score) in enumerate(results, 1):
            print(f"--- نتیجه {i} (امتیاز: {score:.3f}) ---")
            print(text)
            print()
