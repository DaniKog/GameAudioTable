#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using System.IO;

public class WwiseSetupWizard : WwiseSetupWindow 
{
	
	public 	static WwiseSettings Settings = WwiseSettings.LoadSettings();
    private static Texture2D m_Logo = null;
	private static WwiseSetupWizard windowInstance = null;
	private static string m_newIntegrationVersion = string.Empty;

    private const uint SETUP_WINDOW_WIDTH = 1000;
    private const uint SETUP_WINDOW_HEIGHT = 475;

	public static void Init()
	{
		// Get existing open window or if none, make a new one:
        Settings = WwiseSettings.LoadSettings();
		if( windowInstance == null)
		{
			windowInstance = ScriptableObject.CreateInstance<WwiseSetupWizard> ();
            windowInstance.position = new Rect((Screen.currentResolution.width - SETUP_WINDOW_WIDTH) / 2, (Screen.currentResolution.height - SETUP_WINDOW_HEIGHT) / 2, SETUP_WINDOW_WIDTH, SETUP_WINDOW_HEIGHT);
            windowInstance.minSize = new Vector2(SETUP_WINDOW_WIDTH, SETUP_WINDOW_HEIGHT);
			windowInstance.title = "Wwise Setup";
			windowInstance.Show();
		}
    }

	public static void CloseWindow()
	{
		windowInstance.Close ();
		ScriptableObject.DestroyImmediate(windowInstance);
	}
	
	// Go get the Wwise Logo from the Wwise installation folder
    void FetchWwiseLogo()
    {
        // Pre-fetch the wwise logo
        string logoPath = Path.Combine(Application.dataPath, "Wwise\\Gizmos\\wwise_white_on_gray.png");
        logoPath = logoPath.Replace('\\', Path.DirectorySeparatorChar);
        m_Logo = new Texture2D(4, 4);
        try
        {
            FileStream fs = new FileStream(logoPath, FileMode.Open, FileAccess.Read);
            byte[] imageData = new byte[fs.Length];
            fs.Read(imageData, 0, (int)fs.Length);
            m_Logo.LoadImage(imageData);
			
			// Get the Wwise version as well
			string[] newVersionTxtLines = System.IO.File.ReadAllLines(Application.dataPath + "/Wwise/Version.txt");
			m_newIntegrationVersion = newVersionTxtLines[4].Split(' ')[4];
			if(m_newIntegrationVersion[m_newIntegrationVersion.Length-1] == '0') 
				m_newIntegrationVersion = m_newIntegrationVersion.Remove(m_newIntegrationVersion.Length-2);
			
        }
        catch (Exception)
        {
            // Fail silentely, not too bad if we can't show the image...
        }
    }

	
	void OnGUI() 
	{
        // Make sure everything is initialized
		if (m_Logo == null)
        {
            FetchWwiseLogo();
        }
        if (WelcomeStyle == null)
        {
            InitGuiStyles();
        }
		// Use soundbank path, because Wwise project path can be empty.
        if (String.IsNullOrEmpty(Settings.SoundbankPath) && Settings.WwiseProjectPath == null)
        {
            Settings = WwiseSettings.LoadSettings();
        }
		
        GUILayout.BeginHorizontal("box");
        GUILayout.Label(m_Logo, GUILayout.Width(m_Logo.width));
        GUILayout.Label("Welcome to the Wwise Unity Integration " + m_newIntegrationVersion + "!", WelcomeStyle, GUILayout.Height(m_Logo.height));
        GUILayout.EndHorizontal();
        
		// Make the HelpBox font size a little bigger
		GUILayout.Label(
@"This setup wizard will perform the first-time setup of the Wwise Unity integration. 
If this is the first time the Wwise Unity integration is installed for this game project, simply fill in the required fields, and click ""Start Installation"".

If a previous version of the integration has already been installed on this game project, it is still recommended to fill out the required settings below and completing the installation. The game project will be updated to match the new version of the Wwise Unity integration.

To get more information on the installation process, please refer to the ""Install the integration in a Unity project"" section of the integration documentation, found under the menu Help -> Wwise Help.

This integration relies on data from a " + m_newIntegrationVersion + @" Wwise project. Note that it is recommended for the Wwise project to reside in the game project's root folder.
        
For more information on a particular setting, hover your mouse over it.", 
			HelpStyle);
			
		DrawSettingsPart();

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Start Installation", GUILayout.Width(200)))
		{
			if( string.IsNullOrEmpty(Settings.WwiseProjectPath) || string.IsNullOrEmpty(Settings.SoundbankPath) )
			{
				EditorUtility.DisplayDialog("Error", "Please fill all mandatory settings", "Ok");
			}
			else
			{
				WwiseSettings.SaveSettings(Settings);
				if (Setup())
				{
					Debug.Log("WwiseUnity integration installation completed successfully");
				}
				else
				{
					Debug.LogError("Could not complete Wwise Unity integration installation");
				}

				// Setup done; close the window
				WwiseSetupWizard.CloseWindow();
				
				if( !string.IsNullOrEmpty(Settings.WwiseProjectPath) )
				{
					// Pop the Picker window so the user can start working right away
					AkWwisePicker.init();
				}
				
				ShowHelp();
			}
		}
		
        if (GUILayout.Button("Cancel Installation", GUILayout.Width(200)))
		{
			// Ask "Are you sure?"
			if( EditorUtility.DisplayDialog("Warning", "This will completely remove the Wwise Unity Integration. Are you sure?", "Yes", "No") )
			{
				UninstallIntegration();
				WwiseSetupWizard.CloseWindow();
			}
		}		
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.EndVertical();

	}

