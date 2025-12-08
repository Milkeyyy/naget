#!/bin/bash
set -e
set -u

# ------------------------------
RuntimeOs="osx"
RuntimeArch="arm64"
Runtime="${RuntimeOs}-${RuntimeArch}"
ReleaseChannel="nightly"
AppCastBaseUrl="https://nagetupd.milkeyyy.com/${ReleaseChannel}/latest/${Runtime}"
OutputDir="./_Pack"
# ------------------------------

# Load env File
set -o allexport
source ./Build_env.txt
set +o allexport

echo ""
echo "naget - Build Start"
echo ""

cd "$(dirname "$0")"

CommitHash=$(git rev-parse --short HEAD)

echo "-         Runtime: ${Runtime}"
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
./build.sh bundleapp --runtime "${Runtime}" --releasechannel "${ReleaseChannel}" --releasenumber "${CommitHash}"
echo ""

echo "Create Zip"
echo ""
zip -r "${OutputDir}/${Runtime}/naget.zip" "${OutputDir}/${Runtime}/naget.app"
echo ""

echo "Build Installer (DMG)"
echo ""
brew install create-dmg
create-dmg \
--volname "naget Installer" \
--volicon "./Logo/naget.icns" \
--window-size 800 400 \
--icon-size 100 \
--icon "naget.app" 200 190 \
--app-drop-link 600 185 \
--hide-extension "naget.app" \
"${OutputDir}/${Runtime}/naget_${Runtime}.dmg" \
"${OutputDir}/${Runtime}/"

echo ""

echo "Generate App Cast"
echo ""
dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator || true
netsparkle-generate-appcast -n naget -u "${AppCastBaseUrl}" -o "mac-${RuntimeArch}" -a "${OutputDir}/${Runtime}" -b "${OutputDir}/${Runtime}" -e zip --output-file-name "appcast_${ReleaseChannel}_${Runtime}" --output-type json --channel "${ReleaseChannel}"
echo ""

echo "Install AWS CLI"
echo ""
brew install awscli

echo "Upload App Package"
echo ""
# App Package (zip)
aws s3 cp "${OutputDir}/${Runtime}/naget.zip" "s3://naget-update/${ReleaseChannel}/${Runtime}/naget.zip" --endpoint-url "${R2_ENDPOINT}"
echo ""

echo "Upload App Cast"
echo ""
# App Cast
aws s3 cp "${OutputDir}/${Runtime}/appcast_${ReleaseChannel}_${Runtime}.json" "s3://naget-update/appcast/appcast_${ReleaseChannel}_${Runtime}.json" --endpoint-url "${R2_ENDPOINT}"
echo ""
# App Cast Signature
aws s3 cp "${OutputDir}/${Runtime}/appcast_${ReleaseChannel}_${Runtime}.json.signature" "s3://naget-update/appcast/appcast_${ReleaseChannel}_${Runtime}.json.signature" --endpoint-url "${R2_ENDPOINT}"
echo ""

echo ""
echo "Build Finished"
echo ""
