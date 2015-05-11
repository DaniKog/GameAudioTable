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


public abstract class AkBaseInspector : Editor
{

	protected SerializedProperty[]					m_guidProperty;	//all components have 1 guid except switches and states which have 2. Index zero is value guid and index 1 is group guid
	protected AkWwiseProjectData.WwiseObjectType	m_objectType;
	protected string 								m_typeName;

	bool 			m_buttonWasPressed		= false;
	protected bool 	m_isInDropArea 			= false;		//is the mouse on top of the drop area(the button)	
	Rect 			m_dropAreaRelativePos 	= new Rect();	//button position relative to the inspector
	Rect			m_pickerPos = new Rect();

	public abstract void	OnChildInspectorGUI ();
	public abstract string	UpdateIds(System.Guid[] in_guid);	//set object properties and return its name

	public override void OnInspectorGUI()
	{
		serializedObject.ApplyModifiedProperties ();

		/***************************************Handle Drag and Drop********************************************************/
		if(DragAndDrop.paths.Length >= 4 && DragAndDrop.paths[3].Equals(m_typeName))
		{
			if(Event.current.type == EventType.DragUpdated)
			{
				//mousePosition is not available during DragExited event but is available during the DragUpdated event.
				m_isInDropArea = m_dropAreaRelativePos.Contains(Event.current.mousePosition);
				
				if(m_isInDropArea)
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				return;
			}
			if(Event.current.type == EventType.DragExited && m_isInDropArea) 
			{
				AkUtilities.SetByteArrayProperty(m_guidProperty[0], new System.Guid(DragAndDrop.paths[1]).ToByteArray());

				//needed for the undo operation to work
				GUIUtility.hotControl = 0;

				m_isInDropArea = false;
				return;
			}
		}
		/*******************************************************************************************************************/


		/************************************************Update Properties**************************************************/
		System.Guid[] componentGuid = new System.Guid[m_guidProperty.Length];
		for(int i = 0; i < componentGuid.Length; i++)
		{
			byte[] guidBytes = AkUtilities.GetByteArrayProperty (m_guidProperty[i]);
			componentGuid[i] = guidBytes == null ? System.Guid.Empty : new System.Guid(guidBytes); 
		}

		string componentName = UpdateIds (componentGuid);
		/*******************************************************************************************************************/


		/********************************************Draw GUI***************************************************************/
		OnChildInspectorGUI ();

		GUILayout.Space(3);

		GUILayout.BeginHorizontal("box");
		{
			float inspectorWidth = Screen.width - GUI.skin.box.margin.left - GUI.skin.box.margin.right - 19;
			GUILayout.Label (m_typeName + " Name: ", GUILayout.Width (inspectorWidth * 0.4f));
			
			GUIStyle style = new GUIStyle(GUI.skin.button);
			style.alignment = TextAnchor.MiddleLeft;
			if(componentName.Equals(String.Empty))
			{
				componentName = "No " + m_typeName + " is currently selected";
				style.normal.textColor = Color.red;
			}
			
			if(GUILayout.Button(componentName, style, GUILayout.MaxWidth (inspectorWidth * 0.6f - GUI.skin.box.margin.right)))
			{
				m_buttonWasPressed = true;
				
				// We don't want to set object as dirty only because we clicked the button.
				// It will be set as dirty if the wwise object has been changed by the tree view.
				GUI.changed = false;
			}

			//GUILayoutUtility.GetLastRect and AkUtilities.GetLastRectAbsolute must be called in repaint mode 
			if(Event.current.type == EventType.Repaint)
			{
				m_dropAreaRelativePos = GUILayoutUtility.GetLastRect();
				
				if(m_buttonWasPressed)
				{
					m_pickerPos = AkUtilities.GetLastRectAbsolute();
					EditorApplication.delayCall += DelayCreateCall;
					m_buttonWasPressed = false;
				}
			}
		}
		GUILayout.EndHorizontal ();
		
		GUILayout.Space(5);
		/***********************************************************************************************************************/  

		if (GUI.changed)
		{
			EditorUtility.SetDirty(serializedObject.targetObject);
		}
	}

	void DelayCreateCall()
	{
		AkWwiseComponentPicker.Create(m_objectType, m_guidProperty, serializedObject, m_pickerPos);
	}
}

#endif