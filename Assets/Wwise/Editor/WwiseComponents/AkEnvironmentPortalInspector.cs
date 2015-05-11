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


[CustomEditor(typeof(AkEnvironmentPortal))]
public class AkEnvironmentPortalInspector : Editor
{
	[MenuItem("GameObject/Wwise/Environment Portal", false, 1)]
	public static void CreatePortal()
	{
		GameObject portal = new GameObject ("EnvironmentPortal");
	
		portal.AddComponent<AkEnvironmentPortal> ();

		portal.GetComponent<Collider>().isTrigger = true;

		portal.GetComponent<Rigidbody>().useGravity = false;
		portal.GetComponent<Rigidbody>().isKinematic = true;
		portal.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;

		Selection.objects = new UnityEngine.Object[]{portal};
	}

	AkEnvironmentPortal 	m_envPortal;
	int[]					m_selectedIndex = new int[2];

	void OnEnable()
	{
		m_envPortal = target as AkEnvironmentPortal;

		for(int i = 0; i < 2; i++) 
		{
			int index = m_envPortal.envList[i].list.IndexOf (m_envPortal.environments [i]);
			m_selectedIndex [i] = index == -1 ? 0 : index;
		}
	}

	public override void OnInspectorGUI()
	{
		GUILayout.BeginVertical ("Box"); 
		{
			for(int i = 0; i < 2; i++)
			{
				string[] labels = new String[m_envPortal.envList[i].list.Count];
					
				for(int j = 0; j < labels.Length; j++)
				{					
					if(m_envPortal.envList[i].list[j] != null)
					{
						labels[j] = j+1 + ". " + GetEnvironmentName(m_envPortal.envList[i].list[j]) + " (" + m_envPortal.envList[i].list[j].name + ")"; 
					}
					else
					{
						m_envPortal.envList[i].list.RemoveAt(j);
					}
				}

				m_selectedIndex[i] = EditorGUILayout.Popup("Environment #" + (i+1), m_selectedIndex[i], labels);  

				m_envPortal.environments [i] = (m_selectedIndex [i] < 0 || m_selectedIndex [i] >= m_envPortal.envList[i].list.Count) ? null : m_envPortal.envList [i].list [m_selectedIndex [i]];
			}
		}
		GUILayout.EndVertical (); 
	
		GUILayout.Space (2);

		GUILayout.BeginVertical("Box"); 
		{
			string[] axisLabels = {"X", "Y", "Z"};

			int index = 0;
			for(int i = 0; i < 3; i++)
			{
				if(m_envPortal.axis[i] == 1) 
				{
					index = i;
					break;
				}
			}

			index = EditorGUILayout.Popup ("Axis" , index, axisLabels);

			if(m_envPortal.axis[index] != 1)
			{
				m_envPortal.axis.Set (0, 0, 0);
				m_envPortal.axis [index] = 1;

				//We move and replace the game object to trigger the OnTriggerStay function
				ShakePortal();
			}
		}
		GUILayout.EndVertical ();

		Repaint ();
	}

	string GetEnvironmentName(AkEnvironment in_env)
	{
		for(int i = 0; i < AkWwiseProjectInfo.GetData().AuxBusWwu.Count; i++)
		{
			for(int j = 0; j < AkWwiseProjectInfo.GetData().AuxBusWwu[i].List.Count; j++)
			{
				if(in_env.GetAuxBusID() == (uint)AkWwiseProjectInfo.GetData().AuxBusWwu[i].List[j].ID)
				{
					return AkWwiseProjectInfo.GetData().AuxBusWwu[i].List[j].Name;
				}
			}
		}

		return String.Empty;
	}

	public void ShakePortal()
	{
		Vector3 temp = m_envPortal.transform.position;
		temp.x *= 1.0000001f;
		m_envPortal.transform.position = temp;

		EditorApplication.update += ReplacePortal;
	}

	void ReplacePortal()
	{
		Vector3 temp = m_envPortal.transform.position;
		temp.x /= 1.0000001f;
		m_envPortal.transform.position = temp;

		EditorApplication.update -= ReplacePortal;
	}
}

#endif