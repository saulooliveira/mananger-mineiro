# Video Recording Guide

Record demo video of Gerador de Ofertas application running

## Quick Start

### Option 1: Windows 11 Game Bar (Easiest)

**Steps:**
1. Start backend: `cd app/backend && dotnet run`
2. Start frontend: `cd app/frontend && npm start`
3. Wait for browser to open at `http://localhost:3000`
4. Press `Win + G` to open Game Bar
5. Click "Start recording" (red circle)
6. Demo the application (see Demo Script below)
7. Press `Win + G` and click "Stop recording"
8. Video saved to: `Videos\Captures\`

**Pros:**
- Built-in to Windows 11
- No setup required
- Simple UI
- Good quality

**Output:** `.mp4` format, ~50-100MB for 3 minutes

---

### Option 2: OBS Studio (Professional)

**Installation:**
```bash
choco install obs-studio
# or download: https://obsproject.com/
```

**Setup:**
1. Open OBS Studio
2. Click "+" under "Sources"
3. Select "Display Capture" or "Window Capture"
4. Choose your display or browser window
5. Click "Start Recording"
6. Demo the application
7. Click "Stop Recording"
8. Video saved to: Documents\OBS\Recordings\

**Pros:**
- Full control over quality
- Scene management
- Audio recording
- Live streaming capable

**Output:** `.mp4`, `.flv`, `.mkv` formats

---

### Option 3: FFmpeg (Command Line)

**Installation:**
```bash
choco install ffmpeg

# or download: https://ffmpeg.org/download.html
```

**Recording (Windows):**
```powershell
ffmpeg -f gdigrab -framerate 30 -i desktop `
    -c:v libx264 -preset medium -crf 23 `
    -pix_fmt yuv420p `
    -t 180 `
    demo-video.mp4
```

**Parameters:**
- `-framerate 30` — 30 FPS
- `-preset medium` — Speed vs quality (fast/medium/slow)
- `-crf 23` — Quality (0-51, lower=better, 23=default)
- `-t 180` — Duration in seconds (180 = 3 minutes)
- `demo-video.mp4` — Output filename

**Pros:**
- Scriptable automation
- Maximum control
- Small file sizes

**Output:** `.mp4`, `.webm`, `.mkv` formats

---

## Demo Script (3 minutes)

Record these actions in sequence:

### Part 1: API Demonstration (0:00-0:30)

**Terminal window visible:**

```bash
# 1. Health check
curl http://localhost:5274/health | jq .

# Output shows: {"status":"healthy",...}

# 2. Get products
curl http://localhost:5274/api/produtos | jq '.[0:2]'

# Output shows: First 2 products with details
```

### Part 2: Frontend - Produtos Screen (0:30-1:30)

**Browser at http://localhost:3000**

**Actions:**
1. Show Produtos screen (default view)
   - 10 products visible
   - Search bar, filters visible
   - "0 selecionados" message

2. Search for "cerveja"
   - Results filtered
   - Show API call in Network tab

3. Select 4 products (click checkboxes 1-4)
   - Validation message updates: "4 selecionados. 1 folha pronta."
   - Preview button becomes blue (enabled)

4. Click Preview button
   - Modal opens with PDF
   - Show API call: POST /api/print/preview

5. Show PDF preview
   - A4 page with 4 products in 2×2 grid
   - Cards show: title, subtitle, price, unit

6. Click Imprimir button
   - Show API call: POST /api/print/confirm
   - Success notification
   - quantidade_impressa increments visible in table

### Part 3: Layout Editor (1:30-2:30)

**Actions:**
1. Click Layout in sidebar
   - Editor de Layout screen opens
   - A4 canvas visible
   - Test card shows

2. Click card to show sidebar
   - Cards tab opens (default)
   - Element editors visible
   - Edit Title element:
     - Change text
     - Change font size
     - Change position (drag on canvas)
     - Show real-time updates

3. Click Config tab
   - Page configuration visible
   - Show settings:
     - Margem da Página: 0
     - Colunas: 2
     - Linhas: 2
     - Espaçamento: 5

4. Make a change (e.g., change margin)
   - Click Salvar Layout
   - Show API call: POST /api/layout-config
   - Success notification

5. Return to Produtos
   - Select 4 products again
   - Click Preview
   - Show that PDF uses new layout

### Part 4: Database & API Summary (2:30-3:00)

**Terminal window:**

```bash
# Show database changes
sqlite3 app/data/produtos.db \
  "SELECT id, descricao, quantidade_impressa FROM Produtos LIMIT 5;"

# Output shows updated quantities from prints

