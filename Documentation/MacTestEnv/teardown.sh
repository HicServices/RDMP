#!/usr/bin/env bash
# Tears down the Mac test environment created by setup.sh.
#   - stops & removes the SQL Server container
#   - optionally removes the data volume (-v)
#   - restores Tests.Common/TestDatabases.txt to its committed state
#
# Usage:
#   bash teardown.sh           # remove container only, keep DB volume
#   bash teardown.sh --purge   # also delete the persisted DB volume

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

cd "${REPO_ROOT}"

if [[ "${1:-}" == "--purge" ]]; then
    echo "==> Stopping container and deleting data volume..."
    docker compose -f "${SCRIPT_DIR}/docker-compose.yml" down -v
else
    echo "==> Stopping container (volume preserved; pass --purge to delete)..."
    docker compose -f "${SCRIPT_DIR}/docker-compose.yml" down
fi

echo "==> Restoring Tests.Common/TestDatabases.txt to committed state..."
git restore Tests.Common/TestDatabases.txt 2>/dev/null \
    || echo "   (no git restore needed)"

echo "Done."
