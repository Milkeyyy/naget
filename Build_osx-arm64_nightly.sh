#!/bin/bash
set -e
set -u

# ------------------------------
RuntimeOs="osx"
RuntimeArch="arm64"
Runtime="${RuntimeOs}-${RuntimeArch}"
ReleaseChannel="nightly"
AppCastBaseUrl="https://nagetupd.milkeyyy.com/${ReleaseChannel}/${Runtime}"
OutputDir="./_Pack"
VelopackChannel="${RuntimeOs}-${RuntimeArch}-${ReleaseChannel}"
# ------------------------------

# Load env File
set -o allexport
source ./Build_env.txt
set +o allexport

echo ""
echo "naget - Build Start"
echo ""

cd "$(dirname "$0")"

echo "Load Build Info"
echo ""
./build.sh loadandsavebuildinfojson --releasechannel "${ReleaseChannel}" --releasenumber "${CommitHash}"
brew install jq
AppVersion=$(jq -r ".version" "./naget/build.json")
AppFullVersion=$(jq -r ".full_version" "./naget/build.json")
CommitHash=$(git rev-parse --short HEAD)
echo ""

echo "-         Runtime: ${Runtime}"
echo "-         Version: ${AppVersion}"
echo "- Release Channel: ${ReleaseChannel}"
echo "-     Commit Hash: ${CommitHash}"
echo ""

echo "Cleanup Output Directory"
echo ""
rm -rf "${OutputDir}/${Runtime}"
echo ""

echo "Create Output Directory"
mkdir -p "${OutputDir}/${Runtime}"
echo ""

echo "Compile"
echo ""
./build.sh --runtime "${Runtime}" --releasechannel "${ReleaseChannel}" --releasenumber "${CommitHash}"
echo ""

echo "Install Velopack CLI"
echo ""
dotnet tool install -g vpk || true
echo ""

echo "Processing Releases (Velopack)"
echo ""
cd "${OutputDir}/${Runtime}"

echo "Download Previous Release"
vpk download s3 --bucket naget-update --endpoint "${R2_ENDPOINT}" --channel "${VelopackChannel}" || echo "Warning: Failed to download previous release."

# Clean up existing version to avoid overwrite prompt
if [ -d "Releases" ]; then
    echo "Cleaning up existing version ${AppFullVersion}..."
    rm -f "Releases/naget-${AppFullVersion}-osx-${RuntimeArch}-full.nupkg"
    rm -f "Releases/naget-${AppFullVersion}-osx-${RuntimeArch}-delta.nupkg"
    rm -f "Releases/naget-${AppFullVersion}-osx-${RuntimeArch}-Setup.pkg"
    
    if [ -f "Releases/releases.${VelopackChannel}.json" ]; then
        jq "del(.Assets[] | select(.Version == \"${AppFullVersion}\"))" "Releases/releases.${VelopackChannel}.json" > "Releases/releases.tmp.json"
        mv "Releases/releases.tmp.json" "Releases/releases.${VelopackChannel}.json"
    fi
fi

echo "Build Installer"
vpk pack -u naget -v "${AppFullVersion}" -p . -i "Logo/naget.icns" -e naget --channel "${VelopackChannel}" --packAuthors "Milkeyyy"

echo "Upload"
vpk upload s3 --bucket naget-update --endpoint "${R2_ENDPOINT}" --channel "${VelopackChannel}"

echo ""

echo ""
echo "Build Finished"
echo ""
