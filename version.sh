#!/bin/bash

VERSION="$1"

if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
	echo "Invalid version number"
	exit 1
fi

csproj_match="    <Version>"
csproj_ver="    <Version>${VERSION}<\/Version>"
sed -i -E "/$csproj_match/s/.*/$csproj_ver/" src/modernmt-dotnet.csproj

header_match="        private const string PlatformVersion = "
header_ver="        private const string PlatformVersion = \"${VERSION}\";"
sed -i -E "/$header_match/s/.*/$header_ver/" src/ModernMTService.cs
