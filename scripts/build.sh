#!/bin/bash
set -e

echo "Building backend..."
dotnet build Api/GameHub.sln

echo "Building angular hub..."
cd angular
npm ci
npm run build
cd ..

echo "Building angular admin..."
cd angular-admin/GameHub.UI
npm ci
npm run build
cd ../..

echo "Build complete."
