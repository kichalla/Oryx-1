#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

__CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
source "$__CURRENT_DIR/../../build/__hugoConstants.sh"

curl -fsSLO --compressed "https://github.com/gohugoio/hugo/releases/download/v${VERSION}/hugo_${VERSION}_Linux-64bit.tar.gz"
mkdir -p /opt/hugo
tar -xzf hugo_${VERSION}_Linux-64bit.tar.gz -C /opt/hugo
rm hugo_${VERSION}_Linux-64bit.tar.gz