	void OnDisable()
	{
		if( !File.Exists(Path.Combine(Application.dataPath, WwiseSettings.WwiseSettingsFilename)))
		{
			// User has "clicked the x" to close the window, do as if "Cancel" was clicked.
			UninstallIntegration();
		}
	}

	public static void ShowHelp()
	{
		try
		{
#if UNITY_EDITOR_WIN
			string arg = "mk:@MSITStore:" + Application.dataPath + "/Wwise/Documentation/WindowsCommon/en/WwiseUnityIntegrationHelp_en.chm::/pg__installation.html";
			System.Diagnostics.Process.Start("hh.exe",arg);
#elif UNITY_EDITOR_OSX
			string DestPath = AkUtilities.GetFullPath(Application.dataPath, "..");
			UnzipHelp (DestPath);
			string docPath = string.Format ("{0}/WwiseUnityIntegrationHelp_AppleCommon_en/index.html", DestPath);
			if( File.Exists (docPath) )
			{
				AkDocHelper.OpenDoc(docPath);
			}
			else
			{
				Debug.LogError ("Wwise documentation not found: " + docPath);
			}
#endif
		}
		catch(Exception)
		{
			Debug.Log("Wwise: Unable to show documentation. Please unzip WwiseUnityIntegrationHelp_AppleCommon_en.zip manually.");
		}
	}

