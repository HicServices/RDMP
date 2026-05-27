# Mac test environment for RDMP

This folder bootstraps a working `Rdmp.Core.Tests` environment on macOS
(Apple Silicon or Intel) using Docker.  The DB-backed tests (~143 fixtures)
need a real SQL Server instance; this folder spins one up in a container,
creates the four platform databases the tests expect, and points the test
runner at it.

> **Status:** verified end-to-end on `darwin/arm64`, .NET SDK 10.0.107,
> Docker 28.x. All 102 `CohortCreation` tests pass.

---

## TL;DR

```bash
# from the repo root, one-time setup
bash Documentation/MacTestEnv/setup.sh

# run the cohort-creation slice (the slice this folder was created to support)
bash Documentation/MacTestEnv/run-tests.sh

# tear it down at the end of the day
bash Documentation/MacTestEnv/teardown.sh           # keeps the DB volume
bash Documentation/MacTestEnv/teardown.sh --purge   # wipes the DB volume too
```

---

## What's in this folder

| File | Purpose |
|---|---|
| `README.md` | This file. |
| `docker-compose.yml` | Declarative SQL Server 2022 container definition. |
| `setup.sh` | One-shot bootstrap: starts the container, builds the `rdmp` CLI, creates `TEST_*` databases, installs `TestDatabases.txt`. |
| `teardown.sh` | Stops the container and restores `Tests.Common/TestDatabases.txt`. |
| `run-tests.sh` | Thin wrapper around `dotnet test` that adds the warnings-as-errors workaround. |
| `TestDatabases.txt` | The Mac-flavoured config that `setup.sh` copies into `Tests.Common/`. |

`setup.sh` overwrites `Tests.Common/TestDatabases.txt` in your working
tree.  `teardown.sh` runs `git restore` on it to bring the repo back to a
clean state — **do not commit the modified file**.

---

## Prerequisites

| Tool | How to install on macOS |
|---|---|
| Docker Desktop (or Colima) running, ≥ 28.x | <https://docs.docker.com/desktop/install/mac-install/> |
| .NET SDK 10.x | `brew install dotnet` |
| Bash & standard `coreutils` | shipped with macOS |

Apple Silicon users: Docker Desktop must have **Rosetta** enabled
(Settings → General → "Use Rosetta for x86_64/amd64 emulation").
The SQL Server image is x86_64-only and runs under Rosetta.

Verify before continuing:

```bash
dotnet --version          # 10.x
docker version            # daemon must respond
uname -m                  # arm64 (Apple Silicon) or x86_64 (Intel)
```

---

## Step-by-step (what `setup.sh` does, manually)

If you want to understand or audit every step, this is what the script
does.

### 1.  Start SQL Server 2022 in Docker

```bash
docker compose -f Documentation/MacTestEnv/docker-compose.yml up -d
```

Equivalent direct command:

```bash
docker run --platform linux/amd64 \
    --name rdmp-mssql \
    -e "ACCEPT_EULA=Y" \
    -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
    -e "MSSQL_PID=Developer" \
    -p 1433:1433 \
    -v mssql-data:/var/opt/mssql \
    -d mcr.microsoft.com/mssql/server:2022-latest
```

Key choices:
* `--platform linux/amd64` — required on Apple Silicon (no native arm64 image).
* `MSSQL_PID=Developer` — Developer edition is free and full-featured for testing.
* The password **must** be ≥ 8 chars including upper, lower, digit, symbol —
  SQL Server refuses to start otherwise.
* `mssql-data` volume persists the databases across `stop`/`start` cycles.

### 2.  Wait for the engine to accept logins

Under Rosetta emulation, SQL Server takes ~60–90 s on first boot:

```bash
until docker exec rdmp-mssql \
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C \
    -Q "SELECT 1" >/dev/null 2>&1; do
    echo "waiting..."; sleep 5
done
```

### 3.  Build the `rdmp` CLI

```bash
dotnet build Tools/rdmp/rdmp.csproj -c Release \
    -p:WarningsNotAsErrors='"NU1902;NU1903;NU1904"'
```

The `WarningsNotAsErrors` flag is a workaround — see "Known issues" below.

### 4.  Create the four platform databases

```bash
dotnet Tools/rdmp/bin/Release/net10.0/rdmp.dll install \
    "localhost,1433" TEST_ -u sa -p 'YourStrong!Passw0rd' -d
```

This creates: `TEST_Catalogue`, `TEST_DataExport`, `TEST_DQE`,
`TEST_Logging`.  The `-d` flag drops them first if they already exist (so
this command is idempotent).  Verify with:

