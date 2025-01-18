#bin/bash
if [ ! $# -gt 0 ]; then
  echo "Invalid syntax"
  exit 1
fi

CONFIGURATIONS=(
    "Release EXILED"
    "Release NWAPI"
)
VERSION=$(echo "$1")
NEW_VERSION=$(echo "$1" | sed 's/\./, /g')
sed -i "38s/.*/        public override Version Version => new($NEW_VERSION);/" Plugin.cs
sed -i "105s/.*/        [PluginAPI.Core.Attributes.PluginEntryPoint(\"SSMenuSystem\", \"$VERSION\", \"sync all plugins to one server specific with menus.\", \"sky\")]/" Plugin.cs
sed -i "6s/.*/        <version>$VERSION-EXILED<\/version>/" SSMenuSystem-EXILED.nuspec
sed -i "6s/.*/        <version>$VERSION-NWAPI<\/version>/" SSMenuSystem-NWAPI.nuspec
sed -i "34s/.*/[assembly: AssemblyVersion(\"$VERSION\")]/" Properties/AssemblyInfo.cs

for CONFIG in "${CONFIGURATIONS[@]}"; do
    msbuild "SSMenuSystem.csproj" /p:Configuration="$CONFIG"
done

rm -rf "pack"
mkdir "pack"
cp "bin/Release EXILED/SSMenuSystem-EXILED.dll" "pack/SSMenuSystem-EXILED.dll"
cp "bin/Release NWAPI/SSMenuSystem-NWAPI.dll" "pack/SSMenuSystem-NWAPI.dll"
nuget pack SSMenuSystem-EXILED.nuspec -Properties Configuration=Release -OutputDirectory "pack"
nuget pack SSMenuSystem-NWAPI.nuspec -Properties Configuration=Release -OutputDirectory "pack"

if [ $# -gt 1 ]; then # there is more than 1 start argument
    for ((i = 1; i < $# + 1; i++)); do
        arg=${!i}
        if [ "$arg" == "github" ]; then # push new version to github
            git add .
            git commit -m "[AUTOMATIC] push version to $VERSION"
            git push origin dev
            break
        elif [ "$arg" == "--push" ]; then # push new version into the nuget repo
            next_index=$((i + 1))
            if [ -n "${!next_index}" ]; then
                echo "API Key found"
                API_KEY="${!next_index}"
                nuget push "pack/SSMenuSystem-EXILED.$VERSION-EXILED.nupkg" -Source https://api.nuget.org/v3/index.json -ApiKey "$API_KEY"
                nuget push "pack/SSMenuSystem-NWAPI.$VERSION-NWAPI.nupkg" -Source https://api.nuget.org/v3/index.json -ApiKey "$API_KEY"
            else
                echo "Invalid syntax: cannot get api key after \"--push\""
            fi
        fi
    done
fi

exit 0