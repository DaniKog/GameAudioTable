#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System;


public class AkWwiseMenu_Windows : MonoBehaviour {
#if !UNITY_5
	private static AkUnityPluginInstaller_Windows m_installer = new AkUnityPluginInstaller_Windows();

	// private static AkUnityIntegrationBuilder_Windows m_rebuilder = new AkUnityIntegrationBuilder_Windows();

	// Use Unity arch names for folders.
	[MenuItem("Assets/Wwise/Install Plugins/Windows/Win32/Debug", false, (int)AkWwiseMenuOrder.Win32Debug)]
	public static void InstallPlugin_Win32_Debug () {
		m_installer.InstallPluginByArchConfig("x86", "Debug");
	}

	[MenuItem("Assets/Wwise/Install Plugins/Windows/Win32/Profile", false, (int)AkWwiseMenuOrder.Win32Profile)]
	public static void InstallPlugin_Win32_Profile () {
		m_installer.InstallPluginByArchConfig("x86", "Profile");
	}

	[MenuItem("Assets/Wwise/Install Plugins/Windows/Win32/Release", false, (int)AkWwiseMenuOrder.Win32Release)]
	public static void InstallPlugin_Win32_Release () {
		m_installer.InstallPluginByArchConfig("x86", "Release");
	}

	[MenuItem("Assets/Wwise/Install Plugins/Windows/x64/Debug", false, (int)AkWwiseMenuOrder.Win64Debug)]
	public static void InstallPlugin_x64_Debug () {
		m_installer.InstallPluginByArchConfig("x86_64", "Debug");
	}

	[MenuItem("Assets/Wwise/Install Plugins/Windows/x64/Profile", false, (int)AkWwiseMenuOrder.Win64Profile)]
	public static void InstallPlugin_x64_Profile () {
		m_installer.InstallPluginByArchConfig("x86_64", "Profile");
	}

	[MenuItem("Assets/Wwise/Install Plugins/Windows/x64/Release", false, (int)AkWwiseMenuOrder.Win64Release)]
	public static void InstallPlugin_x64_Release () {
		m_installer.InstallPluginByArchConfig("x86_64", "Release");
	}
#endif

    [MenuItem("Help/Wwise Help/Windows", false, (int)AkWwiseHelpOrder.WwiseHelpOrder)]
    public static void OpenDocWindows () 
    {
        string docPath = string.Format("{0}/Wwise/Documentation/WindowsCommon/en/WwiseUnityIntegrationHelp_en.chm", Application.dataPath);
        
        AkDocHelper.OpenDoc(docPath);
    }
    
//	[MenuItem("Assets/Wwise/Rebuild Integration/Windows/Win32/Debug")]
//	public static void RebuildIntegration_Debug_Win32() {
//		m_rebuilder.BuildByConfig("Debug", "x86");
//	}
//
//	// Use AK arch names when building.
//	[MenuItem("Assets/Wwise/Rebuild Integration/Windows/Win32/Profile")]
//	public static void RebuildIntegration_Profile_Win32() {
//		m_rebuilder.BuildByConfig("Profile", "Win32");
//	}
//
//	[MenuItem("Assets/Wwise/Rebuild Integration/Windows/Win32/Release")]
//	public static void RebuildIntegration_Release_Win32() {
//		m_rebuilder.BuildByConfig("Release", "Win32");
//	}
//
//	[MenuItem("Assets/Wwise/Rebuild Integration/Windows/x64/Debug")]
//	public static void RebuildIntegration_Debug_x64() {
//		m_rebuilder.BuildByConfig("Debug", "x64");
//	}
//
//	[MenuItem("Assets/Wwise/Rebuild Integration/Windows/x64/Profile")]
//	public static void RebuildIntegration_Profile_x64() {
//		m_rebuilder.BuildByConfig("Profile", "x64");
//	}
//
//	[MenuItem("Assets/Wwise/Rebuild Integration/Windows/x64/Release")]
//	public static void RebuildIntegration_Release_x64() {
//		m_rebuilder.BuildByConfig("Release", "x64");
//	}
}


public class AkUnityPluginInstaller_Windows : AkUnityPluginInstallerMultiArchBase
{
	public AkUnityPluginInstaller_Windows()
	{
		m_platform = "Windows";
        m_arches = new string[] { "x86", "x86_64" };
		m_excludes.AddRange(new string[] {".pdb", ".exp", ".lib"});
	}

	protected override string GetPluginDestPath(string arch)
	{
		return Path.Combine(m_pluginDir, arch);
	}
}


public class AkUnityIntegrationBuilder_Windows : AkUnityIntegrationBuilderBase
{
	public AkUnityIntegrationBuilder_Windows()
	{
		m_platform = "Windows";
	}
}
#endif // #if UNITY_EDITOR