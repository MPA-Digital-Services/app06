#!/usr/bin/env bash
#
# Entrypoint de la imagen de SQL Server con volumen persistente.
#
# El volumen puede pertenecer a root; este script (que arranca como root)
# ajusta el propietario y luego ejecuta el entrypoint oficial como el
# usuario mssql.
#
set -euo pipefail

mkdir -p /var/opt/mssql
chown -R mssql:mssql /var/opt/mssql

if command -v runuser >/dev/null 2>&1; then
  exec runuser -u mssql -p -- /opt/mssql/bin/launch_sqlservr.sh
fi

if command -v su >/dev/null 2>&1; then
  exec su -p -s /bin/bash mssql -c 'exec /opt/mssql/bin/launch_sqlservr.sh'
fi

echo "ERROR: no se encontró 'runuser' ni 'su' en la imagen de SQL Server." >&2
exit 1
