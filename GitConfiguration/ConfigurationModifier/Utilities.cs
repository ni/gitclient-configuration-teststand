using System.Text.Json;

namespace ConfigurationModifier
{
    internal sealed class Utilities
    {
        private static readonly string TestStandFileDifferSearchString = "\"TestStandFileDiffer\"]";
        private static readonly JsonSerializerOptions JSONWriteOptions = new()
        {
            WriteIndented = true
        };

        public static bool IsTSFileDifferUtilityInstalled()
        {
            // 'FileDifferLauncher.exe' is version agnostic and is only present in the 32-bit TestStand shared directory.
            // We will call into this exe which will launch the version specific TestStand File Differ utility (Bin\FileDiffer.exe).
            string? testStand32bitPath = Environment.GetEnvironmentVariable("TestStand");
            string fileDiffUtilityPath = string.Empty;

            if (!string.IsNullOrEmpty(testStand32bitPath))
            {
                DirectoryInfo? parentDirectoryInfo = Directory.GetParent(testStand32bitPath);
                string parentPath = string.Empty;

                if (parentDirectoryInfo != null)
                {
                    parentPath = parentDirectoryInfo.FullName;
                }

                fileDiffUtilityPath = Path.Combine(parentPath, @"Shared\TestStand\FileDifferLauncher.exe");
            }

            try
            {
                if (File.Exists(fileDiffUtilityPath))
                {
                    WriteLogMessage($"TestStand File Differ found in the system at path {fileDiffUtilityPath}.");
                    return true;
                }
                else
                {
                    WriteLogMessage($"ERROR: TestStand File Differ not found in the system at path {fileDiffUtilityPath}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteLogMessage($"ERROR: Failed to locate TestStand File Differ utility:\n{ex.Message}");
                return false;
            }
        }

        public static string GetClientNameFromConfigurationFile(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string[] tokens = fileName.Split("_");

            return tokens.Length > 0 ? tokens[0] : string.Empty;
        }

        public static bool AddExternalToolToConfigurationFile(string filePath)
        {
            string diffCommandArguments = "\\\"$LOCAL\\\" \\\"$REMOTE\\\"\"";
            string mergeCommandArguments = "\\\"$BASE\\\" \\\"$LOCAL\\\" \\\"$REMOTE\\\" \\\"$MERGED\\\"\"";

            try
            {
                string fileLauncherPath = LauncherToolPath;
                if (!IsTestStandConfigurationPresent(filePath) && !string.IsNullOrEmpty(fileLauncherPath))
                {
                    string[] customConfiguration =
                    [
                        "\n[diff]",
                        "\ttool = TestStandFileDiffer",
                        "[difftool \"TestStandFileDiffer\"]",
                        $"\tcmd = \"\\\"{fileLauncherPath}\\\" {diffCommandArguments}",
                        "[merge]",
                        "\ttool = TestStandFileDiffer",
                        "[mergetool \"TestStandFileDiffer\"]",
                        $"\tcmd = \"\\\"{fileLauncherPath}\\\" {mergeCommandArguments}"
                    ];

                    using (StreamWriter writer = new(filePath, true))
                    {
                        foreach (string line in customConfiguration)
                        {
                            writer.WriteLine(line);
                        }
                    }
                    WriteLogMessage($"Configurations added successfully to '{filePath}'.");
                }
                else if (IsTestStandConfigurationPresent(filePath) && !string.IsNullOrEmpty(fileLauncherPath))
                {
                    WriteLogMessage("TestStand configuration already present in the file. Updating the cmd line in the config file.");

                    string diffToolCommand = $"\tcmd = \"\\\"{fileLauncherPath}\\\" {diffCommandArguments}";
                    string mergeToolCommand = $"\tcmd = \"\\\"{fileLauncherPath}\\\" {mergeCommandArguments}";

                    // Read all lines from the file
                    List<string> lines = [.. File.ReadAllLines(filePath)];

                    // Check if the file contains the search string
                    if (lines.Any(line => line.Contains(TestStandFileDifferSearchString)))
                    {
                        for (int i = 0; i < lines.Count; i++)
                        {
                            if (lines[i].Contains("cmd") && i > 0) // Ensure there's a previous line
                            {
                                if (lines[i - 1].Contains("[difftool " + TestStandFileDifferSearchString))
                                {
                                    lines[i] = diffToolCommand;
                                }
                                else if (lines[i - 1].Contains("[mergetool " + TestStandFileDifferSearchString))
                                {
                                    lines[i] = mergeToolCommand;
                                }
                            }
                        }

                        // Write the updated lines back to the file
                        File.WriteAllLines(filePath, lines);
                        WriteLogMessage($"Configurations added successfully to '{filePath}'.");
                    }
                }
                else
                {
                    WriteLogMessage($"ERROR: Path to FileLauncher.exe, '{fileLauncherPath}' is not valid.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteLogMessage($"ERROR: Failed to update Git External tool in config file {filePath}:\n{ex.Message}");
                return false;
            }

            return true;
        }

        public static bool IsTestStandConfigurationPresent(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.Contains(TestStandFileDifferSearchString))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CopyFilesForConfigurationTool()
        {
            bool success = CopyConfigurationFiles();

            if (success)
            {
                LauncherToolPath = CopyFileLauncherExecutable();
            }

            if (string.IsNullOrEmpty(LauncherToolPath))
            {
                success = false;
            }

            return success;
        }

        public static bool CopyConfigurationFiles()
        {
            bool success = false;
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string destinationDirectory = @"C:\\Users\\Public\\Documents\\National Instruments\\GitExternalToolLauncher\\";

                if (!Directory.Exists(destinationDirectory))
                {
                    DirectoryInfo destinationDirectoryInfo = Directory.CreateDirectory(destinationDirectory);

                    if (!destinationDirectoryInfo.Exists)
                    {
                        WriteLogMessage($"ERROR: Failed to create destination directory '{destinationDirectory}'.");
                        return false;
                    }
                }

                string[] filesToCopy =
                [
                    "fileExtensionToApplicationMapping.json",
                    "defaultGitClient.json"
                ];

                for (int i = 0; i < filesToCopy.Length; i++)
                {
                    string sourceFilePath = Path.Combine(currentDirectory, $"{filesToCopy[i]}");
                    string destinationFilePath = Path.Combine(destinationDirectory, $"{filesToCopy[i]}");

                    // We do not want to overwrite the existing 'fileExtensionToApplicationMapping.json' file
                    // as the user may have added custom mappings for other file extensions. The 'defaultGitClient.json'
                    // file will get updated with the path to the default Git client executable by the end of this configuration.
                    if (!File.Exists(destinationFilePath))
                    {
                        File.Copy(sourceFilePath, destinationFilePath, overwrite: false);
                        WriteLogMessage("File copied successfully to " + destinationFilePath);
                    }
                    else
                    {
                        WriteLogMessage("File already exists at " + destinationFilePath + ", skipping copy.");
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                WriteLogMessage($"ERROR: Failed to copy supporting files:\n{ex.Message}");
            }

            return success;
        }

        public static string CopyFileLauncherExecutable()
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string destinationDirectory = @"C:\\Users\\Public\\Documents\\National Instruments\\GitExternalToolLauncher\\";
                string fileLauncherPath = string.Empty;

                string[] filesToCopy =
                [
                    "FileLauncher.exe",
                    "FileLauncher.dll",
                    "FileLauncher.pdb",
                    "FileLauncher.runtimeconfig.json",
                    "FileLauncher.deps.json"
                ];

                for (int i = 0; i < filesToCopy.Length; i++)
                {
                    string sourceFilePath = Path.Combine(currentDirectory, $"{filesToCopy[i]}");
                    string destinationFilePath = Path.Combine(destinationDirectory, $"{filesToCopy[i]}");

                    File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
                    WriteLogMessage("File copied successfully to " + destinationFilePath);

                    if (filesToCopy[i].Contains("FileLauncher.exe"))
                    {
                        fileLauncherPath = destinationFilePath;
                    }
                }

                return fileLauncherPath;
            }
            catch (Exception ex)
            {
                WriteLogMessage($"ERROR: Failed to copy FileLauncher set of files:\n{ex.Message}");
            }

            return string.Empty;
        }

        public static string GetGlobalGitConfigurationFileForSystem()
        {
            string gitDirPath = GetGitDirPath();

            if (string.IsNullOrEmpty(gitDirPath))
            {
                WriteLogMessage("ERROR: Git is not installed. Please install git and run the exe again to configure Git External tool globally.");
                return string.Empty;
            }

            DirectoryInfo? parentDirectoryInfo = Directory.GetParent(gitDirPath);
            string parentPath = string.Empty;

            if (parentDirectoryInfo != null)
            {
                parentPath = parentDirectoryInfo.FullName;
            }

            string filePath = Path.Combine(parentPath, "etc\\gitconfig");

            return File.Exists(filePath) ? filePath : string.Empty;
        }

        public static string GetGitDirPath()
        {
            string? pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (pathVariable == null)
            {
                return string.Empty;
            }

            string[] paths = pathVariable.Split(';');
            foreach (string path in paths)
            {
                if (path.Contains("Git"))
                {
                    return path;
                }
            }
            return string.Empty;
        }

        public static bool UpdateDefaultGitClient(string defaultGitClientPath)
        {
            bool success = false;
            DirectoryInfo? parentDirectoryInfo = Directory.GetParent(LauncherToolPath);
            string launcherToolParentPath = string.Empty;

            if (parentDirectoryInfo != null)
            {
                launcherToolParentPath = parentDirectoryInfo.FullName;
            }

            string JSONFilePath = Path.Combine(launcherToolParentPath, "defaultGitClient.json");

            if (File.Exists(JSONFilePath))
            {
                try
                {
                    string JSONContent = JsonSerializer.Serialize(defaultGitClientPath, JSONWriteOptions);
                    File.WriteAllText(JSONFilePath, JSONContent);

                    success = true;
                }
                catch (Exception ex)
                {
                    WriteLogMessage($"ERROR: Failed to update default Git client file:\n{ex.Message}");
                    success = false;
                }
            }

            return success;
        }

        public static void WriteLogMessage(string message, bool console = false)
        {
            if (console)
            {
                Console.WriteLine(message);
            }
            else
            {
                try
                {
                    using StreamWriter writer = new(LogFilePath, append: true);
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to create or write to log file:\n{ex.Message}");
                }
            }
        }

        public static string LauncherToolPath { get; private set; } = string.Empty;
        public static string LogFilePath { get; set; } = string.Empty;
    }
}
