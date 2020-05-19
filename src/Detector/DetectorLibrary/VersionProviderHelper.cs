﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Oryx.Detector
{
    internal static class VersionProviderHelper
    {
        internal static IEnumerable<string> GetSupportedVersions(
            IEnumerable<string> optionsVersions,
            string versionsDir)
        {
            return optionsVersions != null ? optionsVersions : GetVersionsFromDirectory(versionsDir);
        }

        internal static IEnumerable<string> GetVersionsFromDirectory(string versionsDir)
        {
            var listOptions = new EnumerationOptions()
            {
                RecurseSubdirectories = false,
                IgnoreInaccessible = false,
            };

            IEnumerable<DirectoryInfo> versionDirectories;
            try
            {
                versionDirectories = Directory.EnumerateDirectories(versionsDir, "*", listOptions)
                    .Select(versionDir => new DirectoryInfo(versionDir));
            }
            catch (IOException)
            {
                return Enumerable.Empty<string>();
            }

            var versions = new List<SemVer.Version>();
            foreach (var versionDir in versionDirectories)
            {
                try
                {
                    var version = new SemVer.Version(versionDir.Name);
                    versions.Add(version);
                }
                catch (ArgumentException)
                {
                    // Ignore non-Semantic-Versioning based strings like 'latest' or 'lts'
                }
            }

            versions.Sort();
            return versions.Select(v => v.ToString());
        }
    }
}
