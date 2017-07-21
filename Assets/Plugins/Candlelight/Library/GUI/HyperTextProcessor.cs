// 
// HyperTextProcessor.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Candlelight.UI
{
	/// <summary>
	/// A class for processing an input string as hyper text.
	/// </summary>
	/// <remarks>
	/// <para>This class's primary function is to extract &lt;a&gt; tags and their relevant information from the value
	/// supplied to the <see cref="HyperTextProcessor.InputText"/> property, and format the resulting string for the
	/// <see cref="HyperTextProcessor.OutputText"/> property. You can then use the
	/// <see cref="M:Candlelight.UI.HyperTextProcessor.GetLinks(System.Collections.Generic.List{Candlelight.UI.HyperTextProcessor.Link})" />
	/// method to get information about the links that were found, such as their character indices in
	/// <see cref="HyperTextProcessor.OutputText"/>. The minimal syntactical requirement for an &lt;a&gt; tag is the
	/// <c>name</c> attribute. For example, the input text <c>"Here is a &lt;a name="some_link"&gt;link&lt;/a&gt;"</c>
	/// will result in the output text <c>"Here is a link".</c></para>
	/// <para>Assigning a <see cref="HyperTextStyles"/> object to the <see cref="HyperTextProcessor.Styles"/> property
	/// also allows for some additional processing. If <see cref="HyperTextProcessor.IsRichTextDesired"/> is
	/// <see langword="true"/>, then it will automatically convert any custom tags and quads found in
	/// <see cref="HyperTextProcessor.InputText"/>, as well as insert &lt;color&gt; tags as needed. If
	/// <see cref="HyperTextProcessor.IsDynamicFontDesired"/> is <see langword="true"/>, then &lt;size&gt; tags will 
	/// automatically be inserted for links, custom tags, and quads. The value of &lt;size&gt; tags depends on either
	/// the font size specified in the styles or the <see cref="HyperTextProcessor.ReferenceFontSize"/> property if no
	/// styles are assigned, as well as the <see cref="HyperTextProcessor.ScaleFactor"/> property. Information about 
	/// custom tags and quads can then be extracted via
	/// <see cref="M:Candlelight.UI.HyperTextProcessor.GetCustomTags(System.Collections.Generic.List{Candlelight.UI.HyperTextProcessor.CustomTag})" />
	/// and
	/// <see cref="M:Candlelight.UI.HyperTextProcessor.GetQuads(System.Collections.Generic.List{Candlelight.UI.HyperTextProcessor.Quad})" />. 
	/// The syntactical requirements for custom styles are:</para>
	/// <para>Link Classes: <c>&lt;a name="some_link" class="class_name"&gt;link&lt;/a&gt;</c></para>
	/// <para>Tags: <c>&lt;custom&gt;text&lt;/custom&gt;</c></para>
	/// <para>Quads: <c>&lt;quad class="class_name" /&gt;</c></para>
	/// <para>You can also assign <see cref="KeywordCollection"/> objects to automatically detect and tag keywords
	/// appearing in <see cref="HyperTextProcessor.InputText"/> as either links or custom tags. Any links automatically
	/// detected in this way will have a <c>name</c> attribute equal to the keyword. For example, the word <c>"dog"</c>
	/// would become <c>"&lt;a name="dog"&gt;dog&lt;/a&gt;"</c>.</para>
	/// <para>The class also allows specification of sizes as percentages rather than raw values. For example, you can 
	/// use the pattern: <c>"&lt;size=120%&gt;BIG TEXT&lt;/size&gt;"</c>.</para>
	/// </remarks>
	[System.Serializable]
	public class HyperTextProcessor : System.IDisposable, ITextSource
	{
		#region Data Types
		/// <summary>
		/// A class for storing information about a custom tag indicated in the text.
		/// </summary>
		public class CustomTag : TagCharacterData
		{
			/// <summary>
			/// Gets the style.
			/// </summary>
			/// <value>The style.</value>
			public HyperTextStyles.Text Style { get; private set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextProcessor.CustomTag"/> class.
			/// </summary>
			/// <param name="indexRange">Index range.</param>
			/// <param name="style">Style.</param>
			public CustomTag(IndexRange indexRange, HyperTextStyles.Text style) : base(indexRange)
			{
				this.Style = style;
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public override object Clone()
			{
				return new CustomTag((IndexRange)this.CharacterIndices.Clone(), this.Style);
			}
		}
		
		/// <summary>
		/// A structure for storing a keyword collection and its associated class. It is used to create associations
		/// between keyword collections and styles specified in the style sheet.
		/// </summary>
		[System.Serializable]
		public struct KeywordCollectionClass : IPropertyBackingFieldCompatible<KeywordCollectionClass>
		{
			/// <summary>
			/// The base hash code to use if the collection is <see langword="null"/>.
			/// </summary>
			private static readonly int s_NullCollectionHash = typeof(KeywordCollection).GetHashCode();

			#region Backing Fields
			[SerializeField]
			private string m_ClassName;
			[SerializeField]
			private KeywordCollection m_Collection;
			#endregion
			
			/// <summary>
			/// Gets the name of the class.
			/// </summary>
			/// <value>The name of the class.</value>
			public string ClassName { get { return m_ClassName = m_ClassName ?? string.Empty; } }
			/// <summary>
			/// Gets the collection.
			/// </summary>
			/// <value>The collection.</value>
			public KeywordCollection Collection { get { return m_Collection; } }

			/// <summary>
			/// Initializes a new instance of the <see cref="KeywordCollectionClass"/> struct.
			/// </summary>
			/// <param name="className">Class name.</param>
			/// <param name="collection">Collection.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if className is null.</exception>
			/// <exception cref="System.ArgumentException">Thrown if className is empty.</exception>
			public KeywordCollectionClass(string className, KeywordCollection collection) : this()
			{
				if (className == null)
				{
					throw new System.ArgumentNullException("className", "Class name cannot be null or empty.");
				}
				else if (
#if UNITY_EDITOR
					Application.isPlaying &&
#endif
					className == string.Empty
				)
				{
					throw new System.ArgumentException("Class name cannot be null or empty", "className");
				}
				m_ClassName = className;
				m_Collection = collection;
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
			/// <see cref="KeywordCollectionClass"/>.
			/// </summary>
			/// <param name="obj">
			/// The <see cref="System.Object"/> to compare with the current <see cref="KeywordCollectionClass"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="KeywordCollectionClass"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public override bool Equals(object obj)
			{
				return ObjectX.Equals(ref this, obj);
			}

			/// <summary>
			/// Determines whether the specified <see cref="KeywordCollectionClass"/> is equal to the current
			/// <see cref="KeywordCollectionClass"/>.
			/// </summary>
			/// <param name="other">
			/// The <see cref="KeywordCollectionClass"/> to compare with the current
			///  <see cref="HyperTextProcessor.KeywordCollectionClass"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="KeywordCollectionClass"/> is equal to the current
			/// <see cref="KeywordCollectionClass"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public bool Equals(KeywordCollectionClass other)
			{
				return GetHashCode() == other.GetHashCode();
			}

			/// <summary>
			/// Serves as a hash function for a <see cref="KeywordCollectionClass"/> object.
			/// </summary>
			/// <returns>
			/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as
			/// a hash table.
			/// </returns>
			public override int GetHashCode()
			{
				return ObjectX.GenerateHashCode(
					this.ClassName.GetHashCode(),
					m_Collection == null ? s_NullCollectionHash : m_Collection.GetHashCode()
				);
			}
			
			/// <summary>
			/// Gets a hash value that is based on the values of the serialized properties of this instance.
			/// </summary>
			/// <returns>A hash value based on the values of the serialized properties on this instance.</returns>
			public int GetSerializedPropertiesHash()
			{
				return GetHashCode();
			}
		}

		/// <summary>
		/// A custom <see cref="UnityEngine.PropertyAttribute"/> to specify information for inspector labels on a
		/// <see cref="HyperTextProcessor.KeywordCollectionClass"/>.
		/// </summary>
		public class KeywordCollectionClassAttribute : PropertyAttribute
		{
			/// <summary>
			/// Gets the label.
			/// </summary>
			/// <value>The label.</value>
			public GUIContent Label { get; private set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextProcessor.KeywordCollectionClassAttribute"/>
			/// class.
			/// </summary>
			/// <param name="identifierLabel">Identifier label.</param>
			/// <param name="identifierDescription">Identifier description.</param>
			public KeywordCollectionClassAttribute(string identifierLabel, string identifierDescription)
			{
				this.Label = new GUIContent(
					identifierLabel, string.Format("{0} with which the collection is associated", identifierDescription)
				);
			}
		}
		
		/// <summary>
		/// A class for storing information about a link indicated in the text.
		/// </summary>
		public class Link : TagCharacterData
		{
			/// <summary>
			/// Gets the name of the class.
			/// </summary>
			/// <value>The name of the class.</value>
			public string ClassName { get; private set; }
			/// <summary>
			/// Gets the value of the <c>name</c> attribute.
			/// </summary>
			/// <value>The value of the <c>name</c> attribute.</value>
			public string Name { get; private set; }
			/// <summary>
			/// Gets or sets the style.
			/// </summary>
			/// <value>The style.</value>
			public HyperTextStyles.Link Style { get; private set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextProcessor.Link"/> class.
			/// </summary>
			/// <param name="linkName">Value of the link's <c>name</c> attribute.</param>
			/// <param name="className">Class name.</param>
			/// <param name="characterIndices">Character indices.</param>
			/// <param name="style">Style.</param>
			public Link(
				string linkName, string className, IndexRange characterIndices, HyperTextStyles.Link style
			) : base(characterIndices)
			{
				this.Name = linkName;
				this.ClassName = className;
				this.Style = style;
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public override object Clone()
			{
				return new Link(this.Name, this.ClassName, (IndexRange)this.CharacterIndices.Clone(), this.Style);
			}
		}
		
		/// <summary>
		/// A class for storing information about a quad indicated in the text.
		/// </summary>
		public class Quad : TagCharacterData
		{
			/// <summary>
			/// Gets the style.
			/// </summary>
			/// <value>The style.</value>
			public HyperTextStyles.Quad Style { get; private set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextProcessor.Quad"/> class.
			/// </summary>
			/// <param name="indexRange">Index range.</param>
			/// <param name="style">Style.</param>
			public Quad(IndexRange indexRange, HyperTextStyles.Quad style) : base(indexRange)
			{
				this.Style = style;
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public override object Clone()
			{
				return new Quad((IndexRange)this.CharacterIndices.Clone(), this.Style);
			}
		}
		
		/// <summary>
		/// A base class for storing data about the characters for a tag appearing in the text.
		/// </summary>
		public abstract class TagCharacterData : System.ICloneable
		{
			/// <summary>
			/// Gets or sets the character indices.
			/// </summary>
			/// <value>The character indices.</value>
			public IndexRange CharacterIndices { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextProcessor.TagCharacterData"/>
			/// class.
			/// </summary>
			/// <param name="indexRange">Index range.</param>
			protected TagCharacterData(IndexRange indexRange)
			{
				this.CharacterIndices = indexRange;
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public abstract object Clone();
		}
		#endregion

		/// <summary>
		/// A table of replacement regular expressions for keywords.
		/// </summary>
		private static readonly Dictionary<int, Regex> s_KeywordRegexTable = new Dictionary<int, Regex>();
		/// <summary>
		/// A regular expression to extract an &lt;a&gt; tag, its arguments, and enclosed text in postprocessed text.
		/// </summary>
		private static readonly Regex s_PostprocessedLinkTagRegex = new Regex(
			string.Format(
				"<a name\\s*=\\s*\"(?<{0}>.*?)\"(\\s+class\\s*=\\s*\"(?<{1}>.*?)\")?>(?<{2}>.*?)(?<{3}></a>)",
				AttributeValueCaptureGroup, ClassNameCaptureGroup, TextCaptureGroup, CloseTagCaptureGroup
			),
			RegexOptions.Singleline | RegexOptions.IgnoreCase
		);
		/// <summary>
		/// A regular expression to extract the attribute value of a &lt;size&gt; tag or the size attribute of a
		/// &lt;quad&gt; tag.
		/// </summary>
		private static readonly Regex s_PostProcessedSizeAttributeRegex = new Regex(
			string.Format(
				@"(?<{0}><size\s*=\s*)(?<{1}>\d+)(?<{2}>>)|(?<{0}><quad\b[^>]*?\bsize=)(?<{1}>\d+)(?<{2}>[^>]*?>)",
				OpenTagCaptureGroup, AttributeValueCaptureGroup, CloseTagCaptureGroup
			),
			RegexOptions.Singleline | RegexOptions.IgnoreCase
		);
		/// <summary>
		/// The base match pattern for any rich text tag in preprocessed text (used when supportRichText = true).
		/// </summary>
		private static readonly string s_PreprocessedAnyTagMatchPattern =
			"</?a\b.*?>|" +
			"<quad\b.*?>|" +
			"</?color\b.*?>|" +
			"</?i>|" +
			"</?b>|" +
			"</?size\b.*?>|" +
			"</?material\b.*?>";
		/// <summary>
		/// A regular expression to match only &lt;a&gt; tags in preprocessed text (used when supportRichText = false).
		/// </summary>
		private static readonly Regex s_PreprocessedLinkTagRegex =
			new Regex("</?a\b.*?>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
		/// <summary>
		/// A regular expression to extract a &lt;size&gt; tag and its arguments in preprocessed text.
		/// </summary>
		private static readonly Regex s_PreprocessedSizeTagRegex = new Regex(
			string.Format(
				"<size\\s*=\\s*(?<{0}>\\d*\\.?\\d+%?)>(?<{1}>.+?)</size>", AttributeValueCaptureGroup, TextCaptureGroup
			),
			RegexOptions.Singleline | RegexOptions.IgnoreCase
		);
		/// <summary>
		/// A regular expression to extract a &lt;quad&gt; tag in text.
		/// </summary>
		private static readonly Regex s_QuadTagRegex = new Regex(
			string.Format("<quad class\\s*=\\s*\"(?<{0}>.+?)\"\\s*.*?/?>", ClassNameCaptureGroup),
			RegexOptions.IgnoreCase
		);
		/// <summary>
		/// Regular expressions for each custom tag.
		/// </summary>
		private static readonly Dictionary<string, Regex> s_TagRegexes = new Dictionary<string, Regex>();

		#region Backing Fields
		private static readonly ReadOnlyCollection<string> s_ReservedTags = new ReadOnlyCollection<string>(
			new [] { "a", "b", "color", "i", "material", "quad", "size" }
		);
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets the name of the capture group used for a close tag in a piece of text.
		/// </summary>
		/// <value>The name of the capture group used for a close tag in a piece of text.</value>
		public static string CloseTagCaptureGroup { get { return "closeTag"; } }
		/// <summary>
		/// Gets the name of the capture group used for an open tag in a piece of text.
		/// </summary>
		/// <value>The name of the capture group used for an open tag in a piece of text.</value>
		public static string OpenTagCaptureGroup { get { return "openTag"; } }
		/// <summary>
		/// Gets the reserved tags.
		/// </summary>
		/// <value>The reserved tags.</value>
		public static ReadOnlyCollection<string> ReservedTags { get { return s_ReservedTags; } }
		/// <summary>
		/// Gets the name of the capture group used for text enclosed in a tag.
		/// </summary>
		/// <value>The name of the capture group used for text enclosed in a tag.</value>
		public static string TextCaptureGroup { get { return "text"; } }
		#endregion

		/// <summary>
		/// Gets the name of the capture group used for a tag's attribute value of interest.
		/// </summary>
		/// <value>The name of the capture group used for a tag's attribute value of interest.</value>
		private static string AttributeValueCaptureGroup { get { return "attributeValue"; } }
		/// <summary>
		/// Gets the name of the capture group used for a tag's class attribute value.
		/// </summary>
		/// <value>The name of the capture group used for a tag's class attribute value.</value>
		private static string ClassNameCaptureGroup { get { return "className"; } }

		/// <summary>
		/// Compares two <see cref="TagCharacterData"/> objects by their start indices. Used to sort cached tag lists.
		/// </summary>
		/// <typeparam name="T">A <see cref="TagCharacterData"/> type.</typeparam>
		/// <param name="a">The first <typeparamref name="T"/>.</param>
		/// <param name="b">The second <typeparamref name="T"/>.</param>
		/// <returns>1 if <paramref name="a"/> comes after <paramref name="b"/>; otherwise, -1.</returns>
		private static int CompareTagsByStartIndex<T>(T a, T b) where T : TagCharacterData
		{
			return a.CharacterIndices.StartIndex.CompareTo(b.CharacterIndices.StartIndex);
		}

		/// <summary>
		/// Gets a regular expression for matching a particular tag and its enclosed text.
		/// </summary>
		/// <returns>A regular expression for matching a particular tag and its enclosed text.</returns>
		/// <param name="tag">Tag.</param>
		private static Regex GetTagRegex(string tag)
		{
			if (!s_TagRegexes.ContainsKey(tag))
			{
				s_TagRegexes[tag] = new Regex(
					string.Format(
						"(?<{0}><{1}>)(?<{2}>.+?)(?<{3}></{1}>)",
						HyperTextProcessor.OpenTagCaptureGroup,
						Regex.Escape(tag),
						HyperTextProcessor.TextCaptureGroup,
						HyperTextProcessor.CloseTagCaptureGroup
					), RegexOptions.Singleline | RegexOptions.IgnoreCase
				);
			}
			return s_TagRegexes[tag];
		}

		/// <summary>
		/// Occurs whenever a value on this instance has changed.
		/// </summary>
		public event ITextSourceEventHandler BecameDirty;

		/// <summary>
		/// A value indicating whether or not m_ProcessedText is currently dirty.
		/// </summary>
		[System.NonSerialized]
		private bool m_IsDirty = true;
		/// <summary>
		/// The custom text style to use inside of the <see cref="MatchEvaluator"/> <see cref="ReplaceCustomTag()"/>.
		/// </summary>
		[System.NonSerialized]
		private HyperTextStyles.Text m_ReplacementCustomTextStyle;
		/// <summary>
		/// The link style to use inside of the <see cref="MatchEvaluator"/> <see cref="ReplaceLink()"/>.
		/// </summary>
		[System.NonSerialized]
		private HyperTextStyles.Link m_ReplacementLinkStyle;
		/// <summary>
		/// The table of index ranges and size scalars to use inside of the <see cref="MatchEvaluator"/> methods
		/// <see cref="ReplaceCustomTag()"/> and <see cref="ReplaceLink()"/>.
		/// </summary>
		[System.NonSerialized]
		private Dictionary<IndexRange, float> m_ReplacementProcessedIndexRangesAndScalars = null;

		#region Backing Fields
		[System.NonSerialized]
		private List<CustomTag> m_CustomTags = new List<CustomTag>();
		[SerializeField, PropertyBackingField(
			typeof(KeywordCollectionClassAttribute), "Class", "Optional class name for custom <a> style"
		)]
		private List<KeywordCollectionClass> m_LinkKeywordCollections = new List<KeywordCollectionClass>();
		[System.NonSerialized]
		private List<Link> m_Links = new List<Link>();
		[SerializeField, PropertyBackingField]
		private string m_InputText = string.Empty;
		[SerializeField, PropertyBackingField]
		private Object m_InputTextSourceObject = null;
		[System.NonSerialized]
		private ITextSource m_InputTextSource;
		[SerializeField, PropertyBackingField]
		private bool m_IsDynamicFontDesired = true;
		[SerializeField, PropertyBackingField]
		private bool m_IsRichTextDesired = true;
		[SerializeField, HideInInspector] // serialize this so editor undo/redo bypasses lazy evaluation
		private string m_OutputText = string.Empty;
		[SerializeField, PropertyBackingField(
			typeof(KeywordCollectionClassAttribute), "Class", "Class name for custom <quad> style"
		)]
		private List<KeywordCollectionClass> m_QuadKeywordCollections = new List<KeywordCollectionClass>();
		[System.NonSerialized]
		private List<Quad> m_Quads = new List<Quad>();
		[SerializeField, PropertyBackingField]
		private int m_ReferenceFontSize = 14;
		private float m_ScaleFactor = 1f;
		[SerializeField, PropertyBackingField]
		private bool m_ShouldOverrideStylesFontSize = false;
		[SerializeField, PropertyBackingField]
		private HyperTextStyles m_Styles = null;
		[SerializeField, PropertyBackingField(
			typeof(KeywordCollectionClassAttribute), "Tag", "Tag name for the custom text style"
		)]
		private List<KeywordCollectionClass> m_TagKeywordCollections = new List<KeywordCollectionClass>();
		#endregion

		#region Event Handlers
		/// <summary>
		/// Raises the input source became dirty event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		private void OnInputSourceBecameDirty(ITextSource sender)
		{
			SetDirty();
		}
		/// <summary>
		/// Raises the keyword collection changed event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		private void OnKeywordCollectionChanged(KeywordCollection sender)
		{
			SetDirty();
		}

		/// <summary>
		/// Raises the styles changed event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		private void OnStylesChanged(HyperTextStyles sender)
		{
			SetDirty();
		}
		#endregion

		#region Inspector Properties
		private KeywordCollectionClass[] GetLinkKeywordCollections()
		{
			return m_LinkKeywordCollections.ToArray();
		}

		private KeywordCollectionClass[] GetQuadKeywordCollections()
		{
			return m_QuadKeywordCollections.ToArray();
		}

		private KeywordCollectionClass[] GetTagKeywordCollections()
		{
			return m_TagKeywordCollections.ToArray();
		}

		private void SetLinkKeywordCollections(KeywordCollectionClass[] value)
		{
			SetLinkKeywordCollections(value as IList<KeywordCollectionClass>);
		}

		private void SetQuadKeywordCollections(KeywordCollectionClass[] value)
		{
			SetQuadKeywordCollections(value as IList<KeywordCollectionClass>);
		}

		private void SetTagKeywordCollections(KeywordCollectionClass[] value)
		{
			SetTagKeywordCollections(value as IList<KeywordCollectionClass>);
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets a GUIStyle for the current property values.
		/// </summary>
		/// <value>A GUIStyle for the current property values.</value>
		public GUIStyle GUIStyle
		{
			get
			{
				GUIStyle result = new GUIStyle();
				result.fontSize = this.FontSizeToUse;
				result.fontStyle = m_Styles == null ? FontStyle.Normal : m_Styles.DefaultFontStyle;
				result.normal.textColor = m_Styles == null ? Color.white : m_Styles.DefaultTextColor;
				result.richText = this.IsRichTextDesired;
				return result;
			}
		}
		/// <summary>
		/// Gets or sets the input text.
		/// </summary>
		/// <value>The input text.</value>
		public string InputText
		{
			get { return m_InputText; }
			set
			{
				if (m_InputText != value)
				{
					m_InputText = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets the input text source. If a value is assigned, its OutputText will be used in place of the
		/// value in the InputText property of this <see cref="HyperTextProcessor"/>.
		/// </summary>
		/// <value>The input text source.</value>
		public ITextSource InputTextSource
		{
			get { return BackingFieldUtility.GetInterfaceBackingField(ref m_InputTextSource, m_InputTextSourceObject); }
			set
			{
				if (
					BackingFieldUtility.SetInterfaceBackingField(
						value,
						ref m_InputTextSource,
						ref m_InputTextSourceObject,
						onAssign: t => t.BecameDirty += OnInputSourceBecameDirty,
						onUnassign: t => t.BecameDirty -= OnInputSourceBecameDirty
					)
				)
				{
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether dynamic font output is desired on this instance.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this dynamic font output is desired on this instance; otherwise,
		/// <see langword="false"/>.
		/// </value>
		public bool IsDynamicFontDesired
		{
			get { return m_IsDynamicFontDesired; }
			set
			{
				if (m_IsDynamicFontDesired != value)
				{
					m_IsDynamicFontDesired = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets a value indicating whether dynamic font output is enabled.
		/// </summary>
		/// <value><see langword="true"/> if dynamic font output is enabled; otherwise, <see langword="false"/>.</value>
		public bool IsDynamicFontEnabled { get { return m_IsDynamicFontDesired && m_IsRichTextDesired; } }
		/// <summary>
		/// Gets or sets a value indicating whether rich text is desired on this instance.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if rich text is desired on this instance; otherwise, <see langword="false"/>.
		/// </value>
		public bool IsRichTextDesired
		{
			get { return m_IsRichTextDesired; }
			set
			{
				if (m_IsRichTextDesired != value)
				{
					m_IsRichTextDesired = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets a value indicating whether rich text is enabled on this instance.
		/// </summary>
		/// <value><see langword="true"/> if rich text is enabled; otherwise, <see langword="false"/>.</value>
		public bool IsRichTextEnabled { get { return m_IsRichTextDesired && m_Styles != null; } }
		/// <summary>
		/// Gets the output text.
		/// </summary>
		/// <value>The output text.</value>
		public string OutputText
		{
			get
			{
				ProcessInputText();
				return m_OutputText;
			}
		}
		/// <summary>
		/// Gets or sets the reference font size. It should correspond to the font size where OutputText will be sent.
		/// </summary>
		/// <value>The reference font size.</value>
		public int ReferenceFontSize
		{
			get { return m_ReferenceFontSize; }
			set
			{
				value = Mathf.Max(value, 0);
				if (m_ReferenceFontSize != value)
				{
					m_ReferenceFontSize = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets the scale factor.
		/// </summary>
		/// <value>The scale factor.</value>
		public float ScaleFactor
		{
			get { return m_ScaleFactor; }
			set
			{
				if (m_ScaleFactor != value)
				{
					m_ScaleFactor = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="HyperTextProcessor"/> should override the font size 
		/// specified in styles, if one is assigned.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if should override the font size specified in styles; otherwise, 
		/// <see langword="false"/>.
		/// </value>
		public bool ShouldOverrideStylesFontSize
		{
			get { return m_ShouldOverrideStylesFontSize; }
			set
			{
				if (m_ShouldOverrideStylesFontSize != value)
				{
					m_ShouldOverrideStylesFontSize = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets the styles.
		/// </summary>
		/// <value>The styles.</value>
		public HyperTextStyles Styles
		{
			get { return m_Styles; }
			set
			{
				if (m_Styles == value)
				{
					return;
				}
				if (m_Styles != null)
				{
					m_Styles.Changed -= OnStylesChanged;
				}
				m_Styles = value;
				if (m_Styles != null)
				{
					m_Styles.Changed += OnStylesChanged;
				}
				SetDirty();
			}
		}

		/// <summary>
		/// Gets the custom tags extracted from the text.
		/// </summary>
		/// <param name="tags">Tags.</param>
		public void GetCustomTags(List<CustomTag> tags)
		{
			ProcessInputText();
			tags.Clear();
			tags.AddRange(from customTag in m_CustomTags select (CustomTag)customTag.Clone());
		}

		/// <summary>
		/// Gets the link keyword collections.
		/// </summary>
		/// <param name="collections">Collections.</param>
		public void GetLinkKeywordCollections(List<KeywordCollectionClass> collections)
		{
			collections.Clear();
			collections.AddRange(m_LinkKeywordCollections);
		}

		/// <summary>
		/// Gets the links extracted from the text.
		/// </summary>
		/// <param name="links">Links.</param>
		public void GetLinks(List<Link> links)
		{
			ProcessInputText();
			links.Clear();
			links.AddRange(from link in m_Links select (Link)link.Clone());
		}

		/// <summary>
		/// Gets the quad keyword collections.
		/// </summary>
		/// <param name="collections">Collections.</param>
		public void GetQuadKeywordCollections(List<KeywordCollectionClass> collections)
		{
			collections.Clear();
			collections.AddRange(m_QuadKeywordCollections);
		}

		/// <summary>
		/// Gets the quads extracted from the text.
		/// </summary>
		/// <param name="quads">Quads.</param>
		public void GetQuads(List<Quad> quads)
		{
			ProcessInputText();
			quads.Clear();
			quads.AddRange(from quad in m_Quads select (Quad)quad.Clone());
		}

		/// <summary>
		/// Gets the tag keyword collections.
		/// </summary>
		/// <param name="collections">Collections.</param>
		public void GetTagKeywordCollections(List<KeywordCollectionClass> collections)
		{
			collections.Clear();
			collections.AddRange(m_TagKeywordCollections);
		}

		/// <summary>
		/// Initializes this instance. Call this method when the provider is enabled, or this instance is otherwise
		/// first initialized.
		/// </summary>
		public void OnEnable()
		{
			if (m_Styles != null)
			{
				m_Styles.Changed -= OnStylesChanged;
				m_Styles.Changed += OnStylesChanged;
			}
			InitializeKeywordCollectionCallbacks(m_LinkKeywordCollections);
			InitializeKeywordCollectionCallbacks(m_QuadKeywordCollections);
			InitializeKeywordCollectionCallbacks(m_TagKeywordCollections);
			if (this.InputTextSource != null)
			{
				m_InputTextSource.BecameDirty -= OnInputSourceBecameDirty;
				m_InputTextSource.BecameDirty += OnInputSourceBecameDirty;
			}
			SetDirty();
		}

		/// <summary>
		/// Sets the link keyword collections.
		/// </summary>
		/// <param name="value">Value.</param>
		public void SetLinkKeywordCollections(IList<KeywordCollectionClass> value)
		{
			SetKeywordCollectionBackingField(m_LinkKeywordCollections, value);
		}

		/// <summary>
		/// Sets the quad keyword collections.
		/// </summary>
		/// <param name="value">Value.</param>
		public void SetQuadKeywordCollections(IList<KeywordCollectionClass> value)
		{
			SetKeywordCollectionBackingField(m_QuadKeywordCollections, value);
		}

		/// <summary>
		/// Sets the tag keyword collections.
		/// </summary>
		/// <param name="value">Value.</param>
		public void SetTagKeywordCollections(IList<KeywordCollectionClass> value)
		{
			SetKeywordCollectionBackingField(m_TagKeywordCollections, value);
		}
		#endregion

		#region System.IDisposable
		/// <summary>
		/// Releases all resource used by the <see cref="HyperTextProcessor"/> object.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="HyperTextProcessor"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="HyperTextProcessor"/> in an unusable state. After calling 
		/// <see cref="Dispose"/>, you must release all references to the <see cref="HyperTextProcessor"/> so the
		/// garbage collector can reclaim the memory that the <see cref="HyperTextProcessor"/> was occupying.
		/// </remarks>
		public void Dispose()
		{
			this.BecameDirty = null;
		}
		#endregion

		/// <summary>
		/// Gets the default link style.
		/// </summary>
		/// <value>The default link style.</value>
		private HyperTextStyles.Link DefaultLinkStyle
		{
			get { return m_Styles == null ? HyperTextStyles.Link.DefaultStyle : m_Styles.DefaultLinkStyle; }
		}
		/// <summary>
		/// Gets the font size to use.
		/// </summary>
		/// <value>The font size to use.</value>
		private int FontSizeToUse
		{
			get
			{
				return m_ShouldOverrideStylesFontSize || m_Styles == null ?
					this.ReferenceFontSize : m_Styles.CascadedFontSize;
			}
		}
		/// <summary>
		/// Gets or sets the input text source object. This property only exists for the inspector.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <value>The input text source object.</value>
		private Object InputTextSourceObject
		{
			get { return m_InputTextSourceObject; }
			set
			{
				BackingFieldUtility.SetInterfaceBackingFieldObject<ITextSource>(
					value, ref m_InputTextSourceObject, o => this.InputTextSource = o
				);
			}
		}
		/// <summary>
		/// Gets the input text to use.
		/// </summary>
		/// <value>The input text to use.</value>
		private string InputTextToUse
		{
			get { return (this.InputTextSource != null ? m_InputTextSource.OutputText : m_InputText) ?? string.Empty; }
		}
		/// <summary>
		/// Gets the size of the font multiplied by the DPI.
		/// </summary>
		/// <value>The size of the font multiplied by the DPI.</value>
		private int ScaledFontSize { get { return (int)(this.FontSizeToUse * this.ScaleFactor); } }
		
		/// <summary>
		/// Gets a version of the quad tag corresponding to the supplied Match with all of its arguments injected.
		/// </summary>
		/// <returns>The postprocessed quad tag corresponding to the supplied Match.</returns>
		/// <param name="quadTagMatch">Quad tag match.</param>
		/// <param name="quadTemplates">The list of quad styles specified on the styles object.</param>
		private string GetPostprocessedQuadTag(Match quadTagMatch, List<HyperTextStyles.Quad> quadTemplates)
		{
			string quadName = quadTagMatch.Groups[HyperTextProcessor.ClassNameCaptureGroup].Value;
			string linkOpenTag = "";
			float sizeScalar = 1f;
			float width, height;
			Vector4 padding;
			float aspect = 1f;
			int templateIndex = quadTemplates.FindIndex(quad => quad.ClassName == quadName);
			if (templateIndex >= 0)
			{
				if (!string.IsNullOrEmpty(quadTemplates[templateIndex].LinkId))
				{
					linkOpenTag = string.Format(
						"<a name=\"{0}\"{1}>",
						quadTemplates[templateIndex].LinkId,
						string.IsNullOrEmpty(quadTemplates[templateIndex].LinkClassName) ?
							"" : string.Format(" class=\"{0}\"", quadTemplates[templateIndex].LinkClassName)
					);
				}
				if (quadTemplates[templateIndex].Sprite != null)
				{
					padding = UnityEngine.Sprites.DataUtility.GetPadding(quadTemplates[templateIndex].Sprite);
					Rect rect = quadTemplates[templateIndex].Sprite.rect;
					width = rect.width - padding.z - padding.x;
					height = rect.height - padding.w - padding.y;
					aspect = height == 0f ? 0f : width / height;
				}
				sizeScalar = quadTemplates[templateIndex].SizeScalar;
			}
			return string.Format(
				"{0}<size={1}><quad class=\"{2}\" width={3}></size>{4}",
				linkOpenTag,
				sizeScalar * this.ScaledFontSize,
				quadName,
				aspect,
				string.IsNullOrEmpty(linkOpenTag) ? "" : "</a>"
			);
		}

		/// <summary>
		/// Gets the replacement regex from the specified table of expressions.
		/// </summary>
		/// <returns>The replacement regex.</returns>
		/// <param name="keyword">Keyword.</param>
		/// <param name="matchMode">Match mode.</param>
		private Regex GetReplacementRegex(string keyword, CaseMatchMode matchMode)
		{
			int hash = ObjectX.GenerateHashCode(keyword.GetHashCode(), (int)matchMode);
			Regex regex;
			if (!s_KeywordRegexTable.ContainsKey(hash))
			{
				regex = new Regex(
					string.Format("(?<=^|\\W){0}(?=\\W|$)", Regex.Escape(keyword)),
					matchMode == CaseMatchMode.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None
				);
				s_KeywordRegexTable[hash] = regex;
			}
			else
			{
				regex = s_KeywordRegexTable[hash];
			}
			return regex;
		}

		/// <summary>
		/// Initializes the keyword collection callbacks for the specified backing field.
		/// </summary>
		/// <param name="backingField">Backing field.</param>
		private void InitializeKeywordCollectionCallbacks(List<KeywordCollectionClass> backingField)
		{
			for (int i = 0; i < backingField.Count; ++i)
			{
				if (backingField[i].Collection != null)
				{
					backingField[i].Collection.Changed -= OnKeywordCollectionChanged;
					backingField[i].Collection.Changed += OnKeywordCollectionChanged;
				}
			}
		}
		
		/// <summary>
		/// Inserts tags arround the supplied keyword into the text segment.
		/// </summary>
		/// <returns>The text segment with custom tags inserted.</returns>
		/// <param name="textSegment">Segment of text to modify.</param>
		/// <param name="keyword">Keyword.</param>
		/// <param name="tag">Tag.</param>
		/// <param name="matchMode">Match mode.</param>
		private string InsertCustomTagsIntoSegment(
			string textSegment, string keyword, string tag, CaseMatchMode matchMode
		)
		{
			if (string.IsNullOrEmpty(tag))
			{
				return textSegment;
			}
			Regex regex = GetReplacementRegex(keyword, matchMode);
			return regex.Replace(textSegment, string.Format("<{0}>{1}</{0}>", tag, regex.Match(textSegment).Value));
		}
		
		/// <summary>
		/// Inserts links for the supplied keyword into the text segment.
		/// </summary>
		/// <returns>The text segment with keyword links inserted.</returns>
		/// <param name="textSegment">Segment of text to modify.</param>
		/// <param name="keyword">Keyword.</param>
		/// <param name="className">Class name.</param>
		/// <param name="matchMode">Match mode.</param>
		private string InsertKeywordLinksIntoSegment(
			string textSegment, string keyword, string className, CaseMatchMode matchMode
		)
		{
			Regex regex = GetReplacementRegex(keyword, matchMode);
			return regex.Replace(
				textSegment,
				string.Format(
					"<a name=\"{0}\"{1}>{2}</a>",
					keyword,
					string.IsNullOrEmpty(className) || !this.IsRichTextEnabled ?
						"" : string.Format(" class=\"{0}\"", className),
					regex.Match(textSegment).Value
				)
			);
		}

		/// <summary>
		/// Inserts a quad tag for the supplied keyword into the text segment.
		/// </summary>
		/// <returns>The text segment with quad tags inserted.</returns>
		/// <param name="textSegment">Segment of text to modify.</param>
		/// <param name="keyword">Keyword.</param>
		/// <param name="className">Class name.</param>
		/// <param name="matchMode">Match mode.</param>
		private string InsertQuadTagIntoSegment(
			string textSegment, string keyword, string className, CaseMatchMode matchMode
		)
		{
			if (string.IsNullOrEmpty(className))
			{
				return textSegment;
			}
			Regex regex = GetReplacementRegex(keyword, matchMode);
			return regex.Replace(textSegment, string.Format("<quad class=\"{0}\">", className));
		}

		/// <summary>
		/// Processes the input text.
		/// </summary>
		private void ProcessInputText()
		{
			// early out if already up to date
			if (!m_IsDirty)
			{
				return;
			}
			using (ListPool<HyperTextStyles.Quad>.Scope cascadedQuadStyles = new ListPool<HyperTextStyles.Quad>.Scope())
			{
				using (
					DictPool<IndexRange, float>.Scope processedIndexRangesAndScalars =
					new DictPool<IndexRange, float>.Scope()
				)
				{
					string textCache;
					using (StringX.StringBuilderScope sb = new StringX.StringBuilderScope())
					{
						// initialize variables used throughout this method
						int indexInRawString = 0;
						using (
							DictPool<string, HyperTextStyles.Link>.Scope linkStyles =
							new DictPool<string, HyperTextStyles.Link>.Scope()
						)
						{
							using (
								DictPool<string, HyperTextStyles.Text>.Scope customTags =
								new DictPool<string, HyperTextStyles.Text>.Scope()
							)
							{
								if (m_Styles != null)
								{
									using (
										ListPool<HyperTextStyles.Text>.Scope styles =
										new ListPool<HyperTextStyles.Text>.Scope()
									)
									{
										m_Styles.GetCascadedCustomTextStyles(styles.List);
										for (int i = 0; i < styles.List.Count; ++i)
										{
											if (
												!string.IsNullOrEmpty(styles.List[i].Tag) &&
												!customTags.Dict.ContainsKey(styles.List[i].Tag)
											)
											{
												customTags.Dict.Add(styles.List[i].Tag, styles.List[i]);
											}
										}
									}
								}
								// insert tags in text for words present in keyword collections
								textCache = SubstituteTagsInForKeywords(this.InputTextToUse);
								// if rich text is enabled, substitute quad arguments, discrete sizes, and custom tag styles into text
								if (m_Styles != null)
								{
									m_Styles.GetCascadedQuadStyles(cascadedQuadStyles.List);
								}
								m_CustomTags.Clear();
								m_Quads.Clear();
								if (this.IsRichTextEnabled)
								{
									// sub quad arguments into text
									List<HyperTextStyles.Quad> quads = cascadedQuadStyles.List; // BUG: WinRT breaks if Scope property is accessed within closure
									textCache = s_QuadTagRegex.Replace(
										textCache, match => GetPostprocessedQuadTag(match, quads)
									);
									// substitute sizes in for percentages
									textCache = s_PreprocessedSizeTagRegex.Replace(
										textCache,
										match => string.Format(
											"<size={0}>{1}</size>",
											match.Groups[HyperTextProcessor.AttributeValueCaptureGroup].Value.EndsWith("%") ?
												(int)(
													float.Parse(
														match.Groups[HyperTextProcessor.AttributeValueCaptureGroup].Value.Substring(
															0, match.Groups[HyperTextProcessor.AttributeValueCaptureGroup].Value.Length - 1
														)
													) * this.ScaledFontSize * 0.01f
												) : (
													(int)float.Parse(
														match.Groups[HyperTextProcessor.AttributeValueCaptureGroup].Value
													) > 0 ?
														(int)float.Parse(
															match.Groups[HyperTextProcessor.AttributeValueCaptureGroup].Value
														) : this.ScaledFontSize
												),
												match.Groups[HyperTextProcessor.TextCaptureGroup].Value
										)
									);
									// substitute text styles in for custom tags
									foreach (HyperTextStyles.Text style in customTags.Dict.Values)
									{
										Regex tagRegex = GetTagRegex(style.Tag);
										while (tagRegex.IsMatch(textCache))
										{
											m_ReplacementCustomTextStyle = style;
											m_ReplacementProcessedIndexRangesAndScalars =
												processedIndexRangesAndScalars.Dict;
											// only replace first instance so indices are properly set for any subsequent matches
											textCache = tagRegex.Replace(textCache, ReplaceCustomTag, 1);
										}
									}
									m_ReplacementProcessedIndexRangesAndScalars = null;
									// collect link styles
									using (
										ListPool<HyperTextStyles.LinkSubclass>.Scope styles =
										new ListPool<HyperTextStyles.LinkSubclass>.Scope()
									)
									{
										this.Styles.GetCascadedLinkStyles(styles.List);
										for (int i = 0; i < styles.List.Count; ++i)
										{
											if (
												!string.IsNullOrEmpty(styles.List[i].ClassName) &&
												!linkStyles.Dict.ContainsKey(styles.List[i].ClassName)
											)
											{
												linkStyles.Dict.Add(styles.List[i].ClassName, styles.List[i].Style);
											}
										}
									}
								}
							}
							// remove <a> tags from processed text and record the link character indices
							string className;
							m_Links.Clear();
							while (s_PostprocessedLinkTagRegex.IsMatch(textCache))
							{
								className = s_PostprocessedLinkTagRegex.Match(
									textCache
								).Groups[HyperTextProcessor.ClassNameCaptureGroup].Value;
								m_ReplacementLinkStyle = linkStyles.Dict.ContainsKey(className) ?
									linkStyles.Dict[className] : this.DefaultLinkStyle;
								m_ReplacementProcessedIndexRangesAndScalars = processedIndexRangesAndScalars.Dict;
								// only replace first instance so indices are properly set for any subsequent matches
								textCache = s_PostprocessedLinkTagRegex.Replace(textCache, ReplaceLink, 1);
							}
							m_ReplacementProcessedIndexRangesAndScalars = null;
						}
						sb.StringBuilder.Append(
							textCache.Substring(indexInRawString, textCache.Length - indexInRawString)
						);
						m_OutputText = sb.StringBuilder.ToString();
					}
					// pull out data for quads and finalize sizes if rich text is enabled
					if (this.IsRichTextEnabled)
					{
						// multiply out overlapping sizes if dynamic font is enabled
						if (this.IsDynamicFontEnabled)
						{
							foreach (KeyValuePair<IndexRange, float> rangeScalar in processedIndexRangesAndScalars.Dict)
							{
								if (rangeScalar.Value <= 0f || rangeScalar.Value == 1f)
								{
									continue;
								}
								string segment =
									m_OutputText.Substring(rangeScalar.Key.StartIndex, rangeScalar.Key.Count);
								int oldLength = segment.Length;
								if (s_PostProcessedSizeAttributeRegex.IsMatch(segment))
								{
									using (StringX.StringBuilderScope sb = new StringX.StringBuilderScope())
									{
										sb.StringBuilder.Append(m_OutputText.Substring(0, rangeScalar.Key.StartIndex));
										segment = s_PostProcessedSizeAttributeRegex.Replace(
											segment,
											match => string.Format(
												"{0}{1}{2}",
												match.Groups[HyperTextProcessor.OpenTagCaptureGroup].Value,
												(int)(
													rangeScalar.Value * float.Parse(
														match.Groups[HyperTextProcessor.AttributeValueCaptureGroup].Value
													)
												),
												match.Groups[HyperTextProcessor.CloseTagCaptureGroup].Value
											)
										);
										sb.StringBuilder.Append(segment);
										sb.StringBuilder.Append(m_OutputText.Substring(rangeScalar.Key.EndIndex + 1));
										m_OutputText = sb.StringBuilder.ToString();
									}
									int delta = segment.Length - oldLength;
									if (delta != 0)
									{
										foreach (IndexRange range in processedIndexRangesAndScalars.Dict.Keys)
										{
											if (range != rangeScalar.Key)
											{
												range.Offset(rangeScalar.Key, delta);
											}
										}
										rangeScalar.Key.EndIndex += delta;
									}
								}
							}
						}
						// pull out quad data
						string quadName;
						bool isQuadGeomAtEndOfTag = UnityVersion.Current < new UnityVersion(5, 3, 0);
						foreach (Match match in s_QuadTagRegex.Matches(m_OutputText))
						{
							// add new quad data to list if its class is known
							quadName = match.Groups[HyperTextProcessor.ClassNameCaptureGroup].Value;
							int templateIndex = cascadedQuadStyles.List.FindIndex(quad => quad.ClassName == quadName);
							int quadGeomIndex = match.Index;
							if (isQuadGeomAtEndOfTag)
							{
								quadGeomIndex += match.Length - 1;
							}
							if (templateIndex >= 0)
							{
								m_Quads.Add(
									new Quad(
										new IndexRange(quadGeomIndex, quadGeomIndex),
										cascadedQuadStyles.List[templateIndex]
									)
								);
							}
						}
					}
				}
			}
			m_CustomTags.Sort(CompareTagsByStartIndex);
			m_Links.Sort(CompareTagsByStartIndex);
			m_Quads.Sort(CompareTagsByStartIndex);
			m_IsDirty = false;
		}

		/// <summary>
		/// A <see cref="MatchEvaluator"/> to replace custom tags with recognized rich text style tags and record their
		/// index ranges.
		/// </summary>
		/// <remarks>
		/// Temporally coupled with <see cref="HyperTextProcessor.m_ReplacementCustomTextStyle"/> and
		/// <see cref="HyperTextProcessor.m_ReplacementProcessedIndexRangesAndScalars"/>.</remarks>
		/// <returns>The string to insert in place of the <paramref name="match"/>.</returns>
		/// <param name="match">Match.</param>
		private string ReplaceCustomTag(Match match)
		{
			IndexRange characterIndices;
			string result;
			if (
				ReplaceSegment(
					match,
					this.IsDynamicFontEnabled ?
						m_ReplacementCustomTextStyle.TextStyle :
						m_ReplacementCustomTextStyle.TextStyle.NonDynamicVersion,
					m_ReplacementProcessedIndexRangesAndScalars,
					out characterIndices,
					out result
				)
			)
			{
				m_CustomTags.Add(new CustomTag(characterIndices, m_ReplacementCustomTextStyle));
			}
			return result;
		}

		/// <summary>
		/// A <see cref="MatchEvaluator"/> to replace links with recognized rich text style tags and record their index
		/// ranges.
		/// </summary>
		/// <remarks>
		/// Temporally coupled with <see cref="HyperTextProcessor.m_ReplacementLinkStyle"/> and
		/// <see cref="HyperTextProcessor.m_ReplacementProcessedIndexRangesAndScalars"/>.</remarks>
		/// <returns>The string to insert in place of the <paramref name="match"/>.</returns>
		/// <param name="match">Match.</param>
		private string ReplaceLink(Match match)
		{
			Group classNameGroup = match.Groups[HyperTextProcessor.ClassNameCaptureGroup];
			string className = classNameGroup.Value;
			IndexRange characterIndices;
			string result;
			if (
				ReplaceSegment(
					match,
					this.IsDynamicFontEnabled ?
						m_ReplacementLinkStyle.TextStyle : m_ReplacementLinkStyle.TextStyle.NonDynamicVersion,
					m_ReplacementProcessedIndexRangesAndScalars,
					out characterIndices,
					out result
				)
			)
			{
				m_Links.Add(
					new Link(
						match.Groups[HyperTextProcessor.AttributeValueCaptureGroup].Value,
						className,
						characterIndices,
						m_ReplacementLinkStyle
					)
				);
			}
			return result;
		}

		/// <summary>
		/// Gets replacement text for a <paramref name="match"/> and records index ranges if new geometry data should
		/// be created, as well as offsetting <paramref name="processedIndexRangesAndScalars"/> as needed.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if new geomety data should be created from the <paramref name="characterIndices"/>;
		/// otherwise, <see langword="false"/> .
		/// </returns>
		/// <param name="match">Match.</param>
		/// <param name="textStyle">Text style to swap in if necessary.</param>
		/// <param name="processedIndexRangesAndScalars">
		/// A table of index ranges and scalars for geometry data that has already been processed.
		/// </param>
		/// <param name="characterIndices">
		/// Character indices that should be used to create new geometry data if needed; otherwise,
		/// <see langword="null"/>.
		/// </param>
		/// <param name="result">String that should be returned by the calling <see cref="MatchEvaluator"/>.</param>
		private bool ReplaceSegment(
			Match match,
			RichTextStyle textStyle,
			Dictionary<IndexRange, float> processedIndexRangesAndScalars,
			out IndexRange characterIndices,
			out string result
		)
		{
			Group textCaptureGroup = match.Groups[HyperTextProcessor.TextCaptureGroup];
			string openTag = this.IsRichTextEnabled ? textStyle.ToStartTag(this.ScaledFontSize) : "";
			string closeTag = this.IsRichTextEnabled ? textStyle.ToEndTag() : "";
			string segment = textCaptureGroup.Value;
			using (
				ListPool<KeyValuePair<IndexRange, int>>.Scope offsets =
				new ListPool<KeyValuePair<IndexRange, int>>.Scope()
			)
			{
				Group closeTagGroup = match.Groups[HyperTextProcessor.CloseTagCaptureGroup];
				int openTagDelta = openTag.Length - (textCaptureGroup.Index - match.Index);
				// add close tag first in case it shifts range backward
				offsets.List.Add(
					new KeyValuePair<IndexRange, int>(
						new IndexRange(
							closeTagGroup.Index,
							// finish one before end of match so end indices of enclosing tags aren't affected
							closeTagGroup.Index + closeTagGroup.Length - 2
						), closeTag.Length - closeTagGroup.Length
					)
				);
				offsets.List.Add(
					new KeyValuePair<IndexRange, int>(
						// start one after match so start indices of enclosing tags aren't affected
						new IndexRange(match.Index + 1, textCaptureGroup.Index - 1),
						openTagDelta
					)
				);
				foreach (IndexRange range in processedIndexRangesAndScalars.Keys)
				{
					foreach (KeyValuePair<IndexRange, int> offset in offsets.List)
					{
						range.Offset(offset.Key, offset.Value);
					}
				}
			}
			if (segment.Length > 0)
			{
				characterIndices = new IndexRange(
					match.Index + openTag.Length,
					match.Index + openTag.Length + textCaptureGroup.Length - 1
				);
				processedIndexRangesAndScalars.Add(characterIndices, textStyle.SizeScalar);
			}
			else
			{
				characterIndices = null;
			}
			result = string.Format("{0}{1}{2}", openTag, segment, closeTag);
			return segment.Length > 0;
		}

		/// <summary>
		/// Sets this instance dirty in order to force a became dirty callback.
		/// </summary>
		private void SetDirty()
		{
			m_IsDirty = true;
			if (this.BecameDirty != null)
			{
				this.BecameDirty(this);
			}
		}

		/// <summary>
		/// Sets the keyword collection backing field.
		/// </summary>
		/// <param name="backingField">Backing field.</param>
		/// <param name="value">Value.</param>
		private void SetKeywordCollectionBackingField(
			List<KeywordCollectionClass> backingField, IList<KeywordCollectionClass> value
		)
		{
			for (int i = 0; i < backingField.Count; ++i)
			{
				if (backingField[i].Collection != null)
				{
					backingField[i].Collection.Changed -= OnKeywordCollectionChanged;
				}
			}
			backingField.Clear();
			backingField.AddRange(value);
			for (int i = 0; i < backingField.Count; ++i)
			{
				if (backingField[i].Collection != null)
				{
					backingField[i].Collection.Changed += OnKeywordCollectionChanged;
				}
			}
			SetDirty();
		}

		/// <summary>
		/// Substitutes the tags in for detected keywords.
		/// </summary>
		/// <returns>Input text with tags patched in around keywords as needed.</returns>
		/// <param name="input">Input.</param>
		private string SubstituteTagsInForKeywords(string input)
		{
			// gather up all keyword collections and their corresponding substitution methods
			Dictionary<KeywordCollection, System.Func<string, string, string>> keywordCollections =
				new Dictionary<KeywordCollection, System.Func<string, string, string>>();
			List<KeywordCollectionClass> allCollectionClasses =
				new List<KeywordCollectionClass>(m_LinkKeywordCollections);
			List<System.Func<string, string, string, CaseMatchMode, string>> collectionSubstitutionMethods =
				new List<System.Func<string, string, string, CaseMatchMode, string>>();
			for (int index = 0; index < m_LinkKeywordCollections.Count; ++index)
			{
				collectionSubstitutionMethods.Add(InsertKeywordLinksIntoSegment);
			}
			if (this.IsRichTextEnabled)
			{
				allCollectionClasses.AddRange(m_QuadKeywordCollections);
				for (int index = 0; index < m_QuadKeywordCollections.Count; ++index)
				{
					collectionSubstitutionMethods.Add(InsertQuadTagIntoSegment);
				}
				allCollectionClasses.AddRange(m_TagKeywordCollections);
				for (int index = 0; index < m_TagKeywordCollections.Count; ++index)
				{
					collectionSubstitutionMethods.Add(InsertCustomTagsIntoSegment);
				}
			}
			for (int index = 0; index < allCollectionClasses.Count; ++index)
			{
				if (allCollectionClasses[index].Collection == null)
				{
					continue;
				}
				if (keywordCollections.ContainsKey(allCollectionClasses[index].Collection))
				{
#if UNITY_EDITOR
					if (Application.isPlaying)
#endif
					{
						Debug.LogError(
							string.Format(
								"Keyword collection {0} used for multiple different styles.",
								allCollectionClasses[index].Collection.name
							)
						);
					}
				}
				else
				{
					System.Func<string, string, string, CaseMatchMode, string> substitutionMethod =
						collectionSubstitutionMethods[index];
					string identifierName = allCollectionClasses[index].ClassName;
					CaseMatchMode caseMatchMode = allCollectionClasses[index].Collection.CaseMatchMode;
					keywordCollections.Add(
						allCollectionClasses[index].Collection,
						(segment, keyword) => substitutionMethod(segment, keyword, identifierName, caseMatchMode)
					);
				}
			}
			// get a regular expression to match tags
			string customTagsMatchPattern = "|".Join(
				from style in m_TagKeywordCollections
				select string.IsNullOrEmpty(style.ClassName) ?
					"" : string.Format("</?{0}\b.*?>", Regex.Escape(style.ClassName))
			);
			Regex tagMatcher = this.IsRichTextEnabled ?
				new Regex(
					string.Format(
						"{0}{1}",
						s_PreprocessedAnyTagMatchPattern,
						string.IsNullOrEmpty(customTagsMatchPattern) ?
							"" : string.Format("|{0}", customTagsMatchPattern)
					),
					RegexOptions.Singleline | RegexOptions.IgnoreCase
				) : s_PreprocessedLinkTagRegex;
			// sub in tags for each keyword
			using (HashPool<string>.Scope processedKeywords = new HashPool<string>.Scope())
			{
				using (StringX.StringBuilderScope sb = new StringX.StringBuilderScope())
				{
					foreach (
						KeyValuePair<KeywordCollection, System.Func<string, string, string>> kv in keywordCollections
					)
					{
						foreach (string keyword in kv.Key.Keywords)
						{
							if (processedKeywords.HashSet.Contains(keyword) || string.IsNullOrEmpty(keyword))
							{
								continue;
							}
							processedKeywords.HashSet.Add(keyword);
							int start;
							MatchCollection tagMatches = tagMatcher.Matches(input);
							// preserve all text inside of tags
							if (tagMatches.Count > 0)
							{
								sb.StringBuilder.Remove(0, sb.StringBuilder.Length);
								for (int matchIndex = 0; matchIndex < tagMatches.Count; ++matchIndex)
								{
									start = matchIndex == 0 ?
										0 : tagMatches[matchIndex - 1].Index + tagMatches[matchIndex - 1].Length;
									// append text preceding tag
									string segment = input.Substring(start, tagMatches[matchIndex].Index - start);
									// append patched text
									sb.StringBuilder.Append(kv.Value(segment, keyword));
									// append tag
									sb.StringBuilder.Append(tagMatches[matchIndex].Value);
									// append segment following final tag
									if (matchIndex == tagMatches.Count - 1)
									{
										segment = input.Substring(
											tagMatches[matchIndex].Index + tagMatches[matchIndex].Length
										);
										sb.StringBuilder.Append(kv.Value(segment, keyword));
									}
								}
								input = sb.StringBuilder.ToString();
							}
							// perform simple substitution if there are no tags present
							else
							{
								input = kv.Value(input, keyword);
							}
						}
					}
				}
			}
			return input;
		}

		#region Obsolete
		[System.Obsolete("Use HyperTextProcessor.GetCustomTags(List<CustomTag>", true)]
		public void GetCustomTags(ref List<CustomTag> tags) {}
		[System.Obsolete("Use HyperTextProcessor.GetLinkKeywordCollections(List<KeywordCollectionClass>", true)]
		public void GetLinkKeywordCollections(ref List<KeywordCollectionClass> collections) {}
		[System.Obsolete("Use HyperTextProcessor.GetLinks(List<Link>", true)]
		public void GetLinks(ref List<Link> links) {}
		[System.Obsolete("Use HyperTextProcessor.GetQuadKeywordCollections(List<KeywordCollectionClass>", true)]
		public void GetQuadKeywordCollections(ref List<KeywordCollectionClass> collections) {}
		[System.Obsolete("Use HyperTextProcessor.GetQuads(List<Quad>", true)]
		public void GetQuads(ref List<Quad> quads) {}
		[System.Obsolete("Use HyperTextProcessor.GetTagKeywordCollections(List<KeywordCollectionClass>", true)]
		public void GetTagKeywordCollections(ref List<KeywordCollectionClass> collections) {}
		[System.Obsolete("Use HyperTextProcessor.SetLinkKeywordCollections(IList<KeywordCollectionClass>", true)]
		public void SetLinkKeywordCollections(IEnumerable<KeywordCollectionClass> value) {}
		[System.Obsolete("Use HyperTextProcessor.SetQuadKeywordCollections(IList<KeywordCollectionClass>", true)]
		public void SetQuadKeywordCollections(IEnumerable<KeywordCollectionClass> value) {}
		[System.Obsolete("Use HyperTextProcessor.SetTagKeywordCollections(IList<KeywordCollectionClass>", true)]
		public void SetTagKeywordCollections(IEnumerable<KeywordCollectionClass> value) {}
		#endregion
	}
}