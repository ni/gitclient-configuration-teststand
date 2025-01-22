using System.Diagnostics;
using System.Text.Json;
using Microsoft.Win32;

using Newtonsoft.Json;

namespace ConfigurationModifier
{
    internal sealed class VisualStudio : IClientConfiguration
    {
        // 'vswhere.exe' is a utility that locates installed Visual Studio instances and can return information about them.
        // However, note that it is installed with Visual Studio 2017 and later.
        private static string _VSWherePath = string.Empty;
        private static readonly JsonSerializerOptions JSONWriteOptions = new()
        {
            WriteIndented = true
        };

        public bool CreateDefaultConfigurationFile(string filePath)
        {
            return CreateDefaultVisualStudioConfigurationFile(filePath);
        }

        public bool ConfigureClient(string configurationFilePath)
        {
            return ConfigureVisualStudio(configurationFilePath);
        }

        public static bool CreateDefaultVisualStudioConfigurationFile(string filePath)
        {
            bool success = true;
            GitClient? latestVisualStudio = GetLatestVisualStudioInstalled();

            if (null == latestVisualStudio)
            {
                Utilities.WriteLogMessage("ERROR: No installation of Visual Studio found!");
                success = false;
            }
            else
            {
                VisualStudioClient defaultVSConfiguration = new()
                {
                    MSVisualStudioClients = [latestVisualStudio],
                    ConfigurationLevel = "client"
                };

                try
                {
                    string JSONContent = System.Text.Json.JsonSerializer.Serialize(defaultVSConfiguration, JSONWriteOptions);
                    File.WriteAllText(filePath, JSONContent);
                    Utilities.WriteLogMessage($"Default configuration file created at: {filePath}");
                }
                catch (Exception ex)
                {
                    success = false;
                    Utilities.WriteLogMessage($"ERROR: Failed to create default configuration file:\n{ex.Message}");
                }
            }

            return success;
        }

        private static GitClient? GetLatestVisualStudioInstalled()
        {
            _VSWherePath = GetVsWherePath();

            if (!File.Exists(_VSWherePath))
            {
                Utilities.WriteLogMessage("ERROR: 'vswhere.exe' not found. Please install Visual Studio 2017 or later and run the application again.");
                return null;
            }

            string output = RunVsWhere();
            if (string.IsNullOrEmpty(output))
            {
                Utilities.WriteLogMessage("ERROR: No Visual Studio installation found.");
                return null;
            }

            List<VSInstance>? visualStudioClients = JsonConvert.DeserializeObject<List<VSInstance>>(output);

            if (visualStudioClients == null || visualStudioClients.Count == 0)
            {
                Utilities.WriteLogMessage("ERROR: No Visual Studio installation found.");
                return null;
            }

            foreach (VSInstance instance in visualStudioClients)
            {
                Utilities.WriteLogMessage($"Visual Studio found: {instance.DisplayName}");
            }

            VSInstance? latestInstance = visualStudioClients.OrderByDescending(static item => item.Catalog?.ProductLineVersion)
                .OrderByDescending(static item => item.DisplayName?.Contains("Enterprise"))
                .ThenByDescending(static item => item.DisplayName?.Contains("Professional"))
                .FirstOrDefault();

            if (latestInstance != null)
            {
                return new GitClient
                {
                    ClientName = latestInstance.DisplayName ?? string.Empty,
                    ClientPath = latestInstance.ProductPath ?? string.Empty,
                    Version = latestInstance.Catalog?.ProductDisplayVersion ?? string.Empty
                };
            }
            else
            {
                Utilities.WriteLogMessage("ERROR: No Visual Studio installation found.");
                return null;
            }
        }
        private static string GetVsWherePath()
        {
            const string registryKey = @"SOFTWARE\Microsoft\VisualStudio\Setup";
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                string? installerPath = key.GetValue("SharedInstallationPath")?.ToString();
                if (!string.IsNullOrEmpty(installerPath))
                {
                    DirectoryInfo? parentDirectoryInfo = Directory.GetParent(installerPath);
                    string parentPath = string.Empty;

                    if (parentDirectoryInfo != null)
                    {
                        parentPath = parentDirectoryInfo.FullName;
                    }

                    string vsWherePath = Path.Combine(parentPath, "Installer", "vswhere.exe");
                    if (File.Exists(vsWherePath))
                    {
                        Utilities.WriteLogMessage($"Found vswhere.exe at: {vsWherePath}");
                        return vsWherePath;
                    }
                }
            }
            return string.Empty;
        }

