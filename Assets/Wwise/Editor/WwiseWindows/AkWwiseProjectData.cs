using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


public class AkWwiseProjectData : ScriptableObject 
{
	[Serializable]
	public class ByteArrayWrapper
	{
		public ByteArrayWrapper(byte[] byteArray)
		{
			bytes = byteArray;
		}

		public byte[] bytes;
	}

	[Serializable]
	public class AkInformation
    {
        public string Name;
        public string Path;
        public List<PathElement> PathAndIcons = new List<PathElement>();
        public int ID; 
		public byte[] Guid = null;
    }

    [Serializable]
    public class GroupValue : AkInformation
    {
        public List<string> values = new List<string>();
        public List<PathElement> ValueIcons = new List<PathElement>();
        public List<int> valueIDs = new List<int>();

		//Unity can't serialize a list of arrays. So we create a serializable wrapper class for our array 
		public List<ByteArrayWrapper> ValueGuids = new List<ByteArrayWrapper>();
    }

    [Serializable]
    public class Event : AkInformation
    {
        public float maxAttenuation;
    }

	[Serializable]
	public class WorkUnit : IComparable
	{
		public WorkUnit(){}

		public WorkUnit(string in_physicalPath)
		{
			PhysicalPath = in_physicalPath;
		}

		public string PhysicalPath;
		public string ParentPhysicalPath;
		public string Guid;
		
		public int CompareTo( object other )
		{
			WorkUnit otherWwu = other as WorkUnit;

			return PhysicalPath.CompareTo (otherWwu.PhysicalPath);
		} 
	}

	public class WorkUnit_CompareByPhysicalPath : IComparer
	{
		int IComparer.Compare(object a, object b)
		{
			WorkUnit wwuA = a as WorkUnit; 
			WorkUnit wwuB = b as WorkUnit;
			
			return wwuA.PhysicalPath.CompareTo (wwuB.PhysicalPath);
		}
	}

    public class AkInformation_CompareByName : IComparer
    {
        int IComparer.Compare(object a, object b)
        {
            AkInformation AkInfA = a as AkInformation;
            AkInformation AkInfB = b as AkInformation;

            return AkInfA.Name.CompareTo(AkInfB.Name);
        }
    }


	[Serializable]
	public class EventWorkUnit : WorkUnit
	{
		public List<Event> List = new List<Event>();
	}


	[Serializable]
	public class AkInfoWorkUnit : WorkUnit
	{
		public List<AkInformation> List = new List<AkInformation>();
	}

	[Serializable]
	public class GroupValWorkUnit : WorkUnit
	{
		public List<GroupValue> List = new List<GroupValue>();
	}
	
    [Serializable]
    public class PathElement
    {
        public string ElementName;
        public WwiseObjectType ObjectType;

        public PathElement(string Name, WwiseObjectType objType)
        {
            ElementName = Name;
            ObjectType = objType;
        }
    }
    
#if UNITY_5
    public string CurrentPluginConfig;
#endif

    public enum WwiseObjectType
    {
        // Insert Wwise icons description here
        NONE,
        AUXBUS,
        BUS,
        EVENT,
        FOLDER,
        PHYSICALFOLDER,
        PROJECT,
        SOUNDBANK,
        STATE,
        STATEGROUP,
        SWITCH,
        SWITCHGROUP,
        WORKUNIT
    }

	//Can't use a list of WorkUnit and cast it when needed because unity will serialize it as 
	//Workunit and all the child class's fields will be deleted
	public List<EventWorkUnit>		EventWwu	= new List<EventWorkUnit>();
	public List<AkInfoWorkUnit> 	AuxBusWwu 	= new List<AkInfoWorkUnit>();
	public List<GroupValWorkUnit>	StateWwu 	= new List<GroupValWorkUnit>();
	public List<GroupValWorkUnit>	SwitchWwu 	= new List<GroupValWorkUnit>();
	public List<AkInfoWorkUnit>		BankWwu		= new List<AkInfoWorkUnit>();

	//Contains the path of all items that are expanded in the Wwise picker
	public List<string> ExpandedItems = new List<string> ();

	//An IComparer that enables us to sort work units by their physical path 
    public static WorkUnit_CompareByPhysicalPath s_compareByPhysicalPath = new WorkUnit_CompareByPhysicalPath();

    //An IComparer that enables us to sort AkInformations by their physical name
    public static AkInformation_CompareByName s_compareAkInformationByName = new AkInformation_CompareByName();

