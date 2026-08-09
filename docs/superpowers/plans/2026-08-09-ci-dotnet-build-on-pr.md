# CI: Build Check en PRs Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> Nota del orquestador: tarea mecánica de un único fichero de configuración con spec completa. Se ejecuta inline en la sesión actual, sin repartir en subagentes (regla del proyecto para ediciones mecánicas de 1-3 ficheros).

**Goal:** Añadir un workflow de GitHub Actions que compile el proyecto .NET en cada PR (y en cada push a la rama de esa PR) contra `develop` o `master`, para detectar builds rotos antes del merge.

**Architecture:** Un único workflow YAML (`.github/workflows/build.yml`) con un job en `ubuntu-latest` que hace checkout, instala el SDK de .NET 8 y ejecuta `dotnet restore` + `dotnet build`. Se activa con el evento `pull_request` (incluye automáticamente `opened` y `synchronize`, que cubre nuevos commits en la rama de la PR).

**Tech Stack:** GitHub Actions, `actions/checkout@v4`, `actions/setup-dotnet@v4`, .NET 8 SDK.

---

### Task 1: Crear el workflow de build

**Files:**
- Create: `.github/workflows/build.yml`

- [ ] **Step 1: Crear el directorio y el fichero del workflow**

Contenido exacto de `.github/workflows/build.yml`:

```yaml
name: Build

on:
  pull_request:
    branches:
      - develop
      - master

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore Scraping.sln

      - name: Build
        run: dotnet build Scraping.sln --configuration Release --no-restore
```

- [ ] **Step 2: Verificar que el YAML es sintácticamente válido**

Run (PowerShell, sin dependencias externas — parseo manual de indentación/estructura no es fiable, así que se usa Python si está disponible; si no, revisión visual de la indentación):

```powershell
python -c "import yaml; yaml.safe_load(open('.github/workflows/build.yml')); print('YAML OK')"
```

Expected: `YAML OK`. Si Python no está disponible en la máquina, saltar este paso y confiar en que GitHub Actions validará el YAML al primer push (se verá reflejado como check en la PR).

- [ ] **Step 3: Commit**

```bash
git add .github/workflows/build.yml docs/superpowers/plans/2026-08-09-ci-dotnet-build-on-pr.md
git commit -m "Añadir workflow de build en PRs contra develop/master"
```

---

### Task 2: Publicar la rama y abrir la PR

**Files:** (ninguno adicional — solo operaciones de git/GitHub)

- [ ] **Step 1: Crear la rama desde develop**

```bash
git checkout -b ci/dotnet-build-on-pr origin/develop
```

(Ejecutar esto ANTES del Task 1, ya que los ficheros se crean sobre esta rama. Ver orden real de ejecución: primero rama, luego fichero, luego commit.)

- [ ] **Step 2: Push usando GITHUB_TOKEN (sin gh CLI)**

Vía PowerShell, con el token cargado desde `$PROFILE`:

```powershell
. $PROFILE
$url = "https://x-access-token:$env:GITHUB_TOKEN@github.com/Junimo-s-Team/junivalley-dataScraping.git"
git push $url ci/dotnet-build-on-pr:ci/dotnet-build-on-pr
```

Expected: `* [new branch]      ci/dotnet-build-on-pr -> ci/dotnet-build-on-pr`

- [ ] **Step 3: Abrir la PR vía API de GitHub hacia develop**

```powershell
. $PROFILE
$headers = @{ Authorization = "token $env:GITHUB_TOKEN"; "User-Agent" = "claude-code"; Accept = "application/vnd.github+json" }
$body = @{
  title = "Añadir workflow de build en PRs (CI)"
  head  = "ci/dotnet-build-on-pr"
  base  = "develop"
  body  = "Añade .github/workflows/build.yml: compila el proyecto en cada PR contra develop/master y en cada push a la rama de la PR (evento pull_request cubre opened y synchronize). Sin paso de tests porque el repo no tiene tests automatizados todavía."
} | ConvertTo-Json

$pr = Invoke-RestMethod -Uri "https://api.github.com/repos/Junimo-s-Team/junivalley-dataScraping/pulls" -Headers $headers -Method Post -Body $body -ContentType "application/json; charset=utf-8"
$pr.html_url
```

Expected: URL de la PR impresa, ej. `https://github.com/Junimo-s-Team/junivalley-dataScraping/pull/5`.

- [ ] **Step 4: Confirmar que el check de Actions arrancó en la PR**

```powershell
. $PROFILE
$headers = @{ Authorization = "token $env:GITHUB_TOKEN"; "User-Agent" = "claude-code" }
Invoke-RestMethod -Uri "https://api.github.com/repos/Junimo-s-Team/junivalley-dataScraping/actions/runs?branch=ci/dotnet-build-on-pr" -Headers $headers | Select-Object -ExpandProperty workflow_runs | Select-Object -First 1 status, conclusion, html_url
```

Expected: un run con `status: queued` o `in_progress` (o `completed`/`success` si ya terminó). Si `status` es `completed` y `conclusion` es `failure`, revisar el log del run antes de dar la tarea por cerrada.
