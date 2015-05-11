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

public class AkWwiseComponentPicker : EditorWindow
{	
	static AkWwiseComponentPicker s_componentPicker = null;

	AkWwiseTreeView						m_treeView = new AkWwiseTreeView();
	SerializedProperty[]				m_selectedItemGuid;
	SerializedObject					m_serializedObject;
	AkWwiseProjectData.WwiseObjectType	m_type;
	bool 								m_close = false;

	static public void Create(AkWwiseProjectData.WwiseObjectType in_type, SerializedProperty[] in_guid, SerializedObject in_serializedObject, Rect in_pos)
	{
		if(s_componentPicker == null)
		{
			s_componentPicker = ScriptableObject.CreateInstance<AkWwiseComponentPicker> ();

			//position the window below the button
			Rect pos = new Rect (in_pos.x, in_pos.yMax, 0, 0);

			//If the window gets out of the screen, we place it on top of the button instead
			if(in_pos.yMax > (Screen.currentResolution.height / 2))
			{
				pos.y = in_pos.y - (Screen.currentResolution.height / 2);
			}

			//We show a drop down window which is automatically destoyed when focus is lost
			s_componentPicker.ShowAsDropDown(pos, new Vector2 (in_pos.width >= 250 ? in_pos.width : 250, Screen.currentResolution.height / 2));  

			s_componentPicker.m_selectedItemGuid	= in_guid;
			s_componentPicker.m_serializedObject	= in_serializedObject;
			s_componentPicker.m_type 				= in_type;

			//Make a backup of the tree's expansion status and replace it with an empty list to make sure nothing will get expanded
			//when we populate the tree 
			List<string> expandedItemsBackUp = AkWwiseProjectInfo.GetData ().ExpandedItems;
			AkWwiseProjectInfo.GetData ().ExpandedItems = new List<string> ();

			s_componentPicker.m_treeView.AssignDefaults(); 
			s_componentPicker.m_treeView.SetRootItem(System.IO.Path.GetFileNameWithoutExtension(WwiseSetupWizard.Settings.WwiseProjectPath), AkWwiseProjectData.WwiseObjectType.PROJECT);

			//Populate the tree with the correct type 
			if(in_type == AkWwiseProjectData.WwiseObjectType.EVENT)
			{
				s_componentPicker.m_treeView.PopulateItem(s_componentPicker.m_treeView.RootItem, "Events", AkWwiseProjectInfo.GetData().EventWwu);
			}
			else if(in_type == AkWwiseProjectData.WwiseObjectType.SWITCH)
			{
				s_componentPicker.m_treeView.PopulateItem(s_componentPicker.m_treeView.RootItem, "Switches", AkWwiseProjectInfo.GetData().SwitchWwu);
			}
			else if(in_type == AkWwiseProjectData.WwiseObjectType.STATE)
			{
				s_componentPicker.m_treeView.PopulateItem(s_componentPicker.m_treeView.RootItem, "States", AkWwiseProjectInfo.GetData().StateWwu);
			}
			else if(in_type == AkWwiseProjectData.WwiseObjectType.SOUNDBANK)
			{
				s_componentPicker.m_treeView.PopulateItem(s_componentPicker.m_treeView.RootItem, "Banks", AkWwiseProjectInfo.GetData().BankWwu);
			}
			else if(in_type == AkWwiseProjectData.WwiseObjectType.AUXBUS)
			{
				s_componentPicker.m_treeView.PopulateItem(s_componentPicker.m_treeView.RootItem, "Auxiliary Busses", AkWwiseProjectInfo.GetData().AuxBusWwu);
			}


			TreeViewItem item = s_componentPicker.m_treeView.GetItemByGuid(new System.Guid(AkUtilities.GetByteArrayProperty( in_guid[0])));
			if(item != null)
			{
				item.ParentControl.SelectedItem = item;

				int itemIndexFromRoot = 0;

				//Expand all the parents of the selected item.
				//Count the number of items that are displayed before the selected item
				while(true)
				{
					item.IsExpanded = true;
						
					if(item.Parent != null)
					{
						itemIndexFromRoot += item.Parent.Items.IndexOf(item) + 1;
						item = item.Parent;
					}
					else
					{
						break;
					}
				}

				//Scroll down the window to make sure that the selected item is always visible when the window opens
				float itemHeight =	item.ParentControl.m_skinSelected.button.CalcSize(new GUIContent(item.Header)).y + 2.0f; //there seems to be 1 pixel between each item so we add 2 pixels(top and bottom) 
				s_componentPicker.m_treeView.SetScrollViewPosition(new Vector2(0.0f, (itemHeight*itemIndexFromRoot)-(Screen.currentResolution.height / 4)));
			}

			//Restore the tree's expansion status
			AkWwiseProjectInfo.GetData ().ExpandedItems = expandedItemsBackUp;
		}
	}

	public void OnGUI()
	{
		GUILayout.BeginVertical ();
		{
			m_treeView.DisplayTreeView(TreeViewControl.DisplayTypes.USE_SCROLL_VIEW);

			EditorGUILayout.BeginHorizontal("Box");
			{
				if(GUILayout.Button("Ok"))
				{
					//Get the selected item
					TreeViewItem selectedItem = m_treeView.GetSelectedItem();

					//Check if the selected item has the correct type
					if(selectedItem != null && m_type == (selectedItem.DataContext as AkWwiseTreeView.AkTreeInfo).ObjectType)
					{
						SetGuid(selectedItem);
					}
					
					//The window can now be closed
					m_close = true;
				}
				else if(GUILayout.Button("Cancel"))
				{
					m_close = true;
				}
				//We must be in 'used' mode in order for this to work
				else if(Event.current.type == EventType.used && m_treeView.LastDoubleClickedItem != null && m_type == (m_treeView.LastDoubleClickedItem.DataContext as AkWwiseTreeView.AkTreeInfo).ObjectType)
				{
					SetGuid(m_treeView.LastDoubleClickedItem);
					m_close = true;
				}
			}
			EditorGUILayout.EndHorizontal ();
		}
		EditorGUILayout.EndVertical ();
	}

	void SetGuid(TreeViewItem in_item)
	{
		m_serializedObject.Update();
		
		//we set the items guid
		AkUtilities.SetByteArrayProperty(m_selectedItemGuid[0], (in_item.DataContext as AkWwiseTreeView.AkTreeInfo).Guid);
		
		//When its a State or a Switch, we set the group's guid
		if(m_selectedItemGuid.Length == 2)
		{
			AkUtilities.SetByteArrayProperty(m_selectedItemGuid[1], (in_item.Parent.DataContext as AkWwiseTreeView.AkTreeInfo).Guid);
		}
		
		m_serializedObject.ApplyModifiedProperties();
	}

	public void Update()
	{
		//Unity sometimes generates an error when the window is closed from the OnGUI function.
		//So We close it here
		if(m_close)
			Close();
	}
}
#endif