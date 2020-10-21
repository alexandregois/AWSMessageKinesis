#!/bin/bash
# "trimmed smaller binary version -->" /p:PublishTrimmed=true
# "not nned to install .net core on target machine --> "  --self-contained
#dotnet publish -r win-x64 -c release 
dotnet publish -r win-x64 -c release /p:PublishSingleFile=true /p:PublishTrimmed=true
