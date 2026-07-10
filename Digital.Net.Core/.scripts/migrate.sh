#!/usr/bin/env bash
# Re-exec under bash when invoked through another shell (e.g. `zsh migrate.sh`).
if [ -z "${BASH_VERSION:-}" ]; then exec bash "$0" "$@"; fi
set -euo pipefail

readonly CONTEXT="DigitalContext"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENTITY_PROJECT="$(dirname "$SCRIPT_DIR")"
ROOT_DIR="$(dirname "$ENTITY_PROJECT")"
STARTUP_PROJECT="$ROOT_DIR/Digital.Net.Tests.Program"

usage() {
    cat >&2 <<'EOF'
Usage:
  migrate  [-n, --name <MigrationName>]  -c, --ConnectionString <value>   (prompts for the name if omitted)
  migrate   -u, --undo                   -c, --ConnectionString <value>
EOF
}

die() {
    echo "Error: $*" >&2
    usage
    exit 1
}

undo_seen=0
name_seen=0
name=""
connection_string=""

while [[ $# -gt 0 ]]; do
    case "$1" in
        -n|--name)
            name_seen=1
            # Name is optional here; we prompt for it below when none is supplied.
            if [[ $# -ge 2 && "$2" != -* ]]; then name="$2"; shift 2; else shift; fi
            ;;
        -u|--undo)
            undo_seen=1
            shift
            ;;
        -c|--ConnectionString)
            if [[ $# -lt 2 ]]; then die "A value is required after $1."; fi
            connection_string="$2"
            shift 2
            ;;
        *)
            die "Unknown argument: $1"
            ;;
    esac
done

if [[ $undo_seen -eq 1 && $name_seen -eq 1 ]]; then die "Cannot combine -u/--undo with -n/--name."; fi
if [[ -z "$connection_string" ]]; then die "A connection string is required (-c/--ConnectionString)."; fi

if [[ $undo_seen -eq 1 ]]; then
    action="undo"
else
    action="add"
    if [[ -z "$name" ]]; then
        if [[ ! -t 0 ]]; then die "A migration name is required (-n/--name) in non-interactive mode."; fi
        while [[ -z "$name" ]]; do
            read -rp "Migration name: " name || die "Aborted: no migration name provided."
        done
    fi
fi

# dotnet global tools (dotnet-ef) install here but aren't always on PATH.
export PATH="$PATH:$HOME/.dotnet/tools"

# The connection string is forwarded to the startup app as a design-time argument (after --).
if [[ "$action" == "add" ]]; then
    echo "Creating migration '$name' for context '$CONTEXT'..."
    dotnet ef migrations add "$name" \
        --project "$ENTITY_PROJECT" \
        --startup-project "$STARTUP_PROJECT" \
        --context "$CONTEXT" \
        -- "$connection_string"
    echo "Migration '$name' created."
else
    echo "Reverting last migration for context '$CONTEXT'..."
    dotnet ef migrations remove \
        --project "$ENTITY_PROJECT" \
        --startup-project "$STARTUP_PROJECT" \
        --context "$CONTEXT" \
        -- "$connection_string"
    echo "Last migration reverted."
fi
