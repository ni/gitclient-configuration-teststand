using System.Text.Json;
using Microsoft.Win32;

namespace ConfigurationModifier
{
    internal sealed class GitCola : IClientConfiguration
    {
        private static readonly JsonSerializerOptions JSONWriteOptions = new()
        {
            WriteIndented = true
        };
        public bool CreateDefaultConfigurationFile(string filePath)
        {
            return CreateDefaultGitColaConfigFile(filePath);
        }

        public bool ConfigureClient(string configurationFilePath)
        {
            return ConfigureGitCola(configurationFilePath);
        }

        public static bool CreateDefaultGitColaConfigFile(string filePath)
        {
            bool success = true;
            GitClient? latestGitCola = GetLatestGitColaInstalled();

            if (null == latestGitCola)
            {
                Utilities.WriteLogMessage("ERROR: No installation of Git Cola found!");
                success = false;
            }
            else
            {
                GitColaClient defaultGitColaConfig = new()
                {
                    GitColaClients = [latestGitCola],
                    ConfigurationLevel = "global"
                };

                try
                {
                    string JSONContent = JsonSerializer.Serialize(defaultGitColaConfig, JSONWriteOptions);
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

        private static GitClient? GetLatestGitColaInstalled()
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\git-cola";

            RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryKey) ?? Registry.CurrentUser.OpenSubKey(registryKey);

            if (key != null)
            {
                string? gitColaPath = key.GetValue("InstallLocation")?.ToString();
                if (!string.IsNullOrEmpty(gitColaPath))
                {
                    string gitColaExecutablePath = Path.Combine(gitColaPath, "bin", "git-cola.exe");
                    if (File.Exists(gitColaExecutablePath))
                    {
                        Utilities.WriteLogMessage($"Found GitCola at: {gitColaExecutablePath}");
                        return new GitClient
                        {
                            ClientName = key.GetValue("DisplayName")?.ToString() ?? string.Empty,
                            ClientPath = gitColaExecutablePath,
                            Version = key.GetValue("DisplayVersion")?.ToString() ?? string.Empty
                        };
                    }
                }
            }

            return null;
        }

        public static bool ConfigureGitCola(string configurationJSONFilePath)
        {
            bool success = true;
            Utilities.WriteLogMessage("Configuring client: Git Cola");

            try
            {
                string JSONContent = File.ReadAllText(configurationJSONFilePath);
                GitColaClient? gitColaConfiguration = JsonSerializer.Deserialize<GitColaClient>(JSONContent);

                Utilities.WriteLogMessage($"Configuring level: {gitColaConfiguration?.ConfigurationLevel}");

                switch (gitColaConfiguration?.ConfigurationLevel?.ToUpperInvariant())
                {
                    case "REPO":
                        string[] repositoryPaths = [.. gitColaConfiguration.RepositoryPaths];
                        success = ConfigureRepoConfig(repositoryPaths);
                        break;

                    case "GLOBAL":
                        success = ConfigureGitColaGlobal();
                        break;

                    default:
                        Utilities.WriteLogMessage($"ERROR: Invalid configuration level '{gitColaConfiguration?.ConfigurationLevel}' for Git Cola.");
                        success = false;
                        break;
                }

                _ = UpdateDefaultClientAsGitCola(configurationJSONFilePath);
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
                    string gitConfigPath = Path.Combine(repositoryPath, ".git\\config");
                    if (!File.Exists(gitConfigPath))
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

                    success = Utilities.AddExternalToolToConfigurationFile(gitConfigPath);
                }
                else
                {
                    Utilities.WriteLogMessage($"ERROR: The provided repo path, '{repositoryPath}' does not exist.");
                    success = false;
                }
            }

            return success;
        }

        private static bool ConfigureGitColaGlobal()
        {
            bool success;

            string gitConfigFilePath = FindGlobalConfigFileForGitCola();
            if (!string.IsNullOrEmpty(gitConfigFilePath))
            {
                success = Utilities.AddExternalToolToConfigurationFile(gitConfigFilePath);
            }
            else
            {
                Utilities.WriteLogMessage("ERROR: Global Git config file not found for GitCola client.");
                success = false;
            }

            return success;
        }

        private static string FindGlobalConfigFileForGitCola()
        {
            string gitConfigFilePath = Utilities.GetGlobalGitConfigurationFileForSystem();
            return gitConfigFilePath;
        }

        private static bool UpdateDefaultClientAsGitCola(string configurationJSONFilePath)
        {
            bool success = false;
            string JSONContent = File.ReadAllText(configurationJSONFilePath);
            GitColaClient? gitColaConfiguration = JsonSerializer.Deserialize<GitColaClient>(JSONContent);
            GitClient[] gitColaInstalled = [.. gitColaConfiguration?.GitColaClients];

            if (gitColaInstalled.Length > 0)
            {
                GitClient defaultClient = gitColaInstalled[0];
                string gitColaPath = defaultClient.ClientPath;

                _ = Utilities.UpdateDefaultGitClient(gitColaPath);
            }

            return success;
        }
    }
    internal sealed class GitColaClient
    {
        public List<GitClient>? GitColaClients { get; set; }
        public string? ConfigurationLevel { get; set; }
        public List<string>? RepositoryPaths { get; set; }
    }
}
