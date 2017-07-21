// 
// FontUpdateTracker.cs
// 
// Copyright (c) 2015-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Collections.Generic;

namespace Candlelight.UI
{
	/// <summary>
	/// This class tracks changes to font textures being used by <see cref="HyperText"/> objects. It mirrors
	/// <see cref="UnityEngine.UI.FontUpdateTracker"/>, which cannot be used because it reads the font property directly
	/// from <see cref="UnityEngine.UI.Text"/> objects.
	/// </summary>
	public static class FontUpdateTracker
	{
		/// <summary>
		/// Fonts being tracked, and their respective <see cref="HyperText"/> objects.
		/// </summary>
		private static Dictionary<Font, List<HyperText>> s_Tracked = new Dictionary<Font, List<HyperText>>();

		/// <summary>
		/// Tracks the supplied <see cref="HyperText"/> object.
		/// </summary>
		/// <param name="hyperText">Hyper text.</param>
		public static void TrackHyperText(HyperText hyperText)
		{
			if (hyperText.FontToUse == null)
			{
				return;
			}
			List<HyperText> exists;
			s_Tracked.TryGetValue(hyperText.FontToUse, out exists);
			if (exists == null)
			{
				exists = new List<HyperText>();
				s_Tracked.Add(hyperText.FontToUse, exists);
#if UNITY_4_6
				hyperText.FontToUse.textureRebuildCallback += RebuildForFont(hyperText.FontToUse);
#else
				Font.textureRebuilt += font => RebuildForFont(hyperText.FontToUse);
#endif
			}
			exists.Add(hyperText);
		}

		/// <summary>
		/// Gets a texture rebuild callback for the supplied font.
		/// </summary>
		/// <returns>A texture rebuild callback.</returns>
		/// <param name="font">Font.</param>
#if UNITY_4_6
		private static Font.FontTextureRebuildCallback RebuildForFont(Font font)
		{
			return () =>
#else
		private static void RebuildForFont(Font font)
		{
#endif
			{
				if (font == null)
				{
					return;
				}
				List<HyperText> texts;
				s_Tracked.TryGetValue(font, out texts);
				if (texts == null)
				{
					return;
				}
				for (int i = 0; i < texts.Count; ++i)
				{
					texts[i].FontTextureChanged();
				}
			};
		}

		/// <summary>
		/// Un-tracks the supplied <see cref="HyperText"/> object.
		/// </summary>
		/// <param name="hyperText">Hyper text.</param>
		public static void UntrackHyperText(HyperText hyperText)
		{
			if (hyperText.FontToUse == null)
			{
				return;
			}
			List<HyperText> texts;
			s_Tracked.TryGetValue(hyperText.FontToUse, out texts);
			if (texts == null)
			{
				return;
			}
			texts.Remove(hyperText);
		}
	}
}