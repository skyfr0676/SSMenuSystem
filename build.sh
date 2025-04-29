#bin/bash
CONFIGURATIONS=(
    "EXILED"
    "LABAPI"
)
VALID_ARGS=("build" "pack" "--version" "push" "draft"  "--push_nuget" "--verbosity" "server_exiled" "server_labapi" "debug")
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

if [ -n "$debug" ]; then
    echo -e "DEBUG version activated. --version, --push_nuget, push, and draft is locked."
fi

if [ -n "$version" ] && [ -z "$debug" ]; then
    VERSION="$version"

    sed -i "40s/.*/        public override Version Version => new($(echo "$VERSION" | sed 's/\./, /g'));/" Plugin.cs
    if [ $verbosity == "all" ]; then
        echo "[VERBOSE/ALL]: changed Plugin.cs line 40 for version $VERSION."
    fi

    sed -i "6s/.*/        <version>$VERSION-EXILED<\/version>/" SSMenuSystem-EXILED.nuspec
    if [ $verbosity == "all" ]; then
        echo "[VERBOSE/ALL]: changed SSMenuSystem-EXILED.nuspec line 6 for version $VERSION (EXILED)"
    fi
    sed -i "6s/.*/        <version>$VERSION-LABAPI-Beta<\/version>/" SSMenuSystem-LABAPI.nuspec
    if [ $verbosity == "all" ]; then
        echo "[VERBOSE/ALL]: changed SSMenuSystem-LABAPI.nuspec line 6 for version $VERSION (LABAPI)"
    fi

    sed -i "34s/.*/[assembly: AssemblyVersion(\"$VERSION\")]/" Properties/AssemblyInfo.cs
    if [ $verbosity == "all" ]; then
        echo "[VERBOSE/ALL]: changed Properties/AssemblyInfo.cs line 34 for version $VERSION"
    fi

    echo -e "\e[36m[INFO]: Changed for version $VERSION.\e[0m"
fi

if [ -n "$build" ]; then
    for CONFIG in "${CONFIGURATIONS[@]}"; do
        if [ -n "$debug" ]; then
            CONFIG="Debug $CONFIG"
        else
            CONFIG="Release $CONFIG"
        fi
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

    if [ -z "$debug" ]; then
        # copie des dépendances dans le dossier pack
        cp "bin/Release EXILED/SSMenuSystem-EXILED.dll" "pack/SSMenuSystem-EXILED.dll"
        cp "bin/Release LABAPI/SSMenuSystem-LABAPI.dll" "pack/SSMenuSystem-LABAPI.dll"
    else
        cp "bin/Debug EXILED/SSMenuSystem-EXILED.dll" "pack/SSMenuSystem-EXILED.dll"
        cp "bin/Debug LABAPI/SSMenuSystem-LABAPI.dll" "pack/SSMenuSystem-LABAPI.dll"
    fi

    cp "bin/Release EXILED/0Harmony.dll" "pack/0Harmony.dll"

    # application des variables
    cp "SSMenuSystem-EXILED.nuspec" "/tmp/SSMenuSystem-EXILED.nuspec"
    cp "SSMenuSystem-LABAPI.nuspec" "/tmp/SSMenuSystem-LABAPI.nuspec"

    sed -i "s#\$(PACKAGE_REF)#$EXILED_REFERENCES#g" SSMenuSystem-EXILED.nuspec
    sed -i "s#\$(PACKAGE_REF)#$EXILED_REFERENCES#g" SSMenuSystem-LABAPI.nuspec

    if [ -z "$debug" ]; then
        nuget pack SSMenuSystem-EXILED.nuspec -Properties Configuration=Release -OutputDirectory "pack"
        nuget pack SSMenuSystem-LABAPI.nuspec -Properties Configuration=Release -OutputDirectory "pack"
    fi

    cp "/tmp/SSMenuSystem-EXILED.nuspec" "SSMenuSystem-EXILED.nuspec"
    cp "/tmp/SSMenuSystem-LABAPI.nuspec" "SSMenuSystem-LABAPI.nuspec"

    rm "/tmp/SSMenuSystem-EXILED.nuspec"
    rm "/tmp/SSMenuSystem-LABAPI.nuspec"

    echo -e "\e[36m[INFO] Successfully packaged all dlls into directory pack/.\e[0m"
