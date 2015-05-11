#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[Serializable]
public class WwiseSettings
{
	public string WwiseProjectPath;
	public string SoundbankPath;
    public bool CreateWwiseGlobal = true;
    public bool CreateWwiseListener = true;
	public bool OldProject = false; //True if the project dates from before integration 2013.2.8
	
	public const string WwiseSettingsFilename = "WwiseSettings.xml";
	
	static WwiseSettings s_Instance = null;    

    public WwiseSettings()
    {
        //Check if this is an old project (pre-2013.2.8) to be migrated
        OldProject = Directory.Exists(Application.dataPath + Path.DirectorySeparatorChar + "Wwise" + Path.DirectorySeparatorChar + "Deployment" + Path.DirectorySeparatorChar + "Examples" );		
    }

	// Save the WwiseSettings structure to a serialized XML file
	public static void SaveSettings(WwiseSettings Settings)
	{
		try
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlSerializer xmlSerializer = new XmlSerializer(Settings.GetType());
			using (MemoryStream xmlStream = new MemoryStream())
			{
				xmlSerializer.Serialize(xmlStream, Settings);
				xmlStream.Position = 0;
				xmlDoc.Load(xmlStream);
				xmlDoc.Save(Path.Combine(Application.dataPath, WwiseSettingsFilename));
			}
		}
		catch (Exception)
		{
			return;
		}
	}
	
	// Load the WwiseSettings structure from a serialized XML file
	public static WwiseSettings LoadSettings(bool ForceLoad = false)
	{
		if (s_Instance != null && !ForceLoad)
            return s_Instance;
			
		WwiseSettings Settings = new WwiseSettings();
		try
		{
			if (File.Exists(Path.Combine(Application.dataPath, WwiseSettingsFilename)))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(Settings.GetType());
				FileStream xmlFileStream = new FileStream(Application.dataPath + "/" + WwiseSettingsFilename, FileMode.Open, FileAccess.Read);
				Settings = (WwiseSettings)xmlSerializer.Deserialize(xmlFileStream);
				xmlFileStream.Close();
			}
			else
			{
				string projectDir = Path.GetDirectoryName(Application.dataPath);
				string[] foundWwiseProjects = Directory.GetFiles(projectDir, "*.wproj", SearchOption.AllDirectories);
				
				if (foundWwiseProjects.Length == 0)
				{
					Settings.WwiseProjectPath = "";
				}
				else
				{
					// MONO BUG: https://github.com/mono/mono/pull/471
					// In the editor, Application.dataPath returns <Project Folder>/Assets. There is a bug in
					// mono for method Uri.GetRelativeUri where if the path ends in a folder, it will
					// ignore the last part of the path. Thus, we need to add fake depth to get the "real"
					// relative path.
					Settings.WwiseProjectPath = AkUtilities.MakeRelativePath(Application.dataPath + "/fake_depth", foundWwiseProjects[0]);
				}
				
				Settings.SoundbankPath = AkBankPathUtil.GetDefaultPath();
			}
			
			s_Instance = Settings;
		}
		catch (Exception)
		{
		}
		
		return Settings;
	}
	
}

public partial class AkUtilities
{
	// Parses the .wproj to find out where soundbanks are generated for the given path
	public static string GetWwiseSoundBankDestinationFolder(string Platform, string WwiseProjectPath)
	{
		try
		{
			if (WwiseProjectPath.Length == 0)
			{
				return "";
			}
			
			XmlDocument doc = new XmlDocument();
			doc.Load(WwiseProjectPath);
			XPathNavigator Navigator = doc.CreateNavigator();
			
			// Navigate the wproj file (XML format) to where generated soundbank paths are stored
			string PathExpression = string.Format("//Property[@Name='SoundBankPaths']/ValueList/Value[@Platform='{0}']", Platform);
			XPathExpression expression = XPathExpression.Compile(PathExpression);
			XPathNavigator node = Navigator.SelectSingleNode(expression);
			string Path = "";
			if( node != null )
			{
				Path = node.Value;
				#if !(UNITY_EDITOR_WIN || UNITY_XBOX360 || UNITY_XBOXONE || UNITY_METRO)
				AkBankPathUtil.ConvertToPosixPath(ref Path);
				#endif // #if !(UNITY_EDITOR_WIN || UNITY_XBOX360 || UNITY_XBOXONE || UNITY_METRO)
			}
			
			return Path;
		}
		catch( Exception )
		{
			// Error happened, return empty string
			return "";
		}
	}
	
