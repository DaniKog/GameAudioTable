#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


public class Postprocessor : AssetPostprocessor
{	
	//This function will be called before script compilation and will save the picker's expantion 
	static void OnPostprocessAllAssets(	string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)		
	{
		AkWwisePicker.treeView.SaveExpansionStatus ();	
	}	
}


class AkWwisePicker : EditorWindow
{
	public static bool WwiseProjectFound = true;
	
    [MenuItem("Window/Wwise Picker", false, (int)AkWwiseWindowOrder.WwisePicker)] 
    public static void init()
    {
		EditorWindow.GetWindow<AkWwisePicker>("Wwise Picker", true, typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow"));
        PopulateTreeview();   
 	} 

    public static AkWwiseTreeView treeView = new AkWwiseTreeView(); 

	void OnEnable()
	{
		if( string.IsNullOrEmpty(WwiseSettings.LoadSettings().WwiseProjectPath) )
		{
			return;
		}
		
		string[] dir = {"Events", "States", "Switches", "Master-Mixer Hierarchy", "SoundBanks"};
		string wwiseProjectPath = Path.GetDirectoryName(AkUtilities.GetFullPath(Application.dataPath, WwiseSettings.LoadSettings().WwiseProjectPath));
		
		try 
		{
			for(int i = 0; i < dir.Length; i++)
			{
				DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(wwiseProjectPath, dir[i]));
				FileInfo[] files = dirInfo.GetFiles("*.wwu", SearchOption.AllDirectories);
					
				ArrayList list = AkWwiseProjectInfo.GetData().GetWwuListByString(dir[i]);
				List<string> pathList = new List<string>(list.Count);
				for(int j = 0; j < list.Count; j++)
				{
					pathList.Add(Path.Combine(wwiseProjectPath, (list[j] as AkWwiseProjectData.WorkUnit).PhysicalPath));
				}

				foreach(FileInfo file in files)
				{
					if(file.LastWriteTime.CompareTo(AkWwiseProjectInfo.GetData().GetLastPopulateTime()) > 0)
						AkWwiseWWUBuilder.s_createdWwu.Add(file.FullName);

					pathList.Remove(file.FullName);
				}

				AkWwiseWWUBuilder.s_deletedWwu.AddRange(pathList);
			}
			if(AkWwiseWWUBuilder.s_createdWwu.Count != 0 || AkWwiseWWUBuilder.s_deletedWwu.Count != 0)
			{
				treeView.SaveExpansionStatus();
				AkWwiseWWUBuilder.AutoPopulate();
				PopulateTreeview();
			}

		}
		catch(Exception)
		{
			WwiseProjectFound = false;
		}
	}

    public void OnGUI()
    {
        GUILayout.BeginHorizontal("Box");
       
        AkWwiseProjectInfo.GetData().autoPopulateEnabled = GUILayout.Toggle(AkWwiseProjectInfo.GetData().autoPopulateEnabled, "Auto populate");

		if (AkWwiseProjectInfo.GetData().autoPopulateEnabled && WwiseProjectFound)
        {
            AkWwiseWWUWatcher.GetInstance().StartWWUWatcher();
        }
        else
        {
            AkWwiseWWUWatcher.GetInstance().StopWWUWatcher();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Refresh Project", GUILayout.Width(200)))
        {
			treeView.SaveExpansionStatus();
            AkWwiseProjectInfo.Populate();   
            PopulateTreeview(); 
        }
        
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        treeView.DisplayTreeView(TreeViewControl.DisplayTypes.USE_SCROLL_VIEW);

        if (GUI.changed && WwiseProjectFound)
        {
        	EditorUtility.SetDirty(AkWwiseProjectInfo.GetData());             
        }
        // TODO: RTP Parameters List
    }

    //////////////////////////////////////////////////////////

    public static void PopulateTreeview()
    {	
        treeView.AssignDefaults(); 
        treeView.SetRootItem(System.IO.Path.GetFileNameWithoutExtension(WwiseSetupWizard.Settings.WwiseProjectPath), AkWwiseProjectData.WwiseObjectType.PROJECT);
		treeView.PopulateItem(treeView.RootItem, "Events", AkWwiseProjectInfo.GetData().EventWwu); 
        treeView.PopulateItem(treeView.RootItem, "Switches", AkWwiseProjectInfo.GetData().SwitchWwu);
        treeView.PopulateItem(treeView.RootItem, "States", AkWwiseProjectInfo.GetData().StateWwu);
		treeView.PopulateItem(treeView.RootItem, "SoundBanks", AkWwiseProjectInfo.GetData().BankWwu);
        treeView.PopulateItem(treeView.RootItem, "Auxiliary Busses", AkWwiseProjectInfo.GetData().AuxBusWwu);

    }

}
#endif