fi
if [ -n "$push" ] && [ -z "$debug" ]; then # push new version to github
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
if [ -n "$draft" ] && [ -z "$debug" ]; then
    if [ -z "$VERSION" ]; then
        echo -e "\e[31m[ERROR]: Version is not defined. You need to add \"--version\" \nExemple: ./build.sh draft --version 3.0.0 \e[0m"
    else
        LATEST_VERSION=$(gh api repos/skyfr0676/SSMenuSystem/releases --jq '[.[] | select(.draft == false)][0].tag_name')
        if [ -n "$VERBOSE" ]; then
            echo -e "[VERBOSE]: Latest version found: $LATEST_VERSION"
        fi
        #LATEST_VERSION=$(gh release view --json tagName -q ".tagName" --repo skyfr0676/SSMenuSystem)
        if [ ! "$LATEST_VERSION" == "v$VERSION" ]; then
            gh release create "v$VERSION" --draft --title "v$VERSION" --notes """## What's changed

**Full Changelog**: https://github.com/skyfr0676/SSMenuSystem/compare/$LATEST_VERSION...v$VERSION""" --repo skyfr0676/SSMenuSystem ./pack/SSMenuSystem-EXILED.dll ./pack/SSMenuSystem-LABAPI.dll ./pack/0Harmony.dll
            echo -e "\e[36m[INFO]: successfully created a release\e[0m"
        else
            echo -e "\e[33m[WARNING]: A release with this version already exist. \e[0m"
        fi
    fi
fi
if [ -n "$push_nuget" ] && [ -z "$debug" ]; then # push new version into the nuget repo
    if [ -z "$VERSION" ]; then
        echo -e "\e[31m[ERROR]: Version is not defined. You need to add \"--version\" \nExemple: ./build.sh --push_nuget API_KEY --version 3.0.0 \e[0m"
    else
        if [ $verbosity == "all" ]; then
            echo -e "[VERBOSE]: API Key found: $push_nuget"
        fi
        nuget push "./pack/SSMenuSystem.$VERSION-EXILED.nupkg" -Source https://api.nuget.org/v3/index.json -ApiKey "$push_nuget"
        nuget push "./pack/SSMenuSystem.$VERSION-LABAPI-Beta.nupkg" -Source https://api.nuget.org/v3/index.json -ApiKey "$push_nuget"
    fi
fi

if [ -n "$server_exiled" ]; then
    cp "pack/SSMenuSystem-EXILED.dll" "$HOME/.config/EXILED/Plugins/SSMenuSystem-EXILED.dll"
    sed -i "2s/.*/is_enabled: true/" "/home/sky/.config/SCP Secret Laboratory/LabAPI-Beta/configs/Exiled Loader/config.yml"
    sed -i "1s/.*/is_enabled: false/" "/home/sky/.config/SCP Secret Laboratory/LabAPI-Beta/configs/SS-Menu System/config.yml"
    echo -e "\e[36m[INFO]: Activated EXILED version!\e[0m"

elif [ -n "$server_labapi" ]; then
    cp "pack/SSMenuSystem-LABAPI.dll" "$HOME/.config/SCP Secret Laboratory/LabAPI-Beta/plugins/SSMenuSystem-LABAPI.dll"
    cp "pack/0Harmony.dll" "$HOME/.config/SCP Secret Laboratory/LabAPI-Beta/dependencies/0Harmony.dll"

    sed -i "2s/.*/is_enabled: false/" "/home/sky/.config/SCP Secret Laboratory/LabAPI-Beta/configs/Exiled Loader/config.yml"
    sed -i "1s/.*/is_enabled: true/" "/home/sky/.config/SCP Secret Laboratory/LabAPI-Beta/configs/SS-Menu System/config.yml"
    echo -e "\e[36m[INFO]: Activated LABAPI version!\e[0m"
fi

_auto_completion()
{
    local cur prev opts
    COMPREPLY=()
    cur="${COMP_WORDS[COMP_CWORD]}"
    prev="${COMP_WORDS[COMP_CWORD-1]}"

    opts="${VALID_ARGS[*]}"

    if [[ "$cur" == --* ]]; then
        COMPREPLY=( $(compgen -W "$(echo "$opts" | grep -- '--')" -- "$cur") )
    else
        COMPREPLY=( $(compgen -W "$opts" -- "$cur") )
    fi
}

complete -F _auto_completion ./build.sh