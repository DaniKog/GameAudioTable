using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FMOD
{
	namespace Studio
	{
		public static class UnityUtil
		{	
			static public VECTOR toFMODVector(this Vector3 vec)
			{
				VECTOR temp;
				temp.x = vec.x;
				temp.y = vec.y;
				temp.z = vec.z;
				
				return temp;
			}
			
			static public ATTRIBUTES_3D to3DAttributes(this Vector3 pos)
			{
				FMOD.Studio.ATTRIBUTES_3D attributes = new FMOD.Studio.ATTRIBUTES_3D();
				attributes.forward = toFMODVector(Vector3.forward);
				attributes.up = toFMODVector(Vector3.up);
				attributes.position = toFMODVector(pos);
				
				return attributes;
			}
			
			static public ATTRIBUTES_3D to3DAttributes(GameObject go, Rigidbody rigidbody = null)
			{
				FMOD.Studio.ATTRIBUTES_3D attributes = new FMOD.Studio.ATTRIBUTES_3D();
				attributes.forward = toFMODVector(go.transform.forward);
				attributes.up = toFMODVector(go.transform.up);
				attributes.position = toFMODVector(go.transform.position);
		
				if (rigidbody)
					attributes.velocity = toFMODVector(rigidbody.velocity);
				
				return attributes;
			}
			
			static public void Log(string msg)
			{
#if FMOD_DEBUG
				UnityEngine.Debug.Log(msg);
#endif
			}
			
			static public void LogWarning(string msg)
			{
                UnityEngine.Debug.LogWarning(msg);
			}
			
			static public void LogError(string msg)
			{
                UnityEngine.Debug.LogError(msg);
			}
			
			static public bool ForceLoadLowLevelBinary()
			{
				// This is a hack that forces Android to load the .so libraries in the correct order
#if UNITY_ANDROID && !UNITY_EDITOR

                if (FMOD.VERSION.number >= 0x00010500)
                {
                    AndroidJavaObject activity = null;

                    // First, obtain the current activity context
                    using (var actClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        activity = actClass.GetStatic<AndroidJavaObject>("currentActivity");
                    }

                    UnityEngine.Debug.Log("FMOD ANDROID AUDIOTRACK: " + (activity == null ? "ERROR NO ACTIVITY" : "VALID ACTIVITY!"));
 
                    using (var fmodJava = new AndroidJavaClass("org.fmod.FMOD"))
                    {
                        if (fmodJava != null)
                        {
                            UnityEngine.Debug.Log("FMOD ANDROID AUDIOTRACK: assigning activity to fmod java");

                            fmodJava.CallStatic("init", activity);
                        }
                        else
                        {
                            UnityEngine.Debug.Log("FMOD ANDROID AUDIOTRACK: ERROR NO FMOD JAVA");
                        }
                    }
                }

				FMOD.Studio.UnityUtil.Log("loading binaries: " + FMOD.Studio.STUDIO_VERSION.dll + " and " + FMOD.VERSION.dll);
				AndroidJavaClass jSystem = new AndroidJavaClass("java.lang.System");
				jSystem.CallStatic("loadLibrary", FMOD.VERSION.dll);
				jSystem.CallStatic("loadLibrary", FMOD.Studio.STUDIO_VERSION.dll);
                
#endif

                // Hack: force the low level binary to be loaded before accessing Studio API
#if !UNITY_IPHONE || UNITY_EDITOR
				FMOD.Studio.UnityUtil.Log("Attempting to call Memory_GetStats");
				int temp1, temp2;
				if (!ERRCHECK(FMOD.Memory.GetStats(out temp1, out temp2)))
				{
					FMOD.Studio.UnityUtil.LogError("Memory_GetStats returned an error");
					return false;
				}
		
				FMOD.Studio.UnityUtil.Log("Calling Memory_GetStats succeeded!");
#endif
		
				return true;
			}
			
			public static bool ERRCHECK(FMOD.RESULT result)
			{
				if (result != FMOD.RESULT.OK)
				{
					LogWarning("FMOD Error (" + result.ToString() + "): " + FMOD.Error.String(result));
				}
				
				return (result == FMOD.RESULT.OK);
			}
		}
	}
}

