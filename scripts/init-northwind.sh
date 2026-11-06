#!/usr/bin/env bash
#
# Inicializa la base de datos Northwind en una instancia de SQL Server ya accesible.
# Crea la base (si no existe) y carga "instnwnd.sql".
#
# Pensado para CI (GitLab CI: servicio de SQL Server en el host "sqlserver";
# local: contenedor de servicio en localhost) y para uso local.
#
# Usa "sqlcmd" del host si está instalado (por ejemplo en el job de GitLab CI).
# Si no lo está, ejecuta sqlcmd dentro de un contenedor efímero de la imagen de
# SQL Server 2022 usando la red del host (requiere Docker).
#
# Variables de entorno:
#   SERVER        Nombre/host del servidor SQL Server (por defecto: localhost)
#   SA_PASSWORD   Contraseña de "sa"                  (por defecto: Your_password123)
#   DB_NAME       Nombre de la base de datos          (por defecto: Northwind)
#   IMAGE         Imagen de SQL Server                (por defecto: mcr.microsoft.com/mssql/server:2022-latest)
#   SCRIPT_PATH   Ruta al script de datos             (por defecto: instnwnd.sql)
#
set -euo pipefail

SERVER="${SERVER:-localhost}"
SA_PASSWORD="${SA_PASSWORD:-Your_password123}"
DB_NAME="${DB_NAME:-Northwind}"
IMAGE="${IMAGE:-mcr.microsoft.com/mssql/server:2022-latest}"
SCRIPT_PATH="${SCRIPT_PATH:-instnwnd.sql}"

SCRIPT_DIR="$(cd "$(dirname "${SCRIPT_PATH}")" && pwd)"
SCRIPT_FILE="$(basename "${SCRIPT_PATH}")"

# Localiza sqlcmd en el host (contenedores de CI) o recurre a Docker.
if command -v sqlcmd >/dev/null 2>&1; then
  SQLCMD_MODE="local"
elif [ -x /opt/mssql-tools18/bin/sqlcmd ]; then
  SQLCMD_MODE="local"
  PATH="$PATH:/opt/mssql-tools18/bin"
elif [ -x /opt/mssql-tools/bin/sqlcmd ]; then
  SQLCMD_MODE="local"
  PATH="$PATH:/opt/mssql-tools/bin"
elif command -v docker >/dev/null 2>&1; then
  SQLCMD_MODE="docker"
else
  echo "ERROR: no se encontró sqlcmd ni Docker para ejecutar la inicialización." >&2
  echo "       Instale mssql-tools18 o ejecute este script en un host con Docker." >&2
  exit 1
fi

# Ejecuta sqlcmd en el host o dentro de un contenedor efímero (red del host).
sqlcmd() {
  if [ "$SQLCMD_MODE" = "local" ]; then
    command sqlcmd "$@"
    return
  fi
  docker run --rm --network host \
    --entrypoint /bin/bash "${IMAGE}" -c \
      'TOOLS=/opt/mssql-tools18/bin/sqlcmd; [ -x "$TOOLS" ] || TOOLS=/opt/mssql-tools/bin/sqlcmd; exec "$TOOLS" "$@"' \
      -- "$@"
}

echo "Esperando a SQL Server en ${SERVER}..."
for attempt in $(seq 1 60); do
  if sqlcmd -C -S "${SERVER}" -U sa -P "${SA_PASSWORD}" -Q "SELECT 1" >/dev/null 2>&1; then
    echo "SQL Server disponible."
    break
  fi
  if [ "${attempt}" -eq 60 ]; then
    echo "ERROR: SQL Server no respondió tras 60 intentos." >&2
    exit 1
  fi
  echo "  intento ${attempt}/60..."
  sleep 5
done

echo "Creando la base de datos '${DB_NAME}' (si no existe)..."
sqlcmd -C -S "${SERVER}" -U sa -P "${SA_PASSWORD}" \
  -Q "IF DB_ID('${DB_NAME}') IS NULL CREATE DATABASE [${DB_NAME}];"

echo "Cargando ${SCRIPT_PATH} en '${DB_NAME}'..."
if [ "$SQLCMD_MODE" = "local" ]; then
  sqlcmd -C -S "${SERVER}" -U sa -P "${SA_PASSWORD}" \
    -d "${DB_NAME}" -i "${SCRIPT_PATH}" -f 65001
else
  docker run --rm --network host \
    -v "${SCRIPT_DIR}/${SCRIPT_FILE}:/scripts/instnwnd.sql:ro" \
    --entrypoint /bin/bash "${IMAGE}" -c \
      'TOOLS=/opt/mssql-tools18/bin/sqlcmd; [ -x "$TOOLS" ] || TOOLS=/opt/mssql-tools/bin/sqlcmd; exec "$TOOLS" "$@"' \
      -- -C -S "${SERVER}" -U sa -P "${SA_PASSWORD}" -d "${DB_NAME}" -i /scripts/instnwnd.sql -f 65001
fi

echo "Base de datos '${DB_NAME}' lista."
