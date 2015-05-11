#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections.Generic;

public enum AkUnsupportedCallbackType
{
	AK_SpeakerVolumeMatrix				= 0x0010,
	AK_MusicSyncAll 					= 0x7f00,
	AK_CallbackBits 					= 0xfffff,
	AK_EnableGetSourcePlayPosition 		= 0x100000,
	AK_EnableGetMusicPlayPosition 		= 0x200000,
	AK_EnableGetSourceStreamBuffering 	= 0x400000,
	AK_Monitoring 						= 0x20000000,
	AK_Bank 							= 0x40000000,
	AK_AudioInterruption				= 0x22000000
}

/// <summary>
/// Event callback information.
/// Event callback functions can receive this structure as a parameter
/// </summary>
public struct AkEventCallbackMsg
{
	public AkCallbackType	type;	///AkSoundEngine.PostEvent callback flags. See the AkCallbackType enumeration for a list of all callbacks
	public GameObject		sender;	///GameObject from whom the callback function was called  
	public object			info;	///More information about the event callback, see the structs in AkCallbackManager.cs
}

[AddComponentMenu("Wwise/AkEvent")]
/// @brief Helper class that knows a Wwise Event and when to trigger it in Unity.
/// \sa
/// - \ref soundengine_events
public class AkEvent : AkUnityEventHandler 
{
#if UNITY_EDITOR
	public byte[] valueGuid = new byte[16];
#endif

	/// ID of the Event as found in the WwiseID.cs file
    public int eventID = 0;
	/// Game object onto which the Event will be posted.  By default, when empty, it is posted on the same object on which the component was added.
	public GameObject soundEmitterObject = null;
	/// Enables additional options to reuse existing events.  Use it to transform a Play event into a Stop event without having to define one in the Wwise Project.
    public bool enableActionOnEvent = false;
	/// Replacement action.  See AK::SoundEngine::ExecuteEventOnAction()
    public AkActionOnEventType actionOnEventType = AkActionOnEventType.AkActionOnEventType_Stop;
	/// Fade curve to use with the new Action.  See AK::SoundEngine::ExecuteEventOnAction() 
    public AkCurveInterpolation curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear;
	/// Duration of the fade.  See AK::SoundEngine::ExecuteEventOnAction() 
    public float transitionDuration = 0.0f;
	//
	public AkEventCallbackData m_callbackData = null;

	private void Callback(object in_cookie, AkCallbackType in_type, object in_info)
	{
		for(int i = 0; i < m_callbackData.callbackFunc.Count; i++)
		{
			if(((int)in_type & m_callbackData.callbackFlags[i]) != 0 && m_callbackData.callbackGameObj[i] != null)
			{
				AkEventCallbackMsg callbackInfo = new AkEventCallbackMsg();
				callbackInfo.type	= in_type;
				callbackInfo.sender	= gameObject;
				callbackInfo.info	= in_info;

				m_callbackData.callbackGameObj[i].SendMessage(m_callbackData.callbackFunc[i], callbackInfo);
			}
		}
	}

	public override void HandleEvent(GameObject in_gameObject)
	{        
		GameObject gameObj = (useOtherObject && in_gameObject != null) ? in_gameObject : gameObject;

		soundEmitterObject = gameObj;

        if(enableActionOnEvent)
			AkSoundEngine.ExecuteActionOnEvent((uint)eventID, actionOnEventType, gameObj, (int)transitionDuration * 1000, curveInterpolation);
		else if(m_callbackData != null)
			AkSoundEngine.PostEvent((uint)eventID, gameObj, (uint)m_callbackData.uFlags, Callback, null, 0, null, AkSoundEngine.AK_INVALID_PLAYING_ID);
		else
			AkSoundEngine.PostEvent((uint)eventID, gameObj);
    }

    public void Stop(int _transitionDuration, AkCurveInterpolation _curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear)
    {
		AkSoundEngine.ExecuteActionOnEvent((uint)eventID, AkActionOnEventType.AkActionOnEventType_Stop, soundEmitterObject, _transitionDuration, _curveInterpolation);
    }
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.