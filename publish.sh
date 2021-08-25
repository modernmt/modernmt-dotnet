#!/bin/bash

if [[ -z "${MMT_DOTNET_API_KEY}" ]]; then
	echo "Environment variable MMT_DOTNET_API_KEY not found."
	exit 1
fi

VERSION="$1"

if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
	echo "Invalid version number"
	exit 1
fi

# shellcheck disable=SC2086
dotnet nuget push modernmt-dotnet.${VERSION}.nupkg --api-key ${MMT_DOTNET_API_KEY} --source https://api.nuget.org/v3/index.json
