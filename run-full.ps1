Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$backendDir = Join-Path $projectRoot 'app\backend'
$frontendDir = Join-Path $projectRoot 'app\frontend'

function Require-Command {
    param([string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        Write-Error "Erro: comando '$Name' não encontrado. Instale-o e tente novamente."
        Exit 1
    }
}

Require-Command dotnet
Require-Command npm

Write-Host 'Restaurando dependências do backend...'
dotnet restore "$backendDir"

Write-Host 'Instalando dependências do frontend...'
npm install --prefix "$frontendDir"

$backendProcess = $null
try {
    Write-Host 'Iniciando backend...'
    $backendProcess = Start-Process -FilePath dotnet -ArgumentList 'run' -WorkingDirectory $backendDir -NoNewWindow -PassThru

    Write-Host 'Aguardando backend inicializar...'
    for ($i = 0; $i -lt 15; $i++) {
        try {
            Invoke-WebRequest -Uri 'http://localhost:5000' -UseBasicParsing -TimeoutSec 2 | Out-Null
            Write-Host 'Backend iniciado.'
            break
        } catch {
            Start-Sleep -Seconds 1
        }
    }

    if (-not $backendProcess.HasExited) {
        $frontendDistDir = Join-Path $frontendDir 'dist'
        $exePath = $null

        if (Test-Path $frontendDistDir) {
            $exePath = Get-ChildItem -Path $frontendDistDir -Recurse -Filter '*.exe' -File |
                Where-Object { $_.DirectoryName -match 'win-unpacked' -or $_.Name -like '*.exe' } |
                Sort-Object FullName |
                Select-Object -First 1

            if ($exePath) {
                $exePath = $exePath.FullName
            }
        }

        if ($exePath) {
            Write-Host "Executável encontrado: $exePath"
            Start-Process -FilePath $exePath
            Write-Host 'Executável iniciado. Aguardando backend...'
            Wait-Process -Id $backendProcess.Id
        } else {
            Write-Host 'Executável não encontrado. Iniciando frontend em modo dev...'
            Set-Location $frontendDir
            npm run dev
        }
    } else {
        throw 'O processo do backend terminou prematuramente.'
    }
} finally {
    if ($backendProcess -and -not $backendProcess.HasExited) {
        Write-Host 'Encerrando backend...'
        Stop-Process -Id $backendProcess.Id -ErrorAction SilentlyContinue
    }
}
