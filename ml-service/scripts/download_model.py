"""
Download model EMBER2024 (malicious/benign classifier) from Hugging Face.

Using:
    python scripts/download_model.py
"""
from huggingface_hub import hf_hub_download
import os

MODEL_REPO = "joyce8/EMBER2024-benchmark-models"
MODEL_FILE = "EMBER2024_PE.model"
OUTPUT_DIR = "./models"

os.makedirs(OUTPUT_DIR, exist_ok=True)

print(f"Downloading model {MODEL_FILE} from Hugging Face...")
local_path = hf_hub_download(
    repo_id=MODEL_REPO,
    filename=MODEL_FILE,
    local_dir=OUTPUT_DIR,
)
print(f"Model saved to: {local_path}")
print("Open server at: uvicorn app.main:app --reload --port 8000")