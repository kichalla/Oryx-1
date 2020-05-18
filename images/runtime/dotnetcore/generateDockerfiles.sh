#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && cd .. && pwd )

source $REPO_DIR/build/__dotNetCoreRunTimeVersions.sh

echo "image type: '$1'"
imageType="$1"

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r DOCKERFILE_TEMPLATE="$DIR/template.Dockerfile"
declare -r RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER="%RUNTIME_BASE_IMAGE_NAME%"
declare -r NETCORE_BUSTER_VERSION_ARRAY=($NET_CORE_APP_31 $NET_CORE_APP_31)
declare -r NETCORE_STRETCH_VERSION_ARRAY=($NET_CORE_APP_10 $NET_CORE_APP_11 $NET_CORE_APP_20 $NET_CORE_APP_21 $NET_CORE_APP_22 $NET_CORE_APP_30)

cd $DIR

VERSIONS_DIRECTORY=()

if [ "$imageType" == "buster" ];then
	VERSIONS_DIRECTORY=("${NETCORE_BUSTER_VERSION_ARRAY[@]}")
else 
	VERSIONS_DIRECTORY=("${NETCORE_STRETCH_VERSION_ARRAY[@]}")
fi 

for VERSION_DIRECTORY in "${VERSIONS_DIRECTORY[@]}"
do
	IFS='.' read -ra SPLIT_VERSION <<< "$VERSION_DIRECTORY"
	VERSION_DIRECTORY="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"

	echo "Generating Dockerfile for image $VERSION_DIRECTORY..."

	TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

	# Replace placeholders
	RUNTIME_BASE_IMAGE_NAME="mcr.microsoft.com/oryx/base:dotnetcore-$VERSION_DIRECTORY-$DOT_NET_CORE_RUNTIME_BASE_TAG-$imageType"
	sed -i "s|$RUNTIME_BASE_IMAGE_NAME_PLACEHOLDER|$RUNTIME_BASE_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
done
