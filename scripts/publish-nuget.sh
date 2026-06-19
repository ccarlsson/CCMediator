#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  scripts/publish-nuget.sh <version> <api-key> [source]

Example:
  scripts/publish-nuget.sh 1.1.0 "$NUGET_API_KEY"

Arguments:
  version   Package version to publish (e.g. 1.1.0)
  api-key   NuGet API key
  source    NuGet source URL (optional)
            default: https://api.nuget.org/v3/index.json

This script enforces publish order:
  1) CCMediator.Core
  2) CCMediator.DependencyInjection
  3) CCMediator (meta package)
EOF
}

if [[ ${1:-} == "-h" || ${1:-} == "--help" ]]; then
  usage
  exit 0
fi

if [[ $# -lt 2 || $# -gt 3 ]]; then
  usage
  exit 1
fi

VERSION="$1"
API_KEY="$2"
SOURCE="${3:-https://api.nuget.org/v3/index.json}"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

PACKAGES=(
  "$ROOT_DIR/CCMediator.Core/bin/Release/CCMediator.Core.$VERSION.nupkg"
  "$ROOT_DIR/CCMediator.DependencyInjection/bin/Release/CCMediator.DependencyInjection.$VERSION.nupkg"
  "$ROOT_DIR/CCMediator/bin/Release/CCMediator.$VERSION.nupkg"
)

echo "Packing solution in Release configuration..."
dotnet pack "$ROOT_DIR/CCMediator.slnx" -c Release

for pkg in "${PACKAGES[@]}"; do
  if [[ ! -f "$pkg" ]]; then
    echo "Missing package: $pkg" >&2
    echo "Check version and build output paths, then try again." >&2
    exit 1
  fi
done

echo "Publishing packages in dependency order..."
for pkg in "${PACKAGES[@]}"; do
  echo "Pushing $(basename "$pkg")"
  dotnet nuget push "$pkg" \
    --api-key "$API_KEY" \
    --source "$SOURCE" \
    --skip-duplicate
done

echo "Publish completed."
