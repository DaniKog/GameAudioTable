#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections.Generic;

[AddComponentMenu("Wwise/AkGameObj")]
///@brief This component represents a sound emitter in your scene.  It will track its position and other game syncs such as Switches, RTPC and environment values.  You can add this to any object that will emit sound.  Note that if it is not present, Wwise will add it automatically, with the default values, to any Unity Game Object that is passed to Wwise API (see AkSoundEngine.cs).  
/// \sa
/// - \ref soundengine_gameobj
/// - \ref soundengine_events
/// - \ref soundengine_switch
/// - \ref soundengine_states
/// - \ref soundengine_environments
[ExecuteInEditMode] //ExecuteInEditMode necessary to maintain proper state of isStaticObject.
public class AkGameObj : MonoBehaviour 
{
	/// When not set to null, the emitter position will be offset relative to the Game Object position by the Position Offset
	public AkGameObjPosOffsetData m_posOffsetData = null;
	
	/// Is this object affected by Environment changes?  Set to false if not affected in order to save some useless calls.  Default is true.
    public bool isEnvironmentAware = true;
	public AkGameObjEnvironmentData m_envData = null;

	/// Maintains and persists the Static setting of the gameobject, which is available only in the editor.
	[SerializeField]
	private bool isStaticObject = false;
	private AkGameObjPositionData m_posData = null;

	void Awake()
    {			
		// If the object was marked as static, don't update its position to save cycles.
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)	
		{
			//set enabled to true and is static to false to make sure that the update function is called in edit mode
			//the correct value for the isStaticObject variable will be set when update is called
			isStaticObject = false;
			enabled = true;
			return;
		}
#endif 
		if(!isStaticObject)
		{
			m_posData = new AkGameObjPositionData();
		}
		if(isEnvironmentAware)
		{
			m_envData = new AkGameObjEnvironmentData();
		}
	
        //Register a Game Object in the sound engine, with its name.		
        AkSoundEngine.RegisterGameObj(gameObject, gameObject.name);
	
		// Get position with offset
		Vector3 position = GetPosition();

		//Set the original position
		AkSoundEngine.SetObjectPosition(
			gameObject,
			position.x,
			position.y,
			position.z,
			transform.forward.x,
			transform.forward.y,
			transform.forward.z);


    }

	void OnEnable()
	{ 
		//if enabled is set to false, then the update function wont be called
		enabled = !isStaticObject;
	}
	
    void OnDestroy()
    {
		// We can't do the code in OnDestroy if the gameObj is unregistered, so do it now.		
		AkUnityEventHandler[] eventHandlers = gameObject.GetComponents<AkUnityEventHandler>();
		foreach( AkUnityEventHandler handler in eventHandlers )
		{
			if( handler.triggerList.Contains(AkUnityEventHandler.DESTROY_TRIGGER_ID) )
			{
				handler.DoDestroy();
			}
		}
		
#if UNITY_EDITOR	
		if (UnityEditor.EditorApplication.isPlaying)
#endif
		{
		
			if (AkSoundEngine.IsInitialized())
			{
				AkSoundEngine.UnregisterGameObj(gameObject);
			}
		}
    }

    void Update()
    {
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			isStaticObject = gameObject.isStatic;
			return;
		}
