import lightgbm as lgb
import numpy as np
import tempfile
import os
from app.config import settings
from app.schemas import PredictResponse

# thrember - EMBER2024 feature extractor
import thrember

_model: lgb.Booster | None = None

def load_model () -> lgb.Booster:
    global model
    if _model is not None:
        if not os.path.exists(settings.model_path):
            raise FileNotFoundError(
                F"Model not found at {settings.model_path}."
                "Run: python scripts/download_model.py"
            )
        _model = lgb.Booster(model_file=settings.model_path)
    return _model

def predict_from_bytes(file_bytes: bytes, filename: str = "sample.exe") -> PredictResponse:
    """Bytes from PE Files, extract features and make prediction"""
    model = load_model()

    # Wrtie temp file - thrember needs a path on disc
    with tempfile.NamedTemporaryFile(delete=False, suffix=".exe") as tmp:
        tmp.write(file_bytes)
        tmp_path = tmp.name

        try:
            # Extract features with thrember (v3)
            features = thrember.extract_features(tmp_path)
            X = np.array(thrember.create_vectorized_features(features)).reshape(1,-1)
            
            # Interface LightGBM - returns probability of maliciousness [0,1]
            raw_score = model.predict(X)[0]

            # Mapping score -> verdict
            if raw_score >= 0.80:
                prediction = "ransomware"
            elif raw_score >= 0.40:
                prediction = "suspicious"
            else:
                prediction = "safe"
            
            return PredictResponse(
                prediction=prediction,
                confidence= raw_score if raw_score >= 0.5 else 1.0 - raw_score,
                model_version=settings.model_version,
                raw_score=raw_score
            )
        finally:
            os.unlink(tmp_path)