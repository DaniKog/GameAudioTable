#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Callbacks;
using System;

#if UNITY_WEBPLAYER
/* Cut down version that compiles under restricted WebPlayer .NET runtime
   and gives useful(hopefully) error message to the developer */
[InitializeOnLoad]
public class FMODEditorExtension : MonoBehaviour
{
    static FMODEditorExtension()
    {
        Debug.LogError("FMOD Studio Integration: WebPlayer not supported. Please change your platform.");
    }
    
    [MenuItem ("FMOD/Import Banks")]
	public static void ImportBanks()
	{
		EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "WebPlayer not supported. Please change your platform.", "OK");
	}
	
	[MenuItem ("FMOD/Refresh Event List")]
	static bool CheckRefreshEventList()
	{
		EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "WebPlayer not supported. Please change your platform.", "OK");
        return false;
	}
	
	[MenuItem ("FMOD/Refresh Event List")]
	public static void RefreshEventList()
    {
		EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "WebPlayer not supported. Please change your platform.", "OK");
    }
    
    [MenuItem ("FMOD/Online Manual")]
	static void OnlineManual()
	{
		EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "WebPlayer not supported. Please change your platform.", "OK");
	}
    
    [MenuItem ("FMOD/Online API Documentation")]
	static void OnlineAPIDocs()
	{
		EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "WebPlayer not supported. Please change your platform.", "OK");
	}
	
	[MenuItem ("FMOD/About Integration")]
	static void AboutIntegration() 
	{
		EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "WebPlayer not supported. Please change your platform.", "OK");
	}
    
    /* Stubs so all the other scripts compile */
    static void Update()
    {
    }
			
	public static void AuditionEvent(FMODAsset asset)
	{
	}
	
	public static void StopEvent()
	{
	}
	
	public static void SetEventParameterValue(int index, float val)
	{
	}
	
	public static FMOD.Studio.EventDescription GetEventDescription(string idString)
	{
        return null;
	}
}

#else
/* Full version of editor extensions */
[InitializeOnLoad]
public class FMODEditorExtension : MonoBehaviour
{
	public static FMOD.Studio.System sFMODSystem;
	static Dictionary<string, FMOD.Studio.EventDescription> events = new Dictionary<string, FMOD.Studio.EventDescription>();	
	static FMOD.Studio.EventInstance currentInstance = null;
	static List<FMOD.Studio.Bank> loadedBanks = new List<FMOD.Studio.Bank>();
	
	const string AssetFolder = "FMODAssets";
	
	static FMODEditorExtension()
	{
        EditorApplication.update += Update;
		EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
	}
 