```bash
docker exec rdmp-mssql /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P 'YourStrong!Passw0rd' -C \
    -Q "SELECT name FROM sys.databases WHERE name LIKE 'TEST_%' ORDER BY name"
```

### 5.  Point the test runner at the container

Copy this folder's `TestDatabases.txt` into `Tests.Common/`:

```bash
cp Documentation/MacTestEnv/TestDatabases.txt Tests.Common/TestDatabases.txt
```

The replacement differs from the committed default in three lines:

```diff
- ServerName:	(localdb)\MSSQLLocalDB
+ ServerName:	localhost,1433
+ Username:	sa
+ Password:	YourStrong!Passw0rd
- MySql:	server=127.0.0.1;Uid=root;Pwd=YourStrong!Passw0rd;AllowPublicKeyRetrieval=True
+ #MySql:	server=127.0.0.1;Uid=root;Pwd=YourStrong!Passw0rd;AllowPublicKeyRetrieval=True
```

* `(localdb)\MSSQLLocalDB` only exists on Windows.
* SA credentials replace integrated security (which the container doesn't support).
* MySQL is commented out because we don't run a MySQL container here.
  If a fixture is annotated with `[RequiresMySql]` or similar it'll be
  skipped rather than fail.

### 6.  Run the tests

```bash
dotnet test Rdmp.Core.Tests/Rdmp.Core.Tests.csproj \
    -p:WarningsNotAsErrors='"NU1902;NU1903;NU1904"' \
    --filter "FullyQualifiedName~CohortCreation"
```

Expected: `Passed: 102, Failed: 0`, total ~3 minutes on Apple Silicon
(amd64 emulation is the bottleneck — Intel Macs are noticeably faster).

---

## Daily commands

```bash
# Container lifecycle
docker compose -f Documentation/MacTestEnv/docker-compose.yml start
docker compose -f Documentation/MacTestEnv/docker-compose.yml stop
docker compose -f Documentation/MacTestEnv/docker-compose.yml logs -f mssql

# Reset the platform DBs without touching the container
dotnet Tools/rdmp/bin/Release/net10.0/rdmp.dll install \
    "localhost,1433" TEST_ -u sa -p 'YourStrong!Passw0rd' -d

# Run a single test
bash Documentation/MacTestEnv/run-tests.sh "FullyQualifiedName~SimpleCohortIdentificationTests.ContainerCreate"

# Run a fixture by class
bash Documentation/MacTestEnv/run-tests.sh "FullyQualifiedName~CohortCompilerTests"

# Run everything (slow under emulation)
bash Documentation/MacTestEnv/run-tests.sh ""

# Open a SQL prompt against the container
docker exec -it rdmp-mssql /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P 'YourStrong!Passw0rd' -C
```

---

## Building a Windows binary from Mac

`dotnet` is a cross-compiler.  As long as the project declares
`<EnableWindowsTargeting>true</EnableWindowsTargeting>` (the GUI project
does) and the host has the .NET 10 SDK, you can produce a self-contained
Windows `.exe` from macOS in one command.  Useful when you want to
validate a UI patch on Windows without setting up a full Windows dev
environment, ship a one-off build to a colleague, or reproduce a
release-style binary locally.

### GUI client (`ResearchDataManagementPlatform.exe`)

```bash
dotnet publish Application/ResearchDataManagementPlatform/ResearchDataManagementPlatform.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:WarningsNotAsErrors='"NU1902;NU1903;NU1904"' \
    -p:PublishReadyToRun=true
```

Output: `Application/ResearchDataManagementPlatform/bin/Release/net10.0-windows/win-x64/publish/`

Expect 2-4 minutes the first time; subsequent publishes are faster
thanks to the package cache.  Zip the `publish/` folder and transfer
to a Windows machine — no .NET install is needed on the target because
the runtime is bundled (`--self-contained true`).

```bash
( cd Application/ResearchDataManagementPlatform/bin/Release/net10.0-windows/win-x64/ && \
  zip -r ~/Downloads/rdmp-win-x64.zip publish/ )
```

### CLI tool (`rdmp.exe`)

Same pattern, different project:

```bash
dotnet publish Tools/rdmp/rdmp.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:WarningsNotAsErrors='"NU1902;NU1903;NU1904"'
```

Output: `Tools/rdmp/bin/Release/net10.0/win-x64/publish/`

### Companion fix on this branch

This branch also ships a one-line fix in
`Rdmp.Core/Repositories/Managers/CommentStoreWithKeywords.cs` that
makes the help-text parser tolerate any line-ending convention.
Without it, a binary built on a non-Windows host crashes on startup
with *"Malformed line in Resources.KeywordHelp"* because the embedded
`KeywordHelp.txt` is checked out with LF endings on Linux/macOS while
the parser splits on `Environment.NewLine` (i.e. `\r\n` at runtime on
Windows).  See the commit message for the full story.

### First launch on Windows

The .exe is unsigned, so Windows SmartScreen typically shows
"Windows protected your PC" on first launch — click **More info** →
**Run anyway**.  Files transferred from another machine may also be
marked untrusted: right-click the .exe → **Properties** → tick
**Unblock**.  Both are expected for any locally-built binary, not a
security issue with the build.

### Why this works

WinForms code compiles to MSIL exactly the same way on Mac as it would
on Windows; what's not portable is the *runtime*, which relies on
Win32 APIs to actually draw a window.  Publishing with
`--self-contained true -r win-x64` bundles the .NET runtime for x64
Windows into the output folder so the target machine doesn't need a
separate .NET install.

That's also why **the resulting binaries cannot be launched on macOS** —
they're Windows-targeted assemblies plus the Windows runtime.  On a
Mac, `dotnet build` succeeds but `dotnet test` against the WinForms
project will fail to load the assemblies — Linux/macOS .NET can't host
a `net10.0-windows` target.  See "Known issues" below.

---

## Credentials

Stored in plaintext on purpose — this is a throwaway local dev environment,
not production.

| Field | Value |
|---|---|
| Server | `localhost,1433` |
| User | `sa` |
| Password | `YourStrong!Passw0rd` |
| Prefix | `TEST_` |

If you change the password, update it in **three** places:
`docker-compose.yml`, `setup.sh`, and `TestDatabases.txt`.

---

## Known issues / workarounds

### Warnings-as-errors during build/test

`Rdmp.Core/Rdmp.Core.csproj` sets `TreatWarningsAsErrors=true` and *overrides*
the inherited `<NoWarn>` from `Directory.Build.props` instead of appending to
it (line 20: `<NoWarn>1701;1702;CS1591;SCS0018</NoWarn>` — drops
`NU1902;NU1903;NU1904`).  The result: package-vulnerability warnings get
promoted back to errors and `dotnet restore`/`build`/`test` all fail.

We side-step it on the command line via:

```
-p:WarningsNotAsErrors='"NU1902;NU1903;NU1904"'
```

A clean fix in the .csproj would be:

```xml
<NoWarn>$(NoWarn);1701;1702;CS1591;SCS0018</NoWarn>
```

(That isn't done here so this folder stays purely additive — no edits to
upstream project files.)

### Performance under amd64 emulation

On Apple Silicon, SQL Server runs through Rosetta and is noticeably slower
than native.  Expect ~3 minutes for the `CohortCreation` slice (102 tests)
vs ~1 minute on native Windows.  This is acceptable for develop-test loops
but worth knowing before running the full suite.

### Tests that need MySQL/Oracle/PostgreSQL

Some fixtures iterate over multiple [DBMS]: ../CodeTutorials/Glossary.md#DBMS types via `[TestCase(DatabaseType.MySql)]`
etc.  With MySQL/Oracle/PostgreSQL absent from `TestDatabases.txt`, those
cases either skip with `Assert.Inconclusive` or fail with a connection
error depending on how they're written.  If you need full coverage, add
matching containers (e.g. a `mysql:8` service in `docker-compose.yml`) and
re-enable the corresponding line in `TestDatabases.txt`.

### Tests that need Windows-specific features

`Rdmp.UI` is Windows Forms and can be *compiled* on macOS (`dotnet build`
succeeds) but **cannot be launched** — Windows Forms has no Mac runtime.
Use a Windows machine or Windows CI runner for any end-to-end UI testing.

---

## Troubleshooting

**`docker exec rdmp-mssql … sqlcmd: not found`**
Older SQL Server images put the tool at `/opt/mssql-tools/bin/sqlcmd`
(without `18`).  The 2022 image we use has it at
`/opt/mssql-tools18/bin/sqlcmd` and requires the `-C` flag (trust the
self-signed cert).

**`Login failed for user 'sa'`**
The password didn't meet complexity rules.  Stop & remove the container,
fix the password in `docker-compose.yml`, and re-run `setup.sh`.

**Tests fail with `Could not find file 'TestDatabases.txt'`**
The file isn't being copied to the test output directory.  Make sure
`Tests.Common/Tests.Common.csproj` still has the `<Content Include="TestDatabases.txt">`
item with `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`.

**Container starts but `setup.sh` hangs at "Waiting for SQL Server"**
Check `docker logs rdmp-mssql`.  Common causes:
  * Insufficient memory (Docker Desktop default 2 GB; bump to ≥ 4 GB).
  * Password rejected (see above).
  * Rosetta disabled on Apple Silicon.
