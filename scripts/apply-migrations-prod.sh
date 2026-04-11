#!/bin/bash

# Script para aplicacao segura de migrations em producao
# Uso: ./apply-migrations-prod.sh [--dry-run]

set -e

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuracoes
STARTUP_PROJECT="PlantaCoreAPI.API"
MIGRATIONS_PROJECT="PlantaCoreAPI.Infrastructure"
DRY_RUN=false

# Parse argumentos
if [[ "$1" == "--dry-run" ]]; then
    DRY_RUN=true
fi

# Funcoes auxiliares
print_header() {
    echo -e "\n${CYAN}========================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}========================================${NC}\n"
}

print_success() {
    echo -e "${GREEN}[OK] $1${NC}"
}

print_error() {
    echo -e "${RED}[ERRO] $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}[AVISO] $1${NC}"
}

print_info() {
    echo -e "${CYAN}[INFO] $1${NC}"
}

# Verifica se esta em producao
check_environment() {
    print_header "VERIFICANDO AMBIENTE"

    if [[ -z "$PROD_DB_CONNECTION" ]]; then
        print_error "PROD_DB_CONNECTION nao esta configurada!"
        print_info "Configure a variavel de ambiente antes de continuar."
        exit 1
    fi

    # Verifica se nao esta em DEV
    if [[ "$PROD_DB_CONNECTION" == *"localhost"* ]] || [[ "$PROD_DB_CONNECTION" == *"127.0.0.1"* ]]; then
        print_warning "A connection string parece ser local (DEV)"
        read -p "Tem certeza que deseja continuar? (s/N) " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Ss]$ ]]; then
            exit 1
        fi
    fi

    print_success "Ambiente validado"
}

# Verifica ferramentas
check_tools() {
    print_header "VERIFICANDO FERRAMENTAS"

    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK nao encontrado"
        exit 1
    fi

    print_info ".NET SDK: $(dotnet --version)"

    # Instala/Atualiza dotnet-ef
    if ! dotnet tool list -g | grep -q "dotnet-ef"; then
        print_info "Instalando dotnet-ef..."
        dotnet tool install --global dotnet-ef
    fi

    print_success "Ferramentas validadas"
}

# Build do projeto
build_project() {
    print_header "BUILDING PROJETO"

    dotnet restore
    dotnet build --configuration Release --no-restore

    if [ $? -ne 0 ]; then
        print_error "Falha no build"
        exit 1
    fi

    print_success "Build concluido"
}

# Lista migrations pendentes
check_pending_migrations() {
    print_header "VERIFICANDO MIGRATIONS PENDENTES"

    dotnet ef migrations list \
        --project $MIGRATIONS_PROJECT \
        --startup-project $STARTUP_PROJECT \
        --no-build

    if [ $? -ne 0 ]; then
        print_error "Erro ao listar migrations"
        exit 1
    fi
}

# Gera script SQL
generate_sql_script() {
    print_header "GERANDO SCRIPT SQL"

    local timestamp=$(date +%Y%m%d-%H%M%S)
    local sql_file="migrations-prod-${timestamp}.sql"

    dotnet ef migrations script \
        --project $MIGRATIONS_PROJECT \
        --startup-project $STARTUP_PROJECT \
        --idempotent \
        --output "$sql_file" \
        --no-build

    if [ $? -ne 0 ]; then
        print_error "Erro ao gerar script SQL"
        exit 1
    fi

    print_success "Script SQL gerado: $sql_file"

    # Analisa o script
    analyze_sql_script "$sql_file"

    echo "$sql_file"
}

