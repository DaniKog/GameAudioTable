#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// Base class for the generic triggering mechanism for Wwise Integration.
/// All Wwise components will use this mechanism to drive their behavior.
/// Derive from this class to add your own triggering condition, as decribed in \ref unity_add_triggers
public abstract class AkTriggerBase : MonoBehaviour 
{
	/// Delegate declaration for all Wwise Triggers.  
	public delegate void Trigger(
	GameObject in_gameObject ///< in_gameObject is used to pass "Collidee" objects when Colliders are used.  Some components have the option "Use other object", this is the object they'll use.
	);
	
	/// All components reacting to the trigger will be registered in this delegate.
	public Trigger triggerDelegate = null;  

	public static Dictionary<uint, string> GetAllDerivedTypes()
	{

		Type	baseType	= typeof(AkTriggerBase);        
#if UNITY_METRO && !UNITY_EDITOR        
        IEnumerable<TypeInfo> typeInfos = baseType.GetTypeInfo().Assembly.DefinedTypes;
#else
        Type[]  types       = baseType.Assembly.GetTypes(); // THIS WORKS ON WP8, not on Metro
#endif

        Dictionary<uint, string> derivedTypes = new Dictionary<uint, string>();
		
#if UNITY_METRO && !UNITY_EDITOR        
 		foreach(TypeInfo typeInfo in typeInfos)
		{
            if(typeInfo.IsClass && (typeInfo.IsSubclassOf(baseType) || baseType.GetTypeInfo().IsAssignableFrom(typeInfo) && baseType != typeInfo.AsType()))
            {
                string typeName = typeInfo.Name;
				derivedTypes.Add(AkUtilities.ShortIDGenerator.Compute(typeName), typeName);
			}
		}
#else
        for (int i = 0; i < types.Length; i++)
		{
            if (types[i].IsClass && (types[i].IsSubclassOf(baseType) || baseType.IsAssignableFrom(types[i]) && baseType != types[i]))
            {
                string typeName = types[i].Name;
                derivedTypes.Add(AkUtilities.ShortIDGenerator.Compute(typeName), typeName);
			}
		}
#endif

        //Add the Awake, Start and Destroy triggers and build the displayed list.
        derivedTypes.Add(AkUtilities.ShortIDGenerator.Compute("Awake"), "Awake");
        derivedTypes.Add(AkUtilities.ShortIDGenerator.Compute("Start"), "Start");
        derivedTypes.Add(AkUtilities.ShortIDGenerator.Compute("Destroy"), "Destroy");
		
		return derivedTypes;
	}
} 

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.