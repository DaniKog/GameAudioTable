#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using System.IO;

public class WwiseSetupWindow : EditorWindow 
{
    protected static GUIStyle WelcomeStyle = null;
    protected static GUIStyle HelpStyle = null;
    protected static GUIStyle VersionStyle = null;
	
	void SetTextColor(GUIStyle style, Color color)
    {
        style.active.textColor = color;
        style.focused.textColor = color;
        style.hover.textColor = color;
        style.normal.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
        style.onHover.textColor = color;
        style.onNormal.textColor = color;
    }

	// Initialize our required styles
    protected void InitGuiStyles()
    {
        WelcomeStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
        WelcomeStyle.fontSize = 20;
        WelcomeStyle.alignment = TextAnchor.MiddleCenter;
		if( !Application.HasProLicense() )
		{
			SetTextColor(WelcomeStyle, Color.black);
		}
		
		VersionStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
		if( !Application.HasProLicense() )
		{
			SetTextColor(VersionStyle, Color.black);
		}
		
		HelpStyle = GUI.skin.GetStyle("box");
		HelpStyle.wordWrap = true;
		HelpStyle.alignment = TextAnchor.UpperLeft;
		HelpStyle.normal.textColor = EditorStyles.textField.normal.textColor;	
		
    }
	
	public void DrawSettingsPart() 
	{
        string description;
        string tooltip;

        GUILayout.Label("Wwise Project", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal("box");
        description = "Wwise Project Path* (relative to Assets folder):";
        tooltip = "Location of the Wwise project associated with this game. It is recommended to put it in the Unity Project root folder. This path is relative to the game's Assets folder";
        GUILayout.Label(new GUIContent(description, tooltip), GUILayout.Width(330));
		EditorGUILayout.SelectableLabel(WwiseSetupWizard.Settings.WwiseProjectPath, "textfield", GUILayout.Height (17));
		if(GUILayout.Button("...", GUILayout.Width(30)))
		{
            string OpenInPath = Path.GetDirectoryName(AkUtilities.GetFullPath(Application.dataPath, WwiseSetupWizard.Settings.WwiseProjectPath));
            string WwiseProjectPathNew = EditorUtility.OpenFilePanel("Select your Wwise Project", OpenInPath, "wproj");
			if( WwiseProjectPathNew.Length != 0 )
			{
				if( WwiseProjectPathNew.EndsWith(".wproj") == false )
				{
					EditorUtility.DisplayDialog("Error", "Please select a valid .wproj file", "Ok");
				}
				else
				{
                    // No need to check if the file exists (the FilePanel does it for us).

					// MONO BUG: https://github.com/mono/mono/pull/471
					// In the editor, Application.dataPath returns <Project Folder>/Assets. There is a bug in
					// mono for method Uri.GetRelativeUri where if the path ends in a folder, it will
					// ignore the last part of the path. Thus, we need to add fake depth to get the "real"
                    // relative path.
                    WwiseSetupWizard.Settings.WwiseProjectPath = AkUtilities.MakeRelativePath(Application.dataPath + "/fake_depth", WwiseProjectPathNew);
				}
			}
			Repaint();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.Label("Asset Management", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        description = "SoundBank Path* (relative to StreamingAssets folder):";
        tooltip = "Location of the SoundBanks are for the game. This has to reside within the StreamingAssets folder.";
		GUILayout.Label(new GUIContent(description, tooltip), GUILayout.Width(330));
		EditorGUILayout.SelectableLabel(WwiseSetupWizard.Settings.SoundbankPath, "textfield", GUILayout.Height(17));
		if(GUILayout.Button("...", GUILayout.Width(30)))
		{
            string OpenInPath = Path.GetDirectoryName(AkUtilities.GetFullPath(Application.streamingAssetsPath, WwiseSetupWizard.Settings.SoundbankPath));
            string SoundbankPathNew = EditorUtility.OpenFolderPanel("Select your Soundbank destination folder", OpenInPath, "");
			if( SoundbankPathNew.Length != 0 )
			{
				int 		stremingAssetsIndex = Application.dataPath.Split('/').Length;
				string[] 	folders 			= SoundbankPathNew.Split('/');

				if(folders.Length - 1 < stremingAssetsIndex || !String.Equals(folders[stremingAssetsIndex], "StreamingAssets", StringComparison.OrdinalIgnoreCase))
				{
					EditorUtility.DisplayDialog("Error", "The soundbank destination folder must be located within the Unity project 'StreamingAssets' folder.", "Ok");
				}
				else
				{
					// MONO BUG: https://github.com/mono/mono/pull/471
					// Need to add fake depth to the streaming assets path because of this bug. Directories should end in /.
                    WwiseSetupWizard.Settings.SoundbankPath = AkUtilities.MakeRelativePath(Application.streamingAssetsPath + "/fake_depth", SoundbankPathNew) + "/";
				}
			}
			Repaint();
		}
        GUILayout.EndHorizontal();
        description = "Create WwiseGlobal GameObject";
        tooltip = "The WwiseGlobal object is a GameObject that contains the Initializing and Terminating scripts for the Wwise Sound Engine. In the Editor workflow, it is added to every scene, so that it can be properly be previewed in the Editor. In the game, only one instance is created, in the first scene, and it is persisted throughout the game. It is recommended to leave this box checked.";
        WwiseSetupWizard.Settings.CreateWwiseGlobal = GUILayout.Toggle(WwiseSetupWizard.Settings.CreateWwiseGlobal, new GUIContent(description, tooltip));

        description = "Add Ak Audio Listener to Main Camera";
        tooltip = "In order for positioning to work, the Ak Audio Listener script needs to be attached to the main camera in every scene. If you wish for your listener to be attached to another GameObject, uncheck this box";
        WwiseSetupWizard.Settings.CreateWwiseListener = GUILayout.Toggle(WwiseSetupWizard.Settings.CreateWwiseListener, new GUIContent(description, tooltip));

        GUILayout.EndVertical();

		
		GUILayout.BeginHorizontal();
		GUILayout.Label("* Mandatory settings");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.EndHorizontal();

	}
}

#endif // UNITY_EDITOR