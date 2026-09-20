"""QLoRA Fine-tuning برای Chat Moderation — فقط روی GPU با 10GB+ VRAM"""
import os
from pathlib import Path
from dotenv import load_dotenv
from unsloth import FastLanguageModel
from datasets import load_dataset
from trl import SFTTrainer, SFTConfig

load_dotenv(Path(__file__).parent.parent / ".env")

BASE_MODEL = "unsloth/Qwen2.5-7B-Instruct"
MAX_SEQ_LENGTH = 2048
OUTPUT_DIR = str(Path(__file__).parent.parent / "models" / "lora_monitor_model")
DATASET_PATH = str(Path(__file__).parent.parent / "training_data" / "monitor_dataset.json")

print("🔧 بارگذاری مدل پایه با 4-bit quantization...")
model, tokenizer = FastLanguageModel.from_pretrained(
    model_name=BASE_MODEL,
    max_seq_length=MAX_SEQ_LENGTH,
    dtype=None,
    load_in_4bit=True,
)

print("🔧 اضافه کردن LoRA adapter...")
model = FastLanguageModel.get_peft_model(
    model, r=16,
    target_modules=["q_proj", "k_proj", "v_proj", "o_proj",
                    "gate_proj", "up_proj", "down_proj"],
    lora_alpha=16, lora_dropout=0, bias="none",
    use_gradient_checkpointing="unsloth", random_state=3407,
)

print("📚 بارگذاری دیتاست...")
def format_conversations(examples):
    texts = []
    for conv in examples["conversations"]:
        text = tokenizer.apply_chat_template(conv, tokenize=False, add_generation_prompt=False)
        texts.append(text)
    return {"text": texts}

dataset = load_dataset("json", data_files=DATASET_PATH, split="train")
dataset = dataset.map(format_conversations, batched=True)
print(f"✓ {len(dataset)} نمونه")

print("🚀 شروع QLoRA training...")
trainer = SFTTrainer(
    model=model, tokenizer=tokenizer, train_dataset=dataset,
    dataset_text_field="text", max_seq_length=MAX_SEQ_LENGTH,
    args=SFTConfig(
        output_dir=OUTPUT_DIR, per_device_train_batch_size=2,
        gradient_accumulation_steps=4, warmup_steps=10,
        num_train_epochs=3, learning_rate=2e-4, fp16=True,
        logging_steps=5, save_strategy="epoch",
        optim="adamw_8bit", weight_decay=0.01,
        lr_scheduler_type="linear", seed=3407, report_to="none",
    ),
)
trainer.train()

print(f"💾 ذخیره در {OUTPUT_DIR}...")
model.save_pretrained(OUTPUT_DIR)
tokenizer.save_pretrained(OUTPUT_DIR)
print("✅ Training کامل شد!")
