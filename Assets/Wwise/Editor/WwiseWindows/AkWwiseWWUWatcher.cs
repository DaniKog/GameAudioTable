#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;



public class AkWwiseWWUWatcher
{
#if UNITY_EDITOR_WIN
	private FileSystemWatcher WwuWatcher;
#else
	private OSX.IO.FileSystemWatcher.FileSystemWatcher WwuWatcher;
#endif

	private string WwiseProjectFolder;
	private System.Threading.Timer CallbackTimer;
	
	private static AkWwiseWWUWatcher Instance = null;
	
	public static AkWwiseWWUWatcher GetInstance()
	{
		if (Instance == null)
		{
			Instance = new AkWwiseWWUWatcher ();
		}
			
		return Instance;
	}
	
	
	private AkWwiseWWUWatcher()
	{
#if UNITY_EDITOR_WIN
		WwuWatcher 			= new FileSystemWatcher ();
#else
		WwuWatcher 			= new OSX.IO.FileSystemWatcher.FileSystemWatcher();
#endif
		CallbackTimer 		= new System.Threading.Timer (RaisePopulateFlag);
		WwiseProjectFolder 	= Path.GetDirectoryName(AkUtilities.GetFullPath(Application.dataPath, WwiseSetupWizard.Settings.WwiseProjectPath));
		
		try
		{
			WwuWatcher.Path = WwiseProjectFolder;
			WwuWatcher.NotifyFilter = NotifyFilters.LastWrite; 
			WwuWatcher.InternalBufferSize = 65536; //64kb (max size)
			
			// Event handlers that are watching for specific event
			WwuWatcher.Created += new FileSystemEventHandler(WWUWatcher_EventHandler);
			WwuWatcher.Changed += new FileSystemEventHandler(WWUWatcher_EventHandler);
			WwuWatcher.Deleted += new FileSystemEventHandler(WWUWatcher_EventHandler);
			// Wwise does not seem to "rename" files. It creates a new file and deletes the old. We don't need to set the "Renamed" event.
			
			WwuWatcher.Filter = "*.wwu";
			WwuWatcher.IncludeSubdirectories = true;
			AkWwisePicker.WwiseProjectFound = true;
		}
		catch( Exception )
		{
			AkWwisePicker.WwiseProjectFound = false;
		}
	}
	
	public void StartWWUWatcher()
	{
		WwuWatcher.EnableRaisingEvents = true; 
	}
	
	public void StopWWUWatcher()
	{
		WwuWatcher.EnableRaisingEvents = false;
	}

	public void SetPath(string in_path)
	{
		WwuWatcher.Path = in_path;
	}
	
	void WWUWatcher_EventHandler(object sender, FileSystemEventArgs e)
	{ 
		if (!e.FullPath.Contains (".wwu")) 
		{
			return;
		}

		if (e.ChangeType == WatcherChangeTypes.Deleted)
		{
			AkWwiseWWUBuilder.s_deletedWwu.Add(e.FullPath);
		}
		else if(e.ChangeType == WatcherChangeTypes.Created)
		{
			AkWwiseWWUBuilder.s_createdWwu.Add(e.FullPath);
		}
		else
		{
			AkWwiseWWUBuilder.s_changedWwu.Add(e.FullPath);
		}

		//Raise flag after 2 secondes. Timer will be reset to 2 secondes if another event is detected before it reaches zero
		CallbackTimer.Change ( 2000, Timeout.Infinite);
	}
	
	void RaisePopulateFlag(object o)
	{
		//Stop Timer callback
		CallbackTimer.Change ( Timeout.Infinite, Timeout.Infinite);
	
		// Signal the main thread it's time to populate (cannot run populate somewhere else than on main thread)
		AkWwiseWWUBuilder.s_populateNow = true;
	}
}
#endif