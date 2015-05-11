#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Collections;
using System.Runtime.InteropServices;

/// This class is an example of how to load banks in Wwise, if the bank data was preloaded in memory.  
/// This would be useful for situations where you use the WWW class
public class AkMemBankLoader : MonoBehaviour
{
	/// Name of the bank to load
	public string bankName = "";
	
	/// Is the bank localized (situated in the language specific folders)
	public bool isLocalizedBank = false;

	private WWW ms_www;
	private GCHandle ms_pinnedArray;
	private IntPtr ms_pInMemoryBankPtr = IntPtr.Zero;
	[HideInInspector]
	public uint ms_bankID = AkSoundEngine.AK_INVALID_BANK_ID;

	private const int WaitMs = 50;

	void Start()
	{
		if (isLocalizedBank)
		{
			LoadLocalizedBank(bankName);
		}
		else
		{
			LoadNonLocalizedBank(bankName);
		}
	}

	/// Load a sound bank from WWW object 
	public AKRESULT LoadNonLocalizedBank(string in_bankFilename)
	{
		string bankPath = Path.Combine(AkBankPathUtil.GetPlatformBasePath(), in_bankFilename);
		return DoLoadBank(bankPath);
	}
	
	/// Load a language-specific bank from WWW object
	public AKRESULT LoadLocalizedBank(string in_bankFilename)
	{
		string bankPath = Path.Combine(Path.Combine (AkBankPathUtil.GetPlatformBasePath(), AkInitializer.GetCurrentLanguage()), in_bankFilename);
		return DoLoadBank(bankPath);
	}

	private AKRESULT DoLoadBank(string in_bankPath)
	{
		ms_www = new WWW(in_bankPath);
		while( ! ms_www.isDone )
		{
#if ! UNITY_METRO
			System.Threading.Thread.Sleep(WaitMs);
#endif // #if ! UNITY_METRO
		}

		uint in_uInMemoryBankSize = 0;
		try
		{
			ms_pinnedArray = GCHandle.Alloc(ms_www.bytes, GCHandleType.Pinned);
			ms_pInMemoryBankPtr = ms_pinnedArray.AddrOfPinnedObject();
			in_uInMemoryBankSize = (uint)ms_www.bytes.Length;	
		}
		catch
		{
			return AKRESULT.AK_Fail;
		}
		
		AKRESULT result = AkSoundEngine.LoadBank(ms_pInMemoryBankPtr, in_uInMemoryBankSize, out ms_bankID);
		
		return result;
	}

	void OnDestroy()
	{
		if (ms_pInMemoryBankPtr != IntPtr.Zero)
		{
			AKRESULT result = AkSoundEngine.UnloadBank(ms_bankID, ms_pInMemoryBankPtr);
			if (result == AKRESULT.AK_Success)
			{
				ms_pinnedArray.Free();	
			}
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.