#if UNITY_EDITOR && UNITY_5
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;

public class AkPluginActivator
{
	public const string ALL_PLATFORMS = "All";
	public const string CONFIG_DEBUG = "Debug";
	public const string CONFIG_PROFILE = "Profile";
	public const string CONFIG_RELEASE = "Release";
	
	
	// Use reflection because projects that were created in Unity 4 won't have the CurrentPluginConfig field
	static string GetCurrentConfig()
	{
		//populate trigger list
		FieldInfo CurrentPluginConfigField = typeof(AkWwiseProjectData).GetField("CurrentPluginConfig");
		string CurrentConfig = string.Empty;
		if( CurrentPluginConfigField != null )
		{
			CurrentConfig = (string)CurrentPluginConfigField.GetValue(AkWwiseProjectInfo.GetData ());
		}
		
		return CurrentConfig;
	}

	static void SetCurrentConfig(string config)
	{
		//populate trigger list
		FieldInfo CurrentPluginConfigField = typeof(AkWwiseProjectData).GetField("CurrentPluginConfig");
		if( CurrentPluginConfigField != null )
		{
			CurrentPluginConfigField.SetValue(AkWwiseProjectInfo.GetData (), config);
		}
	}
	
	[MenuItem("Assets/Wwise/Activate Plugins/Debug")]
	public static void ActivateDebug()
	{
		if( GetCurrentConfig() != CONFIG_DEBUG )
		{
			SetPlugin (ALL_PLATFORMS, GetCurrentConfig(), false);
			SetCurrentConfig(CONFIG_DEBUG);
			SetPlugin(ALL_PLATFORMS, CONFIG_DEBUG, true);
		}
		else
		{
			Debug.Log ("AkSoundEngine Plugins already activated for Debug.");
		}
	}
	
	[MenuItem("Assets/Wwise/Activate Plugins/Profile")]
	public static void ActivateProfile()
	{
		if( GetCurrentConfig() != CONFIG_PROFILE )
		{
			SetPlugin (ALL_PLATFORMS, GetCurrentConfig(), false);
			SetCurrentConfig(CONFIG_PROFILE);
			SetPlugin(ALL_PLATFORMS, CONFIG_PROFILE, true);
		}
		else
		{
			Debug.Log ("AkSoundEngine Plugins already activated for Profile.");		
		}
	}
	
	[MenuItem("Assets/Wwise/Activate Plugins/Release")]
	public static void ActivateRelease()
	{
		if( GetCurrentConfig() != CONFIG_RELEASE )
		{
			SetPlugin (ALL_PLATFORMS, GetCurrentConfig(), false);
			SetCurrentConfig(CONFIG_RELEASE);
			SetPlugin(ALL_PLATFORMS, CONFIG_RELEASE, true);
		}
		else
		{
			Debug.Log ("AkSoundEngine Plugins already activated for Release.");		
		}
	}
	
	public static void RefreshPlugins()
	{
		SetPlugin (ALL_PLATFORMS, GetCurrentConfig(), true);
	}
	