#endif

	    // Get position with offset
	    Vector3 position = GetPosition();

		//Didn't move.  Do nothing.
		if (m_posData.position == position && m_posData.forward == transform.forward)
	        return;

		m_posData.position = position;
		m_posData.forward = transform.forward;            

	    //Update position
	    AkSoundEngine.SetObjectPosition(
	        gameObject,
	        position.x,
	        position.y,
	        position.z,
	        transform.forward.x,
	        transform.forward.y,
	        transform.forward.z);

		if (isEnvironmentAware)
		{
			UpdateAuxSend();
		}        
	}
	/// Gets the position including the position offset, if applyPositionOffset is enabled.
	/// \return  The position.
	public Vector3 GetPosition()
	{
		if (m_posOffsetData != null)
		{
			// Get offset in world space
			Vector3 worldOffset = transform.rotation * m_posOffsetData.positionOffset;
			
			// Add offset to gameobject position
			return transform.position + worldOffset;
		}		
		return transform.position;
	}


    void OnTriggerEnter(Collider other)
    {
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif

        if (isEnvironmentAware)
        {
            AddAuxSend(other.gameObject);
        }
    }

    void AddAuxSend(GameObject in_AuxSendObject)
    {
		AkEnvironmentPortal akPortal = in_AuxSendObject.GetComponent<AkEnvironmentPortal>();
		if(akPortal != null)
		{
			m_envData.activePortals.Add(akPortal);
			
			for(int i = 0; i < 2; i++) 
			{
				if(akPortal.environments[i] != null)
				{
					//Add environment only if its not already there 
					int index = m_envData.activeEnvironments.BinarySearch(akPortal.environments[i], AkEnvironment.s_compareByPriority);
					if(index < 0)
						m_envData.activeEnvironments.Insert(~index, akPortal.environments[i]);//List will still be sorted after insertion
				}
			}

			//Update and send the auxSendArray
			m_envData.auxSendValues = null;
			UpdateAuxSend();
			return;
		}
        
		AkEnvironment akEnvironment = in_AuxSendObject.GetComponent<AkEnvironment>();
		if (akEnvironment != null)
        {
			//Add environment only if its not already there 
			int index = m_envData.activeEnvironments.BinarySearch(akEnvironment, AkEnvironment.s_compareByPriority);
			if(index < 0)
			{
				m_envData.activeEnvironments.Insert(~index, akEnvironment);//List will still be sorted after insertion

				//Update only if the environment was inserted.
				//If it wasn't inserted, it means we're inside a portal so we dont update because portals have a highter priority than environments
				m_envData.auxSendValues = null;
				UpdateAuxSend();
			}
        }
    }

    void OnTriggerExit(Collider other)
    {
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif

        if (isEnvironmentAware)
        {
			AkEnvironmentPortal akPortal = other.gameObject.GetComponent<AkEnvironmentPortal>();
			if(akPortal != null)
			{
				for(int i = 0; i < 2; i++)
				{
					if(akPortal.environments[i] != null)
					{
						//We just exited a portal so we remove its environments only if we're not inside of them
						if(!GetComponent<Collider>().bounds.Intersects(akPortal.environments[i].GetComponent<Collider>().bounds))
						{
							m_envData.activeEnvironments.Remove(akPortal.environments[i]);
						}
					}
				}
				//remove the portal
				m_envData.activePortals.Remove(akPortal);

				//Update and send the auxSendArray
				m_envData.auxSendValues = null;
				UpdateAuxSend();
				return;
			}

			AkEnvironment akEnvironment = other.gameObject.GetComponent<AkEnvironment>();
			if (akEnvironment != null)
			{
				//we check if the environment belongs to a portal
				for(int i = 0; i < m_envData.activePortals.Count; i++)
				{
					for(int j = 0; j < 2; j++)
					{
						if(akEnvironment == m_envData.activePortals[i].environments[j])
						{
							//if it belongs to a portal, then we're inside that portal and we don't remove the environment
							m_envData.auxSendValues = null;
							UpdateAuxSend();
							return;
						}
					}
				}
				//if it doesn't belong to a portal, we remove it
				m_envData.activeEnvironments.Remove(akEnvironment);
				m_envData.auxSendValues = null;
				UpdateAuxSend();
				return;
			}
        }
    }

    void UpdateAuxSend()
    {
		if (m_envData.auxSendValues == null)
        {
			m_envData.auxSendValues = new AkAuxSendArray	(	m_envData.activeEnvironments.Count < AkEnvironment.MAX_NB_ENVIRONMENTS 
			                                      				? 
			                                              		(uint)m_envData.activeEnvironments.Count : (uint)AkEnvironment.MAX_NB_ENVIRONMENTS
			                                      			);            
        }
        else
        {
			m_envData.auxSendValues.Reset();
        }
	

		//we search for MAX_NB_ENVIRONMENTS(4 at this time) environments with the hightest priority that belong to a portal and add them to the auxSendArray
		for(int i = 0; i < m_envData.activePortals.Count; i++)
		{
			for(int j = 0; j < 2; j++)
			{
				AkEnvironment env = m_envData.activePortals[i].environments[j];

				if(env != null)
				{
					if(m_envData.activeEnvironments.BinarySearch(env, AkEnvironment.s_compareByPriority) < AkEnvironment.MAX_NB_ENVIRONMENTS)
					{
						m_envData.auxSendValues.Add(env.GetAuxBusID(), m_envData.activePortals[i].GetAuxSendValueForPosition(transform.position, j));
					}
				}
			}
		}

		//if we still dont have MAX_NB_ENVIRONMENTS in the auxSendArray, we add the next environments with the hightest priority until we reach MAX_NB_ENVIRONMENTS
		//or run out of environments
		if(m_envData.auxSendValues.m_Count < AkEnvironment.MAX_NB_ENVIRONMENTS && m_envData.auxSendValues.m_Count < m_envData.activeEnvironments.Count)
		{
			//Make a copy of all environments
			List<AkEnvironment> sortedEnvList = new List<AkEnvironment>(m_envData.activeEnvironments);

			//sort the list with the selection algorithm 
			sortedEnvList.Sort(AkEnvironment.s_compareBySelectionAlgorithm);

			int environmentsLeft = Math.Min(AkEnvironment.MAX_NB_ENVIRONMENTS - (int)m_envData.auxSendValues.m_Count, m_envData.activeEnvironments.Count - (int)m_envData.auxSendValues.m_Count);

			for(int i = 0; i < environmentsLeft; i++)
			{
				if(!m_envData.auxSendValues.Contains(sortedEnvList[i].GetAuxBusID()))
				{
					//An environment with the isDefault flag set to true is added only if its the only environment.
					//Since an environment with the isDefault flag has the lowest priority, it will be at index zero only if there is no other environment
					if(sortedEnvList[i].isDefault && i != 0)
						continue;

					m_envData.auxSendValues.Add(sortedEnvList[i].GetAuxBusID(), sortedEnvList[i].GetAuxSendValueForPosition(transform.position));

					//No other environment can be added after an environment with the excludeOthers flag set to true
					if(sortedEnvList[i].excludeOthers)
						break;
				}
			}
		}

		AkSoundEngine.SetGameObjectAuxSendValues(gameObject, m_envData.auxSendValues, m_envData.auxSendValues.m_Count);
    }    

#if UNITY_EDITOR
	public void OnDrawGizmosSelected()
	{
		Vector3 position = GetPosition();
		Gizmos.DrawIcon(position, "WwiseAudioSpeaker.png", false);
	}
#endif
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.