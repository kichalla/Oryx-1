﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotnetCore;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class DotnetCoreScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddDotnetCoreScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILanguageDetector, DotnetCoreLanguageDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILanguageScriptGenerator, DotnetCoreScriptGenerator>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IConfigureOptions<DotnetCoreScriptGeneratorOptions>, DotnetCoreScriptGeneratorOptionsSetup>());
            services.AddSingleton<IDotnetCoreVersionProvider, DotnetCoreVersionProvider>();
            return services;
        }
    }
}