	static void HandleOnPlayModeChanged()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode &&
			!EditorApplication.isPaused)
		{
        	UnloadAllBanks();
		}
		
		if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode &&
			!EditorApplication.isPaused)
		{
	        //LoadAllBanks();
		}
	}

    [PostProcessScene]
    public static void OnPostprocessScene()
    {
        // Hack: clean up stale files from old versions of the integration that will mess 
        // with the build. DeleteAsset is a NoOp if the file doesn't exist

        // moved in 1.05.10
        AssetDatabase.DeleteAsset("Assets/Plugins/Android/libfmod.so");
        AssetDatabase.DeleteAsset("Assets/Plugins/Android/libfmodstudio.so");
    }
	
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{		
		if (target == BuildTarget.StandaloneOSXIntel 
			|| target == BuildTarget.StandaloneOSXUniversal
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1)
			|| target == BuildTarget.StandaloneOSXIntel64
#endif
			)
		{		
	        FMOD.Studio.UnityUtil.Log("Post build: copying FMOD DSP plugins to build directory");
			var pluginLocation = Application.dataPath + "/Plugins";
			
			// Assume all .dylibs are FMOD Studio DSP plugins
			string[] files = System.IO.Directory.GetFiles(pluginLocation, "*.dylib");
			
			foreach(var filePath in files)
			{
				var dest = pathToBuiltProject + "/Contents/Plugins/" + System.IO.Path.GetFileName(filePath);
				FMOD.Studio.UnityUtil.Log("COPY: " + filePath + " TO " + dest);
				System.IO.File.Copy(filePath, dest, true);
			}
		}
	}
	
	static void Update()
    {
        if (EditorApplication.isCompiling)
        {
            UnloadAllBanks();
        }
		
        if (sFMODSystem != null && sFMODSystem.isValid())
		{
			ERRCHECK(sFMODSystem.update());
		}
    }
			
	public static void AuditionEvent(FMODAsset asset)
	{
		StopEvent();
		
		var desc = GetEventDescription(asset.id);
		if (desc == null)
		{
			FMOD.Studio.UnityUtil.LogError("Failed to retrieve EventDescription for event: " + asset.path);
		}
					
		if (!ERRCHECK(desc.createInstance(out currentInstance)))
		{
			return;
		}
		
		ERRCHECK(currentInstance.start());
	}
	
	public static void StopEvent()
	{
		if (currentInstance != null && currentInstance.isValid())
		{
			ERRCHECK(currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE));
			currentInstance = null;
		}
	}
	
	public static void SetEventParameterValue(int index, float val)
	{
		if (currentInstance != null && currentInstance.isValid())
		{
			FMOD.Studio.ParameterInstance param;
			currentInstance.getParameterByIndex(index, out param);
			param.setValue(val);
		}		
	}
	
	public static FMOD.Studio.EventDescription GetEventDescription(string idString)
	{
		FMOD.Studio.EventDescription desc = null;
		if (!events.TryGetValue(idString, out desc))
		{
			if (sFMODSystem == null)
			{
				if (!LoadAllBanks())
				{
					return null;
				}
			}
			
			Guid id = new Guid();			
			if (!ERRCHECK(FMOD.Studio.Util.ParseID(idString, out id)))
			{
				return null;
			}
			
			FMOD.RESULT res = FMODEditorExtension.sFMODSystem.getEventByID(id, out desc);
			if (res == FMOD.RESULT.ERR_EVENT_NOTFOUND || desc == null || !desc.isValid() || !ERRCHECK(res))
			{
				return null;
			}
			
			events[idString] = desc;
		}
		return desc;
	}
	
	[MenuItem ("FMOD/Import Banks")]
	public static void ImportBanks()
	{
		if (PrepareIntegration())
		{
		
			string filePath = "";
			if (!LocateProject(ref filePath))
			{
				return;
			}
			
			ImportAndRefresh(filePath + "/" + studioPlatformDirectoryName());
		}
	}	
	
	[MenuItem ("FMOD/Refresh Event List", true)]
	static bool CheckRefreshEventList()
	{
		string guidPathKey = "FMODStudioProjectPath_" + Application.dataPath;
		return EditorPrefs.HasKey(guidPathKey);
	}
	
	[MenuItem ("FMOD/Refresh Event List")]
	public static void RefreshEventList()
	{
		string filePath =  GetDefaultPath();
		
		ImportAndRefresh(filePath + "/Build/" + studioPlatformDirectoryName());
	}
	
	static string studioPlatformDirectoryName()
	{
		var platDirFile = "Assets/Plugins/FMOD/PlatformDirectories.txt";
		if (!System.IO.File.Exists(platDirFile))
		{
			FMOD.Studio.UnityUtil.Log("No PlatformDirectories.txt found in Assets/Plugins/FMOD defaulting to \"Desktop\"");
			return "Desktop";
		}
		
		string platformName = UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup.ToString();
		var stream = new System.IO.StreamReader(platDirFile);
		while (!stream.EndOfStream)
		{
			var line = stream.ReadLine();
			
			string[] s = line.Split(':');
			if (string.Equals(s[0], platformName))
			{
				FMOD.Studio.UnityUtil.Log("target: " + platformName + ", directory: " + s[1]);
				return s[1];
			}
		}

		FMOD.Studio.UnityUtil.LogWarning("Current platform: " + platformName + " not found in PlatformDirectories.txt, defaulting to \"Desktop\"");
		return "Desktop";
	}
	
	static bool CopyBanks(string path)
	{		
		UnloadAllBanks();
		
		var info = new System.IO.DirectoryInfo(path);

        int bankCount = 0;
		string copyBanksString = "";
		var banksToCopy = new List<System.IO.FileInfo>();
		
		bool hasNewStyleStringsBank = false; // PAS - hack fix for having two strings bank
		
		foreach (var fileInfo in info.GetFiles())
		{
			var ex = fileInfo.Extension;			
			if (!ex.Equals(".bank", System.StringComparison.CurrentCultureIgnoreCase) &&
				!ex.Equals(".strings", System.StringComparison.CurrentCultureIgnoreCase))
			{
				FMOD.Studio.UnityUtil.Log("Ignoring unexpected file: \"" + fileInfo.Name + "\": unknown file type: \"" + fileInfo.Extension + "\"");
				continue;
			}
			
			hasNewStyleStringsBank = hasNewStyleStringsBank || fileInfo.FullName.Contains(".strings.bank");

            ++bankCount;

			string bankMessage = "(added)";
			
			var oldBankPath = Application.dataPath + "/StreamingAssets/" + fileInfo.Name;
			if (System.IO.File.Exists(oldBankPath))
			{
				var oldFileInfo = new System.IO.FileInfo(oldBankPath);
				if (oldFileInfo.LastWriteTime == fileInfo.LastWriteTime)
				{
					bankMessage = "(same)";
				}
				else if(oldFileInfo.LastWriteTime < fileInfo.LastWriteTime)
				{
					bankMessage = "(newer)";					
				}
				else
				{
					bankMessage = "(older)";
				}
			}
			
			copyBanksString += fileInfo.Name + " " + bankMessage + "\n";
			banksToCopy.Add(fileInfo);
		}

        if (bankCount == 0)
        {
            EditorUtility.DisplayDialog("FMOD Studio Importer", "No .bank files found in the directory:\n" + path, "OK");
            return false;
        }
				
		if (!EditorUtility.DisplayDialog("FMOD Studio Importer", "The import will modify the following files:\n" + copyBanksString, "Continue", "Cancel"))
		{
			return false;
		}
		
		string bankNames = "";
		foreach (var fileInfo in banksToCopy)
		{
			if (hasNewStyleStringsBank && fileInfo.Extension.Equals(".strings"))
			{
				continue; // skip the stale strings bank
			}
			
			System.IO.Directory.CreateDirectory(Application.dataPath + "/StreamingAssets");
			var oldBankPath = Application.dataPath + "/StreamingAssets/" + fileInfo.Name;
			fileInfo.CopyTo(oldBankPath, true);
			
			bankNames += fileInfo.Name + "\n";
		}
		System.IO.File.WriteAllText(Application.dataPath + "/StreamingAssets/FMOD_bank_list.txt", bankNames);

		return true;
	}
	
	static bool ImportAndRefresh(string filePath)
	{
        FMOD.Studio.UnityUtil.Log("import from path: " + filePath);
		CopyBanks(filePath);
				
		if (!LoadAllBanks())
		{
			return false;
		}
		
		List<FMODAsset> existingAssets = new List<FMODAsset>();
		GatherExistingAssets(existingAssets);
		
		List<FMODAsset> newAssets = new List<FMODAsset>();
		GatherNewAssets(filePath, newAssets);
		
		var assetsToDelete = existingAssets.Except(newAssets, new FMODAssetGUIDComparer());
		var assetsToAdd = newAssets.Except(existingAssets, new FMODAssetGUIDComparer());
		
		var assetsToMoveFrom = existingAssets.Intersect(newAssets, new FMODAssetGUIDComparer());
		var assetsToMoveTo   = newAssets.Intersect(existingAssets, new FMODAssetGUIDComparer());
		
		var assetsToMove = assetsToMoveFrom.Except(assetsToMoveTo, new FMODAssetPathComparer());
		
		if (!assetsToDelete.Any() && !assetsToAdd.Any() && !assetsToMove.Any())
		{
			Debug.Log("FMOD Studio Importer: Banks updated, events list unchanged " + System.DateTime.Now.ToString(@"[hh:mm tt]"));
		}
		else
		{
			string assetsToDeleteFormatted = "";
			foreach (var asset in assetsToDelete)
			{
				assetsToDeleteFormatted += eventToAssetPath(asset.path) + "\n";
			}
			
			string assetsToAddFormatted = "";
			foreach (var asset in assetsToAdd)
			{
				assetsToAddFormatted += eventToAssetPath(asset.path) + "\n";
			}
			
			string assetsToMoveFormatted = "";
			foreach (var asset in assetsToMove)
			{
				var fromPath = assetsToMoveFrom.First( a => a.id == asset.id ).path;
				var toPath = assetsToMoveTo.First( a => a.id == asset.id ).path;
				assetsToMoveFormatted += fromPath + "  moved to  " + toPath + "\n";
			}
			
			string deletionMessage = 
					(assetsToDelete.Count() == 0 ? "No assets removed" : "Removed assets: " + assetsToDelete.Count()) + "\n" +
					(assetsToAdd.Count()    == 0 ? "No assets added"   : "Added assets: "   + assetsToAdd.Count())    + "\n" + 
					(assetsToMove.Count()   == 0 ? "No assets moved"   : "Moved assets: "   + assetsToMove.Count())   + "\n" + 
					((assetsToDelete.Count() != 0 || assetsToAdd.Count() != 0 || assetsToMove.Count() != 0) ? "\nSee console for details" : "");
				
			Debug.Log("FMOD import details " + System.DateTime.Now.ToString(@"[hh:mm tt]") + "\n\n" +
				(assetsToDelete.Count() == 0 ? "No assets removed" : "Removed Assets:\n" + assetsToDeleteFormatted) + "\n" +
				(assetsToAdd.Count()    == 0 ? "No assets added"   : "Added Assets:\n" + assetsToAddFormatted)	    + "\n" +
				(assetsToMove.Count()   == 0 ? "No assets moved"   : "Moved Assets:\n" + assetsToMoveFormatted) 	+ "\n" +
				"________________________________");
			
			if (!EditorUtility.DisplayDialog("FMOD Studio Importer", deletionMessage, "Continue", "Cancel"))
			{
				return false; // user clicked cancel
			}
		}
		
		ImportAssets(assetsToAdd);
		DeleteMissingAssets(assetsToDelete);
		MoveExistingAssets(assetsToMove, assetsToMoveFrom, assetsToMoveTo);
		
		AssetDatabase.Refresh();
		
		return true;
	}
	
	static void CreateDirectories(string assetPath)
	{
		const string root = "Assets";
		var currentDir = System.IO.Directory.GetParent(assetPath);
		Stack<string> directories = new Stack<string>();
		while (!currentDir.Name.Equals(root))
		{
			directories.Push(currentDir.Name);
			currentDir = currentDir.Parent;
		}		
		
		string path = root;
		while (directories.Any())
		{
			var d = directories.Pop();
			
			if (!System.IO.Directory.Exists(Application.dataPath + "/../" + path + "/" + d))
			{				
				FMOD.Studio.UnityUtil.Log("Create folder: " + path + "/" + d);
				AssetDatabase.CreateFolder(path, d);				
			}
			path += "/" + d;
		}
	}
	
	static void MoveExistingAssets(IEnumerable<FMODAsset> assetsToMove, IEnumerable<FMODAsset> assetsToMoveFrom, IEnumerable<FMODAsset> assetsToMoveTo)
	{
		foreach (var asset in assetsToMove)
		{
			var fromAsset = assetsToMoveFrom.First( a => a.id == asset.id );
			var toAsset = assetsToMoveTo.First( a => a.id == asset.id );
			var fromPath = "Assets/" + AssetFolder + eventToAssetPath(fromAsset.path) + ".asset";
			var toPath   = "Assets/" + AssetFolder + eventToAssetPath(toAsset.path)   + ".asset";
			
			CreateDirectories(toPath);
			
			if (!AssetDatabase.Contains(fromAsset))
			{
				FMOD.Studio.UnityUtil.Log("$$ IMPORT ASSET $$");
				AssetDatabase.ImportAsset(fromPath);
			}
			var result = AssetDatabase.MoveAsset(fromPath, toPath);
			if (result != "")
			{
				FMOD.Studio.UnityUtil.Log("Asset move failed: " + result);
			}
			else
			{
				var dir = new System.IO.FileInfo(fromPath).Directory;
				DeleteDirectoryIfEmpty(dir);
			}
			
			fromAsset.path = toAsset.path;
		}
	}

	[MenuItem ("FMOD/Online Manual")]
	static void OnlineManual()
	{
		Application.OpenURL("http://fmod.com/download/fmodstudio/integrations/Unity/Doc/UnityDocumentation.pdf");
	}
    
    [MenuItem ("FMOD/Online API Documentation")]
	static void OnlineAPIDocs()
	{
		Application.OpenURL("http://fmod.com/documentation");
	}
	
	[MenuItem ("FMOD/About Integration")]
	static void AboutIntegration() 
	{
		if (PrepareIntegration())
		{
			if (sFMODSystem == null || !sFMODSystem.isValid())
			{
				CreateSystem();
				
				if (sFMODSystem == null || !sFMODSystem.isValid())
				{
					EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "Unable to retrieve version, check the version number in fmod.cs", "OK");
				}
			}
			
			FMOD.System sys;
			sFMODSystem.getLowLevelSystem(out sys);
			
			uint version;
			if (!ERRCHECK (sys.getVersion(out version)))
			{
				return;
			}

			EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "Version: " + getVersionString(version), "OK");
		}
	}

	static string getVersionString(uint version)
	{		
		uint major = (version & 0x00FF0000) >> 16;
		uint minor = (version & 0x0000FF00) >>  8;
		uint patch = (version & 0x000000FF);

		return major.ToString("X1") + "." + 
			minor.ToString("X2") + "." +
				patch.ToString("X2");
	}
	
	static string guidPathKey
	{
		get { return "FMODStudioProjectPath_" + Application.dataPath; }
	}
	
	static string GetDefaultPath()
	{
		return EditorPrefs.GetString(guidPathKey, Application.dataPath);
	}
	
	static bool LocateProject(ref string filePath)
	{
		var defaultPath = GetDefaultPath();
		
		{
			var workDir = System.Environment.CurrentDirectory;
			filePath = EditorUtility.OpenFolderPanel("Locate build directory", defaultPath, "Build");
			System.Environment.CurrentDirectory = workDir; // HACK: fixes weird Unity bug that causes random crashes after using OpenFolderPanel 
		}
		 
		if (System.String.IsNullOrEmpty(filePath))
		{
			FMOD.Studio.UnityUtil.Log("No directory selected");
			return false;
		}
		
		var directory = new System.IO.DirectoryInfo(filePath);
		if (directory.Name.CompareTo("Build") != 0)
		{
			EditorUtility.DisplayDialog("Incorrect directory", "Incorrect directory selected, select the \"Build\" directory in the FMOD Studio project folder", "OK");			
			return false;
		}
		
		EditorPrefs.SetString(guidPathKey, directory.Parent.FullName);

		var bankPath = filePath + "/" + studioPlatformDirectoryName();
		var info = new System.IO.DirectoryInfo(bankPath);
		
		if (info.GetFiles().Count() == 0)
		{
			EditorUtility.DisplayDialog("FMOD Studio Importer", "No bank files found in directory: " + bankPath + 
				"You must build the FMOD Studio project before importing", "OK");
			return false;
		}
		
		return true;
	}
	
	static void GatherExistingAssets(List<FMODAsset> existingAssets)
	{
		var assetRoot = Application.dataPath + "/" + AssetFolder;
		if (System.IO.Directory.Exists(assetRoot))
		{
			GatherAssetsFromDirectory(assetRoot, existingAssets);
		}
	}

	static void GatherAssetsFromDirectory(string directory, List<FMODAsset> existingAssets)
	{
		var info = new System.IO.DirectoryInfo(directory);
		foreach (var file in info.GetFiles())
		{
			var relativePath = new System.Uri(Application.dataPath).MakeRelativeUri(new System.Uri(file.FullName)).ToString();
			var asset = (FMODAsset)AssetDatabase.LoadAssetAtPath(relativePath, typeof(FMODAsset));
			if (asset != null)
			{
				existingAssets.Add(asset);
			}			
		}
		
		foreach (var dir in info.GetDirectories())
		{
			GatherAssetsFromDirectory(dir.FullName, existingAssets);
		}
	}
		
	static void GatherNewAssets(string filePath, List<FMODAsset> newAssets)
	{		
		if (System.String.IsNullOrEmpty(filePath))
		{
			FMOD.Studio.UnityUtil.LogError("No build folder specified");
			return;
		}
				
		foreach (var bank in loadedBanks)
		{
			int count = 0;
			ERRCHECK(bank.getEventCount(out count));
			
			FMOD.Studio.EventDescription[] descriptions = new FMOD.Studio.EventDescription[count];
			ERRCHECK(bank.getEventList(out descriptions));
			
			foreach (var desc in descriptions)
			{
				string path;
				FMOD.RESULT result = desc.getPath(out path);
				
				if (result == FMOD.RESULT.ERR_EVENT_NOTFOUND || desc == null || !desc.isValid() || !ERRCHECK(result))
				{
					continue;
				}
				ERRCHECK(result);
				
				Guid id;
				ERRCHECK(desc.getID(out id));
				
			    var asset = ScriptableObject.CreateInstance<FMODAsset>();
			    asset.name = path.Substring(path.LastIndexOf('/') + 1);
				asset.path = path;
				asset.id = id.ToString("B");
				//Debug.Log("name = " + asset.name + ", id = " + asset.id);
				
				newAssets.Add(asset);
			}
		}
	}
	
	static string eventToAssetPath(string eventPath)
	{
		if (eventPath.StartsWith("event:"))
		{			
			return eventPath.Substring(6); //trim "event:" from the start of the path
		}
		else if (eventPath.StartsWith("snapshot:"))
		{
			return eventPath.Substring(9); //trim "snapshot:" from the start of the path
		}
		else if (eventPath.StartsWith("/"))
		{
			// Assume 1.2 style paths
			return eventPath;
		}
		
		throw new UnityException("Incorrectly formatted FMOD Studio event path: " + eventPath);		
	}
	
	static void ImportAssets(IEnumerable<FMODAsset> assetsToAdd)
	{
		foreach (var asset in assetsToAdd)
		{
			var path = "Assets/" + AssetFolder + eventToAssetPath(asset.path) + ".asset";
			CreateDirectories(path);
			
       		AssetDatabase.CreateAsset(asset, path);
		}
	}
	
	static void DeleteMissingAssets(IEnumerable<FMODAsset> assetsToDelete)
	{
		foreach (var asset in assetsToDelete)
		{
			var path = AssetDatabase.GetAssetPath(asset);
			AssetDatabase.DeleteAsset(path);
			
			var dir = new System.IO.FileInfo(path).Directory;
			DeleteDirectoryIfEmpty(dir);
		}
	}
	
	static void DeleteDirectoryIfEmpty(System.IO.DirectoryInfo dir)
	{
		FMOD.Studio.UnityUtil.Log("Attempt delete directory: " + dir.FullName);

		if (dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0 && dir.Name != AssetFolder)
		{
			dir.Delete();
			DeleteDirectoryIfEmpty(dir.Parent);
		}
	}
	
	static bool CreateSystem()
	{
		if (!FMOD.Studio.UnityUtil.ForceLoadLowLevelBinary())
		{
			return false;
		}

    	if (!ERRCHECK(FMOD.Studio.System.create(out sFMODSystem)))
		{
			return false;
		}

		// Note plugins wont be used when auditioning inside the Unity editor
		ERRCHECK(sFMODSystem.initialize(256, FMOD.Studio.INITFLAGS.ALLOW_MISSING_PLUGINS, FMOD.INITFLAGS.NORMAL, System.IntPtr.Zero));
		
		return true;
	}
	
	static void UnloadAllBanks()
	{
		if (sFMODSystem != null)
		{
			foreach (var bank in loadedBanks)
			{
				ERRCHECK(bank.unload());
			}
			
			loadedBanks.Clear();
			events.Clear();
			
			sFMODSystem.release();
			sFMODSystem = null;
		}
		else if (loadedBanks.Count != 0)
		{
			FMOD.Studio.UnityUtil.LogError("Banks not unloaded!");
		}
	}
	
	static bool LoadAllBanks()
	{
		UnloadAllBanks();
		
		if (!CreateSystem())
		{
			return false;
		}
	
	    string bankPath = Application.dataPath + "/StreamingAssets";
		FMOD.Studio.UnityUtil.Log("Loading banks in path: " + bankPath);
		
		var info = new System.IO.DirectoryInfo(bankPath);		
		FMOD.Studio.UnityUtil.Log("Directory " + (info.Exists ? "exists" : "doesn't exist!!"));
		
		if (info.Exists)
		{
			var fileInfo = info.GetFiles();			
			FMOD.Studio.UnityUtil.Log("Number of files: " + fileInfo.Length);
			
			List<System.IO.FileInfo> bankFiles = new List<System.IO.FileInfo>();
			foreach (var file in fileInfo)
			{
				bankFiles.Add(file);
			}			
			
			int count = 0;
			foreach (var file in bankFiles)
			{
				FMOD.Studio.UnityUtil.Log("file: " + file.Name);
				var s = info.FullName + "/" + file.Name;				
				var ex = file.Extension;
				
				if (ex.Equals(".bank", System.StringComparison.CurrentCultureIgnoreCase) ||
					ex.Equals(".strings", System.StringComparison.CurrentCultureIgnoreCase))
				{
					FMOD.Studio.Bank bank = null;
					FMOD.RESULT result = sFMODSystem.loadBankFile(s, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);
					if (result == FMOD.RESULT.ERR_VERSION)
					{
						//FMOD.Studio.UnityUtil.LogError("These banks were built with an incompatible version of FMOD Studio. Make sure the unity integration matches the FMOD Studio version. (current integration version = " + getVersionString(FMOD.VERSION.number) + ")" );
						FMOD.Studio.UnityUtil.LogError("Bank " + s + " was built with an incompatible version of FMOD Studio. Make sure the unity integration matches the FMOD Studio version." );
						return false;
					}
					if (result != FMOD.RESULT.OK)
					{
						FMOD.Studio.UnityUtil.LogError("An error occured while loading bank " + s + ": " + result.ToString() + "\n  " + FMOD.Error.String(result));
						return false;
					}
					loadedBanks.Add(bank);
				}
				
				++count;
			}
		}
		
		return true;
	}
	
	static bool ERRCHECK(FMOD.RESULT result)
	{
		return FMOD.Studio.UnityUtil.ERRCHECK(result);
	}
	
	static bool PrepareIntegration()
	{
		// Cleanup stale DLL's from the old versions
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			var projectRoot = new System.IO.DirectoryInfo(Application.dataPath).Parent;
			var lowLevelLib = projectRoot.FullName + "/fmod.dll";					
			DeleteBinaryFile(lowLevelLib);		
			var studioLib = projectRoot.FullName + "/fmodstudio.dll";
			DeleteBinaryFile(studioLib);		
		}
		else if (Application.platform == RuntimePlatform.OSXEditor)
		{
			var projectRoot = new System.IO.DirectoryInfo(Application.dataPath).Parent;
			var lowLevelLib = projectRoot.FullName + "/fmod.dylib";					
			DeleteBinaryFile(lowLevelLib);		
			var studioLib = projectRoot.FullName + "/fmodstudio.dylib";
			DeleteBinaryFile(studioLib);
		}
		
		#if !UNITY_5_0
		if (!UnityEditorInternal.InternalEditorUtility.HasPro())
		{
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				var projectRoot = new System.IO.DirectoryInfo(Application.dataPath).Parent;
				var lowLevelLib = projectRoot.FullName + "/fmod.dll";					
				DeleteBinaryFile(lowLevelLib);		
				var studioLib = projectRoot.FullName + "/fmodstudio.dll";
				DeleteBinaryFile(studioLib);		
			}
			else if (Application.platform == RuntimePlatform.OSXEditor)
			{
				var projectRoot = new System.IO.DirectoryInfo(Application.dataPath).Parent;
				var lowLevelLib = projectRoot.FullName + "/fmod.dylib";					
				DeleteBinaryFile(lowLevelLib);		
				var studioLib = projectRoot.FullName + "/fmodstudio.dylib";
				DeleteBinaryFile(studioLib);
			}
			
			EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "FMOD Studio Integration requires either Unity 4 Pro or Unity 5", "OK");
			return false;
		}
		else
		#endif
		{
			return true;
		}
	}
	
	static void DeleteBinaryFile(string path)
	{
		if (System.IO.File.Exists(path))
		{
			try
			{
				System.IO.File.Delete(path);
			}
			catch (System.UnauthorizedAccessException e)
			{			
				EditorUtility.DisplayDialog("Restart Unity", 
					"The following file is in use and cannot be overwritten, restart Unity and try again\n" + path, "OK");
				
				throw e;
			}
		}
	}
}

public class FMODAssetGUIDComparer : IEqualityComparer<FMODAsset>
{
    public bool Equals(FMODAsset lhs, FMODAsset rhs) 
	{
		return lhs.id.Equals(rhs.id, System.StringComparison.OrdinalIgnoreCase);
	}

    public int GetHashCode(FMODAsset asset) 
	{
        return  asset.id.GetHashCode();
    }
}

public class FMODAssetPathComparer : IEqualityComparer<FMODAsset>
{
    public bool Equals(FMODAsset lhs, FMODAsset rhs) 
	{
		return lhs.path.Equals(rhs.path, System.StringComparison.OrdinalIgnoreCase);
	}

    public int GetHashCode(FMODAsset asset) 
	{
        return  asset.path.GetHashCode();
    }
}

#endif

#endif