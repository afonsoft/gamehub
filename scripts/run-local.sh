#!/bin/bash
set -e

if [ ! -f .env ]; then
    echo "Copying .env.example to .env"
    cp .env.example .env
fi

docker compose up --build -d
