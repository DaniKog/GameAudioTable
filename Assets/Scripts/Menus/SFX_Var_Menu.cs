using UnityEngine;
using System.Collections;

public class SFX_Var_Menu : MonoBehaviour
{

	public void SFX_Var_Native()
	{
		Application.LoadLevel("SFX_Variation_Native");
	}
	public void SFX_Var_Wwise()
	{
		Application.LoadLevel("SFX_Variation_Wwise");
	}
	public void SFX_Var_FMOD()
	{
		Application.LoadLevel("SFX_Variation_FMOD");
	}
}
