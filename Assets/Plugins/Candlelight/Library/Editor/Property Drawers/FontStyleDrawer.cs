// 
// FontStyleDrawer.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for FontStyle.

using UnityEditor;
using UnityEngine;

namespace Candlelight
{
	/// <summary>
	/// Font style drawer.
	/// </summary>
	[CustomPropertyDrawer (typeof(FontStyle))]
	public class FontStyleDrawer : PropertyDrawer
	{
		#region Backing Fields
		private static GUIStyle s_BoldButtonStyle = null;
		private static GUIStyle s_ItalicButtonStyle = null;
		#endregion
		/// <summary>
		/// Gets the bold button style.
		/// </summary>
		/// <value>The bold button style.</value>
		private static GUIStyle BoldButtonStyle
		{
			get
			{
				if (s_BoldButtonStyle == null)
				{
					s_BoldButtonStyle = new GUIStyle(EditorStylesX.MiniButtonLeft);
					s_BoldButtonStyle.fontStyle = FontStyle.Bold;
				}
				return s_BoldButtonStyle;
			}
		}
		/// <summary>
		/// Gets the italic button style.
		/// </summary>
		/// <value>The italic button style.</value>
		private static GUIStyle ItalicButtonStyle
		{
			get
			{
				if (s_ItalicButtonStyle == null)
				{
					s_ItalicButtonStyle = new GUIStyle(EditorStylesX.MiniButtonRight);
					s_ItalicButtonStyle.padding =
						new RectOffset((int)(EditorGUIUtility.singleLineHeight * -0.3f), 0, 0, 0);
					s_ItalicButtonStyle.fontStyle = FontStyle.Italic;
				}
				return s_ItalicButtonStyle;
			}
		}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			{
				if (label != null && label != GUIContent.none)
				{
					EditorGUI.PrefixLabel(position, label);
					position.x = position.x + EditorGUIUtility.labelWidth;
				}
				FontStyle fontStyle = (FontStyle)property.intValue;
				position.width = EditorGUIUtility.singleLineHeight * 1.25f;
				position.height -= 1f;
				EditorGUI.BeginChangeCheck();
				bool bold = fontStyle == FontStyle.Bold || fontStyle == FontStyle.BoldAndItalic;
				if (EditorGUIX.DisplayButton(position, "b", BoldButtonStyle, bold))
				{
					bold = !bold;
				}
				position.x += position.width;
				bool italic = fontStyle == FontStyle.Italic || fontStyle == FontStyle.BoldAndItalic;
				if (EditorGUIX.DisplayButton(position, "i", ItalicButtonStyle, italic))
				{
					italic = !italic;
				}
				if (EditorGUI.EndChangeCheck())
				{
					property.intValue = (int)(
						bold && italic ? FontStyle.BoldAndItalic :
							bold ? FontStyle.Bold : italic ? FontStyle.Italic : FontStyle.Normal
					);
				}
			}
			EditorGUI.EndProperty();
		}
	}
}