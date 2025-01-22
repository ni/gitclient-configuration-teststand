using System.Diagnostics;
using System.Text.Json;

namespace FileLauncher
{
    internal sealed class Program
    {
        private const string _fileExtensionToApplicationMappingFile = "fileExtensionToApplicationMapping.json";
        private const string _defaultGitClientFile = "defaultGitClient.json";
        private const string _TSFileDiffMergeLauncherPath = @"C:\Program Files (x86)\National Instruments\Shared\TestStand\FileDifferLauncher.exe";
        private bool _mergeMode;

        // Version of the ConfigurationModifier tool. This version should be updated
        // whenever there is a new release of the ConfigurationModifier tool.
        private readonly string _version = "1.1";

        private static readonly JsonSerializerOptions JSONWriteOptions = new()
        {
            WriteIndented = true
        };

        public static void Main(string[] args)
        {
            Program program = new();

            program.LogMessage("START: Launching Git External Tool");

            string[] fileList = program.ProcessArguments(args);

            if (fileList == null || fileList.Length == 0)
            {
                return;
            }

            if (program._mergeMode)
            {
                program.LaunchApplication(fileList[0], fileList[1], fileList[2], fileList[3]);
            }
            else
            {
                program.LaunchApplication(fileList[0], fileList[1]);
            }

            program.LogMessage("END: Launching Git External Tool\n\n");
        }

        private string[] ProcessArguments(string[] args)
        {
            if (args is not { Length: 2 } and not { Length: 4 })
            {
                LogMessage(GetToolUsageMessage());
                return [];
            }

            if (args.Length == 1)
            {
                LogMessage(GetToolUsageMessage());
                return [];
            }
            else if (args.Length == 2)
            {
                _mergeMode = false;
            }
            else if (args.Length == 4)
            {
                _mergeMode = true;
            }
            else
            {
                // Not reachable
                return [];
            }

            return args;
        }

        private void LaunchApplication(string file1, string file2, string file3 = "", string file4 = "")
        {
            string fileExtension = GetFileExtensionForComparingFiles(file1, file2, file3, file4);
            string appPath = GetMappedApplicationFromConfigurationFile(fileExtension);

            // When comparing files, Git Kraken happens to lock the file when it is opened.
            // So, we need to copy the file to a temp location and open it.
            if (IsFileLocked(file1))
            {
                file1 = CreateCopyOfLockedFile(file1);
            }

            if (IsFileLocked(file2))
            {
                file2 = CreateCopyOfLockedFile(file2);
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = appPath,
                Arguments = _mergeMode ? $"\"{file1}\" \"{file2}\" \"{file3}\" \"{file4}\"" : $"\"{file1}\" \"{file2}\"",
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

                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR: Failed to launch application\n{ex.Message}");
            }
        }

        private string CreateCopyOfLockedFile(string filePath)
        {
            LogMessage($"File '{filePath}' is locked by another process. Creating a copy for comparing.");
            string tempFileName = "Copy_" + Path.GetFileName(filePath);
            DirectoryInfo? parentDirectoryInfo = Directory.GetParent(filePath);

            if (parentDirectoryInfo != null)
            {
                try
                {
                    string parentPath = parentDirectoryInfo.FullName;
                    string tempFilePath = Path.Combine(parentPath, tempFileName);

                    File.Copy(filePath, tempFilePath, overwrite: true);

                    filePath = tempFilePath;

                    LogMessage($"Copy of file created at '{tempFilePath}'.");
                }
                catch (Exception ex)
                {
                    LogMessage($"ERROR: Failed to create a copy of the file, '{filePath}':\n{ex.Message}");
                }
            }

            return filePath;
        }

        private string GetFileExtensionForComparingFiles(string file1, string file2, string file3, string file4)
        {
            string file1Extension = Path.GetExtension(file1.ToUpperInvariant());
            string file2Extension = Path.GetExtension(file2.ToUpperInvariant());
            string file3Extension = string.Empty;
            string file4Extension = string.Empty;
            string fileExtensionToUse;

            if (!string.IsNullOrEmpty(file3))
            {
                file3Extension = Path.GetExtension(file3.ToUpperInvariant());
            }

            if (!string.IsNullOrEmpty(file4))
            {
                file4Extension = Path.GetExtension(file4.ToUpperInvariant());
            }

            if (IsGitKrakenFileExtension(file1Extension)
                && IsGitKrakenFileExtension(file2Extension)
                && IsGitKrakenFileExtension(file3Extension))
            {
                file1Extension = ".SEQ";
                file2Extension = ".SEQ";
                file3Extension = ".SEQ";
            }

            if (_mergeMode
                && file1Extension == file2Extension
                && file2Extension == file3Extension
                && file3Extension == file4Extension)
            {
                fileExtensionToUse = file1Extension;
            }
            else if (!_mergeMode
                     && file1Extension == file2Extension)
            {
                fileExtensionToUse = file1Extension;
            }
            else
            {
                LogMessage("ERROR: Files have different extensions. Cannot compare files with different file extensions.");
                fileExtensionToUse = string.Empty;
            }

            return fileExtensionToUse;
        }

