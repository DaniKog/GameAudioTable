#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Collections.Generic;

public class AkWwiseXMLBuilder
{
    public static void Populate()
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }

        // Try getting the SoundbanksInfo.xml file for Windows or Mac first, then try to find any other available platform.
        string FullSoundbankPath = AkBankPathUtil.GetPlatformBasePath();
        string filename = Path.Combine(FullSoundbankPath, "SoundbanksInfo.xml");

        if (!File.Exists(filename))
        {
            FullSoundbankPath = Path.Combine(Application.streamingAssetsPath, WwiseSetupWizard.Settings.SoundbankPath);
            string[] foundFiles = Directory.GetFiles(FullSoundbankPath, "SoundbanksInfo.xml", SearchOption.AllDirectories);
            if (foundFiles.Length > 0)
            {
                // We just want any file, doesn't matter which one.
                filename = foundFiles[0];
            }
        }

        if (File.Exists(filename))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            XmlNodeList soundBanks = doc.GetElementsByTagName("SoundBanks");
            for (int i = 0; i < soundBanks.Count; i++)
            {
                XmlNodeList soundBank = soundBanks[i].SelectNodes("SoundBank");
                for (int j = 0; j < soundBank.Count; j++)
                {
                    SerialiseSoundBank(soundBank[j]);
                }
            }
        }
    }

    static void SerialiseSoundBank(XmlNode node)
    {
        XmlNodeList includedEvents = node.SelectNodes("IncludedEvents");
        for (int i = 0; i < includedEvents.Count; i++)
        {
            XmlNodeList events = includedEvents[i].SelectNodes("Event");
            for (int j = 0; j < events.Count; j++)
            {
                SerialiseMaxAttenuation(events[j]);
            }
        }
    }

    static void SerialiseMaxAttenuation(XmlNode node)
    {
        for (int i = 0; i < AkWwiseProjectInfo.GetData().EventWwu.Count; i++)
        {
			for(int j = 0; j < AkWwiseProjectInfo.GetData().EventWwu[i].List.Count; j++)
			{
				if (node.Attributes["MaxAttenuation"] != null && node.Attributes["Name"].InnerText == AkWwiseProjectInfo.GetData().EventWwu[i].List[j].Name)
            	{
					AkWwiseProjectInfo.GetData().EventWwu[i].List[j].maxAttenuation = float.Parse(node.Attributes["MaxAttenuation"].InnerText);
            	    break;
            	}
			}
        }
    }

    /*static void SerialiseEvent(XmlNode node)
    {
        AkWwiseProjectData.Event e = new AkWwiseProjectData.Event();

        e.ID = (int)uint.Parse(node.Attributes["Id"].InnerText);
        e.Name = node.Attributes["Name"].InnerText;
        e.maxAttenuation = 0.0f;

        if (node.Attributes["MaxAttenuation"] != null)
        {        
            e.maxAttenuation = float.Parse(node.Attributes["MaxAttenuation"].InnerText);
        }

        AkWwiseProjectInfo.GetData().events.Add(e);
    }*/
}
#endif