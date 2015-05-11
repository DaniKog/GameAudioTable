#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

// The inspector for AkInitializer is overriden to trap changes to initialization parameters and persist them across scenes.
[CustomEditor(typeof(AkInitializer))]
public class AkInitializerInspector : Editor
{
    AkInitializer m_AkInit;
	
	//This data is a copy of the AkInitializer parameters.  
	//We need it to reapply the same values to copies of the object in different scenes
	SerializedProperty m_basePath;
    SerializedProperty m_language;
    SerializedProperty m_defaultPoolSize;
    SerializedProperty m_lowerPoolSize;
    SerializedProperty m_streamingPoolSize;
    SerializedProperty m_memoryCutoffThreshold;	

    void OnEnable()
    {
        m_AkInit = target as AkInitializer;   

		m_basePath = serializedObject.FindProperty("basePath");
		m_language = serializedObject.FindProperty("language"); 
		m_defaultPoolSize = serializedObject.FindProperty("defaultPoolSize");
		m_lowerPoolSize = serializedObject.FindProperty("lowerPoolSize");
		m_streamingPoolSize = serializedObject.FindProperty("streamingPoolSize");
		m_memoryCutoffThreshold = serializedObject.FindProperty("memoryCutoffThreshold");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(m_basePath, new GUIContent("Base Path"));		
		EditorGUILayout.PropertyField(m_language, new GUIContent("Language"));
		EditorGUILayout.PropertyField(m_defaultPoolSize, new GUIContent("Default Pool Size (KB)"));
		EditorGUILayout.PropertyField(m_lowerPoolSize, new GUIContent("Lower Pool Size (KB)"));
		EditorGUILayout.PropertyField(m_streamingPoolSize, new GUIContent("Streaming Pool Size (KB)"));
		EditorGUILayout.PropertyField(m_memoryCutoffThreshold, new GUIContent("Memory Cutoff Threshold"));
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {            
			AkWwiseProjectInfo.GetData().SaveInitSettings(m_AkInit);
        }
    }
}
#endif