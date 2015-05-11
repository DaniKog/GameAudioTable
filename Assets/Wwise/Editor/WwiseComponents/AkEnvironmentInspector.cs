#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;

[CanEditMultipleObjects]
[CustomEditor(typeof(AkEnvironment))]
public class AkEnvironmentInspector : AkBaseInspector
{
    AkEnvironment m_AkEnvironment;

    SerializedProperty m_auxBusId;
	SerializedProperty m_priority;
	SerializedProperty m_isDefault;
	SerializedProperty m_excludeOthers;

    void OnEnable()
    {
        m_AkEnvironment = target as AkEnvironment;

		m_AkEnvironment.GetComponent<Rigidbody>().useGravity = false;
		m_AkEnvironment.GetComponent<Rigidbody>().isKinematic = true;
		m_AkEnvironment.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
		
		m_auxBusId		= serializedObject.FindProperty ("m_auxBusID");
		m_priority		= serializedObject.FindProperty ("priority");
		m_isDefault 	= serializedObject.FindProperty ("isDefault");
		m_excludeOthers = serializedObject.FindProperty ("excludeOthers");

		m_guidProperty		= new SerializedProperty[1];
		m_guidProperty[0]	= serializedObject.FindProperty("valueGuid.Array");
		
		//Needed by the base class to know which type of component its working with
		m_typeName		= "AuxBus";
		m_objectType	= AkWwiseProjectData.WwiseObjectType.AUXBUS;

		//We move and replace the game object to trigger the OnTriggerStay function
		ShakeEnvironment ();
    }

	public override void OnChildInspectorGUI ()
	{			
		serializedObject.Update ();

		EditorGUILayout.BeginVertical("Box");
		{
			m_priority.intValue = EditorGUILayout.IntField ("Priority: ", m_priority.intValue);

			GUILayout.Space(3);

			m_isDefault.boolValue = EditorGUILayout.Toggle ("Default: ", m_isDefault.boolValue);
			if(m_isDefault.boolValue)
				m_excludeOthers.boolValue = false;

			GUILayout.Space(3);
			
			m_excludeOthers.boolValue = EditorGUILayout.Toggle ("Exclude Others: ", m_excludeOthers.boolValue);
			if(m_excludeOthers.boolValue)
				m_isDefault.boolValue = false;
			
		}
		GUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties ();
	}
	
	public override string UpdateIds (System.Guid[] in_guid)
	{
		for(int i = 0; i < AkWwiseProjectInfo.GetData().AuxBusWwu.Count; i++)
		{
			AkWwiseProjectData.AkInformation akInfo = AkWwiseProjectInfo.GetData().AuxBusWwu[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[0]));
			
			if(akInfo != null)
			{
				serializedObject.Update();
				m_auxBusId.intValue = akInfo.ID;
				serializedObject.ApplyModifiedProperties();

				return akInfo.Name;
			}
		}

		return string.Empty;
	}

	public void ShakeEnvironment()
	{
		Vector3 temp = m_AkEnvironment.transform.position;
		temp.x *= 1.0000001f;
		m_AkEnvironment.transform.position = temp;
		
		EditorApplication.update += ReplaceEnvironment;
	}
	
	void ReplaceEnvironment()
	{
		Vector3 temp = m_AkEnvironment.transform.position;
		temp.x /= 1.0000001f;
		m_AkEnvironment.transform.position = temp;
		
		EditorApplication.update -= ReplaceEnvironment;
	}
}
#endif