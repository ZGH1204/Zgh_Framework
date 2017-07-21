// 
// FlushChildrenDrawer.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer to display an object's children
// flush with the current indent level.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Candlelight
{
	/// <summary>
	/// A property drawer that renders a generic property's children as though they were not child properties.
	/// </summary>
	[CustomPropertyDrawer(typeof(FlushChildrenAttribute))]
	public class FlushChildrenDrawer : PropertyDrawer
	{
		#region Shared Allocations
		private static GUIContent s_Label = new GUIContent();
		#endregion
		/// <summary>
		/// Serialized property types whose default drawers are expandable.
		/// </summary>
		private static HashSet<SerializedPropertyType> s_ExpandableTypes = new HashSet<SerializedPropertyType>(
			new SerializedPropertyType[]
			{
				SerializedPropertyType.Generic, SerializedPropertyType.Quaternion, SerializedPropertyType.Vector4
			}
		);

		/// <summary>
		/// Gets a value indicating whether this <see cref="FlushChildrenDrawer"/> should display foldout.
		/// </summary>
		/// <value><see langword="true"/> if should display foldout; otherwise, <see langword="false"/>.</value>
		public bool ShouldDisplayFoldout { get { return false; } }

		/// <summary>
		/// Displays the specified child property.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="parentProperty">Parent property.</param>
		/// <param name="childProperty">Child property.</param>
		/// <param name="label">Label.</param>
		protected virtual void DisplayChildProperty(
			Rect position, SerializedProperty parentProperty, SerializedProperty childProperty, GUIContent label
		)
		{
			EditorGUI.PropertyField(position, childProperty, label, true);
		}

		/// <summary>
		/// Gets the height of the specified child property.
		/// </summary>
		/// <returns>The child property height.</returns>
		/// <param name="parentProperty">Parent property.</param>
		/// <param name="childProperty">Child property.</param>
		protected virtual float GetChildPropertyHeight(
			SerializedProperty parentProperty, SerializedProperty childProperty
		)
		{
			s_Label.text = childProperty.displayName;
			return EditorGUI.GetPropertyHeight(childProperty, s_Label, true);
		}

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float result = 0f;
			SerializedProperty childProperty = property.Copy();
			childProperty.NextVisible(true);
			Regex match = new Regex(string.Format("^{0}(?=\\.)", Regex.Escape(property.propertyPath)));
			while (match.IsMatch(childProperty.propertyPath))
			{
				result +=
					GetChildPropertyHeight(property, childProperty.Copy()) + EditorGUIUtility.standardVerticalSpacing;
				childProperty.NextVisible(false);
			}
			if (result > 0f)
			{
				result -= EditorGUIUtility.standardVerticalSpacing;
			}
			return result;
		}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// if the property is not an expandable type, then use its default drawer
			if (!s_ExpandableTypes.Contains(property.propertyType))
			{
				EditorGUI.PropertyField(position, property, label, property.hasVisibleChildren && property.isExpanded);
			}
			else
			{
				SerializedProperty childProperty = property.Copy();
				childProperty.NextVisible(true);
				Regex match = new Regex(string.Format("^{0}(?=\\.)", Regex.Escape(property.propertyPath)));
				while (match.IsMatch(childProperty.propertyPath))
				{
					position.height = GetChildPropertyHeight(property, childProperty);
					DisplayChildProperty(position, property, childProperty.Copy(), null);
					position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
					childProperty.NextVisible(false);
				}
			}
		}
	}
}