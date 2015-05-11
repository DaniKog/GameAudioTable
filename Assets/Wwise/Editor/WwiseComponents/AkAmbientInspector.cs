#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

public enum AttenuationSphereOptions
{
	Dont_Show,
	Current_Event_Only,
	All_Events
}

[CanEditMultipleObjects]
[CustomEditor(typeof(AkAmbient))]
public class AkAmbientInspector : AkEventInspector
{
	AkAmbient m_AkAmbient;
	SerializedProperty multiPositionType;
	bool hideDefaultHandle = false;
	int curPointIndex = -1;

	List<int> triggerList;
	
	public static bool populateSoundBank = true;
	public static Dictionary<int, AttenuationSphereOptions> attSphereProperties = new Dictionary<int, AttenuationSphereOptions>();
	public AttenuationSphereOptions currentAttSphereOp;
	
	new void OnEnable()
	{
		base.OnEnable ();

		m_AkAmbient = target as AkAmbient; 

		multiPositionType = serializedObject.FindProperty("multiPositionTypeLabel");
		DefaultHandles.Hidden = hideDefaultHandle;

		if(!attSphereProperties.ContainsKey(target.GetInstanceID()))
			attSphereProperties.Add(target.GetInstanceID(), AttenuationSphereOptions.Dont_Show);
	
		currentAttSphereOp = attSphereProperties [target.GetInstanceID ()];

		AkWwiseXMLWatcher.GetInstance ().StartXMLWatcher ();

		EditorApplication.update += PopulateMaxAttenuation;
	}
	
	void OnDisable()
	{
		DefaultHandles.Hidden = false;
		attSphereProperties [target.GetInstanceID ()] = currentAttSphereOp;
		EditorApplication.update -= PopulateMaxAttenuation; 
	}
	
	void DoMyWindow(int windowID)
	{
		GUILayout.Space(2);
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Add Point"))
		{
			m_AkAmbient.multiPositionArray.Add( m_AkAmbient.transform.InverseTransformPoint( m_AkAmbient.transform.position));
		}
		
		if (curPointIndex >= 0 && GUILayout.Button("Delete Point"))
		{
			m_AkAmbient.multiPositionArray.RemoveAt(curPointIndex);
			curPointIndex = m_AkAmbient.multiPositionArray.Count - 1;
		}
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space(5);
		
		if (hideDefaultHandle)
		{
			if (GUILayout.Button("Show Main Transform"))
			{
				hideDefaultHandle = false;
				DefaultHandles.Hidden = hideDefaultHandle;
			}
		}
		else
		{
			if (GUILayout.Button("Hide Main Transform"))
			{
				hideDefaultHandle = true;
				DefaultHandles.Hidden = hideDefaultHandle;
			}
		}
	}

	public override void OnChildInspectorGUI ()
	{
		//Save trigger mask to know when it chages
		triggerList = m_AkAmbient.triggerList;

		base.OnChildInspectorGUI ();

		GUILayout.Space (3);

		serializedObject.Update ();

		GUILayout.BeginVertical("Box");
		
		MultiPositionTypeLabel type = m_AkAmbient.multiPositionTypeLabel;
		
		EditorGUILayout.PropertyField(multiPositionType, new GUIContent("Position Type: "));
		
		GUILayout.Space (2);

		currentAttSphereOp = (AttenuationSphereOptions)EditorGUILayout.EnumPopup("Show Attenuation Sphere: ", currentAttSphereOp);
		
		GUILayout.EndVertical();

		//Save multi-position type to know if it has changed
		MultiPositionTypeLabel multiPosType = m_AkAmbient.multiPositionTypeLabel;

		serializedObject.ApplyModifiedProperties ();


		if(m_AkAmbient.multiPositionTypeLabel == MultiPositionTypeLabel.MultiPosition_Mode)
		{
			//Here we make sure all AkAmbients that are in multi-position mode and that have the same event also have the same trigger
			UpdateTriggers(multiPosType);
		}

		if (GUI.changed)
		{			
			if (type != m_AkAmbient.multiPositionTypeLabel)
			{
				if (m_AkAmbient.multiPositionTypeLabel != MultiPositionTypeLabel.Large_Mode)
				{
					m_AkAmbient.multiPositionArray.Clear();
					
					// TODO: I need a good method to update the array in edit mode
					//m_AkAmbient.BuildMultiDirectionArray();
				} 
			}
		} 
	}

