#!/bin/bash

read -r -p "Clear nuget cache first? (required to override version) [y/N] " response
if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]
then
    dotnet nuget locals all --clear
fi

dotnet pack src -o .
