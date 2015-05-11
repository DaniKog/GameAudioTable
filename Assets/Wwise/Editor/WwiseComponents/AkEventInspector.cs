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
[CustomEditor(typeof(AkEvent))]
public class AkEventInspector : AkBaseInspector
{
	AkEvent m_akEvent;

	SerializedProperty eventID;
	SerializedProperty enableActionOnEvent;    
	SerializedProperty actionOnEventType;
	SerializedProperty curveInterpolation;
	SerializedProperty transitionDuration;

	string[]	m_supportedCallbackFlags;
	int[]		m_supportedCallbackValues;

	AkUnityEventHandlerInspector m_UnityEventHandlerInspector = new AkUnityEventHandlerInspector();

	public void OnEnable()
	{
		m_akEvent = target as AkEvent;
		
		m_UnityEventHandlerInspector.Init(serializedObject);
		
		eventID				= serializedObject.FindProperty("eventID");
		enableActionOnEvent	= serializedObject.FindProperty("enableActionOnEvent");
		actionOnEventType	= serializedObject.FindProperty("actionOnEventType");
		curveInterpolation	= serializedObject.FindProperty("curveInterpolation");
		transitionDuration	= serializedObject.FindProperty("transitionDuration");
		
		m_guidProperty		= new SerializedProperty[1];
		m_guidProperty[0]	= serializedObject.FindProperty("valueGuid.Array");
		
		//Needed by the base class to know which type of component its working with
		m_typeName		= "Event";
		m_objectType	= AkWwiseProjectData.WwiseObjectType.EVENT;


		//Build a list of all supported callback type names and values
		int[] callbacktypes				= (int[])Enum.GetValues (typeof(AkCallbackType));
		int[] unsupportedCallbackTypes	= (int[])Enum.GetValues (typeof(AkUnsupportedCallbackType));

		m_supportedCallbackFlags 	= new string[callbacktypes.Length - unsupportedCallbackTypes.Length];
		m_supportedCallbackValues 	= new int	[callbacktypes.Length - unsupportedCallbackTypes.Length];

		int index = 0;
		for(int i = 0; i < callbacktypes.Length; i++)
		{
			if(!Contain(unsupportedCallbackTypes, callbacktypes[i]))
			{
				m_supportedCallbackFlags[index] = Enum.GetName(typeof(AkCallbackType), callbacktypes[i]).Substring(3);
				m_supportedCallbackValues[index] = callbacktypes[i];
				index++;
			}
		}
	}

	bool Contain(int[] in_array, int in_value)
	{
		for(int i = 0; i < in_array.Length; i++)
		{
			if(in_array[i] == in_value) return true;
		}

		return false;
	}

