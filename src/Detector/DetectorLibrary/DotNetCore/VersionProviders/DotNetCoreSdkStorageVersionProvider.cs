﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    public class DotNetCoreSdkStorageVersionProvider : SdkStorageVersionProviderBase
    {
        private Dictionary<string, string> _versionMap;
        private string _defaultRuntimeVersion;

        public DotNetCoreSdkStorageVersionProvider(
            IOptions<DetectorOptions> detectorOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(detectorOptions, httpClientFactory, loggerFactory)
        {
        }

        public Dictionary<string, string> SupportedVersionsMap { get; }

        public string GetDefaultRuntimeVersion()
        {
            GetVersionInfo();
            return _defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            GetVersionInfo();
            return _versionMap;
        }

        public void GetVersionInfo()
        {
            if (_versionMap == null)
            {
                var httpClient = _httpClientFactory.CreateClient("general");
                var sdkStorageBaseUrl = GetPlatformBinariesStorageBaseUrl();
                var blobList = httpClient
                    .GetStringAsync($"{sdkStorageBaseUrl}/dotnet?restype=container&comp=list&include=metadata")
                    .Result;

                var xdoc = XDocument.Parse(blobList);

                // keys represent runtime version, values represent sdk version
                var supportedVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var metadataElement in xdoc.XPathSelectElements($"//Blobs/Blob/Metadata"))
                {
                    var childElements = metadataElement.Elements();
                    var runtimeVersionElement = childElements.Where(e => string.Equals(
                            DotNetCoreConstants.StorageSdkMetadataRuntimeVersionElementName,
                            e.Name.LocalName,
                            StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    if (runtimeVersionElement != null)
                    {
                        var sdkVersionElement = childElements.Where(e => string.Equals(
                                DotNetCoreConstants.StorageSdkMetadataSdkVersionElementName,
                                e.Name.LocalName,
                                StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault();

                        supportedVersions[runtimeVersionElement.Value] = sdkVersionElement.Value;
                    }
                }

                _versionMap = supportedVersions;
                _defaultRuntimeVersion = GetDefaultVersion(DotNetCoreConstants.PlatformName, sdkStorageBaseUrl);
            }
        }
    }
}