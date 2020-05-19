﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Abstraction over the repository context.
    /// </summary>
    public abstract partial class RepositoryContext
    {
        public ISourceRepo SourceRepo { get; set; }

        /// <summary>
        /// Gets or sets the version of Node used in the repo.
        /// </summary>
        public string ResolvedNodeVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of .NET Core used in the repo.
        /// </summary>
        public string ResolvedDotNetCoreRuntimeVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of Php used in the repo.
        /// </summary>
        public string ResolvedPhpVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of Python used in the repo.
        /// </summary>
        public string ResolvedPythonVersion { get; set; }
    }
}