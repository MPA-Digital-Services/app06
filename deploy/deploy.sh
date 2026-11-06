#!/usr/bin/env bash
#
# Despliegue de NorthwindApi (ASP.NET Core 10) en un servidor Linux con Docker Compose.
#
# Uso:  bash deploy.sh <archivo.env> [archivo-compose] [archivo-sql]
#
# El pipeline (.gitlab-ci.yml) copia este script al servidor y lo invoca por SSH.
# También se puede ejecutar a mano desde el servidor.
#
# Variables del archivo .env (se cargan en el entorno del proceso):
#   DEPLOY_PATH         Ruta del despliegue           (por defecto /opt/northwind-api)
#   API_IMAGE           Imagen a desplegar            (obligatorio)
#   API_PORT            Puerto HTTP de la API         (por defecto 8080)
#   SQL_PORT            Puerto de SQL Server          (por defecto 1433)
#   MSSQL_SA_PASSWORD   Contraseña de "sa"            (obligatorio)
#   REGISTRY            Host del registro de imágenes (opcional; se deduce de API_IMAGE)
#   REGISTRY_USER       Usuario del registro          (opcional, paquetes privados)
#   REGISTRY_TOKEN      Token del registro            (opcional, paquetes privados)
#   GHCR_USERNAME       Alias heredado de REGISTRY_USER
#   GHCR_TOKEN          Alias heredado de REGISTRY_TOKEN
#   HEALTH_RETRIES      Intentos de espera (SQL Server y /health) (por defecto 30)
#   HEALTH_INTERVAL     Segundos entre intentos       (por defecto 5)
#
# Pasos: credenciales -> descarga de la imagen -> SQL Server -> base de datos
#        -> despliegue de la API -> verificación de /health -> reversión si falla.
#
set -euo pipefail

ENV_FILE="${1:?Uso: deploy.sh <archivo.env> [archivo-compose] [archivo-sql]}"
COMPOSE_SRC="${2:-/tmp/docker-compose.prod.yml}"
SQL_SRC="${3:-/tmp/instnwnd.sql}"

set -a
# shellcheck disable=SC1090
. "$ENV_FILE"
set +a

DEPLOY_PATH="${DEPLOY_PATH:-/opt/northwind-api}"
API_IMAGE="${API_IMAGE:?Falta API_IMAGE}"
API_PORT="${API_PORT:-8080}"
SQL_PORT="${SQL_PORT:-1433}"
MSSQL_SA_PASSWORD="${MSSQL_SA_PASSWORD:?Falta MSSQL_SA_PASSWORD}"
HEALTH_RETRIES="${HEALTH_RETRIES:-30}"
HEALTH_INTERVAL="${HEALTH_INTERVAL:-5}"

COMPOSE_FILE="$DEPLOY_PATH/docker-compose.prod.yml"

log() { echo "[deploy] $*"; }
die() { echo "[deploy] ERROR: $*" >&2; exit 1; }

# Evita dejar credenciales del registro en el servidor.
DOCKER_CONFIG_DIR=""
cleanup() {
  if [ -n "$DOCKER_CONFIG_DIR" ] && [ -d "$DOCKER_CONFIG_DIR" ]; then
    rm -rf "$DOCKER_CONFIG_DIR"
  fi
}
trap cleanup EXIT

# Comprueba si la base "Northwind" ya tiene datos cargados.
db_initialized() {
  docker compose -f "$COMPOSE_FILE" exec -T sqlserver bash -c '
    TOOLS=/opt/mssql-tools18/bin/sqlcmd
    [ -x "$TOOLS" ] || TOOLS=/opt/mssql-tools/bin/sqlcmd
    "$TOOLS" -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
      -Q "SELECT COUNT(*) FROM Northwind.dbo.Customers" -b -o /dev/null
  ' >/dev/null 2>&1
}

# Sondea el endpoint de salud de la API.
health_probe() {
  local url="http://localhost:${API_PORT}/health"
  if command -v curl >/dev/null 2>&1; then
    curl -fsS --max-time 10 "$url" >/dev/null 2>&1
  elif command -v wget >/dev/null 2>&1; then
    wget -q -T 10 -O /dev/null "$url" >/dev/null 2>&1
  else
    docker run --rm --network host curlimages/curl:8.11.1 \
      -fsS --max-time 10 "$url" >/dev/null 2>&1
  fi
}

# Espera a que la API responda 200 en /health.
wait_for_health() {
  local attempt
  for attempt in $(seq 1 "$HEALTH_RETRIES"); do
    if health_probe; then
      return 0
    fi
    log "  la API todavía no responde (intento ${attempt}/${HEALTH_RETRIES})..."
    sleep "$HEALTH_INTERVAL"
  done
  return 1
}

