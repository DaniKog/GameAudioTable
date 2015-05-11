#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2013 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

/// @brief This is an exemple that shows the steps to create a custom build for Unity applications that use Wwise.
/// The build can be started by selecting a target platform and adding scenes to the build in the build settings (File->Build Settings) 
/// and by clicking on "File->Build Unity-Wwise project" on the menu bar.
/// The steps to build a Unity-Wwise project are as follow:
/// 1) 	Copy the soundbank of the current target platform from the Wwise project to the specified folder in the unity project. 
/// 2) 	Build all the scenes defined in unity's build settings.
/// 3) 	Delete the soundbank folder from the Unity project. This step is needed to prevent future builds for other platforms from using that bank.

public class AkExampleAppBuilderBase : MonoBehaviour
{
	//[MenuItem("File/Build Unity-Wwise Project")] 
	public static bool Build()
	{
		//Choose app name and location
		string appPath = EditorUtility.SaveFilePanel (	"Build Unity-Wwise project", 										//window title
		                                              	Application.dataPath.Remove(Application.dataPath.LastIndexOf('/')), //Default app location (unity project root directory)
		                                              	"Unity_Wwise_app", 													//Default app name
		                                              	getPlatFormExtension()												//app extension (depends on target platform)
		                                             );
		//check if the build was cancelled
		bool isUserCancelledBuild = appPath == "";
		if (isUserCancelledBuild)
		{
			UnityEngine.Debug.Log("Wwise: User cancelled the build.");
			return false;
		}

		//get Wwise project file (.wproj) path
		string wwiseProjFile = Path.Combine (Application.dataPath, WwiseSetupWizard.Settings.WwiseProjectPath).Replace('/', '\\');

		//get Wwise project root folder path
		string wwiseProjectFolder = wwiseProjFile.Remove (wwiseProjFile.LastIndexOf(Path.DirectorySeparatorChar));

		//get Wwise platform string (the string isn't the same as unity for some platforms)
		string wwisePlatformString = UnityToWwisePlatformString (EditorUserBuildSettings.activeBuildTarget.ToString());

		//get soundbank location in the Wwise project for the current platform target
		string sourceSoundBankFolder = Path.Combine( wwiseProjectFolder, AkUtilities.GetWwiseSoundBankDestinationFolder (wwisePlatformString, wwiseProjFile));

		//get soundbank destination in the Unity project for the current platform target
		string destinationSoundBankFolder = Path.Combine	(	Application.dataPath + "\\StreamingAssets", 								//Soundbank must be inside the StreamingAssets folder
		                                                  		Path.Combine(WwiseSetupWizard.Settings.SoundbankPath, wwisePlatformString)	
		                                                  	);

		//Copy the soundbank from the Wwise project to the unity project (Inside the StreamingAssets folder as defined in Window->Wwise Settings)
		if(	!AkUtilities.DirectoryCopy 	(	sourceSoundBankFolder, 		//source folder
		                          			destinationSoundBankFolder, //destination
		                          			true						//copy subfolders
										)
		   )
		{
			UnityEngine.Debug.LogError("Wwise: The soundbank folder for the " + wwisePlatformString + " platform doesn't exist. Make sure it was generated in your Wwise project");
			return false;
		}

		//Get all the scenes to build as defined in File->Build Settings
		string[] scenes = new string[EditorBuildSettings.scenes.Length]; 		
		for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)  
		{
			scenes[i] = EditorBuildSettings.scenes[i].path; 
		}

		//Build the app
		BuildPipeline.BuildPlayer	(	scenes,										//scenes to build 
		                           		appPath, 									//Location of the app to create
		                           		EditorUserBuildSettings.activeBuildTarget,	//Platform for which to build the app 
		                           		BuildOptions.None
		                           	);
		
		//Delete the soundbank from the unity project so they dont get copied in the game folder of fututre builds
		Directory.Delete (destinationSoundBankFolder, true); 
		
		return true;
	}


	private static string UnityToWwisePlatformString( string unityPlatormString)
	{
		if(	unityPlatormString == BuildTarget.StandaloneWindows.ToString()
		   	||
		   	unityPlatormString == BuildTarget.StandaloneWindows64.ToString()
		   )
			return "Windows";

		else if(	unityPlatormString == BuildTarget.StandaloneOSXIntel.ToString() 
		        	|| 
		        	unityPlatormString == BuildTarget.StandaloneOSXIntel64.ToString()
		        	||
		        	unityPlatormString == BuildTarget.StandaloneOSXUniversal.ToString()
		        )
			return "Mac";

#if UNITY_5
		else if(unityPlatormString == BuildTarget.iOS.ToString())
#else
		else if(unityPlatormString == BuildTarget.iPhone.ToString())
#endif
			return "iOS";

		else if(unityPlatormString == BuildTarget.XBOX360.ToString())
			return "Xbox360";

		//Android, PS3 and Wii have the correct strings in Wwise v2013.2.7 and Unity version 4.3.4
		return unityPlatormString;
	}

	private static string getPlatFormExtension()
	{
		string unityPlatormString = EditorUserBuildSettings.activeBuildTarget.ToString ();

		if(	unityPlatormString == BuildTarget.StandaloneWindows.ToString()
		   	||
		   	unityPlatormString == BuildTarget.StandaloneWindows64.ToString()
		   )
			return "exe";
		
		else if(	unityPlatormString == BuildTarget.StandaloneOSXIntel.ToString() 
		        	|| 
		        	unityPlatormString == BuildTarget.StandaloneOSXIntel64.ToString()
		        	||
		        	unityPlatormString == BuildTarget.StandaloneOSXUniversal.ToString()
		        )
			return "app";
		
#if UNITY_5
		else if(unityPlatormString == BuildTarget.iOS.ToString())
#else
		else if(unityPlatormString == BuildTarget.iPhone.ToString())
#endif
			return "ipa";
		
		else if(unityPlatormString == BuildTarget.XBOX360.ToString())
			return "XEX";
		else if(unityPlatormString == BuildTarget.Android.ToString())
			return "apk";
		else if(unityPlatormString == BuildTarget.PS3.ToString())
			return "self";		

		return "";
	}
	
}


#endif // #if UNITY_EDITOR