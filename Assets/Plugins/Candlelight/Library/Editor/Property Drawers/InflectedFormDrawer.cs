// 
// InflectedFormDrawer.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight
{
	/// <summary>
	/// Inflected form drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(KeywordsGlossary.InflectedForm))]
	public class InflectedFormDrawer : PropertyDrawer
	{
		/// <summary>
		/// The width of the part of speech field.
		/// </summary>
		private static readonly float s_PartOfSpeechFieldWidth = 70f;
		/// <summary>
		/// The margin of the part of speech field.
		/// </summary>
		private static readonly float s_PartOfSpeechFieldMargin = 2f;
		#region Serialized Properties
		private readonly Dictionary<string, SerializedProperty> m_PartOfSpeech =
			new Dictionary<string, SerializedProperty>();
		private readonly Dictionary<string, SerializedProperty> m_Word = new Dictionary<string, SerializedProperty>();
		#endregion

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Initialize(property);
			return base.GetPropertyHeight(property, label);
		}

		/// <summary>
		/// Initialize the specified property.
		/// </summary>
		/// <param name="property">Property.</param>
		private void Initialize(SerializedProperty property)
		{
			if (!m_PartOfSpeech.ContainsKey(property.propertyPath))
			{
				m_PartOfSpeech.Add(property.propertyPath, property.FindPropertyRelative("m_PartOfSpeech"));
				m_Word.Add(property.propertyPath, property.FindPropertyRelative("m_Word"));
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
#if UNITY_4_6
			// bug 601339
			if (property.isArray && property.propertyType != SerializedPropertyType.String)
			{
				return;
			}
#endif
			Initialize(property);
			position.width -= s_PartOfSpeechFieldWidth + s_PartOfSpeechFieldMargin;
			EditorGUI.PropertyField(position, m_Word[property.propertyPath], GUIContent.none);
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			position.x += position.width + s_PartOfSpeechFieldMargin;
			position.width = s_PartOfSpeechFieldWidth;
			EditorGUI.PropertyField(position, m_PartOfSpeech[property.propertyPath], GUIContent.none);
			EditorGUI.indentLevel = indent;
		}
	}
}