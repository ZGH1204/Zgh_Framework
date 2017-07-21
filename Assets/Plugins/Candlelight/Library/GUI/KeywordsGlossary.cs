// 
// KeywordsGlossary.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Candlelight
{
	/// <summary>
	/// A collection of keywords, possible synonyms, and some basic semantic and grammatical information about them.
	/// </summary>
	public class KeywordsGlossary : KeywordCollection
	{
		#region Enumerations
		/// <summary>
		/// A possible part of speech for a word.
		/// </summary>
		public enum PartOfSpeech
		{
			/// <summary>
			/// A noun.
			/// </summary>
			Noun,
			/// <summary>
			/// A verb.
			/// </summary>
			Verb,
			/// <summary>
			/// An adjective.
			/// </summary>
			Adjective,
			/// <summary>
			/// An adverb.
			/// </summary>
			Adverb
		}
		#endregion

		#region Data Types
		/// <summary>
		/// An inflected form for a word.
		/// </summary>
		[System.Serializable]
		public struct InflectedForm : IPropertyBackingFieldCompatible<InflectedForm>
		{
			#region Backing Fields
			[SerializeField]
			private PartOfSpeech m_PartOfSpeech;
			[SerializeField]
			private string m_Word;
			#endregion
			/// <summary>
			/// Gets the part of speech.
			/// </summary>
			/// <value>The part of speech.</value>
			public PartOfSpeech PartOfSpeech { get { return m_PartOfSpeech; } }
			/// <summary>
			/// Gets the word.
			/// </summary>
			/// <value>The word.</value>
			public string Word { get { return m_Word = m_Word ?? string.Empty; } }

			/// <summary>
			/// Initializes a new instance of the <see cref="KeywordsGlossary.InflectedForm"/> struct.
			/// </summary>
			/// <param name="word">Word.</param>
			/// <param name="partOfSpeech">Part of speech.</param>
			public InflectedForm(string word, PartOfSpeech partOfSpeech) : this()
			{
				m_Word = word ?? "";
				m_PartOfSpeech = partOfSpeech;
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public object Clone()
			{
				return this;
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="KeywordsGlossary.InflectedForm"/>.
			/// </summary>
			/// <param name="obj">
			/// The <see cref="System.Object"/> to compare with the current
			/// <see cref="KeywordsGlossary.InflectedForm"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="KeywordsGlossary.InflectedForm"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public override bool Equals(object obj)
			{
				return ObjectX.Equals(ref this, obj);
			}

			/// <summary>
			/// Determines whether the specified <see cref="KeywordsGlossary.InflectedForm"/> is equal to
			/// the current <see cref="KeywordsGlossary.InflectedForm"/>.
			/// </summary>
			/// <param name="other">
			/// The <see cref="KeywordsGlossary.InflectedForm"/> to compare with the current
			/// <see cref="KeywordsGlossary.InflectedForm"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="KeywordsGlossary.InflectedForm"/> is equal to the
			/// current <see cref="KeywordsGlossary.InflectedForm"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public bool Equals(InflectedForm other)
			{
				return GetHashCode() == other.GetHashCode();
			}

			/// <summary>
			/// Serves as a hash function for a <see cref="KeywordsGlossary.InflectedForm"/> object.
			/// </summary>
			/// <returns>
			/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as
			/// a hash table.
			/// </returns>
			public override int GetHashCode()
			{
				return ObjectX.GenerateHashCode(m_PartOfSpeech.GetHashCode(), this.Word.GetHashCode());
			}

			/// <summary>
			/// Gets a hash value that is based on the values of the serialized properties of this instance.
			/// </summary>
			/// <returns>The serialized properties hash.</returns>
			public int GetSerializedPropertiesHash()
			{
				return GetHashCode();
			}
		}

		/// <summary>
		/// A glossary entry consisting of a main form, definition, and other inflected forms.
		/// </summary>
		[System.Serializable]
		public struct Entry : IPropertyBackingFieldCompatible<Entry>
		{
			#region Backing Fields
			[SerializeField, TextArea(5, 5)]
			private string m_Definition;
			[SerializeField]
			InflectedForm m_MainForm;
			[SerializeField]
			private List<InflectedForm> m_OtherForms;
			#endregion
			/// <summary>
			/// Gets the definition.
			/// </summary>
			/// <value>The definition.</value>
			public string Definition { get { return m_Definition; } }
			/// <summary>
			/// Gets the main form.
			/// </summary>
			/// <value>The main form.</value>
			public InflectedForm MainForm { get { return m_MainForm; } }

			/// <summary>
			/// Initializes a new instance of the <see cref="KeywordsGlossary.Entry"/> class.
			/// </summary>
			/// <param name="mainForm">Main form.</param>
			/// <param name="definition">Definition.</param>
			public Entry(InflectedForm mainForm, string definition)
			{
				m_MainForm = mainForm;
				m_Definition = definition ?? "";
				m_OtherForms = new List<InflectedForm>();
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="KeywordsGlossary.Entry"/> class.
			/// </summary>
			/// <param name="mainForm">Main form.</param>
			/// <param name="definition">Definition.</param>
			/// <param name="otherForms">Other forms.</param>
			public Entry(InflectedForm mainForm, string definition, IList<InflectedForm> otherForms) :
				this(mainForm, definition)
			{
				if (otherForms != null)
				{
					m_OtherForms.AddRange(otherForms);
				}
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public object Clone()
			{
				return this;
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="KeywordsGlossary.Entry"/>.
			/// </summary>
			/// <param name="obj">
			/// The <see cref="System.Object"/> to compare with the current <see cref="KeywordsGlossary.Entry"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="KeywordsGlossary.Entry"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public override bool Equals(object obj)
			{
				return ObjectX.Equals(ref this, obj);
			}

			/// <summary>
			/// Determines whether the specified <see cref="KeywordsGlossary.Entry"/> is equal to the current
			/// <see cref="KeywordsGlossary.Entry"/>.
			/// </summary>
			/// <param name="other">
			/// The <see cref="KeywordsGlossary.Entry"/> to compare with the current
			/// <see cref="KeywordsGlossary.Entry"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="KeywordsGlossary.Entry"/> is equal to the current
			/// <see cref="KeywordsGlossary.Entry"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public bool Equals(Entry other)
			{
				return GetHashCode() == other.GetHashCode();
			}

			/// <summary>
			/// Serves as a hash function for a <see cref="KeywordsGlossary.Entry"/> object.
			/// </summary>
			/// <returns>
			/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as
			/// a hash table.
			/// </returns>
			public override int GetHashCode()
			{
				return ObjectX.GenerateHashCode(
					m_Definition.GetHashCode(), m_MainForm.GetHashCode(), ObjectX.GenerateHashCode(m_OtherForms)
				);
			}

			/// <summary>
			/// Gets the other forms.
			/// </summary>
			/// <param name="otherForms">Other forms.</param>
			public void GetOtherForms(List<InflectedForm> otherForms)
			{
				otherForms.Clear();
				if (m_OtherForms != null)
				{
					otherForms.AddRange(m_OtherForms);
				}
			}

			/// <summary>
			/// Gets a hash value that is based on the values of the serialized properties of this instance.
			/// </summary>
			/// <returns>The serialized properties hash.</returns>
			public int GetSerializedPropertiesHash()
			{
				return GetHashCode();
			}

			#region Obsolete
			[System.Obsolete("Use Entry.GetOtherForms(List<InflectedForm>)", true)]
			public void GetOtherForms(ref List<InflectedForm> otherForms) {}
			#endregion
		}
		#endregion

		/// <summary>
		/// A table to look up entries by keyword.
		/// </summary>
		private Dictionary<string, int> m_EntryTable = null;

		#region Backing Fields
		[SerializeField, PropertyBackingField, HideInInspector]
		private List<Entry> m_Entries = new List<Entry>();
		#endregion

		#region Inspector Properties
		private Entry[] GetEntries()
		{
			return m_Entries.ToArray();
		}

		private void SetEntries(Entry[] value)
		{
			SetEntries(value as IList<Entry>);
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets the entries.
		/// </summary>
		/// <param name="entries">Entries.</param>
		public void GetEntries(ref List<Entry> entries)
		{
			entries = entries ?? new List<Entry>(m_Entries.Count);
			entries.Clear();
			entries.AddRange(m_Entries);
		}

		/// <summary>
		/// Gets the entry for the specified keyword if one exists.
		/// </summary>
		/// <returns>The entry.</returns>
		/// <param name="keyword">Keyword.</param>
		public Entry GetEntry(string keyword)
		{
			if (m_EntryTable == null)
			{
				RebuildEntryTable(ref m_EntryTable);
			}
			return m_EntryTable.ContainsKey(keyword) ? m_Entries[m_EntryTable[keyword]] : default(Entry);
		}

		/// <summary>
		/// Sets the entries.
		/// </summary>
		/// <param name="value">Value.</param>
		public void SetEntries(IList<Entry> value)
		{
			List<Entry> newValue = new List<Entry>();
			if (value != null)
			{
				newValue.AddRange(value);
			}
			if (!newValue.SequenceEqual(m_Entries))
			{
				m_Entries.Clear();
				m_Entries.AddRange(newValue);
				RebuildKeywords();
			}
		}
		#endregion

		/// <summary>
		/// Opens the API reference page.
		/// </summary>
		[ContextMenu("API Reference")]
		private void OpenAPIReferencePage()
		{
			this.OpenReferencePage("uas-hypertext");
		}

		/// <summary>
		/// Populates the supplied keyword list.
		/// </summary>
		/// <param name="keywordList">An empty keyword list.</param>
		protected override void PopulateKeywordList(List<string> keywordList)
		{
			for (int i = 0; i < m_Entries.Count; ++i)
			{
				keywordList.Add(m_Entries[i].MainForm.Word);
				using (ListPool<InflectedForm>.Scope otherForms = new ListPool<InflectedForm>.Scope())
				{
					m_Entries[i].GetOtherForms(otherForms.List);
					for (int j = 0; j < otherForms.List.Count; ++j)
					{
						keywordList.Add(otherForms.List[j].Word);
					}
				}
			}
			RebuildEntryTable(ref m_EntryTable);
		}

		/// <summary>
		/// Rebuilds the entry table.
		/// </summary>
		private void RebuildEntryTable(ref Dictionary<string, int> entryTable)
		{
			entryTable = entryTable ?? new Dictionary<string, int>();
			entryTable.Clear();
			string keyword;
			bool ignoreCase = this.CaseMatchMode == CaseMatchMode.IgnoreCase;
			for (int i = m_Entries.Count - 1; i >= 0; --i)
			{
				keyword = m_Entries[i].MainForm.Word;
				if (ignoreCase)
				{
					keyword = keyword.ToLower();
				}
				entryTable[keyword] = i;
				using (ListPool<InflectedForm>.Scope otherForms = new ListPool<InflectedForm>.Scope())
				{
					m_Entries[i].GetOtherForms(otherForms.List);
					for (int j = otherForms.List.Count - 1; j >= 0; --j)
					{
						keyword = otherForms.List[j].Word;
						if (ignoreCase)
						{
							keyword = keyword.ToLower();
						}
						entryTable[keyword] = i;
					}
				}
			}
		}

		#region Obsolete
		[System.Obsolete("Use KeywordsGlossary.SetEntries(IList<Entry>)", true)]
		public void SetEntries(IEnumerable<Entry> value) {}
		#endregion
	}
}