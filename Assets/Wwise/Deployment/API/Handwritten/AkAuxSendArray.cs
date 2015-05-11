#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Runtime.InteropServices;

public class AkAuxSendArray
{
	public AkAuxSendArray(uint in_Count)
	{
        m_Buffer = Marshal.AllocHGlobal((int)in_Count * (sizeof(uint) + sizeof(float)));
        m_Current = m_Buffer;
        m_MaxCount = in_Count;
        m_Count = 0;
    }

	~AkAuxSendArray()
	{
        Marshal.FreeHGlobal(m_Buffer);
        m_Buffer = IntPtr.Zero;
	}
	
	public void Reset()
	{
		m_Current = m_Buffer;        
        m_Count = 0;
	}
	
    public void Add(uint in_EnvID, float in_fValue)
    {
        if (m_Count >= m_MaxCount)
			Resize(m_Count * 2);
                          
        Marshal.WriteInt32(m_Current, (int)in_EnvID);
        m_Current = (IntPtr)(m_Current.ToInt64() + sizeof(uint));		
        Marshal.WriteInt32(m_Current, BitConverter.ToInt32(BitConverter.GetBytes(in_fValue), 0));  //Marshal doesn't do floats.  So copy the bytes themselves.  Grrr.
        m_Current = (IntPtr)(m_Current.ToInt64() + sizeof(float));
        m_Count++;
    }

	public void Resize(uint in_size)
	{
		if(in_size <= m_Count)
		{
			m_Count = in_size;
			return;
		}
		else
		{
			m_MaxCount = in_size;
		}

		IntPtr newBuffer = Marshal.AllocHGlobal ((int)m_MaxCount * (sizeof(uint) + sizeof(float)));
        IntPtr oldBuffer = m_Buffer;
		m_Current = newBuffer;

		for(int i = 0; i < m_Count; i++)
		{
			Marshal.WriteInt32(m_Current, Marshal.ReadInt32(oldBuffer));

			m_Current	= (IntPtr)(m_Current.ToInt64() + sizeof(uint));
			oldBuffer	= (IntPtr)(oldBuffer.ToInt64() + sizeof(uint));

			Marshal.WriteInt32(m_Current, Marshal.ReadInt32(oldBuffer));

			m_Current	= (IntPtr)(m_Current.ToInt64() + sizeof(float));
			oldBuffer	= (IntPtr)(oldBuffer.ToInt64() + sizeof(float));
		}

		Marshal.FreeHGlobal(m_Buffer);
		m_Buffer = newBuffer;
	}

	public void Remove(uint in_EnvID)
	{
		IntPtr ptr = m_Buffer;

		for(int i = 0; i < m_Count; i++)
		{
			if(in_EnvID == (uint)Marshal.ReadInt32(ptr))
			{
				IntPtr endPtr = (IntPtr)(m_Buffer.ToInt64() + ((m_Count - 1) * (sizeof(uint) + sizeof(float))));

				Marshal.WriteInt32(ptr, Marshal.ReadInt32(endPtr));

				ptr		= (IntPtr)(ptr.ToInt64() + sizeof(float));
				endPtr	= (IntPtr)(endPtr.ToInt64() + sizeof(float));

				Marshal.WriteInt32(ptr, Marshal.ReadInt32(endPtr));

				m_Count--;

				break;
			}

			ptr = (IntPtr)(ptr.ToInt64() + sizeof(uint) + sizeof(float));
		}
	}

	public bool Contains(uint in_EnvID)
	{
		IntPtr ptr = m_Buffer;
		
		for(int i = 0; i < m_Count; i++)
		{
			if(in_EnvID == (uint)Marshal.ReadInt32(ptr))
			{
				return true;
			}
			ptr = (IntPtr)(ptr.ToInt64() + sizeof(uint) + sizeof(float));
		}
		
		return false;
	}

	public int OffsetOf(uint in_EnvID)
	{
		int offset = -1;

		IntPtr ptr = m_Buffer;
		
		for(int i = 0; i < m_Count; i++)
		{
			if(in_EnvID == (uint)Marshal.ReadInt32(ptr))
			{
				offset = ptr.ToInt32() - m_Buffer.ToInt32();
				break;
			}
			ptr = (IntPtr)(ptr.ToInt64() + sizeof(uint) + sizeof(float));
		}

		return offset;
	}

	public void RemoveAt(int in_offset)
	{
		IntPtr ptr		= (IntPtr)(m_Buffer.ToInt64() + in_offset);
		IntPtr endPtr	= (IntPtr)(m_Buffer.ToInt64() + ((m_Count - 1) * (sizeof(uint) + sizeof(float))));

		Marshal.WriteInt32(ptr, Marshal.ReadInt32(endPtr));
		
		ptr		= (IntPtr)(ptr.ToInt64() + sizeof(float));
		endPtr	= (IntPtr)(endPtr.ToInt64() + sizeof(float));
		
		Marshal.WriteInt32(ptr, Marshal.ReadInt32(endPtr));
		
		m_Count--;
	}

	public void ReplaceAt(int in_offset, uint in_EnvID, float in_fValue)
	{
		IntPtr ptr	= (IntPtr)(m_Buffer.ToInt64() + in_offset);

		Marshal.WriteInt32(ptr, (int)in_EnvID);

		ptr	= (IntPtr)(ptr.ToInt64() + sizeof(float));

		Marshal.WriteInt32(ptr, BitConverter.ToInt32(BitConverter.GetBytes(in_fValue), 0));
	}

    public IntPtr m_Buffer;    
    private IntPtr m_Current;
    private uint m_MaxCount;
    public uint m_Count;
};
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.