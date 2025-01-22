// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.Program")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.GitClient")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.GitCola")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.GitColaClient")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.GitKraken")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.GitKrakenClient")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.SourceTree")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.SourceTreeClient")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.VisualStudio")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.VSInstance")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.Catalog")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.VisualStudioClient")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.Utilities")]
[assembly: SuppressMessage("Correctness", "LRT001:There is only one restricted namespace", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.IClientConfiguration")]

[assembly: SuppressMessage("Style", "NI1001:Private, mutable fields must begin with underscore and be camel-cased", Justification = "Reviewed", Scope = "member", Target = "~F:ConfigurationModifier.VisualStudio._VSWherePath")]

[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.Program.Main(System.String[])")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.Program.ProcessArguments(System.String[])~System.Boolean")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitCola.ConfigureRepoConfig(System.String[])~System.Boolean")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitCola.CreateDefaultGitColaConfigFile(System.String)~System.Boolean")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitCola.ConfigureGitColaGlobal~System.Boolean")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitCola.FindGlobalConfigFileForGitCola~System.String")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.VisualStudio.ConfigureRepoConfig(System.String[])~System.Boolean")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.GitKrakenClient")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitKraken.GetLatestGitKrakenInstalled~ConfigurationModifier.GitClient")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitKraken.ConfigureGitKraken(System.String)~System.Boolean")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitKraken.ConfigureGitKrakenGlobal~System.Boolean")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitKraken.CreateDefaultGitKrakenConfigurationFile(System.String)~System.Boolean")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitKraken.FindGlobalConfigurationFileForGitKraken~System.String")]
[assembly: SuppressMessage("Style", "NI1704:Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "type", Target = "~T:ConfigurationModifier.GitKraken")]

[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:Field names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~F:ConfigurationModifier.VisualStudio._VSWherePath")]

[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitCola.ConfigureGitCola(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.Utilities.UpdateDefaultGitClient(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.VisualStudio.UpdateDefaultClientAsVS(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.VisualStudio.ConfigureVisualStudio(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.VisualStudio.CreateDefaultVisualStudioConfigurationFile(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.SourceTree.ConfigureSourceTree(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.SourceTree.CreateDefaultSourceTreeConfigurationFile(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitKraken.ConfigureGitKraken(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitKraken.CreateDefaultGitKrakenConfigurationFile(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitCola.CreateDefaultGitColaConfigFile(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.SourceTree.UpdateDefaultClientAsSourceTree(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitCola.UpdateDefaultClientAsGitCola(System.String)~System.Boolean")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitKraken.UpdateDefaultClientAsGitKraken(System.String)~System.Boolean")]

[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitKraken.GetLatestGitKrakenInstalled~ConfigurationModifier.GitClient")]
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.VisualStudio.GetVsWherePath~System.String")]
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.SourceTree.GetLatestSourceTreeInstalled~ConfigurationModifier.GitClient")]
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Reviewed", Scope = "member", Target = "~M:ConfigurationModifier.GitCola.GetLatestGitColaInstalled~ConfigurationModifier.GitClient")]

[assembly: SuppressMessage("Performance", "CA1812: Avoid uninstantiated internal classes", Justification = "ConfigurationModifier.VSInstance, ConfigurationModifier.Catalog, and ConfigurationModifier.VisualStudioClient are instantiated via deserialization.")]
