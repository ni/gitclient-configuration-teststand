using System.Text.Json;
using Microsoft.Win32;

namespace ConfigurationModifier
{
    internal sealed class GitKraken : IClientConfiguration
    {
        private static readonly JsonSerializerOptions JSONWriteOptions = new()
        {
            WriteIndented = true
        };

        public bool CreateDefaultConfigurationFile(string filePath)
        {
            return CreateDefaultGitKrakenConfigurationFile(filePath);
        }

        public bool ConfigureClient(string configurationFilePath)
        {
            return ConfigureGitKraken(configurationFilePath);
        }

        public static bool CreateDefaultGitKrakenConfigurationFile(string filePath)
        {
            bool success = true;
            GitClient? latestGitKraken = GetLatestGitKrakenInstalled();

            if (null == latestGitKraken)
            {
                Utilities.WriteLogMessage("ERROR: No installation of Git Kraken found!");
                success = false;
            }
            else
            {
                GitKrakenClient defaultGitKrakenConfiguration = new()
                {
                    GitKrakenClients = [latestGitKraken],
                    ConfigurationLevel = "global"
                };

                try
                {
                    string JSONContent = JsonSerializer.Serialize(defaultGitKrakenConfiguration, JSONWriteOptions);
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

        private static GitClient? GetLatestGitKrakenInstalled()
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\gitkraken";
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryKey) ?? Registry.CurrentUser.OpenSubKey(registryKey);
            if (key != null)
            {
                string? gitKrakenPath = key.GetValue("InstallLocation")?.ToString();
                if (!string.IsNullOrEmpty(gitKrakenPath))
                {
                    string gitKrakenExecutablePath = Path.Combine(gitKrakenPath, "gitkraken.exe");
                    if (File.Exists(gitKrakenExecutablePath))
                    {
                        Utilities.WriteLogMessage($"Found Git Kraken at: {gitKrakenExecutablePath}");

                        return new GitClient
                        {
                            ClientName = key.GetValue("DisplayName")?.ToString() ?? string.Empty,
                            ClientPath = gitKrakenExecutablePath,
                            Version = key.GetValue("DisplayVersion")?.ToString() ?? string.Empty
                        };
                    }
                }
            }

            return null;
        }

        public static bool ConfigureGitKraken(string configurationJSONFilePath)
        {
            bool success = true;
            Utilities.WriteLogMessage("Configuring client: Git Kraken");

            try
            {
                string JSONContent = File.ReadAllText(configurationJSONFilePath);
                GitKrakenClient? gitKrakenConfiguration = JsonSerializer.Deserialize<GitKrakenClient>(JSONContent);

                Utilities.WriteLogMessage($"Configuring level: {gitKrakenConfiguration?.ConfigurationLevel}");

                if (gitKrakenConfiguration != null)
                {
                    switch (gitKrakenConfiguration.ConfigurationLevel?.ToUpperInvariant())
                    {
                        case "GLOBAL":
                            success = ConfigureGitKrakenGlobal();
                            break;

                        default:
                            Utilities.WriteLogMessage($"ERROR: Invalid configuration level '{gitKrakenConfiguration?.ConfigurationLevel}' for Git Kraken.");
                            success = false;
                            break;
                    }
                }

                _ = UpdateDefaultClientAsGitKraken(configurationJSONFilePath);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogMessage($"ERROR: Failed to configure GitCola as Git External Tool:\n{ex.Message}");
            }

            return success;
        }

        private static bool ConfigureGitKrakenGlobal()
        {
            bool success;

            string gitConfigurationFilePath = FindGlobalConfigurationFileForGitKraken();
            if (!string.IsNullOrEmpty(gitConfigurationFilePath))
            {
                success = Utilities.AddExternalToolToConfigurationFile(gitConfigurationFilePath);
            }
            else
            {
                Utilities.WriteLogMessage("ERROR: Global Git config file not found for GitKraken client.");
                success = false;
            }

            return success;
        }

        private static string FindGlobalConfigurationFileForGitKraken()
        {
            return Utilities.GetGlobalGitConfigurationFileForSystem();
        }

        private static bool UpdateDefaultClientAsGitKraken(string configurationJSONFilePath)
        {
            bool success = false;
            string JSONContent = File.ReadAllText(configurationJSONFilePath);
            GitKrakenClient? gitKrakenConfiguration = JsonSerializer.Deserialize<GitKrakenClient>(JSONContent);
            GitClient[] gitKrakenInstalled = [.. gitKrakenConfiguration?.GitKrakenClients];

            if (gitKrakenInstalled.Length > 0)
            {
                GitClient defaultClient = gitKrakenInstalled[0];
                string gitKrakenPath = defaultClient.ClientPath;

                _ = Utilities.UpdateDefaultGitClient(gitKrakenPath);
            }

            return success;
        }
    }

    internal sealed class GitKrakenClient
    {
        public List<GitClient>? GitKrakenClients { get; set; }
        public string? ConfigurationLevel { get; set; }
    }
}