# Analisa o script SQL
analyze_sql_script() {
    local sql_file=$1

    print_header "ANALISANDO SCRIPT SQL"

    # Verifica operacoes destrutivas
    local destructive=$(grep -iE "(DROP TABLE|DROP COLUMN|TRUNCATE|DELETE FROM)" "$sql_file" || true)

    if [[ -n "$destructive" ]]; then
        print_warning "OPERACOES DESTRUTIVAS DETECTADAS:"
        echo "$destructive"
        echo ""
    fi

    # Estatisticas
    local create_tables=$(grep -c "CREATE TABLE" "$sql_file" || true)
    local drop_tables=$(grep -c "DROP TABLE" "$sql_file" || true)
    local add_columns=$(grep -c "ADD COLUMN" "$sql_file" || true)
    local drop_columns=$(grep -c "DROP COLUMN" "$sql_file" || true)

    print_info "Estatisticas:"
    echo "  - Tabelas criadas: $create_tables"
    echo "  - Tabelas removidas: $drop_tables"
    echo "  - Colunas adicionadas: $add_columns"
    echo "  - Colunas removidas: $drop_columns"

    # Preview
    print_header "PREVIEW DO SQL (primeiras 30 linhas)"
    head -n 30 "$sql_file"
    echo ""
    print_info "Veja o arquivo completo: $sql_file"
}

# Backup reminder
backup_reminder() {
    print_header "LEMBRETE DE BACKUP"

    print_warning "IMPORTANTE: Certifique-se de que o backup foi realizado!"
    print_info "Backup recomendado via Supabase Dashboard ou pg_dump"

    if [[ "$DRY_RUN" == false ]]; then
        echo ""
        read -p "Backup realizado e validado? (digite 'SIM' para confirmar): " confirmation

        if [[ "$confirmation" != "SIM" ]]; then
            print_warning "Operacao cancelada. Realize o backup antes de continuar."
            exit 0
        fi
    fi
}

# Aplica migrations
apply_migrations() {
    print_header "APLICANDO MIGRATIONS EM PRODUCAO"

    if [[ "$DRY_RUN" == true ]]; then
        print_info "MODO DRY-RUN: Migrations NAO serao aplicadas"
        return
    fi

    print_warning "Esta operacao ira modificar o banco de PRODUCAO!"
    read -p "Confirma a aplicacao das migrations? (digite 'APLICAR'): " final_confirmation

    if [[ "$final_confirmation" != "APLICAR" ]]; then
        print_warning "Operacao cancelada pelo usuario"
        exit 0
    fi

    print_info "Aplicando migrations..."
    print_info "Data/Hora: $(date)"

    dotnet ef database update \
        --project $MIGRATIONS_PROJECT \
        --startup-project $STARTUP_PROJECT \
        --connection "$PROD_DB_CONNECTION" \
        --verbose

    if [ $? -eq 0 ]; then
        print_success "MIGRATIONS APLICADAS COM SUCESSO!"
    else
        print_error "FALHA AO APLICAR MIGRATIONS!"
        print_warning "Verifique os logs e considere rollback se necessario"
        exit 1
    fi
}

# Verifica estado pos-migration
verify_database() {
    print_header "VERIFICANDO ESTADO DO BANCO"

    dotnet ef migrations list \
        --project $MIGRATIONS_PROJECT \
        --startup-project $STARTUP_PROJECT \
        --connection "$PROD_DB_CONNECTION"

    if [ $? -eq 0 ]; then
        print_success "Banco de dados sincronizado"
    else
        print_error "Erro ao verificar estado do banco"
        exit 1
    fi
}

# Main
main() {
    print_header "APLICACAO DE MIGRATIONS - PRODUCAO"

    if [[ "$DRY_RUN" == true ]]; then
        print_info "Executando em modo DRY-RUN (preview apenas)"
    fi

    check_environment
    check_tools
    build_project
    check_pending_migrations

    sql_file=$(generate_sql_script)

    backup_reminder
    apply_migrations

    if [[ "$DRY_RUN" == false ]]; then
        verify_database
    fi

    print_header "PROCESSO CONCLUIDO"

    if [[ "$DRY_RUN" == true ]]; then
        print_info "Para aplicar as migrations, execute sem --dry-run"
    else
        print_success "Migrations aplicadas com sucesso em PRODUCAO!"
        print_info "Script SQL salvo: $sql_file"
    fi
}

# Executa
main
