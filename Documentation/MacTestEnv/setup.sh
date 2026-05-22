#!/usr/bin/env bash
# Bootstraps a Mac (Apple Silicon or Intel) test environment for RDMP:
#   1. starts SQL Server 2022 in Docker
#   2. builds the rdmp CLI
#   3. creates the four TEST_ platform databases
#   4. installs the Mac TestDatabases.txt into Tests.Common/
#
# Re-runnable. Safe to invoke multiple times.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
SA_PASSWORD="YourStrong!Passw0rd"
# NU1902/1903/1904 are NuGet vulnerability warnings.  The Rdmp.Core.csproj
# overrides <NoWarn> from Directory.Build.props (drops these three) while
# TreatWarningsAsErrors=true is still in effect, so they become hard errors.
# We demote them on the command line via an array (eval-safe).
WARN_OVERRIDE=(-p:WarningsNotAsErrors='"NU1902;NU1903;NU1904"')

cd "${REPO_ROOT}"

echo "==> 1/4  Starting SQL Server 2022 container..."
docker compose -f "${SCRIPT_DIR}/docker-compose.yml" up -d

echo "==> 2/4  Waiting for SQL Server to accept logins..."
until docker exec rdmp-mssql /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "${SA_PASSWORD}" -C -Q "SELECT 1" >/dev/null 2>&1; do
    printf '.'
    sleep 5
done
echo " ready"

echo "==> 3/4  Building rdmp CLI (Release)..."
dotnet build Tools/rdmp/rdmp.csproj -c Release "${WARN_OVERRIDE[@]}" >/dev/null

echo "         Creating TEST_ platform databases (drop-and-recreate)..."
dotnet Tools/rdmp/bin/Release/net10.0/rdmp.dll install \
    "localhost,1433" TEST_ -u sa -p "${SA_PASSWORD}" -d >/dev/null
echo "         Databases created: TEST_Catalogue, TEST_DataExport, TEST_DQE, TEST_Logging"

echo "==> 4/4  Installing Mac TestDatabases.txt into Tests.Common/..."
cp "${SCRIPT_DIR}/TestDatabases.txt" "${REPO_ROOT}/Tests.Common/TestDatabases.txt"

echo
echo "Done. Try:"
echo "  bash ${SCRIPT_DIR}/run-tests.sh"
