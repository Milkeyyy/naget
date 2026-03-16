#!/bin/bash
set -e
set -u

# ------------------------------
RuntimeOs="osx"
RuntimeArch="arm64"
Runtime="${RuntimeOs}-${RuntimeArch}"
AppReleaseChannel="nightly"
# AppCastBaseUrl="https://nagetupd.milkeyyy.com/${AppReleaseChannel}/${Runtime}"
OutputDir="./_Pack"
VelopackChannel="${RuntimeOs}-${RuntimeArch}-${AppReleaseChannel}"
# ------------------------------

# 引数チェック: --skip-upload が含まれているかどうか
SKIP_UPLOAD=0
for arg in "$@"; do
    if [ "$arg" = "--skip-upload" ]; then
        SKIP_UPLOAD=1
    fi
done

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

./build.sh loadandsavebuildinfojson --releasechannel "${AppReleaseChannel}"

brew install jq

AppVersion=$(jq -r ".version" "./naget/build.json")
AppFullVersion=$(jq -r ".full_version" "./naget/build.json")
AppReleaseNumber=$(jq -r ".release_number" "./naget/build.json")

echo ""

echo "        Runtime: ${Runtime}"
echo "   Full Version: ${AppFullVersion}"
echo "-         Version: ${AppVersion}"
echo "- Release Channel: ${AppReleaseChannel}"
echo "-  Release Number: ${AppReleaseNumber}"
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
./build.sh --runtime "${Runtime}" --releasechannel "${AppReleaseChannel}" --releasenumber "${AppReleaseNumber}"
echo ""

echo "Install Velopack CLI"
echo ""
dotnet tool install -g vpk || true
echo ""

echo "Processing Releases - Velopack"
echo ""

cd "${OutputDir}/${Runtime}"

echo "Download Previous Release - Velopack"
vpk download github --repoUrl https://github.com/Milkeyyy/naget --channel "${VelopackChannel}" --token "${GITHUB_TOKEN}" --pre -o ./Releases || echo "Warning: Failed to download previous release."

echo "Build Installer - Velopack"
vpk pack -xy -u naget -v "${AppFullVersion}" -p "./Build" -o "./Releases" -i "../../Logo/naget.icns" -e naget --channel "${VelopackChannel}" --packAuthors "Milkeyyy"

if [ "$SKIP_UPLOAD" = "1" ]; then
    echo "Upload SKIPPED"
else
    echo "Upload - Velopack"
    vpk upload github --repoUrl https://github.com/Milkeyyy/naget --channel "${VelopackChannel}" --token "${GITHUB_TOKEN}" --tag nightly --targetCommitish update --releaseName "Nightly Build" --pre --merge --publish -o ./Releases
fi

echo ""

echo ""
echo "Build Finished"
echo ""
