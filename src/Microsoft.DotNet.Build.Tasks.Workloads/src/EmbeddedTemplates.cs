// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.DotNet.Build.Tasks.Workloads
{
    internal class EmbeddedTemplates
    {
        internal static TaskLoggingHelper Log;

        private static readonly string s_namespace = "";

        private static readonly Dictionary<string, string> TemplateResources = new();

        public static string Extract(string filename, string destinationFolder)
        {
            return Extract(filename, destinationFolder, filename);
        }

        public static string Extract(string filename, string destinationFolder, string destinationFilename)
        {
            if (!TemplateResources.TryGetValue(filename, out string resourceName))
            {
                throw new KeyNotFoundException($"No template for '{filename}' exists.");
            }

            // Clean out stale files, just to be safe.
            string destinationPath = Path.Combine(destinationFolder, destinationFilename);
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }
            else
            {
                Directory.CreateDirectory(destinationFolder);
            }

            using Stream rs = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using FileStream fs = new(destinationPath, FileMode.Create, FileAccess.Write);

            if (rs != null)
            {
                rs.CopyTo(fs);
                Log?.LogMessage(MessageImportance.Low, $"Resource '{resourceName}' extracted to '{destinationPath}");
            }
            else
            {
                Log?.LogMessage(MessageImportance.Low, $"Unable to find resource: {resourceName}");
            }

            return destinationPath;
        }

        static EmbeddedTemplates()
        {
            s_namespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;

            TemplateResources = new()
            {
                { "DependencyProvider.wxs", $"{s_namespace}.src.MsiTemplate.DependencyProvider.wxs" },
                { "Directories.wxs", $"{s_namespace}.src.MsiTemplate.Directories.wxs" },
                { "Product.wxs", $"{s_namespace}.src.MsiTemplate.Product.wxs" },
                { "Registry.wxs", $"{s_namespace}.src.MsiTemplate.Registry.wxs" },
                { "Variables.wxi", $"{s_namespace}.src.MsiTemplate.Variables.wxi" },

                { $"msi.swr", $"{s_namespace}.src.SwixTemplate.msi.swr" },
                { $"msi.swixproj", $"{s_namespace}.src.SwixTemplate.msi.swixproj"},
                { $"component.swr", $"{s_namespace}.src.SwixTemplate.component.swr" },
                { $"component.res.swr", $"{s_namespace}.src.SwixTemplate.component.res.swr" },
                { $"component.swixproj", $"{s_namespace}.src.SwixTemplate.component.swixproj" },
                { $"manifest.vsmanproj", $"{s_namespace}.src.SwixTempalte.manifest.vsmanproj"}
            };
        }
    }
}