	public static void UnzipHelp(string DestPath)
	{
		// Start by extracting the zip, if it exists
		string ZipPath = Path.Combine (Path.Combine (Path.Combine (Path.Combine (Path.Combine (Application.dataPath, "Wwise"), "Documentation"), "AppleCommon"), "en"), "WwiseUnityIntegrationHelp_AppleCommon_en.zip");

		if( File.Exists(ZipPath) )
		{
			System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo();
			start.FileName = "unzip";
			
			start.Arguments = "\"" + ZipPath + "\" -d \"" + DestPath + "\"";
			
			start.UseShellExecute = true;
			start.RedirectStandardOutput = false;
			
			string progMsg = "Wwise: Unzipping documentation...";
			string progTitle = "Unzipping Wwise documentation";
			EditorUtility.DisplayProgressBar(progTitle, progMsg, 0.5f);
			
			using(System.Diagnostics.Process process = System.Diagnostics.Process.Start(start))
			{
				while(!process.WaitForExit(1000))
				{
					System.Threading.Thread.Sleep (100);
				}
				try
				{
					//ExitCode throws InvalidOperationException if the process is hanging
					int returnCode = process.ExitCode;
					
					bool isBuildSucceeded = ( returnCode == 0 );
					if ( isBuildSucceeded )
					{
						EditorUtility.DisplayProgressBar(progTitle, progMsg, 1.0f);
						UnityEngine.Debug.Log("Wwise: Documentation extraction succeeded. ");
					}
					else
					{
						UnityEngine.Debug.LogError("Wwise: Extraction failed.");
					}
					
					EditorUtility.ClearProgressBar();
				}
				catch (Exception ex)
				{
					EditorUtility.ClearProgressBar();
					UnityEngine.Debug.LogError(ex.ToString ());
					EditorUtility.ClearProgressBar();
				}
			}
		}
	}

    // Perform all necessary steps to use the Wwise Unity integration.
	bool Setup()
	{
        bool NoErrorHappened = true;
		
		// 0. Make sure the soundbank directory exists
		string sbPath = AkUtilities.GetFullPath(Application.streamingAssetsPath, Settings.SoundbankPath);
		if( !Directory.Exists (sbPath) )
		{
			Directory.CreateDirectory(sbPath);
		}
		
#if !UNITY_5		
		// 1. Disable built-in audio
		if( !AkUnitySettingsParser.SetBoolValue("m_DisableAudio", true, "AudioManager") )
		{
			EditorUtility.DisplayDialog("Warning", "The Audio settings file format has changed. Please disable built-in audio by going to Project->Project Settings->Audio, and check \"Disable Audio\".", "Ok");
		}
#endif
		
        // 2. Create a "WwiseGlobal" game object and set the AkSoundEngineInitializer and terminator scripts
		// 3. Set the SoundBank path property on AkSoundEngineInitializer
        if (!Settings.OldProject)
		    CreateWwiseGlobalObject();
		
		// 4. Set the script order of AkInitializer, AkTerminator, AkGameObj, AkBankLoad (before default time), AkAudioListener by changing the .meta file
        if (!SetAllScriptExecutionOrder())
        {
            EditorUtility.DisplayDialog("Error", "Could not change script exec order!", "Ok");
            NoErrorHappened = false;
        }

		// 5. Add AkAudioListener component to camera
        if (!Settings.OldProject)
            SetListener();

        // 6. Enable "Run In Background" in PlayerSettings (PlayerSettings.runInbackground property)
        PlayerSettings.runInBackground = true;

#if !UNITY_5
		// 7. Install the Profile libraries of the installed platforms. This should actually be a change in the way we build unitypackages.
        if (!InstallAllPlatformProfilePlugins())
        {
            EditorUtility.DisplayDialog("Error", "Could not install some platform plugins!", "Ok");
            NoErrorHappened = false;
        }
#endif

        // 8. Verify DirectX is installed (windows only)
#if UNITY_EDITOR_WIN
		Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\DirectX\\");
        if( key == null )
        {
            EditorUtility.DisplayDialog("Warning", "Detected the DirectX End-User Runtime is not installed. You might have issues using the Windows version of the plugin", "Ok");
        }
#endif
        // 9. Activate WwiseIDs file generation, and point Wwise to the Assets/Wwise folder
		// 10. Change the SoundBanks options so it adds Max Radius information in the Wwise project
        if (!SetSoundbankSettings())
        {
            EditorUtility.DisplayDialog("Warning", "Could not modify Wwise Project to generate the header file!", "Ok");
        }
        
        // 11. Generate the WwiseIDs.cs file from the .h file
		// GenerateWwiseIDsCsFile();

        // 12. Refresh UI/Settings files.
        Repaint();

        // 13. Make sure the installed SDK matches the one that was build on the machine
        ValidateVersion();

        // 14. Move some files out of the assets folder
		// todo.

        // 15. Populate the picker
		AkWwiseProjectInfo.GetData(); // Load data
		if( !String.IsNullOrEmpty (Settings.WwiseProjectPath) )
        {
	        AkWwiseProjectInfo.Populate();
	        AkWwisePicker.PopulateTreeview();
			if (AkWwiseProjectInfo.GetData().autoPopulateEnabled)
			{
			AkWwiseWWUWatcher.GetInstance().SetPath(Path.GetDirectoryName(AkUtilities.GetFullPath(Application.dataPath, WwiseSettings.LoadSettings().WwiseProjectPath)));
				AkWwiseWWUWatcher.GetInstance().StartWWUWatcher();
			}
		}

        return NoErrorHappened;
	}
	
