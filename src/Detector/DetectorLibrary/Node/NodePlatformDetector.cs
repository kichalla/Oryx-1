﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Detector.Exceptions;
using Microsoft.Oryx.Common.Extensions;
using Newtonsoft.Json;
using System;

namespace Microsoft.Oryx.Detector.Node
{
    internal class NodePlatformDetector : IPlatformDetector
    {
        private static readonly string[] IisStartupFiles = new[]
        {
            "default.htm",
            "default.html",
            "default.asp",
            "index.htm",
            "index.html",
            "iisstart.htm",
            "default.aspx",
            "index.php",
        };

        private static readonly string[] TypicalNodeDetectionFiles = new[]
        {
            "server.js",
            "app.js",
        };

        private readonly NodeVersionProvider _versionProvider;
        private readonly ILogger<NodePlatformDetector> _logger;

        public NodePlatformDetector(
            NodeVersionProvider nodeVersionProvider,
            ILogger<NodePlatformDetector> logger)
        {
            _versionProvider = nodeVersionProvider;
            _logger = logger;
        }

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            bool isNodeApp = false;

            var sourceRepo = context.SourceRepo;
            if (sourceRepo.FileExists(NodeConstants.PackageJsonFileName) ||
                sourceRepo.FileExists(NodeConstants.PackageLockJsonFileName) ||
                sourceRepo.FileExists(NodeConstants.YarnLockFileName))
            {
                isNodeApp = true;
            }
            else
            {
                _logger.LogDebug(
                    $"Could not find {NodeConstants.PackageJsonFileName}/{NodeConstants.PackageLockJsonFileName}" +
                    $"/{NodeConstants.YarnLockFileName} in repo");
            }

            if (!isNodeApp)
            {
                // Copying the logic currently running in Kudu:
                var mightBeNode = false;
                foreach (var typicalNodeFile in TypicalNodeDetectionFiles)
                {
                    if (sourceRepo.FileExists(typicalNodeFile))
                    {
                        mightBeNode = true;
                        break;
                    }
                }

                if (mightBeNode)
                {
                    // Check if any of the known iis start pages exist
                    // If so, then it is not a node.js web site otherwise it is
                    foreach (var iisStartupFile in IisStartupFiles)
                    {
                        if (sourceRepo.FileExists(iisStartupFile))
                        {
                            _logger.LogDebug(
                                "App in repo is not a Node.js app as it has the file {iisStartupFile}",
                                iisStartupFile.Hash());
                            return null;
                        }
                    }

                    isNodeApp = true;
                }
                else
                {
                    // No point in logging the actual file list, as it's constant
                    _logger.LogDebug("Could not find typical Node.js files in repo");
                }
            }

            if (!isNodeApp)
            {
                _logger.LogDebug("App in repo is not a NodeJS app");
                return null;
            }

            var version = GetVersion(context);
            version = GetMaxSatisfyingVersionAndVerify(version);

            return new PlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = version,
            };
        }

        public string GetMaxSatisfyingVersionAndVerify(string version)
        {
            var versionInfo = _versionProvider.GetVersionInfo();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exception = new UnsupportedVersionException(
                    NodeConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                _logger.LogError(
                    exception,
                    $"Exception caught, the version '{version}' is not supported for the Node platform.");
                throw exception;
            }

            return maxSatisfyingVersion;
        }

        private string GetVersion(RepositoryContext context)
        {
            if (context.ResolvedNodeVersion != null)
            {
                return context.ResolvedNodeVersion;
            }

            var version = GetVersionFromPackageJson(context);
            if (version != null)
            {
                return version;
            }

            return GetDefaultVersionFromProvider();
        }

        private string GetVersionFromPackageJson(RepositoryContext context)
        {
            var packageJson = GetPackageJsonObject(context.SourceRepo, _logger);
            return packageJson?.engines?.node?.Value as string;
        }

        private string GetDefaultVersionFromProvider()
        {
            var versionInfo = _versionProvider.GetVersionInfo();
            return versionInfo.DefaultVersion;
        }
        private dynamic GetPackageJsonObject(ISourceRepo sourceRepo, ILogger logger)
        {
            dynamic packageJson = null;
            try
            {
                packageJson = ReadJsonObjectFromFile(sourceRepo, NodeConstants.PackageJsonFileName);
            }
            catch (Exception exc)
            {
                // Leave malformed package.json files for Node.js to handle.
                // This prevents Oryx from erroring out when Node.js itself might be able to tolerate the file.
                logger.LogWarning(
                    exc,
                    $"Exception caught while trying to deserialize {NodeConstants.PackageJsonFileName.Hash()}");
            }

            return packageJson;
        }

        private dynamic ReadJsonObjectFromFile(ISourceRepo sourceRepo, string fileName)
        {
            var jsonContent = sourceRepo.ReadFile(fileName);
            return JsonConvert.DeserializeObject(jsonContent);
        }
    }
}