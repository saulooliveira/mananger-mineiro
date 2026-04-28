#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
BACKEND_DIR="$PROJECT_ROOT/app/backend"
FRONTEND_DIR="$PROJECT_ROOT/app/frontend"

function require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Erro: '$1' não encontrado. Instale-o e tente novamente." >&2
    exit 1
  fi
}

require_command dotnet
require_command npm

echo "Restaurando dependências do backend..."
dotnet restore "$BACKEND_DIR"

echo "Instalando dependências do frontend..."
npm install --prefix "$FRONTEND_DIR"

function cleanup() {
  echo "Encerrando processos..."
  if [[ -n "${BACKEND_PID:-}" ]]; then
    kill "$BACKEND_PID" 2>/dev/null || true
  fi
  wait "$BACKEND_PID" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

echo "Iniciando backend..."
cd "$BACKEND_DIR"
dotnet run &
BACKEND_PID=$!

echo "Aguardando backend inicializar..."
if command -v curl >/dev/null 2>&1; then
  for i in {1..15}; do
    if curl -sSf http://localhost:5000 >/dev/null 2>&1; then
      break
    fi
    sleep 1
  done
else
  sleep 5
fi

echo "Iniciando frontend..."
cd "$FRONTEND_DIR"
npm run dev
