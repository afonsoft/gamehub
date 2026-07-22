#!/usr/bin/env bash
set -euo pipefail

# GameHub — install script para API, Admin e Hub (sem infraestrutura)
# Requisito: a infraestrutura (PostgreSQL/Redis/MinIO) deve estar rodando
#            via docker-compose.infra.yml ou equivalente antes de subir a aplicação.

cd "$(dirname "$0")"

COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.app.yml}"

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
                        echo "${key}=gamehub"
                        ;;
                    POSTGRES_USER)
                        echo "${key}=gamehub"
                        ;;
                    REDIS_CONNECTION)
                        echo "${key}=host.docker.internal:6379"
                        ;;
                    STORAGE_PROVIDER)
                        echo "${key}=MinIO"
                        ;;
                    MINIO_ENDPOINT)
                        echo "${key}=http://host.docker.internal:9000"
                        ;;
                    GAMEHUB_API_URL)
                        echo "${key}=https://gamehub-api.afonsoft.dev/"
                        ;;
                    GAMEHUB_HUB_URL)
                        echo "${key}=https://gamehub.afonsoft.dev/"
                        ;;
                    GAMEHUB_ADMIN_URL)
                        echo "${key}=https://gamehub-admin.afonsoft.dev/"
                        ;;
                    GAMEHUB_CORS_ORIGINS)
                        echo "${key}=https://gamehub.afonsoft.dev,https://gamehub-admin.afonsoft.dev"
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
    docker compose -f "$COMPOSE_FILE" build

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
echo "Executando pull, build e up dos containers..."
echo "=============================================="
echo ""

docker compose -f "$COMPOSE_FILE" pull
docker compose -f "$COMPOSE_FILE" build
docker compose -f "$COMPOSE_FILE" up -d

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
