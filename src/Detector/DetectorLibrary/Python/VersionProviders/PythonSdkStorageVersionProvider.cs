﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.Python
{
    internal class PythonSdkStorageVersionProvider : SdkStorageVersionProviderBase
    {
        public PythonSdkStorageVersionProvider(
            IOptions<DetectorOptions> detectorOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(detectorOptions, httpClientFactory, loggerFactory)
        {
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return GetAvailableVersionsFromStorage(
                platformName: PythonConstants.PlatformName,
                versionMetadataElementName: "Version");
        }
    }
}