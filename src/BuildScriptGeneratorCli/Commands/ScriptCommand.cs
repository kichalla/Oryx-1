﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command("script", Description = "Generate build script to standard output.")]
    internal class ScriptCommand : BaseCommand
    {
        [Argument(0, Description = "The source directory.")]
        public string SourceDir { get; set; }

        [Option(
            "-l|--language <name>",
            CommandOptionType.SingleValue,
            Description = "The name of the programming language being used in the provided source directory.")]
        public string Language { get; set; }

        [Option(
            "--language-version <version>",
            CommandOptionType.SingleValue,
            Description = "The version of programming language being used in the provided source directory.")]
        public string LanguageVersion { get; set; }

        [Option(
            "--log-file <file>",
            CommandOptionType.SingleValue,
            Description = "The file to which logs have to be written to.")]
        public string LogFile { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var scriptGenerator = new ScriptGenerator(console, serviceProvider);
            if (!scriptGenerator.TryGenerateScript(out var generatedScript))
            {
                return 1;
            }

            console.WriteLine(generatedScript);

            return 0;
        }

        internal override bool ShowHelp()
        {
            if (string.IsNullOrEmpty(SourceDir))
            {
                return true;
            }
            return false;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;

            if (!Directory.Exists(options.SourceDir))
            {
                console.Error.WriteLine($"Error: Could not find the source code folder '{options.SourceDir}'.");
                return false;
            }

            // Invalid to specify language version without language name
            if (string.IsNullOrEmpty(options.Language) && !string.IsNullOrEmpty(options.LanguageVersion))
            {
                console.Error.WriteLine("Cannot use language version without specifying language name also.");
                return false;
            }

            return true;
        }

        internal override void ConfigureBuildScriptGeneratorOptoins(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                SourceDir,
                destinationDir: null,
                intermediateDir: null,
                Language,
                LanguageVersion,
                LogFile,
                scriptOnly: true);
        }
    }
}