public class FMOD_StudioSystem : MonoBehaviour 
{
	FMOD.Studio.System system;
	Dictionary<string, FMOD.Studio.EventDescription> eventDescriptions = new Dictionary<string, FMOD.Studio.EventDescription>();
	bool isInitialized = false;
	
	static FMOD_StudioSystem sInstance;
	public static FMOD_StudioSystem instance
	{
		get
		{
			if (sInstance == null)
			{
				var go = new GameObject("FMOD_StudioSystem");
				sInstance = go.AddComponent<FMOD_StudioSystem>();
				
				if (!FMOD.Studio.UnityUtil.ForceLoadLowLevelBinary()) // do these hacks before calling ANY fmod functions!
				{
					FMOD.Studio.UnityUtil.LogError("Unable to load low level binary!");
					return sInstance;
				}
				sInstance.Init();
			}
			return sInstance;
		}
	}
	
	public FMOD.Studio.EventInstance GetEvent(FMODAsset asset)
	{
		return GetEvent(asset.id);
	}
	
	public FMOD.Studio.EventInstance GetEvent(string path)
	{
		FMOD.Studio.EventInstance instance = null;
		
		if (string.IsNullOrEmpty(path))
		{
			FMOD.Studio.UnityUtil.LogError("Empty event path!");
			return null;
		}
		
		if (eventDescriptions.ContainsKey(path) && eventDescriptions[path].isValid())
		{
			ERRCHECK(eventDescriptions[path].createInstance(out instance));
		}
		else
		{
			Guid id = new Guid();
			
			if (path.StartsWith("{"))
			{
				ERRCHECK(FMOD.Studio.Util.ParseID(path, out id));
			}
			else if (path.StartsWith("event:"))
			{
				ERRCHECK(system.lookupID(path, out id));
			}
			else
			{
				FMOD.Studio.UnityUtil.LogError("Expected event path to start with 'event:/'");
			}
			
			FMOD.Studio.EventDescription desc = null;
			ERRCHECK(system.getEventByID(id, out desc));
			
			if (desc != null && desc.isValid())
			{
				eventDescriptions[path] = desc;
				ERRCHECK(desc.createInstance(out instance));
			}
		}
		
		if (instance == null)
		{
			FMOD.Studio.UnityUtil.Log("GetEvent FAILED: \"" + path + "\"");
		}
		
		return instance;
	}
	
	public void PlayOneShot(FMODAsset asset, Vector3 position)
	{
		PlayOneShot(asset.id, position);
	}	
	
	public void PlayOneShot(string path, Vector3 position)
	{
		PlayOneShot(path, position, 1.0f);
	}
	
	public void PlayOneShot(string path, Vector3 position, float volume)
	{
		var instance = GetEvent(path);
		if (instance == null) 
		{			
			FMOD.Studio.UnityUtil.LogWarning("PlayOneShot couldn't find event: \"" + path + "\"");
			return;
		}
		
		var attributes = FMOD.Studio.UnityUtil.to3DAttributes(position);
		ERRCHECK( instance.set3DAttributes(attributes) );
		ERRCHECK( instance.setVolume(volume) );
		ERRCHECK( instance.start() );
		ERRCHECK( instance.release() );
	}
	
	public FMOD.Studio.System System
	{
		get { return system; }
	}
	
