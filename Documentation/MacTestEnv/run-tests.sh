#!/usr/bin/env bash
# Convenience wrapper for `dotnet test` against the Mac Docker SQL Server.
#
# Usage:
#   bash run-tests.sh                              # cohort-creation slice (default)
#   bash run-tests.sh "FullyQualifiedName~CohortCompilerTests"   # any NUnit filter
#   bash run-tests.sh ""                           # full Rdmp.Core.Tests suite (slow)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
FILTER="${1-FullyQualifiedName~CohortCreation}"
# See setup.sh for why this flag exists.
WARN_OVERRIDE=(-p:WarningsNotAsErrors='"NU1902;NU1903;NU1904"')

cd "${REPO_ROOT}"

ARGS=(test Rdmp.Core.Tests/Rdmp.Core.Tests.csproj "${WARN_OVERRIDE[@]}" --logger "console;verbosity=minimal")
if [[ -n "${FILTER}" ]]; then
    ARGS+=(--filter "${FILTER}")
fi

dotnet "${ARGS[@]}"