	void UpdateTriggers(MultiPositionTypeLabel in_multiPosType)
	{
		//if we just switched to MultiPosition_Mode
		if(in_multiPosType != m_AkAmbient.multiPositionTypeLabel)
		{
			//Get all AkAmbients in the scene
			AkAmbient[] akAmbients = FindObjectsOfType(typeof(AkAmbient)) as AkAmbient[];
			
			//Find the first AkAmbient that is in multiPosition_Mode and that has the same event as the current AkAmbient
			for(int i = 0; i < akAmbients.Length; i++)
			{
				if(akAmbients[i] != m_AkAmbient && akAmbients[i].multiPositionTypeLabel == MultiPositionTypeLabel.MultiPosition_Mode && akAmbients[i].eventID == m_AkAmbient.eventID)
				{
					//if the current AkAmbient doesn't have the same trigger as the others, we ask the user which one he wants to keep
                    if (!HasSameTriggers(akAmbients[i].triggerList))
					{
						if 	(	EditorUtility.DisplayDialog	(	"AkAmbient Trigger Mismatch", 
						                                   		"All ambients in multi-position mode with the same event must have the same triggers.\n" +
						                                   		"Which triggers would you like to keep?",
						                                  		"Current AkAmbient Triggers",
						                                   		"Other AkAmbients Triggers"
						                                   	)
						     )  
						{
							SetMultiPosTrigger(akAmbients);
						}
						else
						{
							m_AkAmbient.triggerList = akAmbients[i].triggerList;
						}
					}
					break;
				}
			}
		}
		//if the trigger changed or there was an undo/redo operation, we update the triggers of all the AkAmbients in the same group as the current one
        else if (!HasSameTriggers(triggerList) || (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed"))
		{
			AkAmbient[] akAmbients = FindObjectsOfType(typeof(AkAmbient)) as AkAmbient[];
			SetMultiPosTrigger(akAmbients);
		}
	}

    bool HasSameTriggers(List<int> other)
    {
        return other.Count == m_AkAmbient.triggerList.Count && m_AkAmbient.triggerList.Except(other).Count() == 0;
    }

	void SetMultiPosTrigger(AkAmbient[] akAmbients)
	{
		for(int i = 0; i < akAmbients.Length; i++)
		{
			if(akAmbients[i].multiPositionTypeLabel == MultiPositionTypeLabel.MultiPosition_Mode && akAmbients[i].eventID == m_AkAmbient.eventID)
			{
				akAmbients[i].triggerList = m_AkAmbient.triggerList;
			}
		}
	}

	void OnSceneGUI()
	{
		RenderAttenuationSpheres ();

		if (m_AkAmbient.multiPositionTypeLabel == MultiPositionTypeLabel.Simple_Mode)
		{
			return;
		}
		
		int someHashCode = GetHashCode();
		
		Handles.matrix = m_AkAmbient.transform.localToWorldMatrix;
		
		for (int i = 0; i < m_AkAmbient.multiPositionArray.Count; i++)
		{
			Vector3 pos = m_AkAmbient.multiPositionArray[i];
			
			Handles.Label(pos, "Point_" + i);
			
			float handleSize = HandleUtility.GetHandleSize(pos);
			
			// Get the needed data before the handle
			int controlIDBeforeHandle = GUIUtility.GetControlID(someHashCode, FocusType.Passive);
			bool isEventUsedBeforeHandle = (Event.current.type == EventType.used);
			
			Handles.color = Color.green;
			Handles.DrawCapFunction capFunc = Handles.SphereCap;
			Handles.ScaleValueHandle(0, pos, Quaternion.identity, handleSize, capFunc, 0);
			
			if (curPointIndex == i)
			{
				pos = Handles.PositionHandle(pos, Quaternion.identity);
			}
			
			// Get the needed data after the handle
			int controlIDAfterHandle = GUIUtility.GetControlID(someHashCode, FocusType.Passive);
			bool isEventUsedByHandle = !isEventUsedBeforeHandle && (Event.current.type == EventType.used);
			
			if
				((controlIDBeforeHandle < GUIUtility.hotControl &&
				  GUIUtility.hotControl < controlIDAfterHandle) ||
				 isEventUsedByHandle)
			{
				curPointIndex = i;
			}
			
			m_AkAmbient.multiPositionArray[i] = pos;                            
		}                    
		
		
		if(GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
		
		if (m_AkAmbient.multiPositionTypeLabel == MultiPositionTypeLabel.Large_Mode)
		{
			Handles.BeginGUI();
			
			Rect size = new Rect(0, 0, 200, 70);
			GUI.Window(0, new Rect(Screen.width - size.width - 10, Screen.height - size.height - 50, size.width, size.height), DoMyWindow, "AkAmbient Tool Bar");
			
			Handles.EndGUI();
		}
	}

	public void RenderAttenuationSpheres()
	{
		if (currentAttSphereOp == AttenuationSphereOptions.Dont_Show) 
		{
			return;
		}

		if(currentAttSphereOp == AttenuationSphereOptions.Current_Event_Only)
		{
			// Get the max attenuation for the event (if available)
			float radius = AkWwiseProjectInfo.GetData().GetEventMaxAttenuation(m_AkAmbient.eventID);

			if(m_AkAmbient.multiPositionTypeLabel == MultiPositionTypeLabel.Simple_Mode)
			{
				DrawSphere(m_AkAmbient.gameObject.transform.position, radius);
			}
			else if(m_AkAmbient.multiPositionTypeLabel == MultiPositionTypeLabel.Large_Mode)
			{
				Handles.matrix = m_AkAmbient.transform.localToWorldMatrix;

				for (int i = 0; i < m_AkAmbient.multiPositionArray.Count; i++)
				{
					DrawSphere(m_AkAmbient.multiPositionArray[i], radius);
				}
			}
			else
			{
				AkAmbient[] akAmbiants = FindObjectsOfType(typeof(AkAmbient)) as AkAmbient[];

				for(int i = 0; i < akAmbiants.Length; i++)
				{
					if 	(	akAmbiants[i].multiPositionTypeLabel == MultiPositionTypeLabel.MultiPosition_Mode
					   		&&
					   		akAmbiants[i].eventID == m_AkAmbient.eventID
					   	)
					{
						DrawSphere(akAmbiants[i].gameObject.transform.position, radius);
					}

				}
			}
		}
		else
		{
			AkAmbient[] akAmbiants = FindObjectsOfType(typeof(AkAmbient)) as AkAmbient[];

			for(int i = 0; i < akAmbiants.Length; i++)
			{
				// Get the max attenuation for the event (if available)
				float radius = AkWwiseProjectInfo.GetData().GetEventMaxAttenuation(akAmbiants[i].eventID);

				if(akAmbiants[i].multiPositionTypeLabel == MultiPositionTypeLabel.Large_Mode)
				{
					Handles.matrix = akAmbiants[i].transform.localToWorldMatrix;
						
					for (int j = 0; j < akAmbiants[i].multiPositionArray.Count; j++)
					{
						DrawSphere(akAmbiants[i].multiPositionArray[j], radius);
					}

					Handles.matrix = Matrix4x4.identity;
				}
				else 
				{
					DrawSphere(akAmbiants[i].gameObject.transform.position, radius);
				}
			}
		}

	}

	void DrawSphere(Vector3 in_position, float in_radius)
	{
		if(Vector3.SqrMagnitude(SceneView.lastActiveSceneView.camera.transform.position - in_position) > in_radius*in_radius)
		{
			Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.1f);
			Handles.SphereCap(0, in_position, Quaternion.identity, in_radius*2.0f);
		}
		else
		{
			Handles.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			DrawDiscs(new Vector3(0,1,0), new Vector3(0,-1,0), 6, in_position, in_radius);
		}
	}

	void DrawDiscs(Vector3 in_startNormal, Vector3 in_endNormal, uint in_nbDiscs, Vector3 in_position, float in_radius)
	{
		float f = 1.0f / (float)in_nbDiscs;

		for(int i = 0; i < in_nbDiscs; i++)
		{
			Handles.DrawWireDisc(in_position, Vector3.Slerp(in_startNormal, in_endNormal, f*i), in_radius);
		}
	}

	public static void PopulateMaxAttenuation()
	{
		if(populateSoundBank)
		{
			AkWwiseXMLBuilder.Populate ();
			populateSoundBank = false;

			SceneView.RepaintAll();
		}
	}
}
#endif