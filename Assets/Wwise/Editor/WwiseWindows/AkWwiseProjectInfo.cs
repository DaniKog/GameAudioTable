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
using System.Collections.Generic;

public static class AkWwiseProjectInfo
{
    public static AkWwiseProjectData m_Data;

    public static AkWwiseProjectData GetData()
    {
        if (m_Data == null && Directory.Exists(Path.Combine(Application.dataPath, "Wwise")))
        {
			try
			{
				m_Data = (AkWwiseProjectData)AssetDatabase.LoadAssetAtPath("Assets/Wwise/Editor/ProjectData/AkWwiseProjectData.asset", typeof(AkWwiseProjectData));

				// Create the asset only when not running in batch mode.
				string[] arguments = Environment.GetCommandLineArgs();
				if (m_Data == null && Array.IndexOf(arguments, "-batchmode") == -1)
				{
					m_Data = ScriptableObject.CreateInstance<AkWwiseProjectData>();
					string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/Wwise/Editor/ProjectData/AkWwiseProjectData.asset");
					AssetDatabase.CreateAsset(m_Data, assetPathAndName);
				}
			}
			catch( Exception e )
			{
				Debug.Log("Unable to load Wwise Data: " + e.ToString());
			}
        }
		
        return m_Data;
    }


    public static void Populate()
    {
        AkWwiseWWUBuilder.Populate();

        AkWwiseXMLBuilder.Populate();
        
        if( AkWwisePicker.WwiseProjectFound )
        {
			EditorUtility.SetDirty(AkWwiseProjectInfo.GetData ());
		}
    }
}
#endif