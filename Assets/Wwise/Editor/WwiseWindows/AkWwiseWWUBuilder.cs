#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////
#pragma warning disable 0168
using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[InitializeOnLoad]
public class AkWwiseWWUBuilder
{
	static string	s_wwiseProjectPath	= Path.GetDirectoryName(AkUtilities.GetFullPath(Application.dataPath, WwiseSettings.LoadSettings().WwiseProjectPath));
	static string	s_progTitle			= "Populating Wwise Picker";
	static int		s_currentWwuCnt 	= 0;
	static int		s_totWwuCnt 		= 1;

	public static List<string> s_deletedWwu = new List<string>();
	public static List<string> s_createdWwu = new List<string>();	
	public static List<string> s_changedWwu = new List<string>();
	
	// Used for other threads to ask for a populate
	public static bool s_populateNow = false;
	
	public class AssetType
	{
		public string RootDirectoryName;  
		public string XmlElementName;
		public string ChildElementName;
		
		public AssetType(string RootFolder, string XmlElemName, string ChildName)
		{
			RootDirectoryName = RootFolder;
			XmlElementName = XmlElemName;
			ChildElementName = ChildName;
		}
		
		public AssetType() { }
	}
	
	public static void Tick()
	{
		if (s_populateNow) 
		{
			AkWwisePicker.treeView.SaveExpansionStatus();
			AutoPopulate();
			AkWwisePicker.PopulateTreeview();
			s_populateNow = false;

			//Make sure that the Wwise picker and the inspector are updated
			AkUtilities.RepaintInspector ();
		}
	}
	
	static AkWwiseWWUBuilder()
	{
		// We need a tick function, because the WWU watcher can only signal the main thread to populate.
		EditorApplication.update += Tick;
	}

	public static void AutoPopulate() 
	{	
		//Set the total number of work units to process for the progress bar 
		s_currentWwuCnt	= 0; 
		s_totWwuCnt		= s_deletedWwu.Count + s_createdWwu.Count + s_changedWwu.Count;		

		//Remove deleted work units from our data structures
		for(int i = 0; i < s_deletedWwu.Count; i++)
		{
			string relativePath	= s_deletedWwu[i].Remove(0, s_wwiseProjectPath.Length+1);
			string wwuType		= relativePath.Split(Path.DirectorySeparatorChar)[0];

			ArrayList list = AkWwiseProjectInfo.GetData().GetWwuListByString(wwuType);
			if(list != null)
			{
				int index = list.BinarySearch(new AkWwiseProjectData.WorkUnit(relativePath), AkWwiseProjectData.s_compareByPhysicalPath);

				if(index >= 0)
					list.RemoveAt(index);
			}

			//update progress bar
			s_currentWwuCnt++;
		}
		s_deletedWwu.Clear (); 


		//Update changed work units
		for(int i = 0; i < s_changedWwu.Count; i++) 
		{
			string relativePath	= s_changedWwu[i].Remove(0, s_wwiseProjectPath.Length+1); 
			string wwuType 		= relativePath.Split(Path.DirectorySeparatorChar)[0]; 

			ArrayList list = AkWwiseProjectInfo.GetData().GetWwuListByString(wwuType);
			if(list != null)
			{
				int index = list.BinarySearch	(new AkWwiseProjectData.WorkUnit(relativePath), AkWwiseProjectData.s_compareByPhysicalPath);
				//We assume that the work unit already exist, if it doesn't we skip it
				if(index >= 0) 
					createWorkUnit(relativePath, wwuType, s_changedWwu[i]);
			}

			//update progress bar
			s_currentWwuCnt++; 
		}
		s_changedWwu.Clear ();


		//Add newly created work units to our data structures
		for(int i = 0; i < s_createdWwu.Count; i++)
		{
			string relativePath	= s_createdWwu[i].Remove(0, s_wwiseProjectPath.Length+1);
			string wwuType 		= relativePath.Split(Path.DirectorySeparatorChar)[0]; 

			if(AkWwiseProjectInfo.GetData().IsSupportedWwuType(wwuType))
				createWorkUnit(relativePath, wwuType, s_createdWwu[i]);

			//update progress bar
			s_currentWwuCnt++; 
		}
		s_createdWwu.Clear ();


		EditorUtility.ClearProgressBar();  

		//Save the currrent time
		AkWwiseProjectInfo.GetData().SetLastPopulateTime( DateTime.Now);
	}

