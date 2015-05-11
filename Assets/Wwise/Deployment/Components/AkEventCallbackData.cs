#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;


public class AkEventCallbackData : ScriptableObject
{
	////AkSoundEngine.PostEvent callback flags. See the AkCallbackType enumeration for a list of all callbacks
	public List<int> callbackFlags = new List<int>();
	////GameObject that will receive the callback
	public List<GameObject> callbackGameObj = new List<GameObject>();

	////Names of the callback functions.
	public List<string> callbackFunc = new List<string>();
	
	////The sum of the flags of all game objects. This is the flag that will be passed to AkSoundEngine.PostEvent
	public int uFlags = 0;
}






#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.