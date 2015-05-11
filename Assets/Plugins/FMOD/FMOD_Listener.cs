#if FMOD_LIVEUPDATE
#  define RUN_IN_BACKGROUND
#endif

using UnityEngine;
using System.Collections;
using FMOD.Studio;
using System.IO;
using System;

public class FMOD_Listener : MonoBehaviour 
{
	public string[] pluginPaths = 
    {
        // List plugin libraries here
    };
	
	static FMOD_Listener sListener = null;
	Rigidbody cachedRigidBody;
	
	void OnEnable()
	{		
		Initialize();
	}
	
	void OnDisable()
	{
		if (sListener == this)
			sListener = null;
	}
	
	void loadBank(string fileName)
	{
		string bankPath = getStreamingAsset(fileName);
		
		FMOD.Studio.Bank bank = null;
		FMOD.RESULT result = FMOD_StudioSystem.instance.System.loadBankFile(bankPath, LOAD_BANK_FLAGS.NORMAL, out bank);
		if (result == FMOD.RESULT.ERR_VERSION)
		{
			FMOD.Studio.UnityUtil.LogError("These banks were built with an incompatible version of FMOD Studio.");
		}
		
		FMOD.Studio.UnityUtil.Log("bank load: " + (bank != null ? "suceeded" : "failed!!"));
	}
	
