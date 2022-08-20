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
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Window to select how to rename assets.
/// </summary>
public class RenameAssetsWindow : EditorWindow
{
    private readonly IEnumerable<string> ignoreFilter = new string[]
    {
        ".git",
    };

    private readonly IEnumerable<string> assetFolders = new string[]
    {
        "Assets",
        "Packages"
    };

    private readonly IEnumerable<string> fileEndings = new string[]
    {
        ".asmdef",
        ".cs",
        ".csproj",
        ".env",
        ".json",
        ".md",
        ".sh",
        ".yml",
    };

    private const string sourceCompanyName = "nickmaltbie";
    private const string sourceProjectName = "Template Unity Package";

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

    public void OnClickRenameAssets() {
        destCompanyName = destCompanyName.Trim();
        destProjectName = destProjectName.Trim();

        // Reserialize all assets before updating and moving them.
        AssetDatabase.ForceReserializeAssets();

        var renameTargets = new (string, string)[]
        {
            (sourceCompanyName, destCompanyName),
            (sourceProjectName, destProjectName),
        };

        // Rename package path
        string packagePath = Directory.EnumerateDirectories(PackagesPath).First(path =>
            MatchesTransform(path, sourceCompanyName, transformFunctions) &&
            MatchesTransform(path, sourceProjectName, transformFunctions));

        string targetPath = packagePath;
        foreach ((string source, string dest) in renameTargets)
        {
            targetPath = Path.GetRelativePath(ProjectPath, GetRenamedLeaf(targetPath, source, dest, transformFunctions));
        }

        Debug.Log(packagePath + " to " + targetPath);
        Directory.Move(packagePath, targetPath);

        AssetDatabase.ImportAsset(
            targetPath,
            ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);

        foreach (string path in Directory.EnumerateFiles(targetPath, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(ProjectPath, path);
            targetPath = relativePath;

            foreach ((string source, string dest) in renameTargets)
            {
                targetPath = Path.GetRelativePath(ProjectPath, GetRenamedLeaf(targetPath, source, dest, transformFunctions));
            }

            if (!Equals(relativePath, targetPath))
            {
                AssetDatabase.MoveAsset(relativePath, targetPath);
            }
        }

        Close();
    }

    public bool MatchesTransform(string filePath, string source, IEnumerable<Func<string, string>> transforms)
    {
        return transforms.FirstOrDefault(f => f(filePath).Contains(f(source))) != null;
    }

    public string GetRenamedLeaf(string filePath, string source, string dest, IEnumerable<Func<string, string>> transforms)
    {
        return Path.Combine(
            Directory.GetParent(filePath).FullName,
            GetRenamedPath(Path.GetFileName(filePath), source, dest, transforms));
    }

    public string GetRenamedPath(string filePath, string source, string dest, IEnumerable<Func<string, string>> transforms)
    {
        Func<string, string> f = transforms.FirstOrDefault(f => f(filePath).Contains(f(source)));
        return f == null ? filePath : f(filePath).Replace(f(source), f(dest));
    }
}
