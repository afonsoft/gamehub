#!/bin/bash
set -e

EAF_VERSION="9.4.3"
GAMEHUB_DIR="$(cd "$(dirname "$0")/.." && pwd)"
NUPKG_DIR="$GAMEHUB_DIR/nuget-local"

mkdir -p "$NUPKG_DIR"

# If EAF repository is not present as a sibling, clone it.
EAF_DIR="$GAMEHUB_DIR/../EAF"
if [ ! -d "$EAF_DIR/.git" ]; then
    echo "Cloning EAF repository..."
    git clone https://github.com/afonsoft/EAF.git "$EAF_DIR"
fi

cd "$EAF_DIR"
echo "Building EAF $EAF_VERSION packages..."
dotnet restore Eaf.sln
dotnet build Eaf.sln -c Release --no-restore

echo "Copying EAF packages to $NUPKG_DIR..."
find "$EAF_DIR/src" -path "*/bin/Release/*.nupkg" -type f -exec cp -f {} "$NUPKG_DIR/" \;

echo "EAF packages ready in $NUPKG_DIR:"
ls -1 "$NUPKG_DIR"