	string getStreamingAsset(string fileName)
	{
		string bankPath = "";
        if (Application.platform == RuntimePlatform.Android)
		{
			bankPath = "jar:file://" + Application.dataPath + "!/assets";
		}
        #if (UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6)
        else if (Application.platform == RuntimePlatform.MetroPlayerARM || 
                 Application.platform == RuntimePlatform.MetroPlayerX86 || 
                 Application.platform == RuntimePlatform.MetroPlayerX64
            )
        #else // UNITY 5 enum
        else if (Application.platform == RuntimePlatform.WSAPlayerARM || 
		         Application.platform == RuntimePlatform.WSAPlayerX86 || 
		         Application.platform == RuntimePlatform.WSAPlayerX64
            )
        #endif
        {
            // TODO: not sure but I don't think Application.streamingAssetsPath has ms-appx:// format
            bankPath = "ms-appx:///Data/StreamingAssets";
        }
        else
        {
            bankPath = Application.streamingAssetsPath;
		}
		
		string assetPath = bankPath + "/" + fileName;
		
        #if UNITY_ANDROID && !UNITY_EDITOR
		// Unpack the compressed JAR file
		string unpackedJarPath = Application.persistentDataPath + "/" + fileName;
		
		FMOD.Studio.UnityUtil.Log("Unpacking bank from JAR file into:" + unpackedJarPath);
		
		if (File.Exists(unpackedJarPath))
		{
			FMOD.Studio.UnityUtil.Log("File already unpacked!!");
			File.Delete(unpackedJarPath);
			
			if (File.Exists(unpackedJarPath))
			{
				FMOD.Studio.UnityUtil.Log("Could NOT delete!!");				
			}
		}
		
		WWW dataStream = new WWW(assetPath);
		
		while(!dataStream.isDone) {} // FIXME: not safe
		
		
		if (!String.IsNullOrEmpty(dataStream.error))
		{
	        FMOD.Studio.UnityUtil.LogError("### WWW ERROR IN DATA STREAM:" + dataStream.error);
		}
		
		FMOD.Studio.UnityUtil.Log("Android unpacked jar path: " + unpackedJarPath);
		
		File.WriteAllBytes(unpackedJarPath, dataStream.bytes);
		
		//FileInfo fi = new FileInfo(unpackedJarPath);
		//FMOD.Studio.UnityUtil.Log("Unpacked bank size = " + fi.Length);
		
		assetPath = unpackedJarPath;
        #endif

		return assetPath;
	}
	
#if UNITY_METRO && NETFX_CORE
	async void Initialize()
#else
	void Initialize()
#endif
	{
		FMOD.Studio.UnityUtil.Log("Initialize Listener");

		if (sListener != null)
		{
			FMOD.Studio.UnityUtil.LogError("Too many listeners");
		}
		
		sListener = this;
		
		LoadPlugins();
		
		const string listFileName = "FMOD_bank_list.txt";
		string bankListPath = getStreamingAsset(listFileName);
        FMOD.Studio.UnityUtil.Log("Loading Banks");
        try
        {            
            
#if UNITY_METRO && NETFX_CORE
            var reader = Windows.Storage.PathIO.ReadLinesAsync(bankListPath, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            await reader.AsTask().ConfigureAwait(true);
            var bankList = reader.GetResults();
#else
			var bankList = System.IO.File.ReadAllLines(bankListPath);
#endif
			foreach (var bankName in bankList)
			{
				FMOD.Studio.UnityUtil.Log("Load " + bankName);
				loadBank(bankName);
			}
        }
        catch (Exception e)
        {
            FMOD.Studio.UnityUtil.LogError("Cannot read " + bankListPath + ": " + e.Message + " : No banks loaded.");
        }
		
		cachedRigidBody = GetComponent<Rigidbody>();
		
		Update3DAttributes();
	}
	
	void Start()
	{
#if UNITY_EDITOR && RUN_IN_BACKGROUND
		Application.runInBackground = true; // Prevent execution pausing when editor loses focus
#endif
	}
	
	void Update()
	{
		Update3DAttributes();
	}
	
	void Update3DAttributes()
	{
		var sys = FMOD_StudioSystem.instance.System;
		
		if (sys != null && sys.isValid())
		{
			var attributes = UnityUtil.to3DAttributes(gameObject, cachedRigidBody);		
			ERRCHECK(sys.setListenerAttributes(0, attributes));
		}
	}
	
	void LoadPlugins()
	{
		FMOD.System sys = null;
		ERRCHECK(FMOD_StudioSystem.instance.System.getLowLevelSystem(out sys));
		
		if (Application.platform == RuntimePlatform.IPhonePlayer && pluginPaths.Length != 0)
		{
			FMOD.Studio.UnityUtil.LogError("DSP Plugins not currently supported on iOS, contact support@fmod.org for more information");
			return;
		}
		
		foreach (var name in pluginPaths)
		{
			var path = pluginPath + "/" + GetPluginFileName(name);
			
			FMOD.Studio.UnityUtil.Log("Loading plugin: " + path);
            
#if !UNITY_METRO
			if (!System.IO.File.Exists(path))
            {
                FMOD.Studio.UnityUtil.LogWarning("plugin not found: " + path);
            }
#endif
			
			uint handle;
			ERRCHECK(sys.loadPlugin(path, out handle));
		}
	}	
	
	string pluginPath
	{
		get
		{
#if UNITY_METRO
            if (Application.platform == RuntimePlatform.MetroPlayerARM)
            {
                return Application.dataPath + "/Plugins/arm";
            }
            else if (Application.platform == RuntimePlatform.MetroPlayerX86)
            {
                return Application.dataPath + "/Plugins/x86";
            }
#else
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
#if UNITY_5 && UNITY_64
                return Application.dataPath + "/Plugins/x86_64";
#else
				return Application.dataPath + "/Plugins/x86";
#endif
			}
			else if (Application.platform == RuntimePlatform.WindowsPlayer ||
			         Application.platform == RuntimePlatform.OSXEditor ||
			         Application.platform == RuntimePlatform.OSXPlayer ||
			         Application.platform == RuntimePlatform.OSXDashboardPlayer ||
			         Application.platform == RuntimePlatform.LinuxPlayer
#if UNITY_PS4
				     || Application.platform == RuntimePlatform.PS4
#endif
#if UNITY_XBOXONE
				     || Application.platform == RuntimePlatform.XboxOne
#endif
			    	)
			{
				return Application.dataPath + "/Plugins";
			}
			else if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				FMOD.Studio.UnityUtil.LogError("DSP Plugins not currently supported on iOS, contact support@fmod.org for more information");
				return "";
			}
			else if (Application.platform == RuntimePlatform.Android)
			{
				var dirInfo = new System.IO.DirectoryInfo(Application.persistentDataPath);
				string packageName = dirInfo.Parent.Name;
				return "/data/data/" + packageName + "/lib";
			}
			
#endif // #if !UNITY_METRO
			FMOD.Studio.UnityUtil.LogError("Unknown platform!");
			return "";
		}
	}
	
	string GetPluginFileName(string rawName)
	{
		if (Application.platform == RuntimePlatform.WindowsEditor ||
			Application.platform == RuntimePlatform.WindowsPlayer
#if UNITY_XBOXONE
		    || Application.platform == RuntimePlatform.XboxOne
#endif
		    )
		{
			return rawName + ".dll";
		}
		else if (Application.platform == RuntimePlatform.OSXEditor ||
		         Application.platform == RuntimePlatform.OSXPlayer ||
		         Application.platform == RuntimePlatform.OSXDashboardPlayer)
		{
			return rawName + ".dylib";
		}
		else if (Application.platform == RuntimePlatform.Android ||
		         Application.platform == RuntimePlatform.LinuxPlayer)
		{
			return "lib" + rawName + ".so";
		}
#if UNITY_PS4
		else if (Application.platform == RuntimePlatform.PS4)
		{
			return rawName + ".prx";
		}
#endif
		
		FMOD.Studio.UnityUtil.LogError("Unknown platform!");
		return "";		
	}

	void ERRCHECK(FMOD.RESULT result)
	{
		FMOD.Studio.UnityUtil.ERRCHECK(result);
	}
}
