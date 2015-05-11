#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections.Generic;

[AddComponentMenu("Wwise/AkEnvironment")]
/// @brief Use this component to define a reverb zone.  This needs to be added to a collider object to work properly.
/// @details This component can be attached to any collider.  You can specify a roll-off to fade-in/out of the reverb.  
/// The reverb parameters will be defined in the Wwise project, by the sound designer.  All AkGameObj that are 
/// "environement"-aware will receive a send value when entering the attached collider.
/// \sa
/// - \ref integrating_elements_environments
/// - \ref AK::SoundEngine::SetGameObjectAuxSendValues
[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(Collider))]
public class AkEnvironment : MonoBehaviour
{
	public static int MAX_NB_ENVIRONMENTS = 4;

	public class AkEnvironment_CompareByPriority: IComparer<AkEnvironment>
	{
		public int Compare(AkEnvironment a, AkEnvironment b)	
		{
		 	int result = a.priority.CompareTo(b.priority);

			if(result == 0 && a != b)
				return 1;
			else
				return result;
		}
	}
	static public AkEnvironment_CompareByPriority s_compareByPriority = new AkEnvironment_CompareByPriority();



	///The selection algorithm is as follow:
	///1. Environments have priorities (already the case)
	///2. Environments have a "Default" flag.  This flag effectively says that this environement will be bumped out if any other is present.
	///3. Environements have a "Exclude Other" flag.  This flag will tell that this env is not overlappable with others.  So only one (the highest priority) should be selected.
	public class AkEnvironment_CompareBySelectionAlgorithm: IComparer<AkEnvironment>
	{
		int compareByPriority(AkEnvironment a, AkEnvironment b)
		{
			int result = a.priority.CompareTo(b.priority);
			
			if(result == 0 && a != b)
				return 1;
			else
				return result;
		}

		public int Compare(AkEnvironment a, AkEnvironment b)	
		{
			if(a.isDefault)
			{
				if(b.isDefault)
				{
					return compareByPriority(a, b);
				}
				else
				{
					return 1;
				}
			}
			else
			{
				if(b.isDefault)
				{
					return -1;
				}
				else
				{
					//Here a and b are not default. So they could be excludeOthers
					if(a.excludeOthers)
					{
						if(b.excludeOthers)
						{
							return compareByPriority(a, b);
						}
						else
						{
							return -1;
						}
					}
					else
					{
						if(b.excludeOthers)
						{
							return 1;
						}
						else
						{
							return compareByPriority(a, b);
						}
					}
				}
			}
		}
	}
	static public AkEnvironment_CompareBySelectionAlgorithm s_compareBySelectionAlgorithm = new AkEnvironment_CompareBySelectionAlgorithm();

#if UNITY_EDITOR
	public byte[] valueGuid = new byte[16];
#endif

	[SerializeField]
	private int m_auxBusID;

	//smaller number has a higher priority
	public int priority = 0;

	//if isDefault, then this environement will be bumped out if any other is present 
	public bool isDefault = false;

	//if excludeOthers, then only the environment with the excludeOthers flag set to true and with the 
	//hightest priority will be active
	public bool excludeOthers = false;


    public uint GetAuxBusID()
    {
        return (uint)m_auxBusID;
    }

	public void SetAuxBusID(int in_auxBusID)
	{
		m_auxBusID = in_auxBusID;
	}

	public float GetAuxSendValueForPosition(Vector3 in_position)
	{
		return 1;
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.