	public static void Populate()
	{
		try
		{		
			if (EditorApplication.isPlaying)
			{
				return;
			}
			
			if (WwiseSetupWizard.Settings.WwiseProjectPath == null)   
			{
				WwiseSettings.LoadSettings();
			}
			
			if (String.IsNullOrEmpty(WwiseSetupWizard.Settings.WwiseProjectPath))
			{
				Debug.LogError("Wwise project needed to populate from Work Units. Aborting.");
				return;
			}
			
			s_wwiseProjectPath = Path.GetDirectoryName(AkUtilities.GetFullPath(Application.dataPath, WwiseSetupWizard.Settings.WwiseProjectPath));
			
			if( !Directory.Exists (s_wwiseProjectPath) )
			{
				AkWwisePicker.WwiseProjectFound = false;
				return;
			}
			else
			{
				AkWwisePicker.WwiseProjectFound = true;
			}

			AkWwiseProjectInfo.GetData().EventWwu.Clear();
			AkWwiseProjectInfo.GetData().AuxBusWwu.Clear ();
			AkWwiseProjectInfo.GetData().StateWwu.Clear ();
			AkWwiseProjectInfo.GetData().SwitchWwu.Clear ();
			AkWwiseProjectInfo.GetData().BankWwu.Clear();
				
			

			s_currentWwuCnt = 0;
			
			List<AssetType> AssetsToParse = new List<AssetType>();
			AssetsToParse.Add(new AssetType("Events", "Event", ""));
			AssetsToParse.Add(new AssetType("States", "StateGroup", "State"));
			AssetsToParse.Add(new AssetType("Switches", "SwitchGroup", "Switch"));
			AssetsToParse.Add(new AssetType("Master-Mixer Hierarchy", "AuxBus", ""));
			AssetsToParse.Add(new AssetType("SoundBanks", "SoundBank", ""));
			
			s_totWwuCnt = GetNumOfWwus(AssetsToParse);
			
			foreach (AssetType asset in AssetsToParse)
			{
				bool bSuccess = PopulateListOfType(asset);
				if (!bSuccess)
				{
					Debug.LogError("Error when parsing the work units for " + asset.XmlElementName + " .");
				}
				else
				{
					AkWwiseProjectInfo.GetData().GetWwuListByString(asset.RootDirectoryName).Sort( AkWwiseProjectData.s_compareByPhysicalPath);
				}
			}
			
			EditorUtility.ClearProgressBar(); 

			AkWwiseProjectInfo.GetData().SetLastPopulateTime( DateTime.Now);
		}
		catch (Exception e)
		{
			Debug.LogError(e.ToString());
			EditorUtility.ClearProgressBar(); 
		}
	}
	

	
	static int GetNumOfWwus(List<AssetType> in_assetsToParse)
	{
		int numWwu = 0;
		try
		{
			foreach (AssetType asset in in_assetsToParse)
			{
				string WwusPath = Path.Combine(s_wwiseProjectPath, asset.RootDirectoryName);
				DirectoryInfo di = new DirectoryInfo(WwusPath);
				numWwu += di.GetFiles("*.wwu", SearchOption.AllDirectories).Length;
			}
		}
		catch(Exception)
		{
			// We do not want to cause a division by 0 later on...
			numWwu = 1;
		}
		return numWwu;
	}
	
	static bool PopulateListOfType(AssetType in_type)
	{
		string currentPath = "";
		LinkedList<AkWwiseProjectData.PathElement> PathAndIcons = new LinkedList<AkWwiseProjectData.PathElement>();
		
		try
		{
			string WwusPath = Path.Combine(s_wwiseProjectPath, in_type.RootDirectoryName);
			DirectoryInfo di = new DirectoryInfo(WwusPath);
			return RecurseDirectory(in_type, di, currentPath, PathAndIcons);
		}
		catch (Exception e)
		{
			Debug.LogError(e.ToString());
			return false;
		}
		
	}
	
