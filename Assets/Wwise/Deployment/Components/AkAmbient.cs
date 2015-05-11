#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;


public enum MultiPositionTypeLabel 
{
	Simple_Mode,
	Large_Mode,
	MultiPosition_Mode
}


public class AkMultiPosEvent
{
	public List<AkAmbient> list = new List<AkAmbient>(); 
	public bool eventIsPlaying = false;
	
	public void FinishedPlaying(object in_cookie, AkCallbackType in_type, object in_info)
	{
		eventIsPlaying = false;
	}
}


[AddComponentMenu("Wwise/AkAmbient")]
/// @brief Use this component to attach a Wwise Event to any object in a scene.
/// The sound can be started at various moments, dependent on the selected Unity trigger.  This component is more useful for ambient sounds (sounds related to scene-bound objects) but could also be used for other purposes.
/// \sa
/// - \ref soundengine_events
/// - \ref AkGameObj
/// - \ref AkEvent
[RequireComponent (typeof(AkGameObj))]
public class AkAmbient : AkEvent
{
	public MultiPositionTypeLabel multiPositionTypeLabel = MultiPositionTypeLabel.Simple_Mode;
	public List<Vector3> multiPositionArray = new List<Vector3>();
	public AkAmbient ParentAkAmbience { get; set; } 
	
	static public Dictionary<int, AkMultiPosEvent> multiPosEventTree = new Dictionary<int, AkMultiPosEvent>();
	
	void OnEnable()
	{
		if (multiPositionTypeLabel == MultiPositionTypeLabel.Simple_Mode)
		{
			AkGameObj[] gameObj = gameObject.GetComponents<AkGameObj>();
			for(int i = 0; i < gameObj.Length; i++)
				gameObj[i].enabled = true;
		}
		else if(multiPositionTypeLabel == MultiPositionTypeLabel.Large_Mode)
		{
			AkGameObj[] gameObj = gameObject.GetComponents<AkGameObj>();
			for(int i = 0; i < gameObj.Length; i++)
				gameObj[i].enabled = false;

			AkPositionArray positionArray = BuildAkPositionArray();			
			
			AkSoundEngine.SetMultiplePositions(gameObject, positionArray, (ushort)positionArray.Count, MultiPositionType.MultiPositionType_MultiSources);
		}
		else if (multiPositionTypeLabel == MultiPositionTypeLabel.MultiPosition_Mode)
		{
			AkGameObj[] gameObj = gameObject.GetComponents<AkGameObj>();
			for(int i = 0; i < gameObj.Length; i++)
				gameObj[i].enabled = false;

			AkMultiPosEvent eventPosList;
			
			if(multiPosEventTree.TryGetValue(eventID, out eventPosList))
			{
				if(!eventPosList.list.Contains(this))
				{
					eventPosList.list.Add(this);
				}
			}
			else
			{
				eventPosList = new AkMultiPosEvent();
				eventPosList.list.Add(this);
				multiPosEventTree.Add(eventID, eventPosList);
			}
			
			
			AkPositionArray positionArray = BuildMultiDirectionArray(ref eventPosList);
			
			//Set multiple positions
			AkSoundEngine.SetMultiplePositions(eventPosList.list[0].gameObject, positionArray, (ushort)positionArray.Count, MultiPositionType.MultiPositionType_MultiSources);
		}
	}
	
	
	void OnDisable()
	{
		if(multiPositionTypeLabel == MultiPositionTypeLabel.MultiPosition_Mode)
		{
			AkMultiPosEvent eventPosList = multiPosEventTree[eventID];
			
			if(eventPosList.list.Count == 1)
			{
				multiPosEventTree.Remove(eventID); 
				return;
			}
			else
			{
				eventPosList.list.Remove(this);
				
				AkPositionArray positionArray = BuildMultiDirectionArray(ref eventPosList);
				
				//Set multiple positions
				AkSoundEngine.SetMultiplePositions(eventPosList.list[0].gameObject, positionArray, (ushort)positionArray.Count, MultiPositionType.MultiPositionType_MultiSources);
			}
		}
	}
	
	
	public override void HandleEvent(GameObject in_gameObject)
	{  
		if (multiPositionTypeLabel != MultiPositionTypeLabel.MultiPosition_Mode) 
		{
			base.HandleEvent(in_gameObject);
		}
		else
		{
			AkMultiPosEvent multiPositionSoundEmitter = multiPosEventTree[eventID];
			
			if(multiPositionSoundEmitter.eventIsPlaying)
				return;
			
			multiPositionSoundEmitter.eventIsPlaying = true;
			
			soundEmitterObject = multiPositionSoundEmitter.list[0].gameObject;
			
			if (enableActionOnEvent) 
				AkSoundEngine.ExecuteActionOnEvent ((uint)eventID, actionOnEventType, multiPositionSoundEmitter.list[0].gameObject, (int)transitionDuration * 1000, curveInterpolation);
			else 
				AkSoundEngine.PostEvent	(	(uint)eventID, 
				                         	multiPositionSoundEmitter.list[0].gameObject,
				                         	(uint)AkCallbackType.AK_EndOfEvent,
				                        	multiPositionSoundEmitter.FinishedPlaying,
				                         	null,
				                         	0,
				                         	null,
				                         	AkSoundEngine.AK_INVALID_PLAYING_ID
				                         );
		}
	}
	
	
	protected override void OnDestroy()
	{
#if UNITY_EDITOR	
		if (UnityEditor.EditorApplication.isPlaying)
#endif
		{
			base.OnDestroy ();
	
			if (AkSoundEngine.IsInitialized())
			{
				AkSoundEngine.UnregisterGameObj(gameObject);
			}
		}
	}
	
	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawIcon(transform.position, "WwiseAudioSpeaker.png", false);
	}
	
	public AkPositionArray BuildMultiDirectionArray( ref AkMultiPosEvent eventPosList)
	{
		AkPositionArray positionArray = new AkPositionArray((uint)eventPosList.list.Count);

		for (int i = 0; i < eventPosList.list.Count; i++) 
		{
			positionArray.Add (eventPosList.list [i].transform.position, eventPosList.list [i].transform.forward);
		}

		return positionArray;
	}
	
	AkPositionArray BuildAkPositionArray()
	{
		AkPositionArray positionArray = new AkPositionArray((uint)multiPositionArray.Count);

		for (int i = 0; i < multiPositionArray.Count; i++)
		{
			positionArray.Add( transform.position + multiPositionArray[i], transform.forward);
		}

		return positionArray;
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.