﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector.Exceptions;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.Detector.Python
{
    internal class PythonPlatformDetector : IPlatformDetector
    {
        private readonly PythonVersionProvider _versionProvider;
        private readonly ILogger<PythonPlatformDetector> _logger;

        public PythonPlatformDetector(
            PythonVersionProvider pythonVersionProvider,
            ILogger<PythonPlatformDetector> logger)
        {
            _versionProvider = pythonVersionProvider;
            _logger = logger;
        }

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            var sourceRepo = context.SourceRepo;
            if (!sourceRepo.FileExists(PythonConstants.RequirementsFileName)
                && !sourceRepo.FileExists(PythonConstants.SetupDotPyFileName))
            {
                _logger.LogDebug($"'{PythonConstants.SetupDotPyFileName}' or '{PythonConstants.RequirementsFileName}' " +
                    $"does not exist in source repo");
                return null;
            }
            else if (!sourceRepo.FileExists(PythonConstants.RequirementsFileName)
                && sourceRepo.FileExists(PythonConstants.SetupDotPyFileName))
            {
                _logger.LogInformation($"'{PythonConstants.RequirementsFileName} doesn't exist in source repo.' " +
                    $"Oryx will try to build/detect from '{PythonConstants.SetupDotPyFileName}'that exists in source repo");
            }
            else
            {
                _logger.LogInformation($"'{PythonConstants.SetupDotPyFileName} doesn't exist in source repo.' " +
                    $"Oryx will try to build/detect from '{PythonConstants.RequirementsFileName}'that exists in source repo");
            }

            // This detects if a runtime.txt file exists if that is a python file
            var versionFromRuntimeFile = DetectPythonVersionFromRuntimeFile(context.SourceRepo);
            if (string.IsNullOrEmpty(versionFromRuntimeFile))
            {
                var files = sourceRepo.EnumerateFiles(
                    PythonConstants.PythonFileNamePattern,
                    searchSubDirectories: false);

                if (files == null || !files.Any())
                {
                    _logger.LogDebug($"Files with extension '{PythonConstants.PythonFileNamePattern}' do not exist " +
                        "in source repo root");
                    return null;
                }
            }

            var version = GetVersion(context, versionFromRuntimeFile);
            version = GetMaxSatisfyingVersionAndVerify(version);

            return new PlatformDetectorResult
            {
                Platform = PythonConstants.PlatformName,
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
                var exc = new UnsupportedVersionException(
                    PythonConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                _logger.LogError(
                    exc,
                    $"Exception caught, the version '{version}' is not supported for the Python platform.");
                throw exc;
            }

            return maxSatisfyingVersion;
        }

        private string GetVersion(RepositoryContext context, string versionFromRuntimeFile)
        {
            if (context.ResolvedPythonVersion != null)
            {
                return context.ResolvedPythonVersion;
            }

            if (versionFromRuntimeFile != null)
            {
                return versionFromRuntimeFile;
            }

            return GetDefaultVersionFromProvider();
        }

        private string GetDefaultVersionFromProvider()
        {
            var versionInfo = _versionProvider.GetVersionInfo();
            return versionInfo.DefaultVersion;
        }

        private string DetectPythonVersionFromRuntimeFile(ISourceRepo sourceRepo)
        {
            const string versionPrefix = "python-";

            // Most Python sites will have at least a .py file in the root, but
            // some may not. In that case, let them opt in with the runtime.txt
            // file, which is used to specify the version of Python.
            if (sourceRepo.FileExists(PythonConstants.RuntimeFileName))
            {
                try
                {
                    var content = sourceRepo.ReadFile(PythonConstants.RuntimeFileName);
                    var hasPythonVersion = content.StartsWith(versionPrefix, StringComparison.OrdinalIgnoreCase);
                    if (!hasPythonVersion)
                    {
                        _logger.LogDebug(
                            "Prefix {verPrefix} was not found in file {rtFileName}",
                            versionPrefix,
                            PythonConstants.RuntimeFileName.Hash());
                        return null;
                    }

                    var pythonVersion = content.Remove(0, versionPrefix.Length);
                    _logger.LogDebug("Found version {pyVer} in runtime file", pythonVersion);
                    return pythonVersion;
                }
                catch (IOException ex)
                {
                    _logger.LogError(
                        ex,
                        "An error occurred while reading file {rtFileName}",
                        PythonConstants.RuntimeFileName.Hash());
                }
            }
            else
            {
                _logger.LogDebug(
                    "Could not find file '{rtFileName}' in source repo",
                    PythonConstants.RuntimeFileName);
            }

            return null;
        }
    }
}