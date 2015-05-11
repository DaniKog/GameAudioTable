using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using FMOD.Studio;

public class FMOD_StudioEventEmitter : MonoBehaviour 
{
	public FMODAsset asset;
	public string path = "";
	public bool startEventOnAwake = true;

	FMOD.Studio.EventInstance evt;
	bool hasStarted = false;
	
	Rigidbody cachedRigidBody;

	[System.Serializable]
	public class Parameter
	{
		public string name;
		public float value;
	}
	
	public void Play()
	{
		if (evt != null)
		{
			ERRCHECK(evt.start());
		}
		else
		{
			FMOD.Studio.UnityUtil.Log("Tried to play event without a valid instance: " + path);
			return;			
		}
	}
	
	public void Stop()
	{
		if (evt != null)
		{
			ERRCHECK(evt.stop(STOP_MODE.IMMEDIATE));
		}		
	}	
	
	public FMOD.Studio.ParameterInstance getParameter(string name)
	{
		FMOD.Studio.ParameterInstance param = null;
		ERRCHECK(evt.getParameter(name, out param));
			
		return param;
	}

	public FMOD.Studio.PLAYBACK_STATE getPlaybackState()
	{
		if (evt == null || !evt.isValid())
			return FMOD.Studio.PLAYBACK_STATE.STOPPED;
		
		FMOD.Studio.PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
		
		if (ERRCHECK (evt.getPlaybackState(out state)) == FMOD.RESULT.OK)
			return state;
		
		return FMOD.Studio.PLAYBACK_STATE.STOPPED;
	}

	void Start() 
	{
		if (evt == null || !evt.isValid())
		{
			CacheEventInstance();
		}
		
		cachedRigidBody = GetComponent<Rigidbody>();
		
		if (startEventOnAwake)
			StartEvent();
	}
	
	void CacheEventInstance()
	{
		if (asset != null)
		{
			evt = FMOD_StudioSystem.instance.GetEvent(asset.id);				
		}
		else if (!String.IsNullOrEmpty(path))
		{
			evt = FMOD_StudioSystem.instance.GetEvent(path);
		}
		else
		{
			FMOD.Studio.UnityUtil.LogError("No asset or path specified for Event Emitter");
		}
	}

	static bool isShuttingDown = false;

	void OnApplicationQuit() 
	{
		isShuttingDown = true;
	}

	void OnDestroy() 
	{
		if (isShuttingDown)
			return;

		FMOD.Studio.UnityUtil.Log("Destroy called");
		if (evt != null && evt.isValid()) 
		{
			if (getPlaybackState () != FMOD.Studio.PLAYBACK_STATE.STOPPED)
			{
				FMOD.Studio.UnityUtil.Log("Release evt: " + path);
				ERRCHECK (evt.stop(FMOD.Studio.STOP_MODE.IMMEDIATE));
			}
			
			ERRCHECK(evt.release ());
			evt = null;
		}
	}

	public void StartEvent()
	{		
		if (evt == null || !evt.isValid())
		{
			CacheEventInstance();
		}
		
		// Attempt to release as oneshot
		if (evt != null && evt.isValid())
		{
			Update3DAttributes();
			ERRCHECK(evt.start());
			//if (evt.release() == FMOD.RESULT.OK) 
			{
				//evt = null;
			}
		}
		else
		{
			FMOD.Studio.UnityUtil.LogError("Event retrieval failed: " + path);
		}

		hasStarted = true;
	}

	public bool HasFinished()
	{
		if (!hasStarted)
			return false;
		if (evt == null || !evt.isValid())
			return true;
		
		return getPlaybackState () == FMOD.Studio.PLAYBACK_STATE.STOPPED;
	}

	void Update() 
	{
		if (evt != null && evt.isValid ()) 
		{
			Update3DAttributes();
		} 
		else 
		{
			evt = null;
		}
	}
	
	void Update3DAttributes()
	{
		if (evt != null && evt.isValid ()) 
		{
			var attributes = UnityUtil.to3DAttributes(gameObject, cachedRigidBody);			
			ERRCHECK(evt.set3DAttributes(attributes));
		}
	}    
    
    #if (UNITY_EDITOR)
	void OnDrawGizmosSelected() 
	{
        if (asset != null && enabled)
        {
            FMOD.Studio.EventDescription desc = null;
            desc = FMODEditorExtension.GetEventDescription(asset.id);

            if (desc != null)
            {
                float max, min;
                desc.getMaximumDistance(out max);
                desc.getMinimumDistance(out min);

                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, min);
                Gizmos.DrawWireSphere(transform.position, max);
            }
        }		
	}
    #endif
	
	FMOD.RESULT ERRCHECK(FMOD.RESULT result)
	{
		FMOD.Studio.UnityUtil.ERRCHECK(result);
		return result;
	}
}
