import lightgbm as lgb
import numpy as np
import os
from app.config import settings
from app.schemas import PredictResponse

# thrember - EMBER2024 feature extractor
import thrember

_model: lgb.Booster | None = None

def load_model() -> lgb.Booster:
    global _model
    if _model is None:
        if not os.path.exists(settings.model_path):
            raise FileNotFoundError(
                f"Model not found at {settings.model_path}. "
                "Run: python scripts/download_model.py"
            )
        _model = lgb.Booster(model_file=settings.model_path)
    return _model

def predict_from_bytes(file_bytes: bytes, filename: str = "sample.exe") -> PredictResponse:
    """Bytes from PE file, extract features and make prediction"""
    model = load_model()

    # Extract features with thrember (works directly on bytes)
    extractor = thrember.PEFeatureExtractor()
    X = np.array(extractor.feature_vector(file_bytes)).reshape(1, -1)

    # LightGBM - returns probability of maliciousness [0,1]
    raw_score = model.predict(X)[0]

    # Map score -> verdict
    if raw_score >= 0.80:
        prediction = "ransomware"
    elif raw_score >= 0.40:
        prediction = "suspicious"
    else:
        prediction = "safe"

    return PredictResponse(
        prediction=prediction,
        confidence=raw_score if raw_score >= 0.5 else 1.0 - raw_score,
        model_version=settings.model_version,
        raw_score=raw_score
    )
