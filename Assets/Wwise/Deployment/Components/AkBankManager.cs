#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using System;

public class AkBankHandle
{
    int m_RefCount = 0;
    uint m_BankID;

    public string bankName;

    public AkCallbackManager.BankCallback bankCallback;

    public AkBankHandle(string name)
    {
        bankName = name;
        bankCallback = null;
    }

    public int RefCount
    {
        get
        {
            return m_RefCount;
        }
    }

	/// Loads a bank.  This version blocks until the bank is loaded.  See AK::SoundEngine::LoadBank for more information
	public void LoadBank()
	{
		if (m_RefCount == 0)
		{
			AKRESULT res = AkSoundEngine.LoadBank(bankName, AkSoundEngine.AK_DEFAULT_POOL_ID, out m_BankID);
			if (res != AKRESULT.AK_Success)
			{
				Debug.LogWarning("Wwise: Bank " + bankName + " failed to load (" + res.ToString() + ")");
			}
		}

		IncRef();  
	}

	/// Loads a bank.  This version returns right away and loads in background. See AK::SoundEngine::LoadBank for more information
	public void LoadBankAsync(AkCallbackManager.BankCallback callback = null)
	{
		if (m_RefCount == 0)
		{
			bankCallback = callback;
			AkSoundEngine.LoadBank(bankName, AkBankManager.GlobalBankCallback, this, AkSoundEngine.AK_DEFAULT_POOL_ID, out m_BankID);
		}
		IncRef();
	}

    public void IncRef()
    {       
        m_RefCount++;
    }

    public void DecRef()
    {
        m_RefCount--;
        if (m_RefCount == 0)
        {
            IntPtr in_pInMemoryBankPtr = IntPtr.Zero;
            AkSoundEngine.UnloadBank(m_BankID, in_pInMemoryBankPtr, null, null);
        }
    }
}

/// @brief Maintains the list of soundbanks loaded.  This is currently used only with AkAmbient objects.
public static class AkBankManager
{	
    static Dictionary<string, AkBankHandle> m_BankHandles = new Dictionary<string, AkBankHandle>();
	
	static public void GlobalBankCallback(uint in_bankID, IntPtr in_pInMemoryBankPtr, AKRESULT in_eLoadResult, uint in_memPoolId, object in_Cookie)
    {
		m_Mutex.WaitOne();	
		AkBankHandle handle = (AkBankHandle)in_Cookie ;
		AkCallbackManager.BankCallback cb = handle.bankCallback;
		if (in_eLoadResult != AKRESULT.AK_Success)
		{			
			Debug.LogWarning("Wwise: Bank " + handle.bankName + " failed to load (" + in_eLoadResult.ToString() + ")");
			m_BankHandles.Remove(handle.bankName);
		}
		m_Mutex.ReleaseMutex();

		if (cb != null)
			cb(in_bankID, in_pInMemoryBankPtr, in_eLoadResult, in_memPoolId, null);
    }

	/// Loads a bank.  This version blocks until the bank is loaded.  See AK::SoundEngine::LoadBank for more information
    public static void LoadBank(string name)
    {
		m_Mutex.WaitOne();
		AkBankHandle handle = null;
		if (!m_BankHandles.TryGetValue(name, out handle))
		{
			handle = new AkBankHandle(name);
			m_BankHandles.Add(name, handle);			
			m_Mutex.ReleaseMutex();
        	handle.LoadBank();  		
		}
		else
		{
			// Bank already loaded, increment its ref count.
			handle.IncRef();
			m_Mutex.ReleaseMutex();
		}
    }
	
	/// Loads a bank.  This version returns right away and loads in background. See AK::SoundEngine::LoadBank for more information
	public static void LoadBankAsync(string name, AkCallbackManager.BankCallback callback = null)
	{
		m_Mutex.WaitOne();
		AkBankHandle handle = null;
		if (!m_BankHandles.TryGetValue(name, out handle))
		{
			handle = new AkBankHandle(name);
			m_BankHandles.Add(name, handle);			
			m_Mutex.ReleaseMutex();
			handle.LoadBankAsync(callback);  		
		}
		else
		{
			// Bank already loaded, increment its ref count.
			handle.IncRef();
			m_Mutex.ReleaseMutex();
		}
	}

	/// Unloads a bank.  See AK::SoundEngine::UnloadBank for more information
    public static void UnloadBank(string name)
    {
		m_Mutex.WaitOne();
		AkBankHandle handle = null;
		if (m_BankHandles.TryGetValue(name, out handle))
		{
			handle.DecRef();
			if (handle.RefCount == 0)
				m_BankHandles.Remove(name);
		}
		m_Mutex.ReleaseMutex();
    }
	
	static System.Threading.Mutex m_Mutex = new System.Threading.Mutex();
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.