#!/usr/bin/env bash
#
# Inicializa Northwind dentro del contenedor de SQL Server.
#
# Se ejecuta manualmente (por ejemplo con "docker exec") después de crear el
# servicio. Es idempotente: si dbo.Customers ya existe, no vuelve a cargar
# el script. La contraseña se toma de la variable de entorno del contenedor
# (MSSQL_SA_PASSWORD).
#
set -euo pipefail

SA_PASSWORD="${MSSQL_SA_PASSWORD:?Falta la variable MSSQL_SA_PASSWORD}"
DB_NAME="${DB_NAME:-Northwind}"
SCRIPT_PATH="${SCRIPT_PATH:-/scripts/instnwnd.sql}"

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
if [ ! -x "$SQLCMD" ]; then
  SQLCMD=/opt/mssql-tools/bin/sqlcmd
fi
if [ ! -x "$SQLCMD" ]; then
  echo "ERROR: no se encontró sqlcmd en la imagen." >&2
  exit 1
fi
if [ ! -f "$SCRIPT_PATH" ]; then
  echo "ERROR: no se encontró el script ${SCRIPT_PATH}." >&2
  exit 1
fi

echo "Esperando a SQL Server en localhost:1433..."
for attempt in $(seq 1 60); do
  if "$SQLCMD" -C -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" -b >/dev/null 2>&1; then
    echo "SQL Server disponible."
    break
  fi
  if [ "$attempt" -eq 60 ]; then
    echo "ERROR: SQL Server no respondió después de 5 minutos." >&2
    exit 1
  fi
  sleep 5
done

echo "Creando la base de datos '${DB_NAME}' (si no existe)..."
"$SQLCMD" -C -S localhost -U sa -P "$SA_PASSWORD" -b \
  -Q "IF DB_ID('${DB_NAME}') IS NULL CREATE DATABASE [${DB_NAME}];"

if "$SQLCMD" -C -S localhost -U sa -P "$SA_PASSWORD" -d "$DB_NAME" -b \
  -Q "SELECT TOP (1) 1 FROM dbo.Customers" >/dev/null 2>&1; then
  echo "La base de datos ya contiene dbo.Customers; no se recarga."
  exit 0
fi

echo "Cargando ${SCRIPT_PATH} en ${DB_NAME}..."
"$SQLCMD" -C -S localhost -U sa -P "$SA_PASSWORD" -d "$DB_NAME" \
  -i "$SCRIPT_PATH" -f 65001 -b

echo "Base de datos Northwind inicializada."