    // Create a Wwise Global object containing the initializer and terminator scripts. Set the soundbank path of the initializer script.
    // This game object will live for the whole project; there is no need to instanciate one per scene.
	void CreateWwiseGlobalObject()
	{
        // Look for a game object which has the initializer component
        AkInitializer[] AkInitializers = FindObjectsOfType(typeof(AkInitializer)) as AkInitializer[];
        GameObject WwiseGlobalGameObject = null;
        if (AkInitializers.Length > 0)
        {
			GameObject.DestroyImmediate(AkInitializers[0].gameObject);
        }
        WwiseGlobalGameObject = new GameObject("WwiseGlobal");

		// attach initializer component
		AkInitializer AkInit = WwiseGlobalGameObject.AddComponent<AkInitializer>();
		
        // Set the soundbank path property on the initializer
        AkInit.basePath = Settings.SoundbankPath;

        // Set focus on WwiseGlobal
        Selection.activeGameObject = WwiseGlobalGameObject;     
	}

    // Change the execution order of all the necessary scripts
    bool SetAllScriptExecutionOrder()
    {
        Dictionary<string, int> scriptsToModify = new Dictionary<string, int>();

		// !!! WARNING !!! !!! WARNING !!! !!! WARNING !!! !!! WARNING !!! !!! WARNING !!! !!! WARNING !!! !!! WARNING !!!
		// IF YOU MODIFY THIS LIST, MAKE SURE YOU MODIFY IT IN AkWwiseMigrationWindow.migration_setup AS WELL.
		// !!! WARNING !!! !!! WARNING !!! !!! WARNING !!! !!! WARNING !!! !!! WARNING !!! !!! WARNING !!! !!! WARNING !!!
        scriptsToModify.Add("AkInitializer", -100);
        scriptsToModify.Add("AkBank", -75);
        scriptsToModify.Add("AkAudioListener", -50);
        scriptsToModify.Add("AkGameObj", -25);
        scriptsToModify.Add("AkState", -20);
        scriptsToModify.Add("AkSwitch", -10);
        scriptsToModify.Add("AkTerminator", 100);

        foreach (KeyValuePair<string, int> entry in scriptsToModify)
        {
            if( !SetScriptExecutionOrder(entry.Key, entry.Value ) )
            {
                return false;
            }
        }

        return true;
    }

