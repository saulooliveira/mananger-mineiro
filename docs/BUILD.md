# Build Pipeline - Gerador de Ofertas

Unified build pipeline for generating single portable EXE with embedded .NET 8 runtime.

## Output

**Single File:** `Gerador de Ofertas.exe` (~150-200 MB)
- Includes .NET 8 runtime (self-contained)
- Includes all dependencies (QuestPDF, EF Core, etc.)
- Portable - no installation required
- Ready to distribute

## Build Locally

### Prerequisites
- Windows 10/11
- .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- Node.js 18+: https://nodejs.org/
- npm 9+ (bundled with Node.js)

### Quick Build

```powershell
# From project root
.\build.ps1 -Configuration release

# Output files in ./dist/
ls dist/*.exe
```

### Build Options

```powershell
# Release build (optimized, trimmed)
.\build.ps1 -Configuration release

# Debug build (larger, more symbols)
.\build.ps1 -Configuration debug

# Custom output directory
.\build.ps1 -Configuration release -OutputDir "C:/builds"
```

## Build Pipeline Stages

```
1. Backend (.NET)
   ├─ dotnet publish -c Release --self-contained
   ├─ Publishes to: app/frontend/public/backend/
   └─ Output: backend.exe (backend API executable)

2. Frontend (React)
   ├─ npm run build
   ├─ Builds React app
   └─ Output: build/ (static HTML/CSS/JS)

3. Bundle (Electron)
   ├─ npm run dist
   ├─ Packages with electron-builder
   └─ Output: dist/Gerador de Ofertas.exe (portable)

4. Finalize
   ├─ Copy EXE to ./dist/
   └─ Ready for distribution
```

## Self-Contained Configuration

### Backend (`backend.csproj`)

```xml
<PropertyGroup>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <PublishTrimmed>true</PublishTrimmed>
  <PublishReadyToRun>true</PublishReadyToRun>
  <DebugType>embedded</DebugType>
</PropertyGroup>
```

**Results:**
- Single `backend.exe` file (~50-80 MB)
- Includes .NET 8 runtime
- No .NET installation required on target machine
- Trimmed unused assemblies
- Ready-to-run optimization (R2R) for faster startup

### Frontend (`package.json`)

```json
"win": {
  "target": ["portable"]
}
```

**Results:**
- Single portable EXE (no installer)
- Includes Chromium, Node binaries, and app code
- Bundles `backend.exe` inside the package

## CI/CD Pipeline

### GitHub Actions (`.github/workflows/build-exe.yml`)

**Triggers:**
- Push to `master` or `main`
- Git tags (`v*`)
- Manual workflow dispatch

**Steps:**
1. Checkout code
2. Setup .NET 8
3. Setup Node.js 18
4. Install dependencies
5. Run `build.ps1`
6. Upload artifacts (30 day retention)
7. Create GitHub release (on tag)

**Artifact Location:**
- https://github.com/saulooliveira/mananger-mineiro/releases

## File Structure

```
Gerador de Ofertas.exe
├─ Chromium (browser engine)
├─ Electron runtime
├─ React app (HTML/CSS/JS)
├─ backend.exe (embedded)
└─ Data files (config, database)
```

## Deployment

### Install & Run

1. **Download:** `Gerador de Ofertas.exe` from releases
2. **Run:** Double-click to launch
3. **No dependencies:** Works on Windows 10/11 (x64)

### Distribution

- Email as attachment
- Host on web server
- GitHub Releases
- Windows Store (optional, future)

## Troubleshooting

### Build Fails: "dotnet not found"

Ensure .NET 8 SDK is installed:
```powershell
dotnet --version  # Should show 8.x.x
```

### Build Fails: "npm not found"

Ensure Node.js is installed:
```powershell
npm --version  # Should show 9.x or higher
```

### EXE is too large (>200 MB)

- Normal for self-contained builds with Electron
- Includes full .NET runtime and Chromium browser
- Can be reduced with Trimmer or WebAssembly (advanced)

### Backend doesn't start

- Check `app/frontend/public/backend/backend.exe` exists
- Backend runs on `http://localhost:5274`
- Database created at runtime: `app/data/produtos.db`

## Development vs Production

### Development Build

```powershell
.\build.ps1 -Configuration debug
```

- Larger (~250+ MB)
- More debug symbols
- Slower startup
- Use for testing

### Release Build (Default)

```powershell
.\build.ps1 -Configuration release
```

- Trimmed (~150-200 MB)
- ReadyToRun optimization
- Faster startup
- Use for distribution

## Version Management

### Semantic Versioning

Tag releases with semantic version:
```bash
git tag v1.0.0
git push origin v1.0.0
```

This triggers:
1. GitHub Actions build
2. Creates release with EXE
3. Auto-uploaded to GitHub Releases

## Performance

### Build Time
- Local build: ~2-3 minutes (first time) / ~1-2 minutes (cached)
- CI/CD build: ~5-10 minutes (includes setup)

### Runtime
- Startup time: ~3-5 seconds
- Backend API ready on port 5274
- React UI loads in browser

### EXE Size
- Base: ~150-200 MB (including .NET + Electron)
- Smaller alternatives: WebAssembly (not recommended for PDF/QuestPDF)

## Future Optimizations

1. **Code Signing:** Sign EXE for Windows SmartScreen
2. **Auto-Updates:** Implement electron-updater for OTA updates
3. **Installer:** Create NSIS installer for Windows Store
4. **ARM64:** Build for Apple Silicon (future Mac support)
5. **Compression:** Use UPX or similar for smaller distribution

