#!/bin/bash
set -e

echo "Testing backend..."
dotnet test Api/GameHub.sln

echo "Testing angular hub..."
cd angular
npm ci
npm run build
cd ..

echo "Tests complete."
