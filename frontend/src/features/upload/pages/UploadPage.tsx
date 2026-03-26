// src/features/upload/pages/UploadPage.tsx
import { useEffect, useRef, useState } from 'react'
import { UploadZone } from '../components/UploadZone'
import { useFileUpload } from '../hooks/useFileUpload'

const TITLE_TEXT = 'RANSOMWARE DETECTION'
const DESC_TEXT =
  'Upload a PE file (.exe or .dll) for static analysis. Our ML-powered system will detect ransomware characteristics including entropy analysis, suspicious API usage, and PE structure anomalies.'

export function UploadPage() {
  const { uploadFile, isUploading, error } = useFileUpload()
  const [titleText, setTitleText] = useState('')
  const [descText, setDescText] = useState('')
  const [titleDone, setTitleDone] = useState(false)
  const [descDone, setDescDone] = useState(false)
  const titleIndexRef = useRef(0)
  const descIndexRef = useRef(0)

  useEffect(() => {
    titleIndexRef.current = 0
    setTitleText('')
    setTitleDone(false)
    setDescText('')
    setDescDone(false)

    const titleInterval = setInterval(() => {
      const i = titleIndexRef.current
      if (i < TITLE_TEXT.length) {
        setTitleText(TITLE_TEXT.substring(0, i + 1))
        titleIndexRef.current++
      } else {
        clearInterval(titleInterval)
        setTitleDone(true)
      }
    }, 200)

    return () => clearInterval(titleInterval)
  }, [])

  useEffect(() => {
    if (!titleDone) return

    const pause = setTimeout(() => { // 2s pause after title, like mockup
      descIndexRef.current = 0
      const descInterval = setInterval(() => {
        const i = descIndexRef.current
        if (i < DESC_TEXT.length) {
          setDescText(DESC_TEXT.substring(0, i + 1))
          descIndexRef.current++
        } else {
          clearInterval(descInterval)
          setDescDone(true)
        }
      }, 100)
      return () => clearInterval(descInterval)
    }, 2000)

    return () => clearTimeout(pause)
  }, [titleDone])

  return (
    <div className="space-y-6">
      <div className="text-center mb-8">
        <h1 className="text-4xl font-bold text-terminal-green mb-4 flex justify-center items-center gap-2">
          <span className="text-terminal-green text-glow">&gt;</span>
          <span className={`text-glow${!titleDone ? ' typewriter-cursor' : ''}`}>
            {titleText}
          </span>
        </h1>
        <div className="text-terminal-muted max-w-3xl mx-auto min-h-16 flex items-start justify-center px-8">
          <span className={!descDone && titleDone ? 'typewriter-cursor' : ''}>
            {titleDone && (
              <>
                <span className="text-terminal-cyan">$ </span>
                {descText}
              </>
            )}
          </span>
        </div>
      </div>

      <UploadZone
        onFileSelect={uploadFile}
        isUploading={isUploading}
        error={error?.message || null}
      />
    </div>
  )
}