	static bool RecurseDirectory(AssetType in_type, DirectoryInfo in_currentDirectory, string in_currentPath, LinkedList<AkWwiseProjectData.PathElement> in_pathAndIcons)
	{
		try
		{
			in_currentPath = Path.Combine(in_currentPath, in_currentDirectory.Name);
			in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(in_currentDirectory.Name, AkWwiseProjectData.WwiseObjectType.PHYSICALFOLDER));
			
			// Parse each subdirectory in this folder
			DirectoryInfo[] subDirectories = in_currentDirectory.GetDirectories();
			foreach (DirectoryInfo directory in subDirectories)
			{
				bool bSuccess = RecurseDirectory(in_type, directory, in_currentPath, in_pathAndIcons);
				if (!bSuccess)
				{
					return bSuccess;
				}
			}
			
			// Parse each WWU in this folder. We look for WWUs that have the PersistMode="Standalone" attribute.
			// The other WWUs will be parsed when parsing the "root" work units.
			// First, build a list of "root" WWUs
			FileInfo[] WorkUnits = in_currentDirectory.GetFiles("*.wwu");
			List<FileInfo> StandaloneWorkUnits = new List<FileInfo>();
			foreach (FileInfo WorkUnit in WorkUnits)
			{
				if (IsStandAloneWorkUnit(WorkUnit))
				{
					StandaloneWorkUnits.Add(WorkUnit);
				}
			}
			
			// Second, parse the standalone WWUs completely. This will also parse the child WWUs.
			
			string currentPathInProj = in_currentPath;
			if (currentPathInProj.StartsWith(in_type.RootDirectoryName))
			{
				currentPathInProj = in_currentPath.Remove(0, in_type.RootDirectoryName.Length);
				in_pathAndIcons.RemoveFirst(); 
			}
			if( currentPathInProj.StartsWith(Path.DirectorySeparatorChar.ToString ()) )
			{
				currentPathInProj = currentPathInProj.Remove(0, 1);
			}
			foreach (FileInfo StandaloneWorkUnit in StandaloneWorkUnits)
			{
				bool bSuccess = RecurseWorkUnit(in_type, StandaloneWorkUnit, currentPathInProj, in_currentPath, in_pathAndIcons);
				in_pathAndIcons.Clear();
				if (!bSuccess)
				{
					return bSuccess;
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError(e.ToString());
			return false;
		}
		return true;
	}
	
	static bool IsStandAloneWorkUnit(FileInfo in_wwu)
	{
		XmlReader reader = null;
		try
		{
			reader = XmlReader.Create(in_wwu.FullName);
			
			reader.MoveToContent();
			reader.Read();
			while (!reader.EOF && reader.ReadState == ReadState.Interactive)
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("WorkUnit"))
				{
					bool returnVal = false;
					var matchedElement = XNode.ReadFrom(reader) as XElement;
					if (matchedElement.Attribute("Name").Value.ToLower() == Path.GetFileNameWithoutExtension(in_wwu.Name).ToLower())
					{
						returnVal = matchedElement.Attribute("PersistMode").Value == "Standalone";
						reader.Close();
					}
					
					// Stop at the first workunit element found. It represents the current work unit.
					return returnVal;
					
				}
				else
				{
					reader.Read();
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError(e.ToString());
		}
		
		if (reader != null)
		{
			reader.Close();
		}
		return false;
	}


	static bool RecurseWorkUnit(AssetType in_type, FileInfo in_workUnit, string in_currentPathInProj, string in_currentPhysicalPath, LinkedList<AkWwiseProjectData.PathElement> in_pathAndIcons, string in_parentPhysicalPath = "", bool in_autoPopulate = false, bool in_recurse = true)
	{
		XmlReader reader = null;
		try
		{
			//Progress bar stuff
			string msg = "Parsing Work Unit " + in_workUnit.Name;
			EditorUtility.DisplayProgressBar(s_progTitle, msg, (float)s_currentWwuCnt / (float)s_totWwuCnt);
			s_currentWwuCnt++;

			in_currentPathInProj = Path.Combine(in_currentPathInProj, Path.GetFileNameWithoutExtension(in_workUnit.Name));
			in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(Path.GetFileNameWithoutExtension(in_workUnit.Name), AkWwiseProjectData.WwiseObjectType.WORKUNIT));
			string WwuPhysicalPath = Path.Combine(in_currentPhysicalPath, in_workUnit.Name);

			AkWwiseProjectData.WorkUnit wwu = null;
			int wwuIndex;

			if(in_autoPopulate)
				ReplaceWwuEntry(WwuPhysicalPath, in_type, out wwu, out wwuIndex);
			else
				CreateNewWwuEntry(in_type, out wwu, out wwuIndex);

			wwu.ParentPhysicalPath = in_parentPhysicalPath;
			wwu.PhysicalPath = WwuPhysicalPath;
			wwu.Guid = "";
			
			reader = XmlReader.Create(in_workUnit.FullName);
			
			reader.MoveToContent(); 
			reader.Read();
			while (!reader.EOF && reader.ReadState == ReadState.Interactive)
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("WorkUnit"))
				{
					if(wwu.Guid.Equals(""))
						wwu.Guid = reader.GetAttribute ("ID");

					string persistMode = reader.GetAttribute("PersistMode"); 
					if (persistMode == "Reference")
					{
						// ReadFrom advances the reader
						var matchedElement = XNode.ReadFrom(reader) as XElement;
						string newWorkUnitPath = Path.Combine(in_workUnit.Directory.FullName, matchedElement.Attribute("Name").Value + ".wwu");
						FileInfo newWorkUnit = new FileInfo(newWorkUnitPath);
						
						// Parse the referenced Work Unit
						if(in_recurse)
							RecurseWorkUnit(in_type, newWorkUnit, in_currentPathInProj, in_currentPhysicalPath, in_pathAndIcons, WwuPhysicalPath, in_autoPopulate);  
						else
							reader.Read();
					}
					else
					{
						// If the persist mode is "Standalone" or "Nested", it meams the current XML tag
						// is the one corresponding to the current file. We can ignore it and advance the reader
						reader.Read();
					}
				}
				else if(reader.NodeType == XmlNodeType.Element && reader.Name.Equals("AuxBus"))
				{
					in_currentPathInProj = Path.Combine(in_currentPathInProj, reader.GetAttribute("Name"));
					in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(reader.GetAttribute("Name"), AkWwiseProjectData.WwiseObjectType.AUXBUS));
					bool isEmpty = reader.IsEmptyElement;
					AddElementToList(in_currentPathInProj, reader, in_type, in_pathAndIcons, wwuIndex);

					if(isEmpty)
					{
						in_currentPathInProj = in_currentPathInProj.Remove(in_currentPathInProj.LastIndexOf(Path.DirectorySeparatorChar));
						in_pathAndIcons.RemoveLast();
					}
				}
				// Busses and folders act the same for the Hierarchy: simply add them to the path
				else if (reader.NodeType == XmlNodeType.Element && (reader.Name.Equals("Folder") || reader.Name.Equals("Bus")))
				{
					//check if node has children
					if(!reader.IsEmptyElement)
					{
						// Add the folder/bus to the path
						in_currentPathInProj = Path.Combine(in_currentPathInProj, reader.GetAttribute("Name"));
						if (reader.Name.Equals("Folder"))
						{
							in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(reader.GetAttribute("Name"), AkWwiseProjectData.WwiseObjectType.FOLDER));
						}
						else if(reader.Name.Equals("Bus"))
						{
							in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(reader.GetAttribute("Name"), AkWwiseProjectData.WwiseObjectType.BUS));
						}
					}					
					// Advance the reader
					reader.Read();
					
				}
				else if (reader.NodeType == XmlNodeType.EndElement && (reader.Name.Equals("Folder") || reader.Name.Equals("Bus") || reader.Name.Equals("AuxBus")))
				{
					// Remove the folder/bus from the path
					in_currentPathInProj = in_currentPathInProj.Remove(in_currentPathInProj.LastIndexOf(Path.DirectorySeparatorChar));
					in_pathAndIcons.RemoveLast();

					// Advance the reader
					reader.Read();
				}
				else if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals(in_type.XmlElementName)) 
				{
					// Add the element to the list
					AddElementToList(in_currentPathInProj, reader, in_type, in_pathAndIcons, wwuIndex);
				}
				else
				{
					reader.Read();
				}
			}
			
			// Sort the newly populated Wwu alphabetically
			SortWwu(in_type, wwuIndex);
		}
		catch (Exception e)
		{
			Debug.LogError(e.ToString());
			in_pathAndIcons.RemoveLast();
			return false;
		}
		
		if (reader != null)
		{
			reader.Close();
		}
		
		in_pathAndIcons.RemoveLast();
		return true;
	}

    class tmpData
    {
        public string                               valueName;
        public AkWwiseProjectData.PathElement       pathElem;
        public int                                  ID;
        public AkWwiseProjectData.ByteArrayWrapper  Guid;
    };

    class tmpData_CompareByName : IComparer
    {
        int IComparer.Compare(object a, object b)
        {
            tmpData AkInfA = a as tmpData;
            tmpData AkInfB = b as tmpData;

            return AkInfA.valueName.CompareTo(AkInfB.valueName);
        }
    }
    static tmpData_CompareByName s_comparetmpDataByName = new tmpData_CompareByName();

    static void SortValues(AkWwiseProjectData.GroupValue groupToSort)
    {
        if (groupToSort.values.Count > 0)
        {
            tmpData[] listToSort = new tmpData[groupToSort.values.Count];
            for (int i = 0; i < groupToSort.values.Count; i++)
            {
                listToSort[i] = new tmpData();
                listToSort[i].valueName = groupToSort.values[i];
                listToSort[i].pathElem = groupToSort.ValueIcons[i];
                listToSort[i].ID = groupToSort.valueIDs[i];
                listToSort[i].Guid = groupToSort.ValueGuids[i];
            }

            Array.Sort(listToSort, s_comparetmpDataByName);

            for (int i = 0; i < groupToSort.values.Count; i++)
            {
                groupToSort.values[i] = listToSort[i].valueName;
                groupToSort.ValueIcons[i] = listToSort[i].pathElem;
                groupToSort.valueIDs[i] = listToSort[i].ID;
                groupToSort.ValueGuids[i] = listToSort[i].Guid;
            }
        }
    }

    static void SortWwu(AssetType in_type, int in_wwuIndex)
	{
		if (String.Equals(in_type.RootDirectoryName, "Events", StringComparison.OrdinalIgnoreCase))
		{
			ArrayList.Adapter(AkWwiseProjectInfo.GetData().EventWwu[in_wwuIndex].List).Sort(AkWwiseProjectData.s_compareAkInformationByName);
		}
		else if (String.Equals(in_type.RootDirectoryName, "States", StringComparison.OrdinalIgnoreCase))
		{
			List<AkWwiseProjectData.GroupValue> StateList = AkWwiseProjectInfo.GetData().StateWwu[in_wwuIndex].List;
			ArrayList.Adapter(StateList).Sort(AkWwiseProjectData.s_compareAkInformationByName);
			foreach (AkWwiseProjectData.GroupValue StateGroup in StateList)
			{
                SortValues(StateGroup);
			}
		}
		else if (String.Equals(in_type.RootDirectoryName, "Switches", StringComparison.OrdinalIgnoreCase))
		{
			List<AkWwiseProjectData.GroupValue> SwitchList = AkWwiseProjectInfo.GetData().SwitchWwu[in_wwuIndex].List;
			ArrayList.Adapter(SwitchList).Sort(AkWwiseProjectData.s_compareAkInformationByName);
			foreach (AkWwiseProjectData.GroupValue SwitchGroup in SwitchList)
			{
				SortValues(SwitchGroup);
			}
		}
		else if (String.Equals(in_type.RootDirectoryName, "Master-Mixer Hierarchy", StringComparison.OrdinalIgnoreCase))
		{
			ArrayList.Adapter(AkWwiseProjectInfo.GetData().AuxBusWwu[in_wwuIndex].List).Sort(AkWwiseProjectData.s_compareAkInformationByName);
		}
		else if (String.Equals(in_type.RootDirectoryName, "SoundBanks", StringComparison.OrdinalIgnoreCase))
		{
			ArrayList.Adapter(AkWwiseProjectInfo.GetData().BankWwu[in_wwuIndex].List).Sort(AkWwiseProjectData.s_compareAkInformationByName);
		}
	}
	
	static void CreateNewWwuEntry(AssetType in_type, out AkWwiseProjectData.WorkUnit out_wwu, out int out_wwuIndex)
	{		
		out_wwu = AkWwiseProjectInfo.GetData ().NewChildWorkUnit (in_type.RootDirectoryName);

		ArrayList list = AkWwiseProjectInfo.GetData ().GetWwuListByString (in_type.RootDirectoryName);
		list.Add (out_wwu);
		out_wwuIndex = list.Count - 1;
	}

	static void ReplaceWwuEntry(string in_currentPhysicalPath, AssetType in_type, out AkWwiseProjectData.WorkUnit out_wwu, out int out_wwuIndex)
	{
		ArrayList list 	= AkWwiseProjectInfo.GetData ().GetWwuListByString (in_type.RootDirectoryName);
		out_wwuIndex 	= list.BinarySearch (new AkWwiseProjectData.WorkUnit (in_currentPhysicalPath), AkWwiseProjectData.s_compareByPhysicalPath);
		out_wwu 		= AkWwiseProjectInfo.GetData ().NewChildWorkUnit (in_type.RootDirectoryName);

		if(out_wwuIndex < 0)
		{
			out_wwuIndex = ~out_wwuIndex;
			list.Insert(out_wwuIndex, out_wwu);
		}
		else
		{
			list[out_wwuIndex] = out_wwu;
		}
	}
	
	static void AddElementToList(string in_currentPathInProj, XmlReader in_reader, AssetType in_type, LinkedList<AkWwiseProjectData.PathElement> in_pathAndIcons, int in_wwuIndex)
	{
		if (in_type.RootDirectoryName == "Events" || in_type.RootDirectoryName == "Master-Mixer Hierarchy" || in_type.RootDirectoryName == "SoundBanks")
		{
			AkWwiseProjectData.Event valueToAdd = new AkWwiseProjectData.Event();
			
			valueToAdd.Name = in_reader.GetAttribute("Name");
			valueToAdd.Guid = new System.Guid(in_reader.GetAttribute("ID")).ToByteArray();
			valueToAdd.ID = (int)AkUtilities.ShortIDGenerator.Compute(valueToAdd.Name);
			valueToAdd.Path = in_type.RootDirectoryName == "Master-Mixer Hierarchy" ? in_currentPathInProj : Path.Combine(in_currentPathInProj, valueToAdd.Name);
			valueToAdd.PathAndIcons = new List<AkWwiseProjectData.PathElement>(in_pathAndIcons);
			
			if (in_type.RootDirectoryName == "Events")
			{
				valueToAdd.PathAndIcons.Add(new AkWwiseProjectData.PathElement(valueToAdd.Name, AkWwiseProjectData.WwiseObjectType.EVENT));
				AkWwiseProjectInfo.GetData().EventWwu[in_wwuIndex].List.Add(valueToAdd);
			}
			else if (in_type.RootDirectoryName == "SoundBanks")
			{
				valueToAdd.PathAndIcons.Add(new AkWwiseProjectData.PathElement(valueToAdd.Name, AkWwiseProjectData.WwiseObjectType.SOUNDBANK));
				AkWwiseProjectInfo.GetData().BankWwu[in_wwuIndex].List.Add(valueToAdd);
			}
			else
			{
				AkWwiseProjectInfo.GetData().AuxBusWwu[in_wwuIndex].List.Add(valueToAdd);
			}
			
			in_reader.Read();
		}
		else if (in_type.RootDirectoryName == "States" || in_type.RootDirectoryName == "Switches")
		{
			var XmlElement = XNode.ReadFrom(in_reader) as XElement;
			
			AkWwiseProjectData.GroupValue valueToAdd = new AkWwiseProjectData.GroupValue();
			AkWwiseProjectData.WwiseObjectType SubElemIcon;
			valueToAdd.Name = XmlElement.Attribute("Name").Value;
			valueToAdd.Guid = new System.Guid(XmlElement.Attribute("ID").Value).ToByteArray();
			valueToAdd.ID = (int)AkUtilities.ShortIDGenerator.Compute(valueToAdd.Name);
			valueToAdd.Path = Path.Combine(in_currentPathInProj, valueToAdd.Name);
			valueToAdd.PathAndIcons = new List<AkWwiseProjectData.PathElement>(in_pathAndIcons);
			
			if (in_type.RootDirectoryName == "States")
			{
				SubElemIcon = AkWwiseProjectData.WwiseObjectType.STATE;
				valueToAdd.PathAndIcons.Add(new AkWwiseProjectData.PathElement(valueToAdd.Name, AkWwiseProjectData.WwiseObjectType.STATEGROUP));
			}
			else
			{
				SubElemIcon = AkWwiseProjectData.WwiseObjectType.SWITCH;
				valueToAdd.PathAndIcons.Add(new AkWwiseProjectData.PathElement(valueToAdd.Name, AkWwiseProjectData.WwiseObjectType.SWITCHGROUP));
			}
			
			XName ChildrenList = XName.Get("ChildrenList");
			XName ChildElem = XName.Get(in_type.ChildElementName);
			
			XElement ChildrenElement = XmlElement.Element(ChildrenList);
			if (ChildrenElement != null)
			{
				foreach (var element in ChildrenElement.Elements(ChildElem))
				{
					if (element.Name == in_type.ChildElementName)
					{
						string elementName = element.Attribute("Name").Value;
						valueToAdd.values.Add(elementName);
						valueToAdd.ValueGuids.Add(new AkWwiseProjectData.ByteArrayWrapper( new System.Guid(element.Attribute("ID").Value).ToByteArray()));
						valueToAdd.valueIDs.Add((int)AkUtilities.ShortIDGenerator.Compute(elementName));
						valueToAdd.ValueIcons.Add(new AkWwiseProjectData.PathElement(elementName, SubElemIcon));
					}
				}
			}
			
			if (in_type.RootDirectoryName == "States")
			{
				AkWwiseProjectInfo.GetData().StateWwu[in_wwuIndex].List.Add(valueToAdd);
			}
			else
			{
				AkWwiseProjectInfo.GetData().SwitchWwu[in_wwuIndex].List.Add(valueToAdd);
			}
		}
		else
		{
			Debug.LogError("Unknown asset type in WWU parser");
		}
	}

	static void createWorkUnit(string in_relativePath, string in_wwuType, string in_fullPath)
	{
		string ParentID = string.Empty;
		
		try
		{
			XmlReader reader = XmlReader.Create(in_fullPath);
			reader.MoveToContent();

			//We check if the current work unit has a parent and save its guid if its the case
			while (!reader.EOF && reader.ReadState == ReadState.Interactive)
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("WorkUnit"))
				{
					if(reader.GetAttribute("PersistMode").Equals("Nested"))
					{
						ParentID = reader.GetAttribute("OwnerID");
					}
					break;
				}
				
				reader.Read();  
			}
		}
		catch( Exception e)
		{
			Debug.Log("A changed Work unit wasn't found. It must have been deleted " + in_fullPath);
			return;
		}
		
		if(!ParentID.Equals(string.Empty))
		{
			string parentPhysicalPath = string.Empty;

			ArrayList list = AkWwiseProjectInfo.GetData().GetWwuListByString(in_wwuType);

			//search for the parent and save its physical path
			for(int i = 0; i < list.Count; i++)
			{
				if((list[i] as AkWwiseProjectData.WorkUnit).Guid.Equals(ParentID))
				{
					parentPhysicalPath = (list[i] as AkWwiseProjectData.WorkUnit).PhysicalPath;
					break;
				}
			}

			if(!parentPhysicalPath.Equals(string.Empty))
				UpdateWorkUnit(parentPhysicalPath, in_fullPath, in_wwuType, in_relativePath);
		}
		else					
			UpdateWorkUnit( string.Empty, in_fullPath, in_wwuType, in_relativePath); 
	}

	static void UpdateWorkUnit(string in_parentRelativePath, string in_wwuFullPath, string in_wwuType, string in_relativePath)
	{
		string wwuRelPath = in_parentRelativePath;
		
		LinkedList<AkWwiseProjectData.PathElement> PathAndIcons = new LinkedList<AkWwiseProjectData.PathElement>();

		//We need to build the work unit's hierarchy to display it in the right place in the picker
		string currentPathInProj = string.Empty;
		while(!wwuRelPath.Equals(string.Empty))
		{
			//Add work unit name to the hierarchy
			string wwuName = Path.GetFileNameWithoutExtension(wwuRelPath);
			currentPathInProj = Path.Combine(wwuName, currentPathInProj);
			//Add work unit icon to the hierarchy
			PathAndIcons.AddFirst(new AkWwiseProjectData.PathElement(wwuName, AkWwiseProjectData.WwiseObjectType.WORKUNIT));

			//Get the phycical path of the parent work unit if any
			ArrayList list = AkWwiseProjectInfo.GetData().GetWwuListByString(in_wwuType);
			int index = list.BinarySearch(new AkWwiseProjectData.WorkUnit(wwuRelPath), AkWwiseProjectData.s_compareByPhysicalPath);
			wwuRelPath = (list[index] as AkWwiseProjectData.WorkUnit).ParentPhysicalPath;
		}

		//Add physical folders to the hierarchy if the work unit isn't in the root folder
		string[] physicalPath = in_relativePath.Split (Path.DirectorySeparatorChar);
		for(int i = physicalPath.Length-2; i > 0; i --)
		{
			PathAndIcons.AddFirst(new AkWwiseProjectData.PathElement(physicalPath[i], AkWwiseProjectData.WwiseObjectType.PHYSICALFOLDER));
			currentPathInProj = Path.Combine(physicalPath[i], currentPathInProj);
		} 

		//Parse the work unit file
		RecurseWorkUnit	(	GetAssetTypeByRootDir(in_wwuType),  
		                 	new FileInfo(in_wwuFullPath), 
		                 	currentPathInProj, 
		                 	in_relativePath.Remove(in_relativePath.LastIndexOf(Path.DirectorySeparatorChar)), 
		                 	PathAndIcons,
		                 	in_parentRelativePath,
		                 	true,
		                 	false
		                 );  
	}

	static AssetType GetAssetTypeByRootDir(string in_rootDir)
	{		
		if(String.Equals(in_rootDir, "Events", StringComparison.OrdinalIgnoreCase))
		{
			return new AssetType("Events", "Event", "");
		}
		else if(String.Equals(in_rootDir, "States", StringComparison.OrdinalIgnoreCase))
		{
			return new AssetType("States", "StateGroup", "State");
		}
		else if(String.Equals(in_rootDir, "Switches", StringComparison.OrdinalIgnoreCase))
		{
			return new AssetType("Switches", "SwitchGroup", "Switch");
		}
		else if(String.Equals(in_rootDir, "Master-Mixer Hierarchy", StringComparison.OrdinalIgnoreCase))
		{
			return new AssetType("Master-Mixer Hierarchy", "AuxBus", "");
		}
		else if(String.Equals(in_rootDir, "SoundBanks", StringComparison.OrdinalIgnoreCase))
		{
			return new AssetType("SoundBanks", "SoundBank", "");
		}
	
		return null;
	}
}
#endif