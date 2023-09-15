﻿using Luthetus.Common.RazorLib.FileSystem.Models;
using Luthetus.Common.RazorLib.Namespaces.Models;

namespace Luthetus.Ide.RazorLib.FileSystemCase.Models;

public class FileTemplateParameter
{
    public FileTemplateParameter(
        string filename,
        NamespacePath parentDirectory,
        IEnvironmentProvider environmentProvider)
    {
        Filename = filename;
        ParentDirectory = parentDirectory;
        EnvironmentProvider = environmentProvider;
    }

    public string Filename { get; }
    public NamespacePath ParentDirectory { get; }
    public IEnvironmentProvider EnvironmentProvider { get; }
}