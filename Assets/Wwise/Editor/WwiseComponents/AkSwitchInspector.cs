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

[CanEditMultipleObjects]
[CustomEditor(typeof(AkSwitch))]
public class AkSwitchInspector : AkBaseInspector
{
    SerializedProperty m_groupGuid;
    SerializedProperty m_valueGuid;
	SerializedProperty m_groupID;
	SerializedProperty m_valueID;

    AkUnityEventHandlerInspector m_UnityEventHandlerInspector = new AkUnityEventHandlerInspector();

    void OnEnable()
    {
        m_UnityEventHandlerInspector.Init(serializedObject);

		m_groupID = serializedObject.FindProperty ("groupID");
		m_valueID = serializedObject.FindProperty ("valueID");

		m_guidProperty = new SerializedProperty[2];
		m_guidProperty[0] = serializedObject.FindProperty ("valueGuid.Array");
		m_guidProperty[1] = serializedObject.FindProperty ("groupGuid.Array");
		
		//Needed by the base class to know which type of component its working with
		m_typeName		= "Switch";
		m_objectType	= AkWwiseProjectData.WwiseObjectType.SWITCH;
    }

	public override void OnInspectorGUI()
	{
		if(Event.current.type == EventType.DragExited && m_isInDropArea && DragAndDrop.paths.Length >= 4 && DragAndDrop.paths[3].Equals(m_typeName))
		{
			AkUtilities.SetByteArrayProperty(m_guidProperty[1], new System.Guid(DragAndDrop.paths[4]).ToByteArray());
		}
		
		base.OnInspectorGUI ();
	}

	public override void OnChildInspectorGUI ()
	{			
		serializedObject.Update ();

		m_UnityEventHandlerInspector.OnGUI();

		serializedObject.ApplyModifiedProperties ();
	}
	
	public override string UpdateIds (System.Guid[] in_guid)
	{
		string switchName = String.Empty;
		for(int i = 0; i < AkWwiseProjectInfo.GetData().SwitchWwu.Count; i++)
		{
			AkWwiseProjectData.GroupValue switchGroup = AkWwiseProjectInfo.GetData().SwitchWwu[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[1]));
			
			if(switchGroup != null)
			{
				serializedObject.Update();

				switchName = switchGroup.Name + "/";
				m_groupID.intValue = switchGroup.ID;
				
				int index = switchGroup.ValueGuids.FindIndex(x => new System.Guid(x.bytes).Equals(in_guid[0]));
				m_valueID.intValue = switchGroup.valueIDs[index];

				serializedObject.ApplyModifiedProperties();

				return switchName + switchGroup.values[index];

			}
		}

		return string.Empty;
	}
}
#endif