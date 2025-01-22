# FileLauncher and ConfigurationModifier
This repository contains two .NET 8 applications: `FileLauncher` and `ConfigurationModifier`. These tools are designed to assist with file comparison, merging, and configuration management.


## ConfigurationModifier
ConfigurationModifier is a .NET 8 application designed to modify configuration files for various clients. It supports creating default configuration files and configuring Git external tools.

### Features
- Create default configuration files.
- Configure Git external tools for various clients.
- Logs configuration activities to a log file.

### Requirements
- .NET 8 SDK

### Usage
#### Command Line Arguments
ConfigurationModifier.exe <config_file>

Where <config_file> is the JSON file containing the configuration details for the Git client.
<br> e.g. SourceTree_Config.json

#### Example
To configure a Git external tool:
ConfigurationModifier.exe config.json

**Note**: If this file is not present, the tool will create one with the latest version of the Git client available in the system.


### Logging
The application logs its activities to `ConfigurationModifierLog.log` in the application's directory.



## FileLauncher
FileLauncher is a .NET 8 application designed to launch external tools for comparing and merging files. It supports custom file extension to application mappings and handles locked files by creating temporary copies.

### Features
- Compare and merge files using external tools.
- Customizable file extension to application mappings.
- Handles locked files by creating temporary copies.
- Logs application activities to a log file.

### Requirements
- .NET 8 SDK

### Usage
#### Command Line Arguments
FileLauncher.exe  \<file1> \<file2> [\<file3> \<file4>]
<br>where
- `file1`: The first file to compare or the Base file when merging.
- `file2`: The second file to compare or the Local file when merging.
- `file3`: The remote file when merging (optional).
- `file4`: The final merged file.

**Note**: Arguments 'file3' and 'file4' are only required for Git merge operation.

#### Example
To compare two files:
FileLauncher.exe file1.ext file2.ext

To perform four-way merge:
FileLauncher.exe base.ext local.ext remote.ext merged.ext


### Configuration
#### File Extension to Application Mapping
The application uses a JSON file (`fileExtensionToApplicationMapping.json`) to map file extensions to external applications. If the file does not exist, a default mapping file is created.


### Logging
The application logs its activities to `FileLauncherLog.log` in the application's directory.