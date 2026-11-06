# syntax=docker/dockerfile:1

# Imagen de SQL Server 2022 con:
#   - el script instnwnd.sql para inicializar Northwind;
#   - un entrypoint que ajusta el volumen persistente;
#   - sqlcmd disponible para ejecutar la inicialización.
FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

COPY instnwnd.sql /scripts/instnwnd.sql
COPY deploy/sqlserver/init-northwind.sh /scripts/init-northwind.sh
COPY deploy/sqlserver/entrypoint.sh /usr/local/bin/sqlserver-entrypoint.sh

RUN chmod +x /scripts/init-northwind.sh /usr/local/bin/sqlserver-entrypoint.sh \
    && chown -R mssql:mssql /scripts /var/opt/mssql

EXPOSE 1433

ENTRYPOINT ["/usr/local/bin/sqlserver-entrypoint.sh"]
