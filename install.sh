#!/usr/bin/env bash
set -euo pipefail

# GameHub — install script para API, Admin e Hub (sem infraestrutura)
# Requisito: a infraestrutura (PostgreSQL/Redis/MinIO) deve estar rodando
#            no host ou via docker-compose.all.yml antes de subir a aplicação.

cd "$(dirname "$0")"

COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.yml}"
REBUILD=false

usage() {
    echo "Uso: $0 [-r]" >&2
    echo "  -r  Força o rebuild das imagens Docker (--no-cache --pull) e recria os containers" >&2
    exit 1
}

while getopts ":r" opt; do
    case $opt in
        r)
            REBUILD=true
            ;;
        \?)
            echo "Opção inválida: -$OPTARG" >&2
            usage
            ;;
    esac
done

if [ "$REBUILD" = true ]; then
    BUILD_OPTS="--no-cache --pull"
    UP_OPTS="-d --force-recreate"
else
    BUILD_OPTS=""
    UP_OPTS="-d"
fi

if [ ! -f "$COMPOSE_FILE" ]; then
    echo "Erro: arquivo '$COMPOSE_FILE' não encontrado no diretório atual." >&2
    exit 1
fi

echo "Parando containers existentes (se houver)..."
docker compose -f "$COMPOSE_FILE" down --remove-orphans || true
echo ""

if [ ! -f ".env" ]; then
    echo "=============================================="
    echo ".env não encontrado."
    echo "Criando .env com valores 'A PREENCHER'..."
    echo "=============================================="
    echo ""

    if [ ! -f ".env.example" ]; then
        echo "Erro: .env.example não encontrado. Não é possível gerar .env automaticamente." >&2
        exit 1
    fi

    while IFS= read -r line || [ -n "$line" ]; do
        # Preserva linhas em branco e comentários
        if [[ "$line" =~ ^[[:space:]]*$ ]] || [[ "$line" =~ ^[[:space:]]*# ]]; then
            echo "$line"
        else
            key="${line%%=*}"
            if [ -n "$key" ]; then
                case "$key" in
                    ASPNETCORE_ENVIRONMENT)
                        echo "${key}=Production"
                        ;;
                    POSTGRES_HOST)
                        echo "${key}=host.docker.internal"
                        ;;
                    POSTGRES_PORT)
                        echo "${key}=5432"
                        ;;
                    POSTGRES_DB)
                        echo "${key}=gamehub_db"
                        ;;
                    POSTGRES_USER)
                        echo "${key}=gamehub_user"
                        ;;
                    REDIS_CONNECTION)
                        echo "${key}=host.docker.internal:6379,abortConnect=false"
                        ;;
                    STORAGE_PROVIDER)
                        echo "${key}=MinIO"
                        ;;
                    MINIO_ENDPOINT)
                        echo "${key}=http://gamehub-minio:9000"
                        ;;
                    MINIO_ACCESS_KEY)
                        echo "${key}=gamehub_user"
                        ;;
                    MINIO_SECRET_KEY)
                        echo "${key}="
                        ;;
                    MINIO_BUCKET)
                        echo "${key}=gamehub-builds"
                        ;;
                    MINIO_REGION)
                        echo "${key}=us-east-1"
                        ;;
                    MINIO_FORCEPATHSTYLE)
                        echo "${key}=true"
                        ;;
                    OTEL_EXPORTER_OTLP_ENDPOINT)
                        echo "${key}=https://otlp.nr-data.net:4318"
                        ;;
                    OTEL_EXPORTER_OTLP_PROTOCOL)
                        echo "${key}=http/protobuf"
                        ;;
                    OTEL_EXPORTER_OTLP_HEADERS)
                        echo "${key}=api-key=A PREENCHER"
                        ;;
                    GAMEHUB_API_URL)
                        echo "${key}=https://gamehub-api.afonsoft.dev"
                        ;;
                    GAMEHUB_HUB_URL)
                        echo "${key}=https://gamehub.afonsoft.dev"
                        ;;
                    GAMEHUB_ADMIN_URL)
                        echo "${key}=https://gamehub-admin.afonsoft.dev"
                        ;;
                    GAMEHUB_CORS_ORIGINS)
                        echo "${key}=https://gamehub.afonsoft.dev,https://gamehub-admin.afonsoft.dev"
                        ;;
                    Database__Provider)
                        echo "${key}=PostgreSQL"
                        ;;
                    POSTGRES_PASSWORD)
                        echo "${key}="
                        ;;
                    *)
                        echo "${key}=A PREENCHER"
                        ;;
                esac
            else
                echo "$line"
            fi
        fi
    done < .env.example > .env

    echo ".env criado com os DNS de produção preenchidos. Preencha as demais variáveis (PostgreSQL, Redis, JWT, MinIO) antes de subir os containers."
    echo ""
    echo "Executando docker compose pull e build (sem subir os containers)..."
    echo ""

    docker compose -f "$COMPOSE_FILE" pull
    # shellcheck disable=SC2086
    docker compose -f "$COMPOSE_FILE" build $BUILD_OPTS

    echo ""
    echo "=============================================="
    echo "Pull e build concluídos com sucesso."
    echo ""
    echo "Próximos passos:"
    echo "  1. Edite o arquivo .env com os valores reais."
    echo "  2. Execute novamente este script para subir a aplicação."
    echo "=============================================="
    exit 0
fi

echo "=============================================="
echo ".env encontrado."

# Garante abortConnect=false na connection string do Redis para nao falhar no startup
if grep -qE '^REDIS_CONNECTION=' .env && ! grep -qE '^REDIS_CONNECTION=.*abortConnect=false' .env; then
    sed -i 's|^\(REDIS_CONNECTION=.*\)|\1,abortConnect=false|' .env
    echo "Ajustada REDIS_CONNECTION para abortConnect=false (evita crash quando Redis nao esta disponivel no startup)."
fi

echo "Executando pull, build e up dos containers..."
echo "=============================================="
echo ""

docker compose -f "$COMPOSE_FILE" pull
# shellcheck disable=SC2086
docker compose -f "$COMPOSE_FILE" build $BUILD_OPTS
# shellcheck disable=SC2086
docker compose -f "$COMPOSE_FILE" up $UP_OPTS

echo ""
echo "=============================================="
echo "Aplicação iniciada."
echo ""
echo "Serviços disponíveis:"
echo "  - API:    http://localhost:4601"
echo "  - Hub:    http://localhost:4600"
echo "  - Admin:  http://localhost:4602"
echo ""
echo "Para acompanhar logs:"
echo "  docker compose -f $COMPOSE_FILE logs -f"
echo "=============================================="
