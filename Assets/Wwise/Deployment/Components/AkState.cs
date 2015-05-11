#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[AddComponentMenu("Wwise/AkState")]
/// @brief This will call AK::SoundEngine::SetState whenever the selected Unity event is triggered.  For example this component could be set on a Unity collider to trigger when an object enters it.
/// \sa 
/// - \subpage soundengine_states
public class AkState : AkUnityEventHandler
{
#if UNITY_EDITOR
	public byte[] groupGuid = new byte[16];
	public byte[] valueGuid = new byte[16];
#endif

	/// State Group ID, as defined in WwiseID.cs
    public int groupID;

	/// State Value ID, as defined in WwiseID.cs
    public int valueID;

	public override void HandleEvent(GameObject in_gameObject)
	{
		AkSoundEngine.SetState((uint)groupID, (uint)valueID);
    }
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.