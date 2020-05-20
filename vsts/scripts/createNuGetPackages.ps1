# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

function DeleteItem($pathToRemove) {
    if (Test-Path -Path $pathToRemove) {
        Remove-Item -Recurse -Force -Path "$pathToRemove"
    }
}
$version="1.0.0-pre-$env:BUILD_BUILDNUMBER"
$detectorName="Microsoft.Oryx.Detector"
$repoRoot="$PSScriptRoot\..\.."
$artifactsPackagesDir="$repoRoot\artifacts\packages"

cd "$artifactsPackagesDir"

# Delete any existing directory and zip file. Could have been from an earlier build.
DeleteItem "$detectorName"
DeleteItem "$detectorName.zip"
Rename-Item -Path "$detectorName.$version.nupkg" -NewName "$detectorName.zip"
Expand-Archive -Path "$detectorName.zip" -DestinationPath "$detectorName"
DeleteItem "$detectorName.zip"

Copy-Item `
    -Path "$repoRoot\src\Detector\bin\Release\netstandard2.0\$detectorName.dll" `
    -Destination "$detectorName\lib\netstandard2.0\$detectorName.dll" `
    -Force

Compress-Archive -Path "$detectorName\*" -DestinationPath "$detectorName.zip"
Rename-Item -Path "$detectorName.zip" -NewName "$detectorName.$version.nupkg"
DeleteItem "$detectorName"