	// Set soundbank-related bool settings in the wproj file.
	public static bool EnableBoolSoundbankSettingInWproj(string SettingName, string WwiseProjectPath)
	{
		try
		{
			if (WwiseProjectPath.Length == 0)
			{
				//  The setup should not fail if there is no wproj file set. Silently succeed.
				return true;
			}
			
			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.Load(WwiseProjectPath);
			XPathNavigator Navigator = doc.CreateNavigator();
			
			// Navigate the wproj file (XML format) to where our setting should be
			string pathInXml = String.Format("/WwiseDocument/ProjectInfo/Project/PropertyList/Property[@Name='{0}']", SettingName);
			XPathExpression expression = XPathExpression.Compile(pathInXml);
			XPathNavigator node = Navigator.SelectSingleNode(expression);
			if( node == null )
			{
				// Setting isn't in the wproj, add it
				// Navigate to the SoundBankHeaderFilePath property (it is always there)
				expression = XPathExpression.Compile("/WwiseDocument/ProjectInfo/Project/PropertyList/Property[@Name='SoundBankHeaderFilePath']");
				node = Navigator.SelectSingleNode(expression);
				if (node == null)
				{
					// SoundBankHeaderFilePath not in wproj, invalid wproj file
					UnityEngine.Debug.LogError("Could not find SoundBankHeaderFilePath property in Wwise project file. File is invalid.");
					return false;
				}
				
				// Add the setting right above SoundBankHeaderFilePath
				string propertyToInsert = string.Format("<Property Name=\"{0}\" Type=\"bool\" Value=\"True\"/>", SettingName);
				node.InsertBefore(propertyToInsert);
			}
			else
			{
				// Value is present, we simply have to modify it.
				if( node.GetAttribute("Value", "") == "False" )
				{
					// Modify the value to true
					if (!node.MoveToAttribute("Value", ""))
					{
						return false;
					}
					
					node.SetValue("True");
				}
				else
				{
					// Parameter already set, nothing to do!
					return true;
				}
			}
			
			doc.Save(WwiseProjectPath);
		}
		catch (Exception)
		{
			return false;
		}
		
		return true;
	}
	
	public static bool SetSoundbankHeaderFilePath(string WwiseProjectPath, string SoundbankPath)
	{
		try
		{
			if (WwiseProjectPath.Length == 0)
			{
				//  The setup should not fail if there is no wproj file set. Silently succeed.
				return true;
			}
			
			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.Load(WwiseProjectPath);
			XPathNavigator Navigator = doc.CreateNavigator();
			
			// Navigate to where the header file path is saved. The node has to be there, or else the wproj is invalid.
			XPathExpression expression = XPathExpression.Compile("/WwiseDocument/ProjectInfo/Project/PropertyList/Property[@Name='SoundBankHeaderFilePath']");
			XPathNavigator node = Navigator.SelectSingleNode(expression);
			if (node == null)
			{
				UnityEngine.Debug.LogError("Could not find SoundBankHeaderFilePath property in Wwise project file. File is invalid.");
				return false;
			}
			
			// Change the "Value" attribute
			if( !node.MoveToAttribute("Value", "") )
			{
				return false;
			}
			
			node.SetValue(SoundbankPath);
			doc.Save(WwiseProjectPath);
			return true;
		}
		catch (Exception)
		{
			// Error happened, return empty string
			return false;
		}
	}
	
	// Make two paths relative to each other
	public static string MakeRelativePath(string fromPath, string toPath)
	{
		try
		{
			if (string.IsNullOrEmpty(fromPath))
			{
				return toPath;
			}
			
			if (string.IsNullOrEmpty(toPath))
			{
				return "";
			}
			
			Uri fromUri = new Uri(fromPath);
			Uri toUri = new Uri(toPath);
			
			if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.
			
			Uri relativeUri = fromUri.MakeRelativeUri(toUri);
			string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
			
			return relativePath;
		}
		catch
		{
			return toPath;
		}
	}
	