        private static string RunVsWhere()
        {
            string output = string.Empty;

            ProcessStartInfo startInfo = new()
            {
                FileName = _VSWherePath,
                Arguments = "-format json",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            try
            {
                using Process? process = Process.Start(startInfo);
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException("ERROR: Failed to start process.");
                    }

                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteLogMessage($"ERROR: Failed to execute 'VSWhere.exe':\n{ex.Message}");
            }

            return output;
        }

        public static bool ConfigureVisualStudio(string configurationJSONFilePath)
        {
            bool success = true;
            Utilities.WriteLogMessage("Configuring client: Visual Studio");

            try
            {
                string JSONContent = File.ReadAllText(configurationJSONFilePath);
                VisualStudioClient? visualStudioConfiguration = System.Text.Json.JsonSerializer.Deserialize<VisualStudioClient>(JSONContent);

                Utilities.WriteLogMessage($"Configuring level: {visualStudioConfiguration?.ConfigurationLevel}");

                if (visualStudioConfiguration != null)
                {
                    switch (visualStudioConfiguration.ConfigurationLevel?.ToUpperInvariant())
                    {
                        case "REPO":
                            string[] repositoryPaths = [.. visualStudioConfiguration.RepositoryPaths];
                            success = ConfigureRepoConfig(repositoryPaths);
                            break;

                        case "CLIENT":
                            GitClient[] visualStudioInstalled = [.. visualStudioConfiguration.MSVisualStudioClients];
                            success = ConfigureVisualStudioClients(visualStudioInstalled);
                            break;

                        case "GLOBAL":
                            success = ConfigureVisualStudioGlobal();
                            break;

                        default:
                            Utilities.WriteLogMessage($"ERROR: Invalid configuration level '{visualStudioConfiguration.ConfigurationLevel}' for Visual Studio.");
                            success = false;
                            break;
                    }
                }

                _ = UpdateDefaultClientAsVS(configurationJSONFilePath);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogMessage($"ERROR: Failed to configure GitCola as Git External Tool:\n{ex.Message}");
            }

            return success;
        }

        private static bool ConfigureRepoConfig(string[] repositoryPaths)
        {
            bool success = true;

            foreach (string repositoryPath in repositoryPaths)
            {
                if (Directory.Exists(repositoryPath))
                {
                    string gitConfigurationPath = Path.Combine(repositoryPath, ".git\\config");
                    if (!File.Exists(gitConfigurationPath))
                    {
                        Utilities.WriteLogMessage($"Config file not found for repo {repositoryPath}. Attempting to create config file.");

                        if (Directory.Exists(Path.Combine(repositoryPath, ".git")))
                        {
                            // .git folder exists but config file does not exist. Create the config file.
                            try
                            {
                                _ = File.Create(Path.Combine(repositoryPath, ".git\\config"));
                            }
                            catch (Exception ex)
                            {
                                Utilities.WriteLogMessage($"ERROR: Failed to create config file:\n{ex.Message}");
                                success = false;
                            }
                        }
                        else
                        {
                            // The .git directory does not exist => this is not a Git repo.
                            success = false;
                        }

                        success = false;
                    }

                    success = Utilities.AddExternalToolToConfigurationFile(gitConfigurationPath);
                }
                else
                {
                    Utilities.WriteLogMessage($"ERROR: The provided repo path, '{repositoryPath}' does not exist.");
                    success = false;
                }
            }

            return success;
        }