    // Modifies the .meta file associated to a script to change its execution order
    bool SetScriptExecutionOrder(string Script, int ExecutionOrder)
    {
        try
        {
			string DeploymentPath = Path.Combine( Path.Combine( Path.Combine( Application.dataPath, "Wwise"), "Deployment"), "Components");
			string fileName = Path.Combine( DeploymentPath, Script + ".cs.meta");
			string fileContents = File.ReadAllText(fileName);

			// Get start and stop index for the line containing the executionOrder
			int startIndex = fileContents.IndexOf("executionOrder");
			int stopIndex = fileContents.IndexOf((char)0x0A, startIndex); // Line feed; find EOL for executionOrder
			if (startIndex != -1)
			{
				// Find where the digit after "executionOrder" starts
				int digitstartIndex = fileContents.IndexOfAny("-1234567890".ToCharArray(), startIndex);

				// Remove everything from the start of the digit to the EOL, and add our own digit
				fileContents = fileContents.Remove(digitstartIndex, stopIndex - digitstartIndex);
				fileContents = fileContents.Insert(digitstartIndex, ExecutionOrder.ToString());
			}
			else
			{
				// If "executionOrder is not found in the file, add it.
				fileContents += "executionOrder: " + ExecutionOrder.ToString();
			}

			// Temporarily un-hide the file so we can write to it
			FileInfo Info = new FileInfo(fileName);
			Info.Attributes &= ~FileAttributes.Hidden;

			// Write to file
			File.WriteAllText(fileName, fileContents);

			// Re-hide the file
			Info.Attributes |= FileAttributes.Hidden;
        }
        catch (Exception)
        {
        	return false;
        }

        return true;
    }

    void ValidateVersion()
    {
        try
        {
            string fileName = Path.Combine(Path.Combine(Path.Combine(Environment.GetEnvironmentVariable("WWISESDK"), "include"), "AK"), "AkWwiseSDKVersion.h");
            string fileContents = File.ReadAllText(fileName);
            uint installedVersionMajor = 0;
            uint installedVersionMinor = 0;

            int startIndex = fileContents.IndexOf("AK_WWISESDK_VERSION_MAJOR");
            int stopIndex = fileContents.IndexOf((char)0x0A, startIndex); // Line feed; find EOL for executionOrder
            if (startIndex != -1)
            {
                // Find where the digit after "AK_WWISESDK_VERSION_MAJOR" starts
                int digitStartIndex = fileContents.IndexOfAny("-1234567890".ToCharArray(), startIndex);
                int digitStopIndex = fileContents.LastIndexOfAny("-1234567890".ToCharArray(), stopIndex);

                installedVersionMajor = Convert.ToUInt32(fileContents.Substring(digitStartIndex, digitStopIndex - digitStartIndex + 1));
            }

            startIndex = fileContents.IndexOf("AK_WWISESDK_VERSION_MINOR");
            stopIndex = fileContents.IndexOf((char)0x0A, startIndex); // Line feed; find EOL for executionOrder
            if (startIndex != -1)
            {
                // Find where the digit after "AK_WWISESDK_VERSION_MINOR" starts
                int digitStartIndex = fileContents.IndexOfAny("-1234567890".ToCharArray(), startIndex);
                int digitStopIndex = fileContents.LastIndexOfAny("-1234567890".ToCharArray(), stopIndex);

                installedVersionMinor = Convert.ToUInt32(fileContents.Substring(digitStartIndex, digitStopIndex - digitStartIndex + 1));
            }

            uint pluginVersion = AkSoundEngine.GetMajorMinorVersion();

            uint pluginVersionMajor = pluginVersion >> 16;
            uint pluginVersionMinor = pluginVersion & 0x0000FFFF;

            if (pluginVersionMajor != installedVersionMajor || pluginVersionMinor != installedVersionMinor)
            {
                string msg = string.Format("Plugin SDK version does not match the Wwise SDK found on your machine. Installed version: {0}.{1}; Plugin version: {2}.{3}", 
                    installedVersionMajor, installedVersionMinor, pluginVersionMajor, pluginVersionMinor);
                EditorUtility.DisplayDialog("Warning", msg, "ok");
            }
        }
        catch (Exception)
        {
        }
    }


    // Disable the built-in audio listener, and add the AkAudioListener to the camera
    void SetListener()
    {
        // Remove the audio listener script
        AudioListener listener = Camera.main.gameObject.GetComponent<AudioListener>();
        if (listener != null)
        {
            Component.DestroyImmediate(listener);
        }

        // Add the AkAudioListener script
        if (Camera.main.gameObject.GetComponent<AkAudioListener>() == null)
        {
            Camera.main.gameObject.AddComponent<AkAudioListener>();
        }
    }