	void Init()
    {
        FMOD.Studio.UnityUtil.Log("FMOD_StudioSystem: Initialize");

        if (isInitialized)
        {
            return;
        }

        DontDestroyOnLoad(gameObject);

        FMOD.Studio.UnityUtil.Log("FMOD_StudioSystem: System_Create");
        ERRCHECK(FMOD.Studio.System.create(out system));

        FMOD.Studio.INITFLAGS flags = FMOD.Studio.INITFLAGS.NORMAL;

#if FMOD_LIVEUPDATE
        flags |= FMOD.Studio.INITFLAGS.LIVEUPDATE;
		
		// Unity 5 liveupdate workaround
        if (Application.unityVersion.StartsWith("5"))
        {
            FMOD.Studio.UnityUtil.LogWarning("FMOD_StudioSystem: detected Unity 5, running on port 9265");
            FMOD.System sys;
            ERRCHECK(system.getLowLevelSystem(out sys));
            FMOD.ADVANCEDSETTINGS advancedSettings = new FMOD.ADVANCEDSETTINGS();
            advancedSettings.profilePort = 9265;
            ERRCHECK(sys.setAdvancedSettings(ref advancedSettings));
        }
#endif

		#if FMOD_DEBUG
		FMOD.Debug.Initialize(FMOD.DEBUG_FLAGS.LOG, FMOD.DEBUG_MODE.CALLBACK, LogCallback, null);
		#endif


        FMOD.Studio.UnityUtil.Log("FMOD_StudioSystem: system.init");
        FMOD.RESULT result = FMOD.RESULT.OK;
        result = system.initialize(1024, flags, FMOD.INITFLAGS.NORMAL, global::System.IntPtr.Zero);

        if (result == FMOD.RESULT.ERR_HEADER_MISMATCH)
        {
            FMOD.Studio.UnityUtil.LogError("Version mismatch between C# script and FMOD binary, restart Unity and reimport the integration package to resolve this issue.");
        }
        else
        {
            ERRCHECK(result);
        }
		
		// Dummy flush and update to get network state
        ERRCHECK(system.flushCommands());
        result = system.update();
        
        // Restart without liveupdate if there was a socket error
        if (result == FMOD.RESULT.ERR_NET_SOCKET_ERROR)
        {
            FMOD.Studio.UnityUtil.LogWarning("LiveUpdate disabled: socket in already in use");
            flags &= ~FMOD.Studio.INITFLAGS.LIVEUPDATE;
            ERRCHECK(system.release());
            ERRCHECK(FMOD.Studio.System.create(out system));
            FMOD.System sys;
            ERRCHECK(system.getLowLevelSystem(out sys));
            result = system.initialize(1024, flags, FMOD.INITFLAGS.NORMAL, global::System.IntPtr.Zero);
            ERRCHECK(result);
        }

        isInitialized = true;
    }
	
	void OnApplicationPause(bool pauseStatus)
	{
		if (system != null && system.isValid())
		{			
			FMOD.Studio.UnityUtil.Log("Pause state changed to: " + pauseStatus);

			FMOD.System sys;
			ERRCHECK(system.getLowLevelSystem(out sys));

			if (sys == null)
			{
				FMOD.Studio.UnityUtil.LogError("Tried to suspend mixer, but no low level system found");
				return;
			}
			
			if (pauseStatus)
			{
				ERRCHECK(sys.mixerSuspend());
			}
			else
			{
				ERRCHECK(sys.mixerResume());
			}
		}
	}
	
	void Update() 
	{
		if (isInitialized)
		{
			ERRCHECK(system.update());
		}
	}
	
	void OnDisable()
	{
		if (isInitialized)
		{
			FMOD.Studio.UnityUtil.Log("__ SHUT DOWN FMOD SYSTEM __");
			ERRCHECK(system.release());

            if (this == sInstance)
            {
                sInstance = null;
            }
		}
	}
	
	FMOD.RESULT LogCallback(FMOD.DEBUG_FLAGS flags, string file, int line, string func, string message)
	{
		string formattedMessage = String.Format("{2}\t{3}", file, line, func, message);
		if ((flags & FMOD.DEBUG_FLAGS.ERROR) > 0)
		{
			FMOD.Studio.UnityUtil.LogError(formattedMessage);
		}
		else if ((flags & FMOD.DEBUG_FLAGS.WARNING) > 0)
		{
			FMOD.Studio.UnityUtil.LogWarning(formattedMessage);
		}
		else
		{
			FMOD.Studio.UnityUtil.Log(formattedMessage);
		}
        return FMOD.RESULT.OK;
	}
	
	static bool ERRCHECK(FMOD.RESULT result)
	{
		return FMOD.Studio.UnityUtil.ERRCHECK(result);
	}
}
