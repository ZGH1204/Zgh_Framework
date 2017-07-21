// 
// HyperTextQuad.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf


using UnityEngine;

namespace Candlelight.UI
{
	/// <summary>
	/// This component is obsolete. All instances of it can be safely removed from your project and this file deleted.
	/// </summary>
	[ExecuteInEditMode, System.Obsolete]
	public class HyperTextQuad : MonoBehaviour
	{
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			Debug.LogWarning(
				string.Format(
					"The {0} component is no longer necessary. You should use the menu option " +
					"<b>Assets/Candlelight/Clean Up HyperText Quads</b> or remove the component from this object and " +
					"save your scene or apply changes to your prefab as necessary.", GetType()
				), this.gameObject
			);
		}
	}
}