from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.config import settings
from app.routers import health
from app import predictor
from app.config import settings


app = FastAPI(
    title= "RansomGuard ML Service",
    version= settings.model_version,
    description="PE malware detection using EMBER2024 (SIGKDD 2025)"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.allowed_origins,
    allow_methods=["GET", "POST"],
    allow_headers=["Content-Type"],
)

app.include_router(health.router)
app.include_router(predictor.router)

@app.on_event("startup")
async def startup():
    """Pre-loads the model at startup - first request in instant"""
    try:
        predictor.load_model()
        print(f"[startup] EMBER2024 model loaded form {settings.model_path}")
    except FileNotFoundError:
        print("[startup] WARNING: Model not found. Run: python scripts/download_model.py")