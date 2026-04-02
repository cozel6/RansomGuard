from fastapi import APIRouter, UploadFile, File, HTTPException
from app.schemas import PredictResponse
from app import predictor

router = APIRouter(tags=["prediction"])

MAX_FILE_SIZE = 10* 1024 * 1024 # 10MB
ALLOWED_EXTENSIONS = [".exe", ".dll"]

@router.post("/predict", response_model=PredictResponse)
async def predict(file: UploadFile = File(...)):
    # Extension check
    filename = file.filename or ""
    ext = "." + filename.rsplit(".", 1)[-1].lower() if "." in filename else ""
    if ext not in ALLOWED_EXTENSIONS:
        raise HTTPException(status_code=400, detail="Only .exe and .dll files are allowed")
    
    # Read bytes 
    file_bytes = await file.read()

    # Check size
    if len(file_bytes) < 2 or file_bytes[:2] != b"MZ":
        raise HTTPException(status_code=400, detail="Not a valid PE file (missing MZ header)")

    try:
        return predictor.predict_from_bytes(file_bytes, filename)
    except FileNotFoundError as e:
        raise HTTPException(status_code=503, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Analysis failed: {str(e)}")