// 
// KeywordCollectionEditor.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a base class for custom editors for KeywordCollections.

using UnityEditor;

namespace Candlelight
{
	/// <summary>
	/// Keyword collection editor base class.
	/// </summary>
	public abstract class KeywordCollectionEditor<T> : Editor where T: KeywordCollection
	{
		/// <summary>
		/// The editor preference to toggle display of keywords in the inspector.
		/// </summary>
		private static readonly EditorPreference<bool, KeywordCollectionEditor<T>> s_KeywordsFoldoutPreference =
			EditorPreference<bool, KeywordCollectionEditor<T>>.ForFoldoutState("keywords", false);

		/// <summary>
		/// The target object as a KeywordCollection.
		/// </summary>
		protected KeywordCollection Collection { get; private set; }
		/// <summary>
		/// The case match property.
		/// </summary>
		protected SerializedProperty CaseMatchProperty { get; private set; }
		/// <summary>
		/// The prioritization property.
		/// </summary>
		protected SerializedProperty PrioritizationProperty { get; private set; }
		
		/// <summary>
		/// Creates a new asset in the project.
		/// </summary>
		protected static void CreateNewAssetInProject()
		{
			AssetDatabaseX.CreateNewAssetInCurrentProjectFolder<T>();
		}

		/// <summary>
		/// Displays the keyword list.
		/// </summary>
		protected void DisplayKeywordList()
		{
			if (this.serializedObject.targetObjects.Length == 1)
			{
				int numKeywords = this.Collection.Keywords == null ? 0 : this.Collection.Keywords.Count;
				s_KeywordsFoldoutPreference.CurrentValue = EditorGUILayout.Foldout(
					s_KeywordsFoldoutPreference.CurrentValue,
					string.Format("Extracted Keywords ({0} Unique)", numKeywords)
				);
				if (s_KeywordsFoldoutPreference.CurrentValue && numKeywords > 0)
				{
					EditorGUI.BeginDisabledGroup(true);
					++EditorGUI.indentLevel;
					foreach (string kw in this.Collection.Keywords)
					{
						EditorGUILayout.TextArea(kw);
					}
					--EditorGUI.indentLevel;
					EditorGUI.EndDisabledGroup();
				}
			}
		}
		
		/// <summary>
		/// Initialize properties.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.Collection = this.target as KeywordCollection;
			this.CaseMatchProperty = this.serializedObject.FindProperty("m_CaseMatchMode");
			this.PrioritizationProperty = this.serializedObject.FindProperty("m_WordPrioritization");
		}
		
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			DisplayKeywordList();
		}
	}
}