	// Reconcile a base path and a relative path to give a full path without any ".."
	public static string GetFullPath(string BasePath, string RelativePath)
	{
		string tmpString;
		if (string.IsNullOrEmpty(BasePath))
		{
			return "";
		}
		
		char wrongSeparatorChar = (Path.DirectorySeparatorChar == '/') ? '\\' : '/';
		
		if( string.IsNullOrEmpty(RelativePath))
		{
			return BasePath.Replace(wrongSeparatorChar, Path.DirectorySeparatorChar);
		}
		
		if (Path.GetPathRoot(RelativePath) != "")
		{
			return RelativePath.Replace(wrongSeparatorChar, Path.DirectorySeparatorChar);
		}
		
		tmpString = Path.Combine(BasePath, RelativePath);
		tmpString = Path.GetFullPath(new Uri(tmpString).LocalPath);
		
		return tmpString.Replace(wrongSeparatorChar, Path.DirectorySeparatorChar);
	}
	
	
	public static bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);

		if (!dir.Exists)
		{
			Debug.LogError("Wwise: Source directory doesn't exist");
			return false;
		}

		DirectoryInfo[] dirs = dir.GetDirectories();

		
		// If the destination directory doesn't exist, create it. 
		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}
		
		// Get the files in the directory and copy them to the new location.
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files)
		{
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, true);
		}
		
		// If copying subdirectories, copy them and their contents to new location. 
		if (copySubDirs)
		{
			foreach (DirectoryInfo subdir in dirs)
			{
				string temppath = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, copySubDirs);
			}
		}

		return true;
	}

	public static byte[] GetByteArrayProperty(SerializedProperty property)
	{
		if (!property.isArray || property.arraySize == 0)
			return null;

		byte[] byteArray = new byte[property.arraySize];

		for(int i = 0; i < byteArray.Length; i++)
		{
			byteArray[i] = (byte)property.GetArrayElementAtIndex(i).intValue;
		}

		return byteArray;
	}


	public static void SetByteArrayProperty(SerializedProperty property, byte[] byteArray)
	{
		if (!property.isArray || property.arraySize == 0)
			return;

		SerializedProperty iterator = property.Copy ();

		iterator.arraySize = byteArray.Length;

		while(iterator.name != "data")
		{
			iterator.Next(true);
		}

		for(int i = 0; i < byteArray.Length; i++)
		{
			iterator.intValue = byteArray[i];
			iterator.Next(true); 
		}
	}


	///This function returns the absolute position and the width and height of the last drawn GuiLayout(or EditorGuiLayout) element in the inspector window.
	///This function must be called in the OnInspectorGUI function
	/// 
	///The inspector must be in repaint mode in order to get the correct position 
	///Example => if(Event.current.type == EventType.Repaint) Rect pos = AkUtilities.GetLastRectAbsolute();
	public static Rect GetLastRectAbsolute()
	{ 
		Type inspectorType = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
		
		FieldInfo 		currentInspectorFieldInfo	= inspectorType.GetField("s_CurrentInspectorWindow", BindingFlags.Public | BindingFlags.Static);
		FieldInfo 		scrollPosInfo				= inspectorType.GetField("m_ScrollPosition", BindingFlags.Public | BindingFlags.Instance);
		PropertyInfo	positionPropInfo 			= inspectorType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
		
		Rect	InspectorPosition	= (Rect)positionPropInfo.GetValue(currentInspectorFieldInfo.GetValue(null), null);
		Vector2	scrollPos 			= (Vector2)scrollPosInfo.GetValue(currentInspectorFieldInfo.GetValue(null));
		
		Rect relativePos = GUILayoutUtility.GetLastRect();
		
		Rect absolutePos = new Rect	(	InspectorPosition.x + relativePos.x - scrollPos.x, 
		                             	InspectorPosition.y + relativePos.y - scrollPos.y, 
		                             	relativePos.width, 
		                             	relativePos.height
		                             );

		return absolutePos;  
	}

	public static void RepaintInspector ()
	{
		Type 		inspectorType		= Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
		MethodInfo 	getAllInspectorInfo = inspectorType.GetMethod ("GetAllInspectorWindows", BindingFlags.Public | BindingFlags.Static);

		EditorWindow[] inspector = (EditorWindow[])getAllInspectorInfo.Invoke (null, null);
		if(inspector == null) 
			return;

		for(int i = 0; i < inspector.Length; i++)
		{
			inspector [i].Repaint ();
		}
	}

	///This Function removes the bit at index from an interger the same way you would remove an item from a List.
	///Index zero represents the least significant bit.
	public static int RemoveBitAt(int in_32Bits, int in_index)
	{
		for(int i = in_index; i < 32; i++)
		{
			if((in_32Bits & (1 << (i+1))) != 0)
			{
				in_32Bits |= (1 << i);
			}
			else
			{
				in_32Bits &= ~(1 << i);
			}
		}

		return in_32Bits;
	}

	///This Function inserts a bit at index in an interger the same way you would insert an item in a List.
	///Index zero represents the least significant bit.
	///in_value must be 0 or 1
	public static int InsertBitAt(int in_32Bits, int in_value, int in_index)
	{
		for(int i = 31; i >= in_index; i--)
		{
			if((in_32Bits & (1 << i)) != 0)
			{
				in_32Bits |= (1 << i+1);
			}
			else
			{
				in_32Bits &= ~(1 << i+1);
			}
		}

		if(in_value == 0)
		{
			in_32Bits &= ~(1 << in_index);
		}
		else
		{
			in_32Bits |= (1 << in_index);
		}

		return in_32Bits;
	}
}
#endif // UNITY_EDITOR

    /// <summary>
	/// This is based on FNVHash as used by the DataManager
	/// to assign short IDs to objects. Be sure to keep them both in sync
	/// when making changes!
	/// </summary>
public partial class AkUtilities
{
	public class ShortIDGenerator
	{
		static ShortIDGenerator()
		{
			HashSize = 32;
		}
		
		public static byte HashSize
		{
			get
			{
				return s_hashSize;
			}
			
			set
			{
				s_hashSize = value;
				s_mask = (uint)((1 << s_hashSize) - 1);
			}
		}
		
		public static uint Compute(string in_name)
		{
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(in_name.ToLower());
			
			// Start with the basis value
			uint hval = s_offsetBasis32;
			
			for (int i = 0; i < buffer.Length; i++)
			{
				// multiply by the 32 bit FNV magic prime mod 2^32
				hval *= s_prime32;
				
				// xor the bottom with the current octet
				hval ^= buffer[i];
			}
			
			if (s_hashSize == 32)
				return hval;
			
			// XOR-Fold to the required number of bits
			return (hval >> s_hashSize) ^ (hval & s_mask);
		}
		
		private static byte s_hashSize;
		private static uint s_mask;
		
		private const uint s_prime32 = 16777619;
		private const uint s_offsetBasis32 = 2166136261;
	}
}	

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.