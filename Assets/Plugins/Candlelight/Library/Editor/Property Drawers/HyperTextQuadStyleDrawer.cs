// 
// HyperTextQuadStyleDrawer.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for HyperTextStyles.Quad.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight.UI
{
	/// <summary>
	/// Hyper text quad style drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(HyperTextStyles.Quad))]
	public class HyperTextQuadStyleDrawer : HyperTextStyleDrawer
	{
		#region Labels
		private static readonly GUIContent colorizationGUIContent =
			new GUIContent("Colorize", "Enable if text color styling should be applied to instances of this quad.");
		private static readonly GUIContent s_LinkClassGuiContent = new GUIContent(
			"Link Class", "if not empty, all instances of this quad will use custom link styles of the specified class."
		);
		private static readonly GUIContent s_LinkIdGuiContent = new GUIContent(
			"Link ID", "If not empty, all instances of this quad will be wrapped in a link tag with the specified ID."
		);
		#endregion
		/// <summary>
		/// The height of the property.
		/// </summary>
		public static readonly float propertyHeight =
			6f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

		#region Serialized Properties
		private readonly Dictionary<string, SerializedProperty> m_ClassName =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_LinkClassName =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_LinkId = new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_ShouldRespectColorization =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_Sprite = new Dictionary<string, SerializedProperty>();
		#endregion

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <value>The height of the property.</value>
		protected override float PropertyHeight { get { return propertyHeight; } }

		/// <summary>
		/// Displays the custom fields.
		/// </summary>
		/// <returns>The number of lines drawn in the inspector.</returns>
		/// <param name="firstLinePosition">Position of the first line.</param>
		/// <param name="property">Property.</param>
		protected override int DisplayCustomFields(Rect firstLinePosition, SerializedProperty property)
		{
			EditorGUI.PropertyField(firstLinePosition, m_Sprite[property.propertyPath]);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(
				firstLinePosition, m_ShouldRespectColorization[property.propertyPath], colorizationGUIContent
			);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			firstLinePosition.width =
				0.5f * (firstLinePosition.width - EditorGUIX.StandardHorizontalSpacing);
			EditorGUI.PropertyField(firstLinePosition, m_LinkId[property.propertyPath], s_LinkIdGuiContent);
			firstLinePosition.x += firstLinePosition.width + EditorGUIX.StandardHorizontalSpacing;
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_LinkId[property.propertyPath].stringValue));
			{
				EditorGUI.PropertyField(firstLinePosition, m_LinkClassName[property.propertyPath], s_LinkClassGuiContent);
			}
			EditorGUI.EndDisabledGroup();
			return 3;
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
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			return propertyHeight;
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		/// <param name="property">Property.</param>
		protected override void Initialize(SerializedProperty property)
		{
			base.Initialize(property);
			if (m_ClassName.ContainsKey(property.propertyPath))
			{
				return;
			}
			m_ClassName.Add(property.propertyPath, property.FindPropertyRelative("m_ClassName"));
			m_LinkClassName.Add(property.propertyPath, property.FindPropertyRelative("m_LinkClassName"));
			m_LinkId.Add(property.propertyPath, property.FindPropertyRelative("m_LinkId"));
			m_ShouldRespectColorization.Add(
				property.propertyPath, property.FindPropertyRelative("m_ShouldRespectColorization")
			);
			m_Sprite.Add(property.propertyPath, property.FindPropertyRelative("m_Sprite"));
		}
	}
}