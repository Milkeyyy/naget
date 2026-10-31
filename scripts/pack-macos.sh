#!/bin/bash
set -e
set -u

AppId="naget"
PackAuthors="Milkeyyy"
BundleId="com.milkeyyy.naget"
Version=""
Channel=""
BuildDir="./Build"
ReleasesDir="./Releases"
BundleDir="./Bundle"
IconPath="../../Logo/naget.icns"
AssumeYes=0
LegacyConsole=0

while [ "$#" -gt 0 ]; do
	case "$1" in
		--version)
			Version="$2"
			shift 2
			;;
		--channel)
			Channel="$2"
			shift 2
			;;
		--build-dir)
			BuildDir="$2"
			shift 2
			;;
		--releases-dir)
			ReleasesDir="$2"
			shift 2
			;;
		--bundle-dir)
			BundleDir="$2"
			shift 2
			;;
		--icon)
			IconPath="$2"
			shift 2
			;;
		--yes)
			AssumeYes=1
			shift
			;;
		--legacy-console)
			LegacyConsole=1
			shift
			;;
		*)
			echo "Unknown option: $1" >&2
			exit 1
			;;
	esac
done

if [ -z "$Version" ] || [ -z "$Channel" ]; then
	echo "Usage: $0 --version <version> --channel <channel> [--yes] [--legacy-console]" >&2
	exit 1
fi

PackFlags=""
if [ "$AssumeYes" = "1" ]; then
	PackFlags="-y"
fi
if [ "$LegacyConsole" = "1" ]; then
	PackFlags="${PackFlags} -x"
fi

vpk bundle -u "$AppId" -v "$Version" -p "$BuildDir" -o "$BundleDir" -e "$AppId" -i "$IconPath" --bundleId "$BundleId"

PlistPath="${BundleDir}/${AppId}.app/Contents/Info.plist"
/usr/libexec/PlistBuddy -c "Delete :CFBundleLocalizations" "$PlistPath" 2>/dev/null || true
/usr/libexec/PlistBuddy -c "Add :CFBundleLocalizations array" -c "Add :CFBundleLocalizations:0 string en" -c "Add :CFBundleLocalizations:1 string ja" "$PlistPath"

vpk pack $PackFlags -u "$AppId" -v "$Version" -p "${BundleDir}/${AppId}.app" -o "$ReleasesDir" -e "$AppId" -i "$IconPath" --channel "$Channel" --packAuthors "$PackAuthors" --bundleId "$BundleId"
