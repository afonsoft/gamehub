#!/bin/bash
set -e

if [ ! -f .env ]; then
    echo "Copying .env.example to .env"
    cp .env.example .env
fi

docker compose -f docker-compose.infra.yml -f docker-compose.yml up --build -d
