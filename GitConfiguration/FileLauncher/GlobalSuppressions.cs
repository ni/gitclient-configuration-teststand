// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Correctness", "LRT001: All types should be in the 'NationalInstruments' namespace.", Justification = "Reviewed", Scope = "type", Target = "~T:FileLauncher.Program")]
[assembly: SuppressMessage("Correctness", "LRT001: All types should be in the 'NationalInstruments' namespace.", Justification = "Reviewed", Scope = "type", Target = "~T:FileLauncher.FileExtensionToApplicationMapping")]

[assembly: SuppressMessage("Style", "NI1704: Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:FileLauncher.Program.Main(System.String[])")]
[assembly: SuppressMessage("Style", "NI1704: Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:FileLauncher.Program.ProcessArguments(System.String[])~System.String[]")]
[assembly: SuppressMessage("Style", "NI1704: Identifiers should be spelled correctly", Justification = "Reviewed", Scope = "member", Target = "~M:FileLauncher.Program.IsGitKrakenFileExtension(System.String)")]

[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:FileLauncher.Program.GetGitClientToInvoke~System.String")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:FileLauncher.Program.GetMappedApplicationFromConfigurationFile(System.String)~System.String")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Reviewed", Scope = "member", Target = "~M:FileLauncher.Program.CreateDefaultFileExtensionToApplicationMappingFile(System.String)")]
