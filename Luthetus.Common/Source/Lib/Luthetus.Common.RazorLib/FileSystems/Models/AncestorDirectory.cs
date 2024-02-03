﻿namespace Luthetus.Common.RazorLib.FileSystems.Models;

public class AncestorDirectory
{
    public AncestorDirectory(
        string nameNoExtension,
        string path,
        IEnvironmentProvider environmentProvider)
    {
        NameNoExtension = nameNoExtension;
        Path = path;
        EnvironmentProvider = environmentProvider;
    }

    public string NameNoExtension { get; }
    public string Path { get; }
    public IEnvironmentProvider EnvironmentProvider { get; }

    public string NameWithExtension => NameNoExtension + EnvironmentProvider.DirectorySeparatorChar;
}