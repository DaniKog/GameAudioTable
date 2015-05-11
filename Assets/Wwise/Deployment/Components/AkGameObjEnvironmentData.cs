#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;


public class AkGameObjEnvironmentData
{
	//Contains all active environments sorted by priority, event those inside a portal. 
	public List<AkEnvironment>			activeEnvironments	= new List<AkEnvironment>();
	public List<AkEnvironmentPortal>	activePortals		= new List<AkEnvironmentPortal>();
	public AkAuxSendArray 				auxSendValues;
}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.