	private static void SetStandaloneTarget(PluginImporter pluginImporter, BuildTarget target)
	{
		switch(target)
		{
		case BuildTarget.StandaloneLinux:
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinux, "CPU", "x86");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinux64, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinuxUniversal, "CPU", "x86");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXIntel, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXIntel64, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXUniversal, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneWindows, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneWindows64, "CPU", "None");
			return;
		case BuildTarget.StandaloneLinux64:
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinux, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinux64, "CPU", "x86_64");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinuxUniversal, "CPU", "x86_64");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXIntel, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXIntel64, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXUniversal, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneWindows, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneWindows64, "CPU", "None");
			return;
		case BuildTarget.StandaloneOSXIntel:
		case BuildTarget.StandaloneOSXIntel64:
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinux, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinux64, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinuxUniversal, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXIntel, "CPU", "AnyCPU");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXIntel64, "CPU", "AnyCPU");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXUniversal, "CPU", "AnyCPU");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneWindows, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneWindows64, "CPU", "None");
			return;
		case BuildTarget.StandaloneWindows:
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinux, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinux64, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinuxUniversal, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXIntel, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXIntel64, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXUniversal, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneWindows, "CPU", "AnyCPU");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneWindows64, "CPU", "None");
			return;
		case BuildTarget.StandaloneWindows64:
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinux, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinux64, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneLinuxUniversal, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXIntel, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXIntel64, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneOSXUniversal, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneWindows, "CPU", "None");
			pluginImporter.SetPlatformData (BuildTarget.StandaloneWindows64, "CPU", "AnyCPU");
			return;
		default:
			return;
		}
	}
	
	// Properly set the platforms for Ak plugins. If platformToActivate is set to ALL_PLATFORMS, all platforms
	// will be activated.
	public static void SetPlugin(string platformToActivate, string configToActivate, bool Activate)
	{
		PluginImporter[] importers = PluginImporter.GetAllImporters();
		bool ChangedSomeAssets = false;

		foreach(PluginImporter pluginImporter in importers)
		{
			if( pluginImporter.assetPath.StartsWith("Assets/Plugins", StringComparison.CurrentCultureIgnoreCase) && pluginImporter.assetPath.Contains("AkSoundEngine") )
			{
				AssetDatabase.DeleteAsset(pluginImporter.assetPath);
				continue;
			}

			if( !pluginImporter.assetPath.Contains("AkSoundEngine") )
			{
				continue;
			}
			
			// Special case for WP8. Somehow, unity thinks .cpp and .h files are plugins
			if( pluginImporter.assetPath.Contains(".h") || pluginImporter.assetPath.Contains(".cpp") )
			{
				if( pluginImporter.GetCompatibleWithAnyPlatform() )
				{
					pluginImporter.SetCompatibleWithAnyPlatform(false);
					AssetDatabase.ImportAsset(pluginImporter.assetPath);
				}
				continue;
			}
									
			string[] splitPath = pluginImporter.assetPath.Split('/');
			
			// Path is Assets/Wwise/Deployment/Plugins/Platform. We need the platform string
			string pluginPlatform = splitPath[4];
			if( pluginPlatform != platformToActivate && platformToActivate != ALL_PLATFORMS )
			{
				continue;
			}
			
			// Architecture and configuration (Debug, Profile, Release) are platform-dependent
			string pluginArch = string.Empty;
			string pluginConfig = string.Empty;
			string editorCPU = string.Empty;
			string editorOS = string.Empty;
			List<BuildTarget> targetsToSet = new List<BuildTarget>();
			bool setEditor = false;
			switch (pluginPlatform)			
			{
			case "Android":
				pluginConfig = splitPath[6];
				targetsToSet.Add (BuildTarget.Android);
				pluginImporter.SetPlatformData(BuildTarget.Android, "CPU", "ARMv7");
				break;
			case "iOS":
				pluginConfig = splitPath[5];
				targetsToSet.Add (BuildTarget.iOS);
				break;
			case "Linux":
				pluginArch = splitPath[5];
				pluginConfig = splitPath[6];
				if( pluginArch == "x86" )
				{
					targetsToSet.Add (BuildTarget.StandaloneLinux);
					SetStandaloneTarget(pluginImporter, BuildTarget.StandaloneLinux);
				}
				else if( pluginArch == "x86_64" )
				{
					targetsToSet.Add (BuildTarget.StandaloneLinux64);
					SetStandaloneTarget(pluginImporter, BuildTarget.StandaloneLinux64);
				}
				else
				{
					Debug.Log("Architecture not found: " + pluginArch);
				}
				targetsToSet.Add (BuildTarget.StandaloneLinuxUniversal);
				break;
			case "Mac":
				pluginConfig = splitPath[5];
				SetStandaloneTarget(pluginImporter, BuildTarget.StandaloneOSXIntel);
				SetStandaloneTarget(pluginImporter, BuildTarget.StandaloneOSXIntel64);
				targetsToSet.Add (BuildTarget.StandaloneOSXIntel);
				targetsToSet.Add (BuildTarget.StandaloneOSXIntel64);
				targetsToSet.Add(BuildTarget.StandaloneOSXUniversal);
				editorCPU = "AnyCPU";
				editorOS = "OSX";
				setEditor = true;
				break;
			case "Metro":
				pluginConfig = splitPath[6];
				targetsToSet.Add (BuildTarget.WSAPlayer);
				break;
			case "PS3":
				pluginConfig = splitPath[5];
				targetsToSet.Add(BuildTarget.PS3);
				break;
			case "PS4":
				pluginConfig = splitPath[5];
				targetsToSet.Add(BuildTarget.PS4);
				break;
			case "Vita":
				pluginConfig = splitPath[6];
				targetsToSet.Add(BuildTarget.PSP2);
				break;
			case "Windows":
				pluginArch = splitPath[5];
				pluginConfig = splitPath[6];
				if( pluginArch == "x86" )
				{
					targetsToSet.Add (BuildTarget.StandaloneWindows);
					SetStandaloneTarget(pluginImporter, BuildTarget.StandaloneWindows);
					editorCPU = "X86";
				}
				else if( pluginArch == "x86_64" )
				{
					targetsToSet.Add (BuildTarget.StandaloneWindows64);
					SetStandaloneTarget(pluginImporter, BuildTarget.StandaloneWindows64);
					editorCPU = "X86_64";
				}
				else
				{
					Debug.Log("Architecture not found: " + pluginArch);
				}
				setEditor = true;
				editorOS = "Windows";
				break;
			case "XBox360":
				pluginConfig = splitPath[5];
				targetsToSet.Add(BuildTarget.XBOX360);
				break;
			case "XboxOne":
				pluginConfig = splitPath[5];
				targetsToSet.Add(BuildTarget.XboxOne);
				break;
			case "WiiU": // todo: wiiu not in buildtarget...
			default:
				Debug.Log ("Unknown platform: " + pluginPlatform);
				continue;
			}
			
			bool AssetChanged = false;
			
			if( pluginImporter.GetCompatibleWithAnyPlatform() )
			{
				pluginImporter.SetCompatibleWithAnyPlatform(false);
				AssetChanged = true;
			}
			
			if( pluginConfig != configToActivate )
			{
				// This is not the configuration we want to activate, make sure it is deactivated
				foreach(BuildTarget target in targetsToSet)
				{
					AssetChanged |= pluginImporter.GetCompatibleWithPlatform(target);
					pluginImporter.SetCompatibleWithPlatform(target, false);
				}
				if( setEditor )
				{
					AssetChanged |= pluginImporter.GetCompatibleWithEditor();
					pluginImporter.SetCompatibleWithEditor(false);
				}
			}
			else
			{
				// Set this plugin
				foreach(BuildTarget target in targetsToSet)
				{
					AssetChanged |= (pluginImporter.GetCompatibleWithPlatform(target) != Activate);
					pluginImporter.SetCompatibleWithPlatform(target, Activate);
				}
				
				if( setEditor )
				{
					AssetChanged |= (pluginImporter.GetCompatibleWithEditor() != Activate);
					pluginImporter.SetCompatibleWithEditor(Activate);
					pluginImporter.SetEditorData("CPU", editorCPU);
					pluginImporter.SetEditorData("OS", editorOS);
				}
			}

			if( AssetChanged )
			{
				ChangedSomeAssets = true;
				AssetDatabase.ImportAsset(pluginImporter.assetPath);
			}
		}
		
		if( Activate && ChangedSomeAssets )
		{
			Debug.Log ("AkSoundEngine Plugins successfully activated for " + configToActivate + ".");
		}
	}
	
	public static void DeactivateAllPlugins()
	{
		PluginImporter[] importers = PluginImporter.GetAllImporters();
		
		foreach(PluginImporter pluginImporter in importers)
		{
			pluginImporter.SetCompatibleWithAnyPlatform(false);
			AssetDatabase.ImportAsset(pluginImporter.assetPath);
		}
	}
}
#endif