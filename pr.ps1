#!/usr/bin/env pwsh

<#
.SYNOPSIS
Abre PR de developer para main

.EXAMPLE
.\pr.ps1
#>

Write-Host ""
Write-Host "Abrindo PR: developer -> main" -ForegroundColor Cyan
Write-Host ""

# Verificar se gh esta instalado
if (Get-Command gh -ErrorAction SilentlyContinue) {
    # Usar GitHub CLI
    gh pr create --base main --head developer --web
} else {
    # Abrir no navegador
    $url = "https://github.com/LuluDiegs/PlantaCoreBackend/compare/main...developer?expand=1"
    Start-Process $url
    Write-Host "Navegador aberto!" -ForegroundColor Green
}

Write-Host ""