# 0) Preparación del directorio de despliegue.
mkdir -p "$DEPLOY_PATH"
cp "$COMPOSE_SRC" "$COMPOSE_FILE"
# El servicio "db-init" monta "./instnwnd.sql" junto al archivo compose.
if [ -f "$SQL_SRC" ]; then
  cp "$SQL_SRC" "$DEPLOY_PATH/instnwnd.sql"
else
  log "Aviso: no se encontró $SQL_SRC (sólo hace falta para la primera carga de datos)."
fi
log "Directorio de despliegue: $DEPLOY_PATH"
log "Imagen a desplegar: ${API_IMAGE} (puerto ${API_PORT})"

# Las variables llegan al "docker compose" por el entorno.
export MSSQL_SA_PASSWORD API_IMAGE API_PORT SQL_PORT
if [ -n "${SQLSERVER_IMAGE:-}" ]; then
  export SQLSERVER_IMAGE
fi

# 1) Credenciales del registro de imágenes (sólo si el paquete es privado).
REGISTRY="${REGISTRY:-${CI_REGISTRY:-}}"
if [ -z "$REGISTRY" ]; then
  case "${API_IMAGE%%/*}" in
    *.*|*:*) REGISTRY="${API_IMAGE%%/*}" ;;
    *)       REGISTRY="docker.io" ;;
  esac
fi
REGISTRY_USER="${REGISTRY_USER:-${CI_REGISTRY_USER:-${GHCR_USERNAME:-$(id -un)}}}"
REGISTRY_TOKEN="${REGISTRY_TOKEN:-${CI_REGISTRY_PASSWORD:-${GHCR_TOKEN:-}}}"

if [ -n "$REGISTRY_TOKEN" ]; then
  DOCKER_CONFIG_DIR="$(mktemp -d)"
  export DOCKER_CONFIG="$DOCKER_CONFIG_DIR"
  log "Autenticando en ${REGISTRY}..."
  printf '%s' "$REGISTRY_TOKEN" \
    | docker login "$REGISTRY" -u "$REGISTRY_USER" --password-stdin >/dev/null
fi

# 2) Descarga de las imágenes publicadas por el pipeline.
log "Descargando las imágenes..."
docker compose -f "$COMPOSE_FILE" pull sqlserver api

# 3) SQL Server: levantar y esperar a que esté saludable.
log "Levantando SQL Server..."
docker compose -f "$COMPOSE_FILE" up -d sqlserver
for attempt in $(seq 1 "$HEALTH_RETRIES"); do
  status="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{end}}' \
    northwind-sqlserver 2>/dev/null || true)"
  if [ "$status" = "healthy" ]; then
    break
  fi
  if [ "$attempt" -eq "$HEALTH_RETRIES" ]; then
    die "SQL Server no quedó saludable tras ${HEALTH_RETRIES} intentos."
  fi
  log "  esperando a SQL Server (intento ${attempt}/${HEALTH_RETRIES})..."
  sleep "$HEALTH_INTERVAL"
done

# 4) Base de datos: inicializarla sólo si aún no tiene datos.
if db_initialized; then
  log "Base de datos Northwind ya inicializada."
else
  log "Inicializando la base de datos Northwind (primera vez)..."
  docker compose -f "$COMPOSE_FILE" --profile init run --rm db-init
fi

# 5) Despliegue de la API (guardando la imagen anterior para poder revertir).
PREVIOUS_IMAGE="$(docker inspect --format '{{.Config.Image}}' northwind-api 2>/dev/null || true)"
log "Desplegando la API (imagen anterior: ${PREVIOUS_IMAGE:-ninguna})..."
docker compose -f "$COMPOSE_FILE" up -d --remove-orphans api
printf '%s\n' "$API_IMAGE" > "$DEPLOY_PATH/last-image"

# 6) Verificación de salud; si falla, se revierte a la imagen anterior.
log "Verificando /health..."
if ! wait_for_health; then
  docker compose -f "$COMPOSE_FILE" logs --tail 100 api >&2 || true
  if [ -n "$PREVIOUS_IMAGE" ] && [ "$PREVIOUS_IMAGE" != "$API_IMAGE" ]; then
    log "Revirtiendo a la imagen anterior: $PREVIOUS_IMAGE"
    export API_IMAGE="$PREVIOUS_IMAGE"
    docker compose -f "$COMPOSE_FILE" up -d api || true
  fi
  die "la API no respondió correctamente en /health."
fi

log "Despliegue completado correctamente: ${API_IMAGE}"
