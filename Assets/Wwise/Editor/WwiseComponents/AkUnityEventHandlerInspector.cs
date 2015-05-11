#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class AkUnityEventHandlerInspector
{
	SerializedProperty	m_triggerList;
	SerializedProperty	m_useOtherObject;
	static string[] 			m_triggerTypeNames;
	static uint[] 		    	m_triggerTypeIDs;
    static Dictionary<uint, string> m_triggerTypes;

	private string m_label = "Trigger On: ";

	private bool m_showUseOtherToggle = true;

	///Defines the triggers that make use of useOtherObjectMask
	static readonly string[] useOtherObjectTriggers = {"AkTriggerEnter", "AkTriggerExit", "AkTriggerCollisionEnter", "AkTriggerCollisionExit"};

    public void Init(SerializedObject in_serializedObject, string in_listName = "triggerList", string in_label = "Trigger On: ", bool in_showUseOtherToggle = true)
    {
		m_label = in_label;
		m_showUseOtherToggle = in_showUseOtherToggle;

        m_triggerList       = in_serializedObject.FindProperty(in_listName);
		m_useOtherObject	= in_serializedObject.FindProperty("useOtherObject");

		//Get the updated list of all triggers
        if (m_triggerTypes == null)
        {
            m_triggerTypes = AkTriggerBase.GetAllDerivedTypes();
            m_triggerTypeNames = new string[m_triggerTypes.Count];
            m_triggerTypes.Values.CopyTo(m_triggerTypeNames, 0);
            m_triggerTypeIDs = new uint[m_triggerTypes.Count];
            m_triggerTypes.Keys.CopyTo(m_triggerTypeIDs, 0);
        }

        //apply the modifications made to the mask property
		in_serializedObject.ApplyModifiedProperties ();
    }

    public void OnGUI()
    {
        GUILayout.Space(3);

        GUILayout.BeginVertical("Box");
		{
            List<uint> currentTriggers = GetCurrentTriggers();
            int oldMask = BuildCurrentMaskValue(currentTriggers);

            int newMask = EditorGUILayout.MaskField(m_label, oldMask, m_triggerTypeNames);

            if (oldMask != newMask)
            {
                currentTriggers.Clear();
                for (int i = 0; i < m_triggerTypeNames.Length; i++)
                {
                    uint curTriggerID = AkUtilities.ShortIDGenerator.Compute(m_triggerTypeNames[i]);
                    if ((newMask & (1 << i)) != 0 && !currentTriggers.Contains(curTriggerID))
                    {
                        currentTriggers.Add(curTriggerID);
                    }
                }
                SaveNewTriggers(currentTriggers);
            }

			if(m_showUseOtherToggle)
			{
				bool toggleWasDisplayed = false;

				for(int i = 0; i < m_triggerTypeNames.Length; i++)
				{
                    if ((newMask & (1 << i)) != 0 && Contain(useOtherObjectTriggers, m_triggerTypeNames[i]))
					{
						EditorGUILayout.PropertyField(m_useOtherObject, new GUIContent("Use Other Object: "));
						toggleWasDisplayed = true;
						break;
					}
				}

				if(!toggleWasDisplayed) m_useOtherObject.boolValue = false;
			}

		}
        GUILayout.EndVertical();

        GUILayout.Space(3);
    }

    List<uint> GetCurrentTriggers()
    {
        List<uint> newList = new List<uint>();
        for (int i = 0; i < m_triggerList.arraySize; i++)
        {
            newList.Add((uint)m_triggerList.GetArrayElementAtIndex(i).intValue);
        }

        return newList;
    }

    int GetIdIndex(uint in_ID)
    {
        int index = -1;
        for (int i = 0; i < m_triggerTypeIDs.Length; i++)
        {
            if (m_triggerTypeIDs[i] == in_ID)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    int BuildCurrentMaskValue(List<uint> currentTriggers)
    {
        int maskToReturn = 0;
        for (int i = 0; i < currentTriggers.Count; i++)
        {
            int idIndex = GetIdIndex(currentTriggers[i]);
            if (idIndex != -1)
            {
                maskToReturn |= (1 << idIndex);
            }
        }

        return maskToReturn;
    }

    void SaveNewTriggers(List<uint> currentTriggers)
    {
        m_triggerList.ClearArray();
        for (int i = 0; i < currentTriggers.Count; i++)
        {
            m_triggerList.InsertArrayElementAtIndex(i);
            m_triggerList.GetArrayElementAtIndex(i).intValue = (int)currentTriggers[i];
        }
    }

    bool Contain(string[] in_array, string in_value)
	{
		for(int i = 0; i < in_array.Length; i++)
		{
			if(in_array[i].Equals(in_value)) return true;
		}
		
		return false;
	}
}
#endif