	public override void OnChildInspectorGUI ()
	{	
		serializedObject.Update ();

		m_UnityEventHandlerInspector.OnGUI();
		
		GUILayout.Space(2);
		
		GUILayout.BeginVertical("Box");
		{
			EditorGUILayout.PropertyField(enableActionOnEvent, new GUIContent("Action On Event: "));
			
			if(enableActionOnEvent.boolValue)
			{
				EditorGUILayout.PropertyField(actionOnEventType, new GUIContent("Action On EventType: "));
				EditorGUILayout.PropertyField(curveInterpolation, new GUIContent("Curve Interpolation: "));
				EditorGUILayout.Slider(transitionDuration, 0.0f, 60.0f, new GUIContent("Fade Time (secs): "));
			}
			
		}
		GUILayout.EndVertical();

		GUILayout.Space (2);

		GUILayout.BeginVertical ("Box");
		{
			bool useCallback = m_akEvent.m_callbackData != null;
			useCallback = EditorGUILayout.Toggle ("Use Callback: ", useCallback);
			if(m_akEvent.m_callbackData == null && useCallback)
			{
				m_akEvent.m_callbackData = ScriptableObject.CreateInstance<AkEventCallbackData>();

				m_akEvent.m_callbackData.callbackFunc.Add(string.Empty);
				m_akEvent.m_callbackData.callbackFlags.Add(0);
				m_akEvent.m_callbackData.callbackGameObj.Add(null);
			}
			else if(!useCallback)
			{
				m_akEvent.m_callbackData = null;
			}

			if(m_akEvent.m_callbackData != null)
			{
				GUILayout.Space(3);

				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Game Object");
					GUILayout.Label("Callback Function");
					GUILayout.Label("Callback Flags");
				}
				GUILayout.EndHorizontal();

				m_akEvent.m_callbackData.uFlags = 0;

				for(int i = 0; i < m_akEvent.m_callbackData.callbackFunc.Count; i++)
				{
					GUILayout.BeginHorizontal();
					{
						m_akEvent.m_callbackData.callbackGameObj[i]	= (GameObject)EditorGUILayout.ObjectField(m_akEvent.m_callbackData.callbackGameObj[i], typeof(GameObject), true); 
						m_akEvent.m_callbackData.callbackFunc[i]	= EditorGUILayout.TextField(m_akEvent.m_callbackData.callbackFunc[i]);

						//Since some callback flags are unsupported, some bits are not used.
						//But when using EditorGUILayout.MaskField, clicking the third flag will set the third bit to one even if the third flag in the AkCallbackType enum is unsupported.
						//This is a problem because cliking the third supported flag would internally select the third flag in the AkCallbackType enum which is unsupported.
						//To slove this problem we use a mask for display and another one for the actual callback
						int displayMask = GetDisplayMask(m_akEvent.m_callbackData.callbackFlags[i]);
						displayMask	= EditorGUILayout.MaskField(displayMask, m_supportedCallbackFlags);
						m_akEvent.m_callbackData.callbackFlags[i] = GetWwiseCallbackMask(displayMask);

						 
						if(GUILayout.Button("X"))
						{
							if( m_akEvent.m_callbackData.callbackFunc.Count == 1)
							{
								m_akEvent.m_callbackData.callbackFunc[0]	= string.Empty;
								m_akEvent.m_callbackData.callbackFlags[0]	= 0;
								m_akEvent.m_callbackData.callbackGameObj[0]	= null;

								//Changes to the textfield string will not be picked up by the text editor if it is selected.
								//So we remove focus from the textfield to make sure it will get updated
								GUIUtility.keyboardControl = 0;
								GUIUtility.hotControl = 0;
							}
							else
							{
								m_akEvent.m_callbackData.callbackFunc.RemoveAt(i);
								m_akEvent.m_callbackData.callbackFlags.RemoveAt(i);
								m_akEvent.m_callbackData.callbackGameObj.RemoveAt(i);


								GUIUtility.keyboardControl = 0;
								GUIUtility.hotControl = 0;

								i--;
								continue;
							}
						}

						m_akEvent.m_callbackData.uFlags |= m_akEvent.m_callbackData.callbackFlags[i];
					}
					GUILayout.EndHorizontal();
				}

				GUILayout.Space(3);

				if(GUILayout.Button("Add"))
				{
					m_akEvent.m_callbackData.callbackFunc.Add(string.Empty);
					m_akEvent.m_callbackData.callbackFlags.Add(0);
					m_akEvent.m_callbackData.callbackGameObj.Add(null);
				}			

				GUILayout.Space(3);
			}
		}
		GUILayout.EndVertical ();

		serializedObject.ApplyModifiedProperties ();
	}

	public override string UpdateIds (System.Guid[] in_guid)
	{
		for(int i = 0; i < AkWwiseProjectInfo.GetData().EventWwu.Count; i++)
		{
			AkWwiseProjectData.Event e = AkWwiseProjectInfo.GetData().EventWwu[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[0]));
			
			if(e != null)
			{
				serializedObject.Update();
				eventID.intValue = e.ID;
				serializedObject.ApplyModifiedProperties();

				return e.Name;
			}
		}

		return string.Empty;
	}

	int GetDisplayMask(int in_wwiseCallbackMask)
	{
		int displayMask = 0;
		for(int i = 0; i < m_supportedCallbackValues.Length; i++)
		{
			if((m_supportedCallbackValues[i] & in_wwiseCallbackMask) != 0)
				displayMask |= (1 << i);
		}

		return displayMask;
	}

	int GetWwiseCallbackMask(int in_displayMask)
	{
		int wwiseCallbackMask = 0;
		for(int i = 0; i < m_supportedCallbackValues.Length; i++)
		{			
			if((in_displayMask & (1 << i)) != 0)
			{
				wwiseCallbackMask |= m_supportedCallbackValues[i];
			}
		}

		return wwiseCallbackMask;
	}
}

#endif