        private static bool ConfigureVisualStudioClients(GitClient[] visualStudioInstalled)
        {
            bool success = true;

            foreach (GitClient visualStudio in visualStudioInstalled)
            {
                string gitConfigurationFilePath = FindGitClientConfigurationFileForVS(visualStudio);
                if (!string.IsNullOrEmpty(gitConfigurationFilePath))
                {
                    success = Utilities.AddExternalToolToConfigurationFile(gitConfigurationFilePath);
                }
                else
                {
                    Utilities.WriteLogMessage("ERROR: Git config file not found for Visual Studio client.");
                    success = false;
                }
            }

            return success;
        }

        private static string FindGitClientConfigurationFileForVS(GitClient visualStudio)
        {
            string? clientDirectory = Path.GetDirectoryName(visualStudio.ClientPath);
            string gitDirectory;
            string gitConfigurationFilePath = string.Empty;

            if (!string.IsNullOrEmpty(clientDirectory))
            {
                gitDirectory = Path.Combine(clientDirectory, "CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer\\Git\\etc");
                gitConfigurationFilePath = Path.Combine(gitDirectory, "gitconfig");
            }

            return gitConfigurationFilePath;
        }

        private static bool ConfigureVisualStudioGlobal()
        {
            bool success;

            string gitConfigurationFilePath = FindGlobalConfigurationFileForVS();
            if (!string.IsNullOrEmpty(gitConfigurationFilePath))
            {
                success = Utilities.AddExternalToolToConfigurationFile(gitConfigurationFilePath);
            }
            else
            {
                Utilities.WriteLogMessage("ERROR: Global Git config file not found for Visual Studio client.");
                success = false;
            }

            return success;
        }

        private static string FindGlobalConfigurationFileForVS()
        {
            return Utilities.GetGlobalGitConfigurationFileForSystem();
        }

        private static bool UpdateDefaultClientAsVS(string configurationJSONFilePath)
        {
            bool success = false;
            string JSONContent = File.ReadAllText(configurationJSONFilePath);
            VisualStudioClient? visualStudioConfiguration = System.Text.Json.JsonSerializer.Deserialize<VisualStudioClient>(JSONContent);
            GitClient[] visualStudioInstalled = [.. visualStudioConfiguration?.MSVisualStudioClients];

            if (visualStudioInstalled.Length > 0)
            {
                GitClient defaultClient = visualStudioInstalled[0];
                string vsDiffMergeToolPath = GetVsDiffMergePath(defaultClient);

                if (!string.IsNullOrEmpty(vsDiffMergeToolPath))
                {
                    _ = Utilities.UpdateDefaultGitClient(vsDiffMergeToolPath);
                }
            }

            return success;
        }

        private static string GetVsDiffMergePath(GitClient defaultClient)
        {
            string? clientDirectory = Path.GetDirectoryName(defaultClient.ClientPath);
            string vsDiffMergeToolPath = string.Empty;

            if (!string.IsNullOrEmpty(clientDirectory))
            {
                vsDiffMergeToolPath = Path.Combine(clientDirectory, "CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer\\vsDiffMerge.exe");
            }

            return File.Exists(vsDiffMergeToolPath) ? vsDiffMergeToolPath : string.Empty;
        }
    }

    internal sealed class VSInstance
    {
        public string? ProductPath { get; set; }
        public string? DisplayName { get; set; }
        public Catalog? Catalog { get; set; }
    }

    internal sealed class Catalog
    {
        public string? ProductLineVersion { get; set; }
        public string? ProductDisplayVersion { get; set; }
    }

    internal sealed class VisualStudioClient
    {
        public List<GitClient>? MSVisualStudioClients { get; set; }
        public string? ConfigurationLevel { get; set; }
        public List<string>? RepositoryPaths { get; set; }
    }
}