# Show API performance
curl -w "Time: %{time_total}s\n" \
  http://localhost:5274/api/produtos > /dev/null

# Output shows: Time: 0.045s
```

---

## Recording Tips

### Audio
- Consider muting system sounds
- Optional: Add narration in post-processing
- Use OBS to mix audio from multiple sources

### Quality Settings

**For .mp4:**
```bash
# High quality (larger file, slower)
ffmpeg ... -crf 18 -preset slow ...

# Medium quality (balanced)
ffmpeg ... -crf 23 -preset medium ...

# Fast/smaller (lower quality)
ffmpeg ... -crf 28 -preset fast ...
```

**File sizes (3 minutes, 1920×1080):**
- High quality (crf 18): ~80-120 MB
- Medium quality (crf 23): ~40-60 MB
- Low quality (crf 28): ~20-30 MB

### Cursor Highlighting

Use OBS or post-processing to add:
- Cursor highlight/magnification
- Click indicators
- Pointer trails

Tools:
- [PointerFocus](https://pointerscript.com/) — Windows cursor magnifier
- [Hover](https://github.com/paulmach/hover) — Click visualization
- Adobe Premiere/DaVinci Resolve — Post-processing

---

## Post-Processing (Optional)

### Trim Video
```bash
ffmpeg -i demo-video.mp4 \
  -ss 00:00:10 -to 00:03:00 \
  -c copy output.mp4
```

### Add Intro/Outro
```bash
# Create intro image
convert -size 1920x1080 xc:black \
  -fill white -pointsize 80 \
  -gravity center -annotate +0+0 "Gerador de Ofertas Demo" \
  intro.png

# Concatenate videos
# (requires ffmpeg concat demuxer)
```

### Add Subtitles
```bash
ffmpeg -i demo-video.mp4 -vf subtitles=demo.srt output.mp4
```

### Compress for Web
```bash
# Reduce bitrate for YouTube/web
ffmpeg -i demo-video.mp4 \
  -b:v 2500k -b:a 128k \
  demo-video-compressed.mp4
```

---

## Automated Recording Script

**PowerShell script included:** `record-demo.ps1`

**Usage:**
```powershell
# Record MP4 for 3 minutes
.\record-demo.ps1 -Format mp4 -Duration 180

# Record WebM for 5 minutes
.\record-demo.ps1 -Format webm -Duration 300

# Custom output location
.\record-demo.ps1 -Output "C:\Videos\gerador-ofertas"
```

**Requires:**
- FFmpeg installed
- Windows 10+ (for gdigrab screen capture)

---

## Upload to Repository

**Store video at:**
```
/media/demo-video.mp4
```

**Update README:**
```markdown
## Demo

[![Watch Demo](media/thumbnail.jpg)](media/demo-video.mp4)

Click to watch full demo (3 minutes)
- Product selection and PDF preview
- Layout editor configuration
- Real-time API integration
```

---

## Video Specifications

**Recommended for repository:**
- Format: `.mp4` (widely compatible)
- Resolution: `1920×1080` (1080p)
- Framerate: `30 fps`
- Duration: `3-5 minutes`
- File size: `40-100 MB`
- Bitrate: `2500-4000 kbps`

**For YouTube/streaming:**
- Format: `.mp4` (H.264 video, AAC audio)
- Resolution: `1280×720` or higher
- Framerate: `30 fps`
- Bitrate: `2500-4000 kbps`

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "gdigrab not found" | Install ffmpeg properly, add to PATH |
| Black screen | Make sure window is visible, not minimized |
| Audio not recording | Add `-f dshow -i audio="Microphone"` to ffmpeg |
| File too large | Use lower `-crf` value (higher = smaller) |
| Video corrupted | Use `-c copy` to check file integrity |

---

## Example Full Command

```bash
# Windows 10/11 - Record 180 seconds at 30fps
ffmpeg -f gdigrab -framerate 30 -i desktop \
  -c:v libx264 -preset medium -crf 23 \
  -pix_fmt yuv420p -t 180 \
  gerador-ofertas-demo.mp4

# macOS - Record display 1
ffmpeg -f avfoundation -framerate 30 -i "1" \
  -c:v libx264 -preset medium -crf 23 \
  -pix_fmt yuv420p -t 180 \
  gerador-ofertas-demo.mp4

# Linux - Record main display
ffmpeg -f x11grab -framerate 30 -i :0.0 \
  -c:v libx264 -preset medium -crf 23 \
  -pix_fmt yuv420p -t 180 \
  gerador-ofertas-demo.mp4
```

