#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CanEditMultipleObjects]
[CustomEditor(typeof(AkBank))]
public class AkBankInspector : AkBaseInspector
{
	SerializedProperty bankName;
	SerializedProperty loadAsync;

	AkUnityEventHandlerInspector m_LoadBankEventHandlerInspector = new AkUnityEventHandlerInspector();
	AkUnityEventHandlerInspector m_UnloadBankEventHandlerInspector = new AkUnityEventHandlerInspector();
	
	void OnEnable()
	{
		m_LoadBankEventHandlerInspector.Init (serializedObject, "triggerList", "Load On: ", false);
		m_UnloadBankEventHandlerInspector.Init (serializedObject, "unloadTriggerList", "Unload On: ", false);

		bankName	= serializedObject.FindProperty("bankName");
		loadAsync	= serializedObject.FindProperty("loadAsynchronous");
		
		m_guidProperty		= new SerializedProperty[1];
		m_guidProperty[0]	= serializedObject.FindProperty("valueGuid.Array");

		//Needed by the base class to know which type of component its working with
		m_typeName		= "Bank";
		m_objectType	= AkWwiseProjectData.WwiseObjectType.SOUNDBANK;
	}
	
	public override void OnChildInspectorGUI ()
	{				
		serializedObject.Update ();

		m_LoadBankEventHandlerInspector.OnGUI();
		m_UnloadBankEventHandlerInspector.OnGUI ();
		
		GUILayout.Space(5);

		GUILayout.BeginVertical("Box");
		{
			EditorGUILayout.PropertyField(loadAsync, new GUIContent("Asynchronous:"));
		}
		GUILayout.EndVertical ();

		serializedObject.ApplyModifiedProperties ();
	}
	
	public override string UpdateIds (System.Guid[] in_guid)
	{
		for(int i = 0; i < AkWwiseProjectInfo.GetData().BankWwu.Count; i++)
		{
			AkWwiseProjectData.AkInformation bank = AkWwiseProjectInfo.GetData().BankWwu[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[0]));
			
			if(bank != null)
			{
				serializedObject.Update();
				bankName.stringValue = bank.Name;
				serializedObject.ApplyModifiedProperties();
  
				return bank.Name;
			}
		}

		return string.Empty;
	}
}
#endif