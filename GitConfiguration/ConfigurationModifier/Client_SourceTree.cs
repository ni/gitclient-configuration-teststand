using System.Text.Json;
using Microsoft.Win32;

namespace ConfigurationModifier
{
    internal sealed class SourceTree : IClientConfiguration
    {
        private static readonly JsonSerializerOptions JSONWriteOptions = new()
        {
            WriteIndented = true
        };

        public bool CreateDefaultConfigurationFile(string filePath)
        {
            return CreateDefaultSourceTreeConfigurationFile(filePath);
        }

        public bool ConfigureClient(string configurationFilePath)
        {
            return ConfigureSourceTree(configurationFilePath);
        }

        public static bool CreateDefaultSourceTreeConfigurationFile(string filePath)
        {
            bool success = true;
            GitClient? latestSourceTree = GetLatestSourceTreeInstalled();

            if (null == latestSourceTree)
            {
                Utilities.WriteLogMessage("ERROR: No installation of Sourcetree found!");
                success = false;
            }
            else
            {
                SourceTreeClient defaultSourceTreeConfiguration = new()
                {
                    SourceTreeClients = [latestSourceTree],
                    ConfigurationLevel = "client"
                };

                try
                {
                    string JSONContent = JsonSerializer.Serialize(defaultSourceTreeConfiguration, JSONWriteOptions);
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

        private static GitClient? GetLatestSourceTreeInstalled()
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SourceTree";
            RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryKey) ?? Registry.CurrentUser.OpenSubKey(registryKey);

            if (key != null)
            {
                string? sourceTreePath = key.GetValue("InstallLocation")?.ToString();
                if (!string.IsNullOrEmpty(sourceTreePath))
                {
                    string sourceTreeExecutablePath = Path.Combine(sourceTreePath, "SourceTree.exe");
                    if (File.Exists(sourceTreeExecutablePath))
                    {
                        Utilities.WriteLogMessage($"Found SourceTree at: {sourceTreeExecutablePath}");

                        return new GitClient
                        {
                            ClientName = key.GetValue("DisplayName")?.ToString() ?? string.Empty,
                            ClientPath = sourceTreeExecutablePath,
                            Version = key.GetValue("DisplayVersion")?.ToString() ?? string.Empty
                        };
                    }
                }
            }

            return null;
        }

        public static bool ConfigureSourceTree(string configurationJSONFilePath)
        {
            bool success = true;
            Utilities.WriteLogMessage("Configuring client: Sourcetree");

            try
            {
                string JSONContent = File.ReadAllText(configurationJSONFilePath);
                SourceTreeClient? sourceTreeConfiguration = JsonSerializer.Deserialize<SourceTreeClient>(JSONContent);

                Utilities.WriteLogMessage($"Configuring level: {sourceTreeConfiguration?.ConfigurationLevel}");

                if (sourceTreeConfiguration != null)
                {
                    switch (sourceTreeConfiguration.ConfigurationLevel?.ToUpperInvariant())
                    {
                        case "REPO":
                            string[] repositoryPaths = [.. sourceTreeConfiguration.RepositoryPaths];
                            success = ConfigureRepositoryConfiguration(repositoryPaths);
                            break;

                        case "CLIENT":
                            GitClient[] sourceTreeInstalled = [.. sourceTreeConfiguration.SourceTreeClients];
                            success = ConfigureSourceTreeClients(sourceTreeInstalled);
                            break;

                        case "GLOBAL":
                            success = ConfigureSourceTreeGlobal();
                            break;

                        default:
                            Utilities.WriteLogMessage($"ERROR: Invalid configuration level '{sourceTreeConfiguration?.ConfigurationLevel}' for Sourcetree.");
                            success = false;
                            break;
                    }
                }

                _ = UpdateDefaultClientAsSourceTree(configurationJSONFilePath);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogMessage($"ERROR: Failed to configure GitCola as Git External Tool:\n{ex.Message}");
            }

            return success;
        }

        private static bool ConfigureRepositoryConfiguration(string[] repositoryPaths)
        {
            bool success = true;

            foreach (string repositoryPath in repositoryPaths)
            {
                if (Directory.Exists(repositoryPath))
                {
                    string gitConfigurationFilePath = Path.Combine(repositoryPath, ".git\\config");
                    if (!File.Exists(gitConfigurationFilePath))
                    {
                        Utilities.WriteLogMessage($"Config file not found for repo '{repositoryPath}'. Attempting to create config file.");

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

                    success = Utilities.AddExternalToolToConfigurationFile(gitConfigurationFilePath);
                }
                else
                {
                    Utilities.WriteLogMessage($"ERROR: The provided repo path, '{repositoryPath}' does not exist.");
                    success = false;
                }
            }

            return success;
        }

        private static bool ConfigureSourceTreeClients(GitClient[] sourceTreeInstalled)
        {
            bool success = true;

            foreach (GitClient sourceTree in sourceTreeInstalled)
            {
                string gitConfigurationFilePath = FindGitClientConfigurationFileForSourceTree(sourceTree);
                if (!string.IsNullOrEmpty(gitConfigurationFilePath))
                {
                    success = Utilities.AddExternalToolToConfigurationFile(gitConfigurationFilePath);
                }
                else
                {
                    Utilities.WriteLogMessage("ERROR: Git config file not found for SourceTree client.");
                    success = false;
                }
            }

            return success;
        }

        private static string FindGitClientConfigurationFileForSourceTree(GitClient sourceTree)
        {
            string gitConfigurationFilePath = string.Empty;
            DirectoryInfo? parentDirectoryInfo = Directory.GetParent(sourceTree.ClientPath);
            string parentPath = string.Empty;

            if (parentDirectoryInfo != null && parentDirectoryInfo.Parent != null)
            {
                parentPath = parentDirectoryInfo.Parent.FullName;
            }

            try
            {
                string gitDirectory = Path.Combine(parentPath, "Atlassian\\SourceTree\\git_local");
                if (Directory.Exists(gitDirectory))
                {
                    gitConfigurationFilePath = Path.Combine(gitDirectory, "etc\\gitconfig");
                }
                else
                {
                    // Get the path of the system level global git config file. As that is being used as the Client config file for SourceTree.
                    gitConfigurationFilePath = Utilities.GetGlobalGitConfigurationFileForSystem();
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteLogMessage($"ERROR: Failed to get Git config file for Sourcetree:\n{ex.Message}");
            }

            return gitConfigurationFilePath;
        }

        private static bool ConfigureSourceTreeGlobal()
        {
            bool success;

            string gitConfigurationFilePath = FindGlobalConfigurationFileForSourceTree();
            if (!string.IsNullOrEmpty(gitConfigurationFilePath))
            {
                success = Utilities.AddExternalToolToConfigurationFile(gitConfigurationFilePath);
            }
            else
            {
                Utilities.WriteLogMessage("Global Git config file not found for SourceTree client.");
                success = false;
            }

            return success;
        }

        private static string FindGlobalConfigurationFileForSourceTree()
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string gitConfigurationFilePath = Path.Combine(userProfilePath, ".gitconfig");
            return File.Exists(gitConfigurationFilePath) ? gitConfigurationFilePath : string.Empty;
        }

        private static bool UpdateDefaultClientAsSourceTree(string configurationJSONFilePath)
        {
            bool success = false;
            string JSONContent = File.ReadAllText(configurationJSONFilePath);
            SourceTreeClient? sourceTreeConfiguration = JsonSerializer.Deserialize<SourceTreeClient>(JSONContent);
            GitClient[] sourceTreeInstalled = [.. sourceTreeConfiguration?.SourceTreeClients];

            if (sourceTreeInstalled.Length > 0)
            {
                GitClient defaultClient = sourceTreeInstalled[0];
                string sourceTreePath = defaultClient.ClientPath;

                _ = Utilities.UpdateDefaultGitClient(sourceTreePath);
            }

            return success;
        }
    }

    internal sealed class SourceTreeClient
    {
        public List<GitClient>? SourceTreeClients { get; set; }
        public string? ConfigurationLevel { get; set; }
        public List<string>? RepositoryPaths { get; set; }
    }
}
