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

[InitializeOnLoad]
public class AkPortalManager
{
	static private AkPortalManager s_portalManager = null;

	public List<AkEnvironment>			EnvironmentList	= new List<AkEnvironment>(); 
	public List<AkEnvironmentPortal>	PortalList 		= new List<AkEnvironmentPortal>();

	public Dictionary<int, List<AkEnvironment>>[] IntersectingEnvironments = new []
	{
		new Dictionary<int, List<AkEnvironment>>(), //All environments on the negative side of each portal(opposite to the direction of the chosen axis)
		new Dictionary<int, List<AkEnvironment>>()	//All environments on the positive side of each portal(same direction as the chosen axis)
	};

	float m_timeStamp = Time.realtimeSinceStartup;

	static AkPortalManager()
	{
		//This constructor is called before any game object is created when there is a compilation which makes the 'FindObjectsOfType' function return null.
		//So we register the init function to be called at hte first update.
		EditorApplication.update += Init;
	}

	static public void Init()
	{
		//Do nothing if Manager exists
		if(s_portalManager == null)
		{
			s_portalManager = new AkPortalManager(); 			
		
			s_portalManager.Populate();

			//Register the update function to be called at each frame
			EditorApplication.update += s_portalManager.UpdateEnvironments;
		}

		//Unregister in case we were registered
		EditorApplication.update -= Init;
	}

	static public AkPortalManager GetManager()
	{
		return s_portalManager;
	}

	public void Populate()
	{
		//Add all environments in the scene to the environment list 
		AkEnvironment[] akEnv = UnityEngine.Object.FindObjectsOfType(typeof(AkEnvironment)) as AkEnvironment[];
		s_portalManager.EnvironmentList.Clear ();
		s_portalManager.EnvironmentList.AddRange (akEnv);

		//Add all portals in the scene to the portal list 
		AkEnvironmentPortal[] akPortals = UnityEngine.Object.FindObjectsOfType(typeof(AkEnvironmentPortal)) as AkEnvironmentPortal[];
		s_portalManager.PortalList.Clear ();
		s_portalManager.PortalList.AddRange (akPortals);

		//check for portal-environment intersections and populate the IntersectingEnvironments dictionary
		for(int i = 0; i < s_portalManager.PortalList.Count; i++)
		{
			s_portalManager.UpdatePortal(s_portalManager.PortalList[i]);
		}
	}
	
	public void UpdatePortal(AkEnvironmentPortal in_portal)
	{
		List<AkEnvironment>[] envList = new List<AkEnvironment>[2];

		for(int i = 0; i < 2; i++)
		{
			if(!IntersectingEnvironments[i].TryGetValue(in_portal.GetInstanceID(), out envList[i]))
			{
				envList[i] = new List<AkEnvironment>();
				IntersectingEnvironments[i][in_portal.GetInstanceID()] = envList[i];
			}
			else
			{
				envList[i].Clear();
			}

		}

		//We check if a portal intersects any environment 
		for(int i = EnvironmentList.Count - 1; i >= 0 ; i--)//Iterate in reverse order for safe removal form list while iterating
		{
			if(EnvironmentList[i] != null)
			{
				//if there is an intersection
				if(in_portal.GetComponent<Collider>().bounds.Intersects(EnvironmentList[i].GetComponent<Collider>().bounds))
				{
					//We check if the environment is on the left(negatif side of chosen axis) or right side of the portal
					if(Vector3.Dot(in_portal.transform.rotation * in_portal.axis, EnvironmentList[i].transform.position - in_portal.transform.position) >= 0)
					{
						envList[1].Add(EnvironmentList[i]);
					}
					else
					{
						envList[0].Add(EnvironmentList[i]);
					}
				}
			}
			else
			{
				EnvironmentList.RemoveAt(i);
			}
		}
	}

	void UpdateEnvironment(AkEnvironment in_env)
	{
		for(int i = PortalList.Count -1; i >= 0 ; i--)//Iterate in reverse order for safe removal form list while iterating
		{
			if(PortalList[i] != null)
			{
				if(in_env.GetComponent<Collider>().bounds.Intersects(PortalList[i].GetComponent<Collider>().bounds))
				{
					List<AkEnvironment> envList = null;
						
					//Get index of the list that should contain this environment
					//Index = 0 means that the enviroment is on the negative side of the portal (opposite to the direction of the chosen axis)
					//Index = 1 means that the enviroment is on the positive side of the portal (same direction as the chosen axis)
					int index = (Vector3.Dot(PortalList[i].transform.rotation * PortalList[i].axis, in_env.transform.position - PortalList[i].transform.position) >= 0) ? 1 : 0;
						
					if(!IntersectingEnvironments[index].TryGetValue(PortalList[i].GetInstanceID(), out envList))
					{
						envList = new List<AkEnvironment>();
						envList.Add(in_env);
						IntersectingEnvironments[index][PortalList[i].GetInstanceID()] = envList;
					}
					else
					{
						if(!envList.Contains(in_env))
						{
							envList.Add(in_env);
						}
					}
				}
			}
			else
			{
				PortalList.RemoveAt(i);
			}
		}
	}
	
	public void UpdateEnvironments()
	{ 
		//Timer is reset when starting play mode and when coming back to editor mode 
		if (Time.realtimeSinceStartup < m_timeStamp)
		{
			m_timeStamp = Time.realtimeSinceStartup;

			//The PortalManager object doesn't get destroyed but all game objects in our lists become null
			//So we populate
			Populate();

			return;
		}

		//The update is done once every second
		if(Time.realtimeSinceStartup - m_timeStamp < 1.0f)
			return;

		m_timeStamp = Time.realtimeSinceStartup;		


		UnityEngine.Object[] portals = Selection.GetFiltered (typeof(AkEnvironmentPortal), SelectionMode.Unfiltered);
		if(portals != null) 
		{
			for(int i = 0; i < portals.Length; i++) 
			{
				if(!PortalList.Contains((AkEnvironmentPortal)portals[i])) 
				{
					PortalList.Add((AkEnvironmentPortal)portals[i]);
				}

				UpdatePortal((AkEnvironmentPortal)portals[i]);
			}
		}

		UnityEngine.Object[] envs = Selection.GetFiltered (typeof(AkEnvironment), SelectionMode.Unfiltered);
		if(envs != null)
		{
			for(int i = 0; i < envs.Length; i++)
			{
				if(!EnvironmentList.Contains((AkEnvironment)envs[i]))
				{
					EnvironmentList.Add((AkEnvironment)envs[i]);
				}
					
				UpdateEnvironment((AkEnvironment)envs[i]);
			}
		}
	}
}

#endif