	// Install all available plugins (dynamic libraries) in the Assets/plugins folder
    bool InstallAllPlatformProfilePlugins()
    {
		// Find available platforms
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/Wwise/Editor/WwiseMenu/");
		DirectoryInfo[] availablePlatforms = di.GetDirectories();

		// Instantiate the installer class for the platforms
        foreach (DirectoryInfo platform in availablePlatforms)
        {
            if (platform.Name == "Common")
            {
                continue;
            }

            InstallPlugin(platform);
        }
        return true;
    }

    public static void InstallPlugin(DirectoryInfo Platform)
    {
		bool result = true;
        string installerType = "AkUnityPluginInstaller_" + Platform.Name;
        AkUnityPluginInstallerBase installer = (AkUnityPluginInstallerBase)Activator.CreateInstance(Type.GetType(installerType));
        if (installer.m_arches.GetLength(0) > 0)
        {
            foreach (string arch in installer.m_arches)
            {
				if(!installer.InstallPluginByArchConfig(arch, "Profile"))
				{
					result = false;
					break;
				}
            }
        }
        else
        {
			result = installer.InstallPluginByConfig("Profile");
        }

		if(result)
		{
			try
			{
				string[] platforms = File.ReadAllLines(Application.dataPath + "/Wwise/Version.txt")[8].Split(' ');
					
				if(!Array.Exists<string>(platforms, x => x.Equals(Platform.Name)))
					File.AppendAllText(Application.dataPath + "/Wwise/Version.txt", " " + Platform.Name);
			}
			catch(Exception e)
			{
				Debug.Log (e.ToString());
			}
		}
    }

	// Modify the .wproj file to set needed soundbank settings
    bool SetSoundbankSettings()
    {
		if( string.IsNullOrEmpty(Settings.WwiseProjectPath) )
		{
			// Nothing to do here, because setup should succees if Wwise project is not given
			return true;
		}
		string WprojPath = AkUtilities.GetFullPath(Application.dataPath, Settings.WwiseProjectPath);
        string SoundbankPath = AkUtilities.GetFullPath(Application.streamingAssetsPath, Settings.SoundbankPath);
#if UNITY_EDITOR_OSX
		SoundbankPath = "Z:" + SoundbankPath;
#endif

        if (AkUtilities.EnableBoolSoundbankSettingInWproj("SoundBankGenerateHeaderFile", WprojPath))
        {
            if (AkUtilities.SetSoundbankHeaderFilePath(WprojPath, SoundbankPath))
            {
                return AkUtilities.EnableBoolSoundbankSettingInWproj("SoundBankGenerateMaxAttenuationInfo", WprojPath);
            }
        }
        return false;
    }
    
    void GenerateWwiseIDsCsFile()
    {
		string SoundbankPath = AkUtilities.GetFullPath(Application.streamingAssetsPath, Settings.SoundbankPath);
        if (File.Exists(Path.Combine(SoundbankPath, "Wwise_IDs.h")))
        {
            AkWwiseIDConverter converter = new AkWwiseIDConverter(SoundbankPath);
            converter.Convert(false);
        }
    }

    [MenuItem("Assets/Wwise/Uninstall Wwise Integration", false, (int)AkWwiseMenuOrder.Uninstall)]
    public static void UninstallIntegrationWithConfirm()
	{
		// Pop a "Are you sure?" window
        if (EditorUtility.DisplayDialog("Warning", "This will completely remove the Wwise Unity Integration. Are you sure?", "Yes", "No"))
        {
            UninstallIntegration();
        }
	}