        private bool IsGitKrakenFileExtension(string? fileExtension)
        {
            return fileExtension is ".SEQ-BASE" or ".SEQ-OURS" or ".SEQ-THEIRS";
        }

        // This method reads the file extension to application mapping file and returns the application path to launch.
        // If the file extension is not found in the mapping file, it returns the default Git client configured in 'defaultGitClient.json'.
        //
        // To add more file extensions and their corresponding applications, update the 'fileExtensionToApplicationMapping.json' file.
        private string GetMappedApplicationFromConfigurationFile(string fileExtension)
        {
            string? appPath = string.Empty;
            string JSONFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _fileExtensionToApplicationMappingFile);

            if (!File.Exists(JSONFilePath))
            {
                LogMessage($"File extension to application mapping file '{JSONFilePath}' not found.");
                LogMessage("Creating default extension to application mapping file for TestStand files.");

                CreateDefaultFileExtensionToApplicationMappingFile(JSONFilePath);
            }

            try
            {
                string JSONContent = File.ReadAllText(JSONFilePath);
                List<FileExtensionToApplicationMapping>? mappings = JsonSerializer.Deserialize<List<FileExtensionToApplicationMapping>>(JSONContent);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                FileExtensionToApplicationMapping? mapping = mappings?.FirstOrDefault(predicate: m => m.FileExtension.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (mapping != null)
                {
                    appPath = mapping.ApplicationPath;
                }
                else
                {
                    LogMessage($"No application mapped for file extension '{fileExtension}'.");
                    LogMessage("Using the configured default Git client in file 'defaultGitClient.json' as the diff and merge tool.");

                    appPath = GetGitClientToInvoke();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR: Failed to read file extension to application mapping file:\n{ex.Message}");
            }

            return appPath!;
        }

        private string GetGitClientToInvoke()
        {
            string? appPath = string.Empty;
            string JSONFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _defaultGitClientFile);

            if (File.Exists(JSONFilePath))
            {
                try
                {
                    string JSONContent = File.ReadAllText(JSONFilePath);
                    appPath = JsonSerializer.Deserialize<string>(JSONContent);
                }
                catch (Exception ex)
                {
                    LogMessage($"ERROR: Failed to read configured default Git client in file 'defaultGitClient.json':\n{ex.Message}");
                }
            }

            return appPath!;
        }

        private void CreateDefaultFileExtensionToApplicationMappingFile(string filePath)
        {
            List<FileExtensionToApplicationMapping> defaultMappings =
            [
                new()
                {
                    FileExtension = ".seq",
                    ApplicationPath = _TSFileDiffMergeLauncherPath
                },
                new()
                {
                    FileExtension = ".tpj",
                    ApplicationPath = _TSFileDiffMergeLauncherPath
                },
                new()
                {
                    FileExtension = ".tsw",
                    ApplicationPath = _TSFileDiffMergeLauncherPath
                }
            ];

            try
            {
                string JSONContent = JsonSerializer.Serialize(defaultMappings, JSONWriteOptions);
                File.WriteAllText(filePath, JSONContent);
                LogMessage($"Default file extension to application mapping file created at '{filePath}'.");
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR: Failed to creat default file extension to application mapping file:\n{ex.Message}");
            }
        }

        private string GetToolUsageMessage()
        {
            string usageMessage = $"FileLauncher.exe [Version: {_version}]";
            usageMessage += "Usage: FileLauncher.exe <file1> <file2> [<file3> <file4>]";
            usageMessage += "\n\nArguments:";
            usageMessage += "\n  file1: The first file to compare or the Base file when merging.";
            usageMessage += "\n  file2: The second file to compare or the Local file when merging.";
            usageMessage += "\n  file3: The remote file when merging.";
            usageMessage += "\n  file4: The final merged file.";
            usageMessage += "\n  Note: Arguments 'file3' and 'file4' are only required for Git merge operation.";

            return usageMessage;
        }

        private bool IsFileLocked(string filePath)
        {
            try
            {
                using FileStream stream = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        private void LogMessage(string message)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileLauncherLog.log");
            try
            {
                using StreamWriter writer = new(logFilePath, append: true);
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to create or write to log file:\n{ex.Message}");
            }
        }
    }

    internal sealed class FileExtensionToApplicationMapping
    {
        public string? FileExtension { get; set; }
        public string? ApplicationPath { get; set; }
    }
}