    public ArrayList GetWwuListByString(string in_wwuType)
    {
        if (String.Equals(in_wwuType, "Events", StringComparison.OrdinalIgnoreCase))
        {
            return ArrayList.Adapter(EventWwu);
        }
        else if (String.Equals(in_wwuType, "States", StringComparison.OrdinalIgnoreCase))
        {
            return ArrayList.Adapter(StateWwu);
        }
        else if (String.Equals(in_wwuType, "Switches", StringComparison.OrdinalIgnoreCase))
        {
            return ArrayList.Adapter(SwitchWwu);
        }
        else if (String.Equals(in_wwuType, "Master-Mixer Hierarchy", StringComparison.OrdinalIgnoreCase))
        {
            return ArrayList.Adapter(AuxBusWwu);
        }
        else if (String.Equals(in_wwuType, "SoundBanks", StringComparison.OrdinalIgnoreCase))
        {
            return ArrayList.Adapter(BankWwu);
        }

        return null;
    }

	public WorkUnit NewChildWorkUnit(string in_wwuType)
	{
		if(String.Equals(in_wwuType, "Events", StringComparison.OrdinalIgnoreCase))
		{
			return new EventWorkUnit();
		}
		else if(String.Equals(in_wwuType, "States", StringComparison.OrdinalIgnoreCase) || String.Equals(in_wwuType, "Switches", StringComparison.OrdinalIgnoreCase))
		{
			return new GroupValWorkUnit();
		}
		else if(String.Equals(in_wwuType, "Master-Mixer Hierarchy", StringComparison.OrdinalIgnoreCase) || String.Equals(in_wwuType, "SoundBanks", StringComparison.OrdinalIgnoreCase))
		{
			return new AkInfoWorkUnit();
		}

		return null;
	}

