// Copyright (C) 2022 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jads.Tools;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Window to select how to rename assets.
/// </summary>
public class RenameAssetsWindow : EditorWindow
{
    private readonly IEnumerable<string> ignorePrefixFilter = new string[]
    {
        ".git",
        ".vs",
        "Logs",
        "UserSettings",
        "Builds",
    };

    private readonly IEnumerable<string> includeSuffixFilter = new string[]
    {
        ".asmdef",
        ".cs",
        ".csproj",
        ".env",
        ".json",
        ".txt",
        ".md",
        ".sh",
        ".yml",
    };

    private const string sourceCompanyName = "nickmaltbie";
    private const string sourceProjectName = "Template Unity Package";

    private bool regenerateGUIDs = true;
    private string destCompanyName = "nickmaltbie";
    private string destProjectName = "Template Unity Package";

    [MenuItem("Tools/Rename Template Assets")]
    public static void ShowMyEditor()
    {
        // This method is called when the user selects the menu item in the Editor
        EditorWindow wnd = GetWindow<RenameAssetsWindow>();
        wnd.titleContent = new GUIContent("Rename Template Assets");
    }

    public void OnGUI()
    {
        PromptField("Project", sourceProjectName, ref destProjectName);
        PromptField("Company", sourceCompanyName, ref destCompanyName);
        regenerateGUIDs = EditorGUILayout.Toggle($"Regenerate Asset GUIDs: ", regenerateGUIDs);

        if (GUILayout.Button("Rename Assets"))
        {
            OnClickRenameAssets();
            GUIUtility.ExitGUI();
        }
    }

    public void PromptField(string name, string source, ref string dest)
    {
        EditorGUILayout.LabelField($"Current {name} Name: {source}");
        dest = EditorGUILayout.TextField($"Rename {name} to", dest);

        EditorGUILayout.Space();
    }

    public static string NoSpaces(string str) => str.Replace(" ", "");
    public static string Lower(string str) => str.ToLower();
    public static string Identity(string str) => str;
    public static string LowerNoSpaces(string str) => Lower(NoSpaces(str));

    public static IEnumerable<Func<string, string>> transformFunctions = new Func<string, string>[]
    {
        Identity,
        NoSpaces,
        Lower,
        LowerNoSpaces
    };

    public static string ProjectPath => Directory.GetParent(Application.dataPath).FullName;

    public static string PackagesPath => Path.Combine(ProjectPath, "Packages");

    public void OnClickRenameAssets()
    {
        destCompanyName = destCompanyName.Trim();
        destProjectName = destProjectName.Trim();

        (string, string)[] companyNameTransforms = GetTransformed(sourceCompanyName, destCompanyName, transformFunctions).ToArray();
        (string, string)[] projectNameTransforms = GetTransformed(sourceCompanyName, destCompanyName, transformFunctions).ToArray();
        (string, string)[] renameTargets = companyNameTransforms.Union(projectNameTransforms).ToArray();

        // Rename package path
        string packagePath = Directory.EnumerateDirectories(PackagesPath).First(path =>
            companyNameTransforms.Any(pair => path.Contains(pair.Item1)) &&
            projectNameTransforms.Any(pair => path.Contains(pair.Item1)));

        // regenerate guids for project files
        if (regenerateGUIDs)
        {
            RegenerateGUIDS(new[]
                {
                    Path.GetRelativePath(ProjectPath, packagePath),
                    Path.GetRelativePath(ProjectPath, Application.dataPath)
                });
        }

        // Rename files that have project or company name in file name
        foreach (string path in Directory.EnumerateFiles(packagePath, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(ProjectPath, path);
            string targetPath = ApplyRenameTargets(relativePath, renameTargets);
            AssetDatabase.MoveAsset(relativePath, targetPath);
        }

        // Replace contents of files that contain company name or project name
        foreach (string path in Directory.EnumerateFiles(ProjectPath, "*", SearchOption.AllDirectories))
        {
            string relPath = Path.GetRelativePath(ProjectPath, path);
            string fileName = Path.GetFileName(path);
            if (ignorePrefixFilter.Any(ignore => relPath.StartsWith(ignore)))
            {
                continue;
            }

            if (!includeSuffixFilter.Any(end => fileName.EndsWith(end)))
            {
                continue;
            }

            ReplaceTextInFiles(path, renameTargets);
        }

        // Move package
        packagePath = MovePackageFolder(packagePath, renameTargets);
        Close();
    }

    public static IEnumerable<(string, string)> GetTransformed(string source, string dest, IEnumerable<Func<string, string>> fns)
    {
        return fns.Select(fn => (fn(source), fn(dest))).Distinct();
    }

    public static void RegenerateGUIDS(string[] paths)
    {
        // Regenerate GUIDs for project folders
        IEnumerable<string> fileGUIDs = AssetDatabase.FindAssets("*", paths);

        AssetDatabase.StartAssetEditing();
        AssetGUIDRegenerator.RegenerateGUIDs(fileGUIDs.ToArray(), false);
        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static string ApplyRenameTargets(string filePath, IEnumerable<(string, string)> renameTargets)
    {
        string targetPath = filePath;
        foreach ((string source, string dest) in renameTargets)
        {
            targetPath = Path.GetRelativePath(ProjectPath, GetRenamedLeaf(targetPath, source, dest));
        }

        return targetPath;
    }

    public static string MovePackageFolder(string packagePath, IEnumerable<(string, string)> renameTargets)
    {
        string targetPath = ApplyRenameTargets(packagePath, renameTargets);

        if (packagePath == targetPath)
        {
            return targetPath;
        }

        Directory.Move(packagePath, targetPath);

        AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        return targetPath;
    }

    public static string GetRenamedLeaf(string filePath, string source, string dest)
    {
        return Path.Combine(
            Directory.GetParent(filePath).FullName,
            Path.GetFileName(filePath).Replace(source, dest));
    }

    public static void ReplaceTextInFiles(string filePath, IEnumerable<(string, string)> replaceTargets)
    {
        string text = File.ReadAllText(filePath);
        foreach ((string source, string dest) in replaceTargets)
        {
            text = text.Replace(source, dest);
        }

        File.WriteAllText(filePath, text, System.Text.Encoding.UTF8);
    }
}
