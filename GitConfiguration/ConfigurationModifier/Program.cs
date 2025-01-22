namespace ConfigurationModifier
{
    internal sealed class Program
    {
        private string _configurationFileToUse = string.Empty;
        private string _fileLauncherPath = string.Empty;

        // Version of the ConfigurationModifier tool. This version should be updated
        // whenever there is a new release of the ConfigurationModifier tool.
        private readonly string _version = "1.1";

        public static void Main(string[] args)
        {
            Program program = new();

            Utilities.LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigurationModifierLog.log");
            Utilities.WriteLogMessage("START: Configuring Git External Tool");

            if (!program.ProcessArguments(args))
            {
                return;
            }

            bool configurationStatus = program.StartConfiguringGitExternalTool();

            if (configurationStatus)
            {
                Utilities.WriteLogMessage("Git external tool configured successfully.");
            }
            else
            {
                Utilities.WriteLogMessage("ERROR: Git external tool configuration failed.");
            }

            Utilities.WriteLogMessage("END: Configuring Git External Tool\n\n");
        }

        private bool ProcessArguments(string[] args)
        {
            if (args.Length != 1
                || (args.Length == 1
                    && (args[0] == "?" || args[0].Equals("HELP", StringComparison.InvariantCultureIgnoreCase))))
            {
                Utilities.WriteLogMessage(GetToolUsageMessage());
                Console.WriteLine(GetToolUsageMessage());
                return false;
            }
            else
            {
                _configurationFileToUse = args[0];
                return true;
            }
        }

        private bool StartConfiguringGitExternalTool()
        {
            bool isConfigured = true;

            if (!Utilities.IsTSFileDifferUtilityInstalled())
            {
                Utilities.WriteLogMessage("ERROR: TestStand File Differ Launcher not found. Please install TestStand File Differ launcher and run this exe again.");
                isConfigured = false;
            }
            else
            {
                string configurationJSONFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configurationFileToUse);

                if (!File.Exists(configurationJSONFilePath))
                {
                    Utilities.WriteLogMessage($"Configuration file not found: {configurationJSONFilePath}");
                    Utilities.WriteLogMessage("Creating default file for TestStand extensions...");

                    isConfigured = CreateDefaultConfigurationFileForGitClient(configurationJSONFilePath);
                }

                if (isConfigured)
                {
                    isConfigured = ConfigureGitExternalTool(configurationJSONFilePath);
                }
            }

            return isConfigured;
        }

        private bool CreateDefaultConfigurationFileForGitClient(string filePath)
        {
            bool success = false;
            string configurationJSONFile = Path.GetFileName(filePath);
            string clientName = Utilities.GetClientNameFromConfigurationFile(configurationJSONFile);

            if (!string.IsNullOrEmpty(clientName))
            {
                // To add new client support, add a new case statement here.
                // The new client should have a class that implements the IClientConfiguration interface.
                // The new client class should be added to the ConfigurationModifier project.

                IClientConfiguration? clientConfiguration = clientName switch
                {
                    "VisualStudio" => new VisualStudio(),
                    "Sourcetree" => new SourceTree(),
                    "GitKraken" => new GitKraken(),
                    "GitCola" => new GitCola(),
                    _ => null
                };

                if (clientConfiguration != null)
                {
                    success = clientConfiguration.CreateDefaultConfigurationFile(filePath);
                }
                else
                {
                    Utilities.WriteLogMessage("Client not supported!");
                    success = false;
                }
            }

            return success;
        }

        private bool ConfigureGitExternalTool(string configurationJSONFilePath)
        {
            bool success = true;
            string configurationJSONFile = Path.GetFileName(configurationJSONFilePath);
            string clientName = Utilities.GetClientNameFromConfigurationFile(configurationJSONFile);

            if (!string.IsNullOrEmpty(clientName))
            {
                if (Utilities.CopyFilesForConfigurationTool())
                {
                    _fileLauncherPath = Utilities.LauncherToolPath;
                }

                if (!string.IsNullOrEmpty(_fileLauncherPath))
                {
                    Utilities.WriteLogMessage($"Configuring '{clientName}' as the Git External Tool.");

                    // To add new client support, add a new case statement here.
                    // The new client should have a class that implements the IClientConfiguration interface.
                    // The new client class should be added to the ConfigurationModifier project.

                    IClientConfiguration? clientConfiguration = clientName switch
                    {
                        "VisualStudio" => new VisualStudio(),
                        "Sourcetree" => new SourceTree(),
                        "GitKraken" => new GitKraken(),
                        "GitCola" => new GitCola(),
                        _ => null
                    };

                    if (clientConfiguration != null)
                    {
                        success = clientConfiguration.ConfigureClient(configurationJSONFilePath);
                    }
                    else
                    {
                        Utilities.WriteLogMessage($"ERROR: Client '{clientName}' is not supported!");
                        success = false;
                    }
                }
                else
                {
                    Utilities.WriteLogMessage("ERROR: File Launcher application and supporting files could not be copied!");
                    return false;
                }
            }

            return success;
        }

        private string GetToolUsageMessage()
        {
            string usageMessage = $"ConfigurationModifier.exe [Version: {_version}]";
            usageMessage += "\nUsage: ConfigurationModifier.exe <config_file>";
            usageMessage += "\n\nWhere <config_file> is the JSON file containing the configuration details for the Git client.";
            usageMessage += "\n  e.g. SourceTree_Config.json";
            usageMessage += "\nNote: If this file is not present, the tool will create one with the latest version of the Git client";
            usageMessage += "\n      available in the system and configure it as per the default level mentioned in the table below:";
            usageMessage += "\n      |=========================|===============|";
            usageMessage += "\n      | Git client              | Default level |";
            usageMessage += "\n      |=========================|===============|";
            usageMessage += "\n      | Microsoft Visual Studio | Client        |";
            usageMessage += "\n      |-------------------------|---------------|";
            usageMessage += "\n      | Atlassian Sourcetree    | Client        |";
            usageMessage += "\n      |-------------------------|---------------|";
            usageMessage += "\n      | Git Cola                | Global        |";
            usageMessage += "\n      |-------------------------|---------------|";
            usageMessage += "\n      | GitKraken               | Global        |";
            usageMessage += "\n      |=========================|===============|\n";

            return usageMessage;
        }
    }

    internal sealed class GitClient
    {
        public string ClientName { get; set; } = string.Empty;
        public string ClientPath { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}
