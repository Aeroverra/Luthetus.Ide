﻿using Luthetus.Ide.ClassLib.MenuCase;
using Luthetus.Ide.ClassLib.NugetCase;
using Microsoft.Extensions.DependencyInjection;
using Fluxor;
using Luthetus.Common.RazorLib;
using Luthetus.TextEditor.RazorLib;
using Luthetus.CompilerServices.Lang.CSharp.CompilerServiceCase;
using Luthetus.CompilerServices.Lang.CSharpProject.CompilerServiceCase;
using Luthetus.CompilerServices.Lang.DotNetSolution.CompilerServiceCase;
using Luthetus.CompilerServices.Lang.FSharp;
using Luthetus.CompilerServices.Lang.JavaScript;
using Luthetus.CompilerServices.Lang.Json;
using Luthetus.CompilerServices.Lang.Razor.CompilerServiceCase;
using Luthetus.CompilerServices.Lang.TypeScript;
using Luthetus.CompilerServices.Lang.Xml;
using Luthetus.Ide.ClassLib.CommandCase;
using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;
using Microsoft.Extensions.Logging;
using Luthetus.Ide.RazorLib.FileSystemCase.FileTemplatesCase;
using Luthetus.Common.RazorLib.FileSystem.Classes.InMemoryFileSystem;
using Luthetus.Common.RazorLib.FileSystem.Classes.Local;
using Luthetus.Common.RazorLib.FileSystem.Interfaces;
using Luthetus.Common.RazorLib.Theme;
using Luthetus.Ide.ClassLib.ComponentRenderersCase;
using Luthetus.Ide.RazorLib.FormsGeneric;
using Luthetus.Ide.RazorLib.CSharpProjectForm;
using Luthetus.Ide.RazorLib.File;
using Luthetus.Ide.RazorLib.Git;
using Luthetus.Ide.RazorLib.InputFile;
using Luthetus.Ide.RazorLib.NuGet;
using Luthetus.Ide.RazorLib.TreeViewImplementations;
using Luthetus.CompilerServices.Lang.Css;

namespace Luthetus.Ide.RazorLib;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLuthetusIdeRazorLibServices(
        this IServiceCollection services,
        LuthetusHostingInformation hostingInformation,
        Func<LuthetusIdeOptions, LuthetusIdeOptions>? configure = null)
    {
        var ideOptions = new LuthetusIdeOptions();

        if (configure is not null)
            ideOptions = configure.Invoke(ideOptions);

        if (ideOptions.AddLuthetusTextEditor)
        {
            services.AddLuthetusTextEditor(hostingInformation, inTextEditorOptions => inTextEditorOptions with
            {
                CustomThemeRecords = LuthetusTextEditorCustomThemeFacts.AllCustomThemes,
                InitialThemeKey = ThemeFacts.VisualStudioDarkThemeClone.Key,
            });
        }

        return services
            .AddSingleton(ideOptions)
            .AddSingleton<ILuthetusIdeComponentRenderers>(_ideComponentRenderers)
            .AddLuthetusIdeFileSystem(hostingInformation, ideOptions)
            .AddLuthetusIdeClassLibServices(hostingInformation);
    }

    private static IServiceCollection AddLuthetusIdeFileSystem(
        this IServiceCollection services,
        LuthetusHostingInformation hostingInformation,
        LuthetusIdeOptions ideOptions)
    {
        Func<IServiceProvider, IEnvironmentProvider> environmentProviderFactory;
        Func<IServiceProvider, IFileSystemProvider> fileSystemProviderFactory;

        if (hostingInformation.LuthetusHostingKind == LuthetusHostingKind.Photino)
        {
            environmentProviderFactory = _ => new LocalEnvironmentProvider();
            fileSystemProviderFactory = _ => new LocalFileSystemProvider();
        }
        else
        {
            environmentProviderFactory = _ => new InMemoryEnvironmentProvider();

            fileSystemProviderFactory = serviceProvider => new InMemoryFileSystemProvider(
                serviceProvider.GetRequiredService<IEnvironmentProvider>());
        }

        return services
            .AddSingleton(environmentProviderFactory.Invoke)
            .AddSingleton(fileSystemProviderFactory.Invoke);
    }

    public static IServiceCollection AddLuthetusIdeClassLibServices(
        this IServiceCollection services,
        LuthetusHostingInformation hostingInformation)
    {
        hostingInformation.BackgroundTaskService.RegisterQueue(FileSystemBackgroundTaskWorker.Queue);

        services.AddSingleton(sp => new FileSystemBackgroundTaskWorker(
            FileSystemBackgroundTaskWorker.Queue.Key,
            sp.GetRequiredService<IBackgroundTaskService>(),
            sp.GetRequiredService<ILoggerFactory>()));

        hostingInformation.BackgroundTaskService.RegisterQueue(TerminalBackgroundTaskWorker.Queue);

        services.AddSingleton(sp => new TerminalBackgroundTaskWorker(
            TerminalBackgroundTaskWorker.Queue.Key,
            sp.GetRequiredService<IBackgroundTaskService>(),
            sp.GetRequiredService<ILoggerFactory>()));

        if (hostingInformation.LuthetusHostingKind == LuthetusHostingKind.ServerSide)
        {
            services.AddHostedService(sp => sp.GetRequiredService<FileSystemBackgroundTaskWorker>());
            services.AddHostedService(sp => sp.GetRequiredService<TerminalBackgroundTaskWorker>());
        }

        services
            .AddScoped<ICommandFactory, CommandFactory>()
            .AddScoped<XmlCompilerService>()
            .AddScoped<DotNetSolutionCompilerService>()
            .AddScoped<CSharpProjectCompilerService>()
            .AddScoped<CSharpCompilerService>()
            .AddScoped<RazorCompilerService>()
            .AddScoped<CssCompilerService>()
            .AddScoped<FSharpCompilerService>()
            .AddScoped<JavaScriptCompilerService>()
            .AddScoped<TypeScriptCompilerService>()
            .AddScoped<JsonCompilerService>()
            .AddScoped<IMenuOptionsFactory, MenuOptionsFactory>()
            .AddScoped<IFileTemplateProvider, FileTemplateProvider>()
            .AddScoped<INugetPackageManagerProvider, NugetPackageManagerProviderAzureSearchUsnc>();

        services
            .AddFluxor(options =>
                options.ScanAssemblies(
                    typeof(ServiceCollectionExtensions).Assembly,
                    typeof(LuthetusCommonOptions).Assembly,
                    typeof(LuthetusTextEditorOptions).Assembly));

        return services;
    }

    private static readonly LuthetusIdeTreeViews _ideTreeViews = new(
        typeof(TreeViewNamespacePathDisplay),
        typeof(TreeViewAbsolutePathDisplay),
        typeof(TreeViewGitFileDisplay),
        typeof(TreeViewCompilerServiceDisplay),
        typeof(TreeViewCSharpProjectDependenciesDisplay),
        typeof(TreeViewCSharpProjectNugetPackageReferencesDisplay),
        typeof(TreeViewCSharpProjectToProjectReferencesDisplay),
        typeof(TreeViewCSharpProjectNugetPackageReferenceDisplay),
        typeof(TreeViewCSharpProjectToProjectReferenceDisplay),
        typeof(TreeViewSolutionFolderDisplay));

    private static readonly LuthetusIdeComponentRenderers _ideComponentRenderers = new(
        typeof(BooleanPromptOrCancelDisplay),
        typeof(FileFormDisplay),
        typeof(DeleteFileFormDisplay),
        typeof(NuGetPackageManager),
        typeof(GitChangesDisplay),
        typeof(RemoveCSharpProjectFromSolutionDisplay),
        typeof(InputFileDisplay),
        _ideTreeViews);
}