    static void UninstallIntegration()
    {
        try
        {
        	// Close the Picker window
			AkWwisePicker window = EditorWindow.GetWindow<AkWwisePicker>("AkWwisePicker", true, typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow"));
			window.Close ();
			
            // Remove the WwiseGlobal object
            AkInitializer[] AkInitializers = FindObjectsOfType(typeof(AkInitializer)) as AkInitializer[];
            if (AkInitializers.Length > 0)
            {
                GameObject.DestroyImmediate(AkInitializers[0].gameObject);
            }

			// Remove the AkAudioListener component from the camera
			AkAudioListener listener = Camera.main.gameObject.GetComponent<AkAudioListener>();
			if (listener != null)
			{
				Component.DestroyImmediate(listener);
			}

			// Put back the built-in Audio Listener
			if(Camera.main.gameObject.GetComponent<AkAudioListener>() == null)
			{
				Camera.main.gameObject.AddComponent<AudioListener>();		
			}
			
            // Remove the plugins
            string pluginsDir = Path.Combine(Application.dataPath, "Plugins");
            if( Directory.Exists(pluginsDir) )
            {
				string[] foundBundles = Directory.GetDirectories(pluginsDir, "AkSoundEngine*.bundle");
				foreach(string bundle in foundBundles)
				{
					Directory.Delete (bundle, true);
				}

	            string[] foundPlugins = Directory.GetFiles(pluginsDir, "AkSoundEngine*", SearchOption.AllDirectories);
	            foreach (string plugin in foundPlugins)
	            {
	                File.Delete(plugin);
	            }
	        }

            // Remove the wwise settings xml file
            if (File.Exists(Path.Combine(Application.dataPath, WwiseSettings.WwiseSettingsFilename)))
            {
                File.Delete(Path.Combine(Application.dataPath, WwiseSettings.WwiseSettingsFilename));
            }

            // Delete the Wwise folder within the Assets folder
            if( Directory.Exists(Path.Combine(Application.dataPath, "Wwise")))
            {
                Directory.Delete(Path.Combine(Application.dataPath, "Wwise"), true);
				if( File.Exists(Path.Combine(Application.dataPath, "Wwise.meta") ) )
				{
					File.Delete(Path.Combine(Application.dataPath, "Wwise.meta"));
				}
            }
			
			// Remove the generated SoundBanks
			string sbPath = AkUtilities.GetFullPath(Application.streamingAssetsPath, Settings.SoundbankPath);
			if( Directory.Exists (sbPath) )
			{
				Directory.Delete(sbPath, true);
			}
			
			// Delete the Mac documentation
			string docPath = Path.Combine (Path.Combine (Application.dataPath, ".."), "WwiseUnityIntegrationHelp_AppleCommon_en");
			if( Directory.Exists (docPath))
			{
				Directory.Delete (docPath, true);
			}
			
			// Re-activate built-in audio
			AkUnitySettingsParser.SetBoolValue("m_DisableAudio", false, "AudioManager");
			
			AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
        	Debug.Log (e.ToString());
        }
    }
	
    public static void PackageDemo()
    {
        string[] VersionTxtLines = System.IO.File.ReadAllLines(Application.dataPath + "/Wwise/Version.txt");
        string integrationBuildString = VersionTxtLines[2];
        string integrationSvnRev= integrationBuildString.Split(' ')[3];

        string destDir = Path.Combine(Path.Combine(Path.Combine(Path.Combine("..", ".."), "wwiseunity"), "Installers"), "stable_" + integrationSvnRev);
        Directory.CreateDirectory(destDir);
        destDir = Path.Combine(destDir, "WwiseDemoScene.unitypackage");
        string[] dirsToExport = {
            "Assets/Standard Assets",
            "Assets/StreamingAssets",
            "Assets/Wwise",
            "Assets/WwiseDemoScene",
            "Assets/WwiseProject",
            "Assets/WwiseSettings.xml",
            };
        AssetDatabase.ExportPackage(dirsToExport, destDir, ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
    }

}

#endif // UNITY_EDITOR