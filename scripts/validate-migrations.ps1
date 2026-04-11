#!/usr/bin/env pwsh

<#
.SYNOPSIS
Script para validacao de migrations do EF Core antes de aplicar no banco.

.DESCRIPTION
Este script verifica migrations pendentes, gera preview SQL e valida se ha
operacoes destrutivas antes de aplicar no banco de dados.

.PARAMETER Environment
Ambiente alvo: DEV ou PROD

.PARAMETER Apply
Se deve aplicar as migrations automaticamente (padrao: false)

.EXAMPLE
.\validate-migrations.ps1 -Environment DEV
.\validate-migrations.ps1 -Environment PROD -Apply
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("DEV", "PROD")]
    [string]$Environment,

    [Parameter(Mandatory=$false)]
    [switch]$Apply = $false
)

$ErrorActionPreference = "Stop"

# Cores para output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
}

# Configuracoes
$ProjectRoot = $PSScriptRoot
$StartupProject = "PlantaCoreAPI.API"
$MigrationsProject = "PlantaCoreAPI.Infrastructure"

Write-Header "VALIDACAO DE MIGRATIONS - $Environment"

# Verifica se dotnet-ef esta instalado
Write-ColorOutput "Verificando ferramentas..." "Yellow"
$efTool = dotnet tool list -g | Select-String "dotnet-ef"

if (-not $efTool) {
    Write-ColorOutput "dotnet-ef nao encontrado. Instalando..." "Yellow"
    dotnet tool install --global dotnet-ef
}

Write-ColorOutput "dotnet-ef instalado" "Green"

# Restaura dependencias
Write-ColorOutput "Restaurando dependencias..." "Yellow"
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "Falha ao restaurar dependencias" "Red"
    exit 1
}

# Build do projeto
Write-ColorOutput "Building projeto..." "Yellow"
dotnet build --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "Falha no build" "Red"
    exit 1
}

Write-ColorOutput "Build concluido" "Green"

# Lista migrations
Write-Header "LISTANDO MIGRATIONS"
dotnet ef migrations list `
    --project $MigrationsProject `
    --startup-project $StartupProject `
    --no-build

if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "Erro ao listar migrations" "Red"
    exit 1
}

# Gera script SQL
Write-Header "GERANDO SCRIPT SQL"
$SqlFile = "migrations-$Environment-$(Get-Date -Format 'yyyyMMdd-HHmmss').sql"

dotnet ef migrations script `
    --project $MigrationsProject `
    --startup-project $StartupProject `
    --idempotent `
    --output $SqlFile `
    --no-build

if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "Erro ao gerar script SQL" "Red"
    exit 1
}

Write-ColorOutput "Script SQL gerado: $SqlFile" "Green"

# Analisa o script SQL
Write-Header "ANALISANDO SCRIPT SQL"

$SqlContent = Get-Content $SqlFile -Raw

# Patterns destrutivos
$DestructivePatterns = @(
    "DROP TABLE",
    "DROP COLUMN",
    "TRUNCATE",
    "DELETE FROM"
)

$BreakingPatterns = @(
    "ALTER COLUMN.*NOT NULL",
    "DROP CONSTRAINT",
    "DROP INDEX"
)

$HasDestructive = $false
$HasBreaking = $false

Write-ColorOutput "Verificando operacoes destrutivas..." "Yellow"
foreach ($pattern in $DestructivePatterns) {
    if ($SqlContent -imatch $pattern) {
        Write-ColorOutput "  Encontrado: $pattern" "Red"
        $HasDestructive = $true
    }
}

Write-ColorOutput "Verificando breaking changes..." "Yellow"
foreach ($pattern in $BreakingPatterns) {
    if ($SqlContent -imatch $pattern) {
        Write-ColorOutput "  Possivel breaking change: $pattern" "Yellow"
        $HasBreaking = $true
    }
}

if (-not $HasDestructive -and -not $HasBreaking) {
    Write-ColorOutput "Nenhuma operacao destrutiva detectada" "Green"
}

# Estatisticas
$TableCreations = ([regex]::Matches($SqlContent, "CREATE TABLE")).Count
$TableDrops = ([regex]::Matches($SqlContent, "DROP TABLE")).Count
$ColumnAdds = ([regex]::Matches($SqlContent, "ADD COLUMN")).Count
$ColumnDrops = ([regex]::Matches($SqlContent, "DROP COLUMN")).Count

Write-Header "ESTATISTICAS"
Write-ColorOutput "  Tabelas criadas: $TableCreations" "Cyan"
Write-ColorOutput "  Tabelas removidas: $TableDrops" "Cyan"
Write-ColorOutput "  Colunas adicionadas: $ColumnAdds" "Cyan"
Write-ColorOutput "  Colunas removidas: $ColumnDrops" "Cyan"

# Preview do SQL
Write-Header "PREVIEW DO SQL (primeiras 50 linhas)"
$SqlContent -split "`n" | Select-Object -First 50 | ForEach-Object {
    Write-Host $_ -ForegroundColor DarkGray
}

if ($SqlContent.Split("`n").Count -gt 50) {
    Write-ColorOutput "... (truncado, veja o arquivo completo: $SqlFile)" "DarkGray"
}

# Aplicar migrations?
if ($Apply) {
    Write-Header "APLICANDO MIGRATIONS NO AMBIENTE: $Environment"

    if ($HasDestructive) {
        Write-ColorOutput "ATENCAO: Operacoes destrutivas detectadas!" "Red"
        $confirmation = Read-Host "Deseja continuar? (digite 'SIM' para confirmar)"

        if ($confirmation -ne "SIM") {
            Write-ColorOutput "Operacao cancelada pelo usuario" "Yellow"
            exit 0
        }
    }

    # Carrega connection string do ambiente
    $ConnectionString = $null

    if ($Environment -eq "DEV") {
        $ConnectionString = $env:DEV_DB_CONNECTION
    } else {
        $ConnectionString = $env:PROD_DB_CONNECTION
    }

    if (-not $ConnectionString) {
        Write-ColorOutput "Connection string nao configurada para $Environment" "Red"
        Write-ColorOutput "Configure a variavel de ambiente: ${Environment}_DB_CONNECTION" "Yellow"
        exit 1
    }

    Write-ColorOutput "Aplicando migrations..." "Yellow"

    dotnet ef database update `
        --project $MigrationsProject `
        --startup-project $StartupProject `
        --connection $ConnectionString `
        --verbose

    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "Migrations aplicadas com sucesso!" "Green"
    } else {
        Write-ColorOutput "Falha ao aplicar migrations" "Red"
        exit 1
    }
} else {
    Write-Header "MODO PREVIEW"
    Write-ColorOutput "Para aplicar as migrations, execute:" "Yellow"
    Write-ColorOutput "  .\validate-migrations.ps1 -Environment $Environment -Apply" "Cyan"
}

Write-Header "VALIDACAO CONCLUIDA"
Write-ColorOutput "Script SQL salvo em: $SqlFile" "Green"
