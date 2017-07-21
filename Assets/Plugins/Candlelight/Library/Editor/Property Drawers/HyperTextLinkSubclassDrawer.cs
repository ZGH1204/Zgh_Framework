// 
// HyperTextLinkSubclassDrawer.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight.UI
{
	/// <summary>
	/// HyperText link subclass drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(HyperTextStyles.LinkSubclass))]
	public class HyperTextLinkSubclassDrawer : HyperTextTextStyleDrawer
	{
		#region Labels
		private static readonly GUIContent s_DisabledColorGuiContent =
			new GUIContent("Disabled", "State color for disabled link.");
		private static readonly GUIContent s_FadeDurationGuiContent =
			new GUIContent("Fade", "Length of fade between state colors during transitions.");
		private static readonly GUIContent s_HighlightColorGuiContent =
			new GUIContent("Highlight", "State color for highlighted link.");
		private static readonly GUIContent s_MultiplierGuiContent =
			new GUIContent("Multiplier", "Value multiplied into state color before blending.");
		private static readonly GUIContent s_NormalColorGuiContent =
			new GUIContent("Normal", "State color for normal link.");
		private static readonly GUIContent s_PressedColorGuiContent =
			new GUIContent("Pressed", "State color for pressed link.");
		private static readonly GUIContent s_TintModeGuiContent =
			new GUIContent("Tint", HyperTextStyles.Link.ColorTintModeExplanation);
		private static readonly GUIContent s_TweenModeGuiContent =
			new GUIContent("Tween", "What channels in the state colors should be blended into the base color?");
		#endregion

		/// <summary>
		/// The height of the property.
		/// </summary>
		new public static readonly float propertyHeight =
			9f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

		#region Serialized Properties
		private readonly Dictionary<string, SerializedProperty> m_ClassName =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_ColorMultiplier =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_ColorTintMode =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_ColorTweenMode =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_DisabledColor =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_FadeDuration =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_HighlightedColor =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_NormalColor =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_PressedColor =
			new Dictionary<string, SerializedProperty>();
		#endregion

		/// <summary>
		/// Gets the offset property name prefix.
		/// </summary>
		/// <value>The offset property name prefix.</value>
		protected override string OffsetPropertyNamePrefix { get { return "m_Style."; } }
		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <value>The height of the property.</value>
		protected override float PropertyHeight { get { return propertyHeight; } }
		/// <summary>
		/// Gets the size property name prefix.
		/// </summary>
		/// <value>The size property name prefix.</value>
		protected override string SizePropertyNamePrefix
		{
			get { return string.Format("m_Style.{0}", base.SizePropertyNamePrefix); }
		}

		/// <summary>
		/// Displays the custom fields.
		/// </summary>
		/// <returns>The number of lines drawn in the inspector.</returns>
		/// <param name="firstLinePosition">Position of the first line.</param>
		/// <param name="property">Property.</param>
		protected override int DisplayCustomFields(Rect firstLinePosition, SerializedProperty property)
		{
			int numLines = base.DisplayCustomFields(firstLinePosition, property);
			float horizontalMargin = EditorGUIX.StandardHorizontalSpacing;
			firstLinePosition.y +=
				numLines * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
			firstLinePosition.width =
				0.5f * (firstLinePosition.width - EditorGUIX.StandardHorizontalSpacing);
			EditorGUI.PropertyField(firstLinePosition, m_NormalColor[property.propertyPath], s_NormalColorGuiContent);
			firstLinePosition.x += firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(
				firstLinePosition, m_HighlightedColor[property.propertyPath], s_HighlightColorGuiContent
			);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			firstLinePosition.x -= firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(firstLinePosition, m_PressedColor[property.propertyPath], s_PressedColorGuiContent);
			firstLinePosition.x += firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(
				firstLinePosition, m_DisabledColor[property.propertyPath], s_DisabledColorGuiContent
			);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			firstLinePosition.x -= firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(
				firstLinePosition, m_ColorMultiplier[property.propertyPath], s_MultiplierGuiContent
			);
			firstLinePosition.x += firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(firstLinePosition, m_ColorTintMode[property.propertyPath], s_TintModeGuiContent);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			firstLinePosition.x -= firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(firstLinePosition, m_FadeDuration[property.propertyPath], s_FadeDurationGuiContent);
			firstLinePosition.x += firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(firstLinePosition, m_ColorTweenMode[property.propertyPath], s_TweenModeGuiContent);
			return numLines + 4;
		}

		/// <summary>
		/// Displays the identifier field for this style.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		protected override void DisplayIdentifierField(Rect position, SerializedProperty property)
		{
			EditorGUI.PropertyField(position, m_ClassName[property.propertyPath], classNameGUIContent);
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		/// <param name="property">Property.</param>
		protected override void Initialize (SerializedProperty property)
		{
			base.Initialize(property);
			if (m_ClassName.ContainsKey(property.propertyPath))
			{
				return;
			}
			m_ClassName.Add(property.propertyPath, property.FindPropertyRelative("m_ClassName"));
			m_ColorMultiplier.Add(
				property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_ColorMultiplier")
			);
			m_ColorTintMode.Add(property.propertyPath, property.FindPropertyRelative("m_Style.m_ColorTintMode"));
			m_ColorTweenMode.Add(property.propertyPath, property.FindPropertyRelative("m_Style.m_ColorTweenMode"));
			m_DisabledColor.Add(
				property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_DisabledColor")
			);
			m_HighlightedColor.Add(
				property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_HighlightedColor")
			);
			m_NormalColor.Add(property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_NormalColor"));
			m_PressedColor.Add(property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_PressedColor"));
			m_FadeDuration.Add(property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_FadeDuration"));
		}
	}
}