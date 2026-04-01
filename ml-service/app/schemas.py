from pydantic import BaseModel, Field
from typing import List, Optional


class PredictResponse(BaseModel):
    """Repsonse returned by the .NET backend"""
    prediction: str = Field(..., description="'ransomware', 'suspicious', or 'safe'")
    confidence: float = Field(..., ge=0.0, le=1.0, description="Confidence score (0-1)")
    model_version: str
    raw_score: float = Field(..., description="Brut score of the model, in range of (0-1)")

    model_config = {
        "json_schema_extra": {
            "example": {
                "prediction": "ransomware",
                "confidence": 0.943,
                "model_version": "ember2024-1.0",
                "raw_score": 0.943,
            }
        }
    }


class HealthResponse(BaseModel):
    status: str
    timestamp: str
    model: str
    model_loaded: bool