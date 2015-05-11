#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading;
#pragma warning disable 0219, 0414


[AddComponentMenu("Wwise/AkTerminator")]
/// This script deals with termination of the Wwise audio engine.  
/// It must be present on one Game Object that gets destroyed last in the game.
/// It must be executed AFTER any other monoBehaviors that use AkSoundEngine.
/// \sa
/// - \ref workingwithsdks_termination
/// - AK::SoundEngine::Term()
public class AkTerminator : MonoBehaviour
{
	static private AkTerminator ms_Instance = null;

	void Awake()
	{
		if (ms_Instance != null)
		{			
			//Check if there are 2 objects with this script.  If yes, remove this component.
			if (ms_Instance != this)
				Object.DestroyImmediate(this);
            return; 
		}

		DontDestroyOnLoad(this);
		ms_Instance = this;		
	}	
	
	void OnApplicationQuit() 
	{
		//This happens before OnDestroy.  Stop the sound engine now.
		Terminate();
		
		// NOTE: AkCallbackManager needs to handle last few events after sound engine terminates
		// So it has to terminate after sound engine does.  See OnDestroy.
	}
	
	void OnDestroy()
    {   
		if (ms_Instance == this)
			ms_Instance = null;        
    }
	
	void Terminate()
	{
		if (ms_Instance == null || ms_Instance != this || !AkSoundEngine.IsInitialized())
			return; //Don't term twice        
				
		// Mop up the last callbacks that will be sent from Term with blocking.  
		// It may happen that the term sends so many callbacks that it will use up 
		// all the callback memory buffer and lock the calling thread. 

		// WG-25356 Thread is unsupported in Windows Store App API.

		AkSoundEngine.StopAll();
		AkSoundEngine.RenderAudio();
		const double IdleMs = 1.0;
		const uint IdleTryCount = 50;
		for(uint i=0; i<IdleTryCount; i++)
		{
			AkCallbackManager.PostCallbacks();
			using (EventWaitHandle tmpEvent = new ManualResetEvent(false)) {
				tmpEvent.WaitOne(System.TimeSpan.FromMilliseconds(IdleMs));
			}
		}

		AkSoundEngine.Term();
	
		ms_Instance = null;

		AkCallbackManager.Term();
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.