from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.config import settings
from app.routers import health


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