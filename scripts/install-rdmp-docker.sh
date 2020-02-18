#!/bin/bash

set -ev

/opt/mssql/bin/sqlservr &> /dev/null &
# TODO(rkm 2020-02-12) Find a better way to wait for the DB server to start...
sleep 10
./rdmp install localhost $DB_PREFIX -u SA -p $MSSQL_SA_PASSWORD

# NOTE(rkm 2020-02-12) Not needed for the installation, but allows the rdmp-cli to be used
cat <<EOT > ./Databases.yaml
CatalogueConnectionString: Server=localhost;user=SA;password=$MSSQL_SA_PASSWORD;Database=${DB_PREFIX}Catalogue;
DataExportConnectionString: Server=localhost;user=SA;password=$MSSQL_SA_PASSWORD;Database=${DB_PREFIX}DataExport;
EOT
