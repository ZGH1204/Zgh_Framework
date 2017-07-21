// 
// LocalizableText.cs
// 
// Copyright (c) 2015-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Candlelight.UI
{
	/// <summary>
	/// A class for storing text values for different locales.
	/// </summary>
	public class LocalizableText : ScriptableObject, ITextSource
	{
		/// <summary>
		/// Locale override entry attribute.
		/// </summary>
		public class LocaleOverrideEntryAttribute : PropertyAttribute {}

		/// <summary>
		/// A basic class to wrap and identify a string to be used as an override for a particular locale.
		/// </summary>
		[System.Serializable]
		private class LocaleOverride : IdentifiableBackingFieldCompatibleObjectWrapper<string, string>
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="LocalizableText.LocaleOverride"/> class.
			/// </summary>
			/// <param name="locale">Locale.</param>
			/// <param name="text">Text.</param>
			public LocaleOverride(string locale, string text) : base(locale, text) {}
		}

		/// <summary>
		/// Occurs when the locale is set for all instances.
		/// </summary>
		private static event System.Action<string> OnSetLocaleForAll = null;

		/// <summary>
		/// The locale to use for all instances.
		/// </summary>
		private static string s_LocaleForAll = null;

		/// <summary>
		/// Default locale string.
		/// </summary>
		private static readonly string s_DefaultLocale = "[DEFAULT]";

		#region Backing Fields
		private static readonly ReadOnlyCollection<string> s_TenMostCommonLanguages = new ReadOnlyCollection<string>(
			new [] { "en", "ja", "ko", "zh", "de", "fr", "pt", "es", "it", "ms" }
		);
		#endregion

		/// <summary>
		/// Gets locale strings for the ten most common languages for mobile apps according to
		/// http://todaysweb.net/top-mobile-apps-games-localization-languages-maximum-revenue/
		/// </summary>
		/// <value>The ten most common languages for mobile apps.</value>
		public static ReadOnlyCollection<string> TenMostCommonLanguages { get { return s_TenMostCommonLanguages; } }

		/// <summary>
		/// Sets the locale for all instances of <see cref="LocalizableText"/>.
		/// </summary>
		/// <remarks>
		/// If you specify a <paramref name="locale"/> value of <see langword="null"/>, then all instances will use
		/// whatever value was serialized for them when they are first loaded. Otherwise, they will automatically use
		/// the specified value when they are first deserialized.
		/// </remarks>
		/// <param name="locale">Locale.</param>
		public static void SetLocaleForAll(string locale)
		{
			s_LocaleForAll = locale;
			if (OnSetLocaleForAll != null)
			{
				OnSetLocaleForAll(locale);
			}
		}

		/// <summary>
		/// Occurs whenever the text on this instance has changed.
		/// </summary>
		public event ITextSourceEventHandler BecameDirty;

		#region Backing Fields
		[SerializeField, PropertyBackingField(typeof(PopupAttribute), "GetCurrentLocalePopupContents")]
		private string m_CurrentLocale = s_DefaultLocale;
		[SerializeField, PropertyBackingField(typeof(TextAreaAttribute), 3, 10)]
		private string m_DefaultText = "";
		[SerializeField, PropertyBackingField(typeof(LocaleOverrideEntryAttribute))]
		private List<LocaleOverride> m_LocaleOverrides = new List<LocaleOverride>();
		#endregion

		/// <summary>
		/// Gets or sets the current locale.
		/// </summary>
		/// <value>The current locale.</value>
		public string CurrentLocale
		{
			get { return m_CurrentLocale; }
			set
			{
				value = value ?? "";
				if (m_CurrentLocale != value)
				{
					m_CurrentLocale = value;
					if (this.BecameDirty != null)
					{
						this.BecameDirty(this);
					}
				}
			}
		}
		/// <summary>
		/// Gets or sets the default text.
		/// </summary>
		/// <value>The default text.</value>
		public string DefaultText
		{
			get { return m_DefaultText; }
			set
			{
				value = value ?? "";
				if (m_DefaultText != value)
				{
					m_DefaultText = value;
					if (this.BecameDirty != null)
					{
						this.BecameDirty(this);
					}
				}
			}
		}
		/// <summary>
		/// Gets the output text.
		/// </summary>
		/// <value>The output text.</value>
		public string OutputText
		{
			get
			{
				int index = m_LocaleOverrides.FindIndex(k => k.Identifier == m_CurrentLocale);
				return index < 0 ? m_DefaultText : m_LocaleOverrides[index].Data;
			}
		}

		/// <summary>
		/// Gets the current locale popup contents. Included for inspector.
		/// </summary>
		/// <returns>The current locale popup contents.</returns>
		/// <param name="labels">Labels.</param>
		/// <param name="values">Values.</param>
		private int GetCurrentLocalePopupContents(List<GUIContent> labels, List<object> values)
		{
			labels.Clear();
			values.Clear();
			int currentIndex = -1;
			for (int i = m_LocaleOverrides.Count - 1; i >= 0; --i)
			{
				labels.Add(new GUIContent(m_LocaleOverrides[i].Identifier));
				values.Add(m_LocaleOverrides[i].Identifier);
				currentIndex = m_LocaleOverrides[i].Identifier == m_CurrentLocale ? i : currentIndex;
			}
			labels.Add(new GUIContent(s_DefaultLocale));
			values.Add(s_DefaultLocale);
			++currentIndex;
			labels.Reverse();
			values.Reverse();
			return currentIndex;
		}

		/// <summary>
		/// Gets the localized text.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <returns>The localized text.</returns>
		private LocaleOverride[] GetLocaleOverrides()
		{
			return m_LocaleOverrides.ToArray();
		}

		/// <summary>
		/// Gets the localized text.
		/// </summary>
		/// <param name="localizedText">A dictionary of string for different locales to populate.</param>
		public void GetLocaleOverrides(Dictionary<string, string> localizedText)
		{
			BackingFieldUtility.GetKeyedListBackingFieldAsDict(m_LocaleOverrides, localizedText, t => t.Data);
		}

		/// <summary>
		/// Raises the disable event.
		/// </summary>
		protected virtual void OnDisable()
		{
			LocalizableText.OnSetLocaleForAll -= SetLocale;
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			LocalizableText.OnSetLocaleForAll += SetLocale;
			if (s_LocaleForAll != null)
			{
				this.CurrentLocale = s_LocaleForAll;
			}
			if (m_LocalizedText.Count == 0)
			{
				return;
			}
			m_LocaleOverrides.Clear();
			for (int i = 0; i < m_LocalizedText.Count; ++i)
			{
				m_LocaleOverrides.Add(new LocaleOverride(m_LocalizedText[i].Locale, m_LocalizedText[i].Text));
			}
			m_LocalizedText.Clear();
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
			Debug.LogWarning(
				"Updated serialization layout. Save your project and commit this object to version control.",
				this
			);
#endif
		}

		/// <summary>
		/// Opens the API reference page.
		/// </summary>
		[ContextMenu("API Reference")]
		private void OpenAPIReferencePage()
		{
			this.OpenReferencePage("uas-hypertext");
		}

		/// <summary>
		/// Sets the locale on this instance.
		/// </summary>
		/// <param name="locale">Locale.</param>
		private void SetLocale(string locale)
		{
			this.CurrentLocale = locale;
		}

		/// <summary>
		/// Sets the localized text.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <param name="value">Value.</param>
		private void SetLocaleOverrides(LocaleOverride[] value)
		{
			if (
				BackingFieldUtility.SetKeyedListBackingFieldFromStringKeyedArray(
					m_LocaleOverrides, value, (locale, wrapper) => new LocaleOverride(locale, wrapper.Data)
				) && this.BecameDirty != null
			)
			{
				this.BecameDirty(this);
			}
		}

		/// <summary>
		/// Sets the localized text.
		/// </summary>
		/// <param name="value">
		/// A dictionary of different possible <see cref="OutputText"/> values for this instance, keyed by locale.
		/// </param>
		public void SetLocaleOverrides(Dictionary<string, string> value)
		{
			if (
				BackingFieldUtility.SetKeyedListBackingFieldFromStringKeyedDict(
					m_LocaleOverrides, value, (locale, text) => new LocaleOverride(locale, text)
				) && this.BecameDirty != null
			)
			{
				this.BecameDirty(this);
			}
		}

		#region Obsolete
		[System.Serializable, System.Obsolete]
		private struct LocalizedText
		{
			#region Backing Fields
			[SerializeField]
			private string m_Locale;
			[SerializeField]
			private string m_Text;
			#endregion
			public string Locale { get { return m_Locale = m_Locale ?? string.Empty; } }
			public string Text { get { return m_Text = m_Text ?? string.Empty; } }
			public LocalizedText(string locale, string text)
			{
				m_Locale = locale;
				m_Text = text;
			}
		}
		#pragma warning disable 612
		[SerializeField]
		private List<LocalizedText> m_LocalizedText = new List<LocalizedText>();
		#pragma warning restore 612
		[System.Obsolete("Use LocalizableText.GetLocaleOverrides(Dictionary<string, string>)", true)]
		public void GetLocaleOverrides(ref Dictionary<string, string> localizedText) {}
		#endregion
	}
}