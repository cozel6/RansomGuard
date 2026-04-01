from pydantic_settings import BaseSettings
from typing import List


class Settings(BaseSettings):
    model_path: str = "./models/ember2024_malicious.txt"
    model_version: str = "ember2024-1.0"
    debug: bool = False
    allowed_origins: List[str] = [
        "http://localhost:5087",
        "https://localhost:7179",
        "http://localhost:5173",
    ]

    class Config:
        env_file = ".env"


settings = Settings()