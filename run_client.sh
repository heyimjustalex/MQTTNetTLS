#!/bin/bash

CLIENTPROJ=$(find ~/ -iname Client.csproj 2>/dev/null)
dotnet build $CLIENTPROJ
CLIENTDLL=$(find ~/ -iname Client.dll 2>/dev/null)
dotnet $CLIENTDLL
