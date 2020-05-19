﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.Detector.Python
{
    public class PythonOnDiskVersionProvider
    {
        private const string DefaultOnDiskVersion = PythonConstants.PythonLtsVersion;
        private readonly ILogger<PythonOnDiskVersionProvider> _logger;

        public PythonOnDiskVersionProvider(ILogger<PythonOnDiskVersionProvider> logger)
        {
            _logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            _logger.LogDebug("Getting list of versions from {installDir}", PythonConstants.InstalledPythonVersionsDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            PythonConstants.InstalledPythonVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(installedVersions, DefaultOnDiskVersion);
        }
    }
}
