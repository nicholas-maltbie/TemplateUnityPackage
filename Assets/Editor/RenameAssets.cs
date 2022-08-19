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
using UnityEditor;
using UnityEngine;

/// <summary>
/// Window to select how to rename assets.
/// </summary>
public class RenameAssetsWindow : EditorWindow
{
    private string sourceCompanyName = "nickmaltbie";
    private string sourceProjectName = "Template Unity Package";

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
        sourceProjectName = EditorGUILayout.TextField($"Source Project Name:", sourceProjectName);
        sourceCompanyName = EditorGUILayout.TextField($"Source Company Name:", sourceCompanyName);

        destProjectName = EditorGUILayout.TextField($"Rename Project to:", destProjectName);
        destCompanyName = EditorGUILayout.TextField($"Rename Company to:", destCompanyName);

        if (GUILayout.Button("Rename Assets")) {
            OnClickRenameAssets();
            GUIUtility.ExitGUI();
        }
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

    public void OnClickRenameAssets() {
        sourceCompanyName = sourceCompanyName.Trim();
        sourceProjectName = sourceProjectName.Trim();
        destCompanyName = destCompanyName.Trim();
        destProjectName = destProjectName.Trim();

        var renameTargets = new (string, string)[]
        {
            (sourceCompanyName, destCompanyName),
            (sourceProjectName, destProjectName),
        };

        foreach ((string source, string dest) in renameTargets)
        {
            foreach(Func<string, string> fn in transformFunctions)
            {
                RenameAssets(fn(source), fn(dest));
            }
        }

        foreach ((string source, string dest) in renameTargets)
        {
            foreach(Func<string, string> fn in transformFunctions)
            {
                RenameAssets(fn(source), fn(dest), true);
            }
        }

        Close();
    }

    public void RenameAssets(string source, string dest, bool ignoreCase = false)
    {

    }
}
