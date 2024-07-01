#!/usr/bin

if [ $# -ne 1 ]; then
    echo "Usage: $0 <MigrationName>"
    exit 1
fi

dotnet ef migrations add $1 --project "SafariDigital.Database" --context "SafariDigitalContext"