	public bool IsSupportedWwuType(string in_wwuType)
	{
		if(String.Equals(in_wwuType, "Events", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		else if(String.Equals(in_wwuType, "States", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		else if(String.Equals(in_wwuType, "Switches", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		else if(String.Equals(in_wwuType, "Master-Mixer Hierarchy", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		else if(String.Equals(in_wwuType, "SoundBanks", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		
		return false;
	}

	public byte[] GetEventGuidById(int in_ID)
	{
		for(int i = 0; i < AkWwiseProjectInfo.GetData().EventWwu.Count; i++)
		{
			AkWwiseProjectData.Event e = AkWwiseProjectInfo.GetData().EventWwu[i].List.Find(x => x.ID == in_ID);
			
			if(e != null)
			{
				return e.Guid;
			}
		}
		
		return null;
	}
	
	public byte[] GetBankGuidByName(string in_name)
	{
		for(int i = 0; i < AkWwiseProjectInfo.GetData().BankWwu.Count; i++)
		{
			AkWwiseProjectData.AkInformation bank = AkWwiseProjectInfo.GetData().BankWwu[i].List.Find(x => x.Name.Equals(in_name));
			
			if(bank != null)
			{  
				return bank.Guid;
			}
		}

		return null;
	}
	
	public byte[] GetEnvironmentGuidByName(string in_name)
	{
		for(int i = 0; i < AkWwiseProjectInfo.GetData().AuxBusWwu.Count; i++)
		{
			AkWwiseProjectData.AkInformation auxBus = AkWwiseProjectInfo.GetData().AuxBusWwu[i].List.Find(x => x.Name.Equals(in_name));
			
			if(auxBus != null)
			{  
				return auxBus.Guid;
			}
		}

		return null;
	}
	
	public byte[][] GetStateGuidByName(string in_groupName, string in_valueName)
	{
		byte[][] guids = new byte[][]{new byte[16], new byte[16]};
	
		for(int i = 0; i < AkWwiseProjectInfo.GetData().StateWwu.Count; i++)
		{		
			AkWwiseProjectData.GroupValue stateGroup = AkWwiseProjectInfo.GetData().StateWwu[i].List.Find(x => x.Name.Equals(in_groupName));
			
			if(stateGroup != null)
			{			
				guids[0] = stateGroup.Guid;
				
				int index = stateGroup.values.FindIndex(x => x == in_valueName);
				guids[1] = stateGroup.ValueGuids[index].bytes;
				
				return guids;				
			}
		}
		
		return null;
	}
	
	public byte[][] GetSwitchGuidByName(string in_groupName, string in_valueName)
	{
		byte[][] guids = new byte[][]{new byte[16], new byte[16]};
	
		for(int i = 0; i < AkWwiseProjectInfo.GetData().SwitchWwu.Count; i++)
		{		
			AkWwiseProjectData.GroupValue switchGroup = AkWwiseProjectInfo.GetData().SwitchWwu[i].List.Find(x => x.Name.Equals(in_groupName));
			
			if(switchGroup != null)
			{			
				guids[0] = switchGroup.Guid;
				
				int index = switchGroup.values.FindIndex(x => x == in_valueName);
				guids[1] = switchGroup.ValueGuids[index].bytes;
				
				return guids;				
			}
		}
		
		return null;
	}
	
	
	//DateTime Objects are not serializable, so we have to use its binary format (64 bit long).
	//But apparently long isn't serializable neither, so we split it into two int
	[SerializeField] private int m_lastPopulateTimePsrt1 = 0;
	[SerializeField] private int m_lastPopulateTimePart2 = 0;

	public void SetLastPopulateTime(DateTime in_time)
	{
		long timeBin = in_time.ToBinary ();

		m_lastPopulateTimePsrt1 = (int)timeBin;
		m_lastPopulateTimePart2 = (int)(timeBin >> 32);
	}

	public DateTime GetLastPopulateTime()
	{
		long timeBin = (long)m_lastPopulateTimePart2;
		timeBin <<= 32;
		timeBin |= (uint)m_lastPopulateTimePsrt1;

		return DateTime.FromBinary (timeBin);
	}
  

    public bool autoPopulateEnabled = true;
	
	//This data is a copy of the AkInitializer parameters.  
	//We need it to reapply the same values to copies of the object in different scenes
	//It sits in this object so it is serialized in the same "asset" file
	public string basePath = AkBankPathUtil.GetDefaultPath();
	public string language = AkInitializer.c_Language;
	public int defaultPoolSize = AkInitializer.c_DefaultPoolSize;
	public int lowerPoolSize = AkInitializer.c_LowerPoolSize;
	public int streamingPoolSize = AkInitializer.c_StreamingPoolSize;
	public float memoryCutoffThreshold = AkInitializer.c_MemoryCutoffThreshold;	



	public void SaveInitSettings(AkInitializer in_AkInit)
	{
		if (!CompareInitSettings(in_AkInit))
		{
			basePath =				in_AkInit.basePath;
			language =              in_AkInit.language;
			defaultPoolSize =       in_AkInit.defaultPoolSize;
			lowerPoolSize =         in_AkInit.lowerPoolSize;
			streamingPoolSize =     in_AkInit.streamingPoolSize;
			memoryCutoffThreshold = in_AkInit.memoryCutoffThreshold;
			EditorUtility.SetDirty(this);
		}
	}
	
	public void CopyInitSettings(AkInitializer in_AkInit)
	{
		if (!CompareInitSettings(in_AkInit))
		{			
			in_AkInit.basePath = 				basePath;				
			in_AkInit.language =                language;              
			in_AkInit.defaultPoolSize =         defaultPoolSize;
			in_AkInit.lowerPoolSize =           lowerPoolSize;        
			in_AkInit.streamingPoolSize =       streamingPoolSize;
			in_AkInit.memoryCutoffThreshold =   memoryCutoffThreshold;
			EditorUtility.SetDirty(in_AkInit);
		}
	}

	private bool CompareInitSettings(AkInitializer in_AkInit)
	{
		return basePath ==			in_AkInit.basePath &&
			language ==              in_AkInit.language &&
			defaultPoolSize ==       in_AkInit.defaultPoolSize &&
			lowerPoolSize ==         in_AkInit.lowerPoolSize &&
			streamingPoolSize ==     in_AkInit.streamingPoolSize &&
			memoryCutoffThreshold == in_AkInit.memoryCutoffThreshold;
	}


    public float GetEventMaxAttenuation(int in_eventID)
    {
		for(int i = 0; i < EventWwu.Count; i++)
		{
			for(int j = 0; j < EventWwu[i].List.Count; j++)
			{
				if(EventWwu[i].List[j].ID.Equals(in_eventID))
					return EventWwu[i].List[j].maxAttenuation;
			}
		}
        return 0.0f;
    }

    public void Reset()
    {
        EventWwu = new List<EventWorkUnit>();
		StateWwu = new List<GroupValWorkUnit>();
		SwitchWwu = new List<GroupValWorkUnit>();
        BankWwu = new List<AkInfoWorkUnit>();
		AuxBusWwu = new List<AkInfoWorkUnit>();
    }
}