namespace ConfigurationModifier
{
    internal interface IClientConfiguration
    {
        bool CreateDefaultConfigurationFile(string filePath);
        bool ConfigureClient(string configurationFilePath);
    }
}