// 
// HyperTextStyleDrawer.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a base property drawer class for HyperText styles.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight.UI
{
	/// <summary>
	/// Hyper text style drawer base class.
	/// </summary>
	public abstract class HyperTextStyleDrawer : PropertyDrawer
	{
		#region Labels
		protected static readonly GUIContent classNameGUIContent = new GUIContent(
			"Class", "Unique name in the styles for this tag used to reference style with the class attribute."
		);
		private static readonly GUIContent offsetGUIContent = new GUIContent(
			"Offset", "Vertical offset of instances of this style as a percentage of the surrounding font size."
		);
		private static readonly GUIContent s_SizeScalarGuiContent =
			new GUIContent("Scale", "Scale of instances of this style relative to the surrounding font size.");
		#endregion

		#region SerializedProperties;
		private readonly Dictionary<string, SerializedProperty> m_SizeScalar =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_VerticalOffset =
			new Dictionary<string, SerializedProperty>();
		#endregion

		/// <summary>
		/// Gets the width of child field labels.
		/// </summary>
		/// <value>The width of child field labels</value>
		protected float ChildFieldsLabelWidth { get { return 50f; } }
		/// <summary>
		/// Gets the offset property name prefix.
		/// </summary>
		/// <value>The offset property name prefix.</value>
		protected virtual string OffsetPropertyNamePrefix { get { return ""; } }
		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <value>The height of the property.</value>
		protected abstract float PropertyHeight { get; }
		/// <summary>
		/// Gets the size property name prefix.
		/// </summary>
		/// <value>The size property name prefix.</value>
		protected virtual string SizePropertyNamePrefix { get { return ""; } }

		/// <summary>
		/// Displays the custom fields.
		/// </summary>
		/// <returns>The number of lines drawn in the inspector.</returns>
		/// <param name="firstLinePosition">Position of the first line.</param>
		/// <param name="baseProperty">Base property.</param>
		protected virtual int DisplayCustomFields(Rect firstLinePosition, SerializedProperty baseProperty)
		{
			return 0;
		}

		/// <summary>
		/// Displays the identifier field for this style.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="baseProperty">Base property.</param>
		protected virtual void DisplayIdentifierField(Rect position, SerializedProperty baseProperty)
		{

		}

		/// <summary>
		/// Displays the offset and scale fields.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		private void DisplayOffsetAndScale(Rect position, SerializedProperty property)
		{
			// TODO: account for inspector wide mode
			position.width = 0.5f * (position.width - EditorGUIX.StandardHorizontalSpacing);
			EditorGUI.PropertyField(position, m_VerticalOffset[property.propertyPath], offsetGUIContent);
			position.x += position.width + EditorGUIX.StandardHorizontalSpacing;
			EditorGUI.PropertyField(position, m_SizeScalar[property.propertyPath], s_SizeScalarGuiContent);
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		/// <param name="property">Property.</param>
		protected virtual void Initialize(SerializedProperty property)
		{
			if (m_SizeScalar.ContainsKey(property.propertyPath))
			{
				return;
			}
			m_SizeScalar.Add(
				property.propertyPath,
				property.FindPropertyRelative(string.Format("{0}m_SizeScalar", this.SizePropertyNamePrefix))
			);
			m_VerticalOffset.Add(
				property.propertyPath,
				property.FindPropertyRelative(string.Format("{0}m_VerticalOffset", this.OffsetPropertyNamePrefix))
			);
		}

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return this.PropertyHeight;
		}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Initialize(property);
			EditorGUI.BeginProperty(position, label, property);
			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PrefixLabel(position, label);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			position.x += EditorGUIX.pixelsPerIndentLevel;
			position.width -= EditorGUIX.pixelsPerIndentLevel;
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			float oldLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = this.ChildFieldsLabelWidth;
			DisplayIdentifierField(position, property);
			position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			DisplayOffsetAndScale(position, property);
			position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			DisplayCustomFields(position, property);
			EditorGUIUtility.labelWidth = oldLabelWidth;
			EditorGUI.indentLevel = oldIndent;
			EditorGUI.EndProperty();
		}
	}
}