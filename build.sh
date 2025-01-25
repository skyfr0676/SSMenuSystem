#bin/bash
CONFIGURATIONS=(
    "Release EXILED"
    "Release NWAPI"
)
VALID_ARGS=("build" "pack" "--version" "push" "draft"  "--push_nuget" "--verbosity")
PASS_ARGUMENTS=()

verbosity="normal"

for ((i = 1; i < $# + 1; i++)); do
    arg=${!i}
    if [[ "${PASS_ARGUMENTS[*]}" =~ $arg ]]; then
        continue
    fi

    if [[ ! "${VALID_ARGS[*]}" =~ $arg ]]; then
        echo "[WARNING]: $arg is not a valid argument"
        exit 1
    fi
    if [[ "$arg" =~ ^-- ]]; then
        next_index=$((i + 1))
        if [ -z "${!next_index}" ]; then
            echo -e "\e[31m[ERROR]: Invalid argument for $arg. You need to add an argument after\nExemple: ./build.sh $arg something \e[0m"
            exit 1
        fi
        eval "${arg:2}=${!next_index}"
        PASS_ARGUMENTS+=("${!next_index}")
    else
        eval "${arg}=\"true\""
    fi
done

if [ -n "$version" ]; then
    VERSION="$version"

    sed -i "37s/.*/        public override Version Version => new($(echo "$VERSION" | sed 's/\./, /g'));/" Plugin.cs
    if [ $verbosity == "all" ]; then
        echo "[VERBOSE/ALL]: changed Plugin.cs line 37 for version $VERSION (EXILED)"
    fi

    sed -i "104s/.*/        [PluginAPI.Core.Attributes.PluginEntryPoint(\"SSMenuSystem\", \"$VERSION\", \"sync all plugins to one server specific with menus.\", \"sky\")]/" Plugin.cs
    if [ $verbosity == "all" ]; then
        echo "[VERBOSE/ALL]: changed Plugin.cs line 104 for version $VERSION (NWAPI)"
    fi

    sed -i "6s/.*/        <version>$VERSION-EXILED<\/version>/" SSMenuSystem-EXILED.nuspec
    if [ $verbosity == "all" ]; then
        echo "[VERBOSE/ALL]: changed SSMenuSystem-EXILED.nuspec line 6 for version $VERSION (EXILED)"
    fi

    sed -i "6s/.*/        <version>$VERSION-NWAPI<\/version>/" SSMenuSystem-NWAPI.nuspec
    if [ $verbosity == "all" ]; then
        echo "[VERBOSE/ALL]: changed SSMenuSystem-NWAPI.nuspec line 6 for version $VERSION (NWAPI)"
    fi

    sed -i "34s/.*/[assembly: AssemblyVersion(\"$VERSION\")]/" Properties/AssemblyInfo.cs
    if [ $verbosity == "all" ]; then
        echo "[VERBOSE/ALL]: changed Properties/AssemblyInfo.cs line 34 for version $VERSION"
    fi

    echo -e "\e[36m[INFO]: Changed for version $VERSION.\e[0m"
fi

if [ -n "$build" ]; then
    for CONFIG in "${CONFIGURATIONS[@]}"; do
        if [ $verbosity == "all" ]; then
            msbuild "SSMenuSystem.csproj" /p:Configuration="$CONFIG"
        else
            msbuild "SSMenuSystem.csproj" /p:Configuration="$CONFIG" /verbosity:quiet > /dev/null 2>&1
        fi
        # shellcheck disable=SC2181
        if [ "$?" -ne 0 ]; then
            echo -e "\e[31m[ERROR]: An error as occured while building $CONFIG - Aborting... \e[0m"
            exit 1
        else
            echo -e "\e[36m[INFO] Build for configuration $CONFIG is completed.\e[0m"
        fi
    done
fi
if [ -n "$pack" ]; then
    rm -rf "pack"
    mkdir "pack"
    cp "bin/Release EXILED/SSMenuSystem-EXILED.dll" "pack/SSMenuSystem-EXILED.dll"
    cp "bin/Release NWAPI/SSMenuSystem-NWAPI.dll" "pack/SSMenuSystem-NWAPI.dll"
    cp "bin/Release EXILED/0Harmony.dll" "pack/0Harmony.dll"
    nuget pack SSMenuSystem-EXILED.nuspec -Properties Configuration=Release -OutputDirectory "pack"
    nuget pack SSMenuSystem-NWAPI.nuspec -Properties Configuration=Release -OutputDirectory "pack"
    echo -e "\e[36m[INFO] Successfully packaged all packages into directory pack/.\e[0m"
fi
if [ -n "$push" ]; then # push new version to github
    if [ -z "$VERSION" ]; then
        echo -e "\e[31m[ERROR]: Version is not defined. You need to add \"--version\" \nExemple: ./build.sh push --version 3.0.0  \e[0m"
    else
        if ! git push --dry-run origin dev &>/dev/null 2>&1; then
            echo -e "\e[33m[WARNING]: you don't have permissions to push into the repo. Please create a pull request with your fork.\e[0m"
        else
            if [ $verbosity == "all" ]; then
                echo -e "[VERBOSE]: Pushing into github..."
            fi
            git add . &>/dev/null 2>&1;
            git commit -m "[AUTOMATIC] push version to $VERSION" &>/dev/null 2>&1;
            git push origin dev &>/dev/null 2>&1;
            echo -e "\e[36m[INFO]: pushed last changes (including version refactoring) into github.\e[0m"
        fi
    fi
fi
if [ -n "$draft" ]; then
    if [ -z "$VERSION" ]; then
        echo -e "\e[31m[ERROR]: Version is not defined. You need to add \"--version\" \nExemple: ./build.sh draft --version 3.0.0 \e[0m"
    else
        LATEST_VERSION=$(gh release list --repo skyfr0676/SSMenuSystem --json tagName,isDraft -q 'sort_by(.createdAt)[0].tagName')
        if [ -n "$VERBOSE" ]; then
            echo -e "[VERBOSE]: Latest version found: $LATEST_VERSION"
        fi
        #LATEST_VERSION=$(gh release view --json tagName -q ".tagName" --repo skyfr0676/SSMenuSystem)
        if [ ! "$LATEST_VERSION" == "v$VERSION" ]; then
            gh release create v2.0.4 --draft --title "v$VERSION" --notes """## What's changed
**Full Changelog**: https://github.com/skyfr0676/SSMenuSystem/compare/$LATEST_VERSION...V$VERSION""" --repo skyfr0676/SSMenuSystem ./pack/SSMenuSystem-EXILED.dll ./pack/SSMenuSystem-NWAPI.dll ./pack/0Harmony.dll
            echo -e "\e[36m[INFO]: successfully created a release\e[0m"
        else
            echo -e "\e[33m[WARNING]: A release with this version already exist. \e[0m"
        fi
    fi
fi
if [ -n "$push_nuget" ]; then # push new version into the nuget repo
    if [ -z "$VERSION" ]; then
        echo -e "\e[31m[ERROR]: Version is not defined. You need to add \"--version\" \nExemple: ./build.sh --push_nuget API_KEY --version 3.0.0 \e[0m"
    else
        if [ $verbosity == "all" ]; then
            echo -e "[VERBOSE]: API Key found: $push_nuget"
        fi
        nuget push "./pack/SSMenuSystem.$VERSION-EXILED.nupkg" -Source https://api.nuget.org/v3/index.json -ApiKey "$push_nuget"
        nuget push "./pack/SSMenuSystem.$VERSION-NWAPI.nupkg" -Source https://api.nuget.org/v3/index.json -ApiKey "$push_nuget"
    fi
fi