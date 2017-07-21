// 
// PopupDrawer.cs
// 
// Copyright (c) 2015-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace Candlelight
{
	/// <summary>
	/// A property drawer to display some field as a popup.
	/// </summary>
	[CustomPropertyDrawer(typeof(PopupAttribute), true)]
	public class PopupDrawer : PropertyDrawer
	{
		#region Internal Types
		private class PopupData
		{
			#region Backing Fields
			private List<GUIContent> m_Labels = new List<GUIContent>();
			private List<object> m_Values = new List<object>();
			#endregion

			#region Public Properties
			public List<GUIContent> Labels { get { return m_Labels; } }
			public List<object> Values { get { return m_Values; } }
			public PopupAttribute.GetPopupContentsCallback GetContentsCallback { get; set; }
			public StatusPropertyAttribute.GetStatusCallback GetStatusCallback { get; set; }
			#endregion
		}
		#endregion

		/// <summary>
		/// The type array for parameters on a <see cref="PopupAttribute.GetPopupContentsCallback"/>.
		/// </summary>
		private static readonly System.Type[] s_ContentsGetterParams =
			new [] { typeof(List<GUIContent>), typeof(List<object>) };

		/// <summary>
		/// For each property path, the popup data.
		/// </summary>
		private Dictionary<string, PopupData> m_PopupData = new Dictionary<string, PopupData>();

		/// <summary>
		/// Gets the <see cref="Candlelight.PopupAttribute"/>.
		/// </summary>
		/// <value>The attribute.</value>
		private PopupAttribute Attribute { get { return this.attribute as PopupAttribute; } }

		/// <summary>
		/// Displays the popup.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		protected virtual void DisplayPopup(Rect position, SerializedProperty property, GUIContent label)
		{
			FieldInfo field;
			object provider = property.GetProvider(out field);
			string key = property.propertyPath;
			if (!m_PopupData.ContainsKey(key))
			{
				m_PopupData[key] = new PopupData();
			}
			PopupData popupData = m_PopupData[key];
			// initialize the contents getter method
			if (popupData.GetContentsCallback == null)
			{
				MethodInfo method =
					provider.GetType().GetInstanceMethod(this.Attribute.PopupContentsGetter, s_ContentsGetterParams);
				if (method != null)
				{
					popupData.GetContentsCallback = System.Delegate.CreateDelegate(
						typeof(PopupAttribute.GetPopupContentsCallback), provider, method
					) as PopupAttribute.GetPopupContentsCallback;
				}
			}
			// if method cannot be found, display error icon
			if (popupData.GetContentsCallback == null)
			{
				EditorGUIX.DisplayPropertyFieldWithStatus(
					position,
					property,
					ValidationStatus.Error,
					label,
					true,
					string.Format(
						"Unabled to find method: int {0}.{1} (List<GUIContent> labels, List<object> values)",
						provider.GetType(), this.Attribute.PopupContentsGetter
					)
				);
				return;
			}
			else if (
				property.propertyType == SerializedPropertyType.Generic ||
				property.propertyType == SerializedPropertyType.Gradient
			)
			{
				EditorGUIX.DisplayPropertyFieldWithStatus(
					position,
					property,
					ValidationStatus.Error,
					label,
					true,
					string.Format(
						"SerializedPropertyType.{0} not supported for popup drawer.", property.propertyType
					)
				);
				return;
			}
			EditorGUI.BeginProperty(position, label, property);
			{
				int index = popupData.GetContentsCallback(popupData.Labels, popupData.Values);
				EditorGUI.BeginChangeCheck();
				{
					index = EditorGUI.Popup(position, label, index, popupData.Labels.ToArray());
				}
				if (EditorGUI.EndChangeCheck() && index > -1 && index < popupData.Values.Count)
				{
					property.SetValue(popupData.Values[index]);
					popupData.GetContentsCallback = null;
				}
			}
			EditorGUI.EndProperty();
		}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// initialize the status getter method
			string key = property.propertyPath;
			if (!m_PopupData.ContainsKey(key))
			{
				m_PopupData[key] = new PopupData();
			}
			PopupData popupData = m_PopupData[key];
			if (!string.IsNullOrEmpty(this.Attribute.StatusGetter))
			{
				popupData.GetStatusCallback =
					StatusPropertyDrawer.GetStatusCallback(property, this.Attribute.StatusGetter);
			}
			Rect iconPosition = new Rect();
			ValidationStatus status = ValidationStatus.None;
			string statusTooltip = null;
			if (popupData.GetStatusCallback != null || !string.IsNullOrEmpty(this.Attribute.StatusGetter))
			{
				FieldInfo field;
				object provider = property.GetProvider(out field);
				if (popupData.GetStatusCallback != null)
				{
					status = popupData.GetStatusCallback(provider, property.GetValue(), out statusTooltip);
				}
				else
				{
					status = ValidationStatus.Warning;
					statusTooltip = string.Format(
						"{0} {1}.{2} not found.",
						typeof(StatusPropertyAttribute.GetStatusCallback),
						provider.GetType(),
						this.Attribute.StatusGetter
					);
				}
				if (status != ValidationStatus.None)
				{
					position.width -= EditorGUIUtility.singleLineHeight;
					iconPosition = position;
					iconPosition.x += iconPosition.width;
					iconPosition.width = EditorGUIUtility.singleLineHeight;
				}
			}
			DisplayPopup(position, property, label);
			if (status != ValidationStatus.None)
			{
				EditorGUIX.DisplayValidationStatusIcon(iconPosition, status, statusTooltip);
			}
		}
	}
}