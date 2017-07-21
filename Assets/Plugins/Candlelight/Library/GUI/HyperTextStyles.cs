// 
// HyperTextStyles.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Candlelight.UI
{
	/// <summary>
	/// A class for describing different styles for a <see cref="HyperText"/> component.
	/// </summary>
	public class HyperTextStyles : ScriptableObject
	{
		#region Delegates
		/// <summary>
		/// A delegate for listening for changes to a <see cref="HyperTextStyles"/> object.
		/// </summary>
		public delegate void ChangedEventHandler(HyperTextStyles sender);
		#endregion

		#region Data Types
		/// <summary>
		/// A structure for storing information about a link style.
		/// </summary>
		[System.Serializable]
		public struct Link : IPropertyBackingFieldCompatible<Link>
		{
			#region Backing Fields
			private const string k_ColorTintModeExplanation = "Base color is <color> tag value if enabled, font " +
				"color if not. Each state color is multiplied by the color multiplier. Color tint mode describes how " +
				"the post-multiplied state color should blend with the base color.";
			#endregion
			/// <summary>
			/// Gets the color tint mode explanation.
			/// </summary>
			/// <value>The color tint mode explanation.</value>
			public static string ColorTintModeExplanation { get { return k_ColorTintModeExplanation; } }

			/// <summary>
			/// Gets the default style.
			/// </summary>
			/// <value>The default style.</value>
			public static Link DefaultStyle
			{
				get
				{
					UnityEngine.UI.ColorBlock colors = new UnityEngine.UI.ColorBlock();
					colors.normalColor = new Color(0.1f, 0.5f, 1f);
					colors.highlightedColor = new Color(0.13f, 0.76f, 1f);
					colors.pressedColor = new Color(0.08f, 0.32f, 0.63f);
					colors.disabledColor = new Color(0.45f, 0.54f, 0.64f);
					colors.fadeDuration = 0.1f;
					colors.colorMultiplier = 1;
					return new Link(
						colors,
						ColorTintMode.Constant,
						ColorTween.Mode.All,
						new RichTextStyle(1f, FontStyle.Normal),
						0f
					);
				}
			}

			#region Backing Fields
			[SerializeField]
			private float m_VerticalOffset;
			[SerializeField]
			private RichTextStyle m_TextStyle;
			[SerializeField]
			private UnityEngine.UI.ColorBlock m_Colors;
			[SerializeField, Tooltip(k_ColorTintModeExplanation)]
			private ColorTintMode m_ColorTintMode;
			[SerializeField]
			private ColorTween.Mode m_ColorTweenMode;
			#endregion

			/// <summary>
			/// Gets the colors.
			/// </summary>
			/// <value>The colors.</value>
			public UnityEngine.UI.ColorBlock Colors { get { return m_Colors; } }
			/// <summary>
			/// Gets the color tint mode.
			/// </summary>
			/// <value>The color tint mode.</value>
			public ColorTintMode ColorTintMode { get { return m_ColorTintMode; } }
			/// <summary>
			/// Gets the color tween mode.
			/// </summary>
			/// <value>The color tween mode.</value>
			public ColorTween.Mode ColorTweenMode { get { return m_ColorTweenMode; } }
			/// <summary>
			/// Gets the text style.
			/// </summary>
			/// <value>The text style.</value>
			public RichTextStyle TextStyle { get { return m_TextStyle; } }
			/// <summary>
			/// Gets the vertical offset.
			/// </summary>
			/// <value>The vertical offset.</value>
			public float VerticalOffset { get { return m_VerticalOffset; } }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextStyles.Link"/> struct.
			/// </summary>
			/// <param name="colors">Colors.</param>
			/// <param name="colorTintMode">Color tint mode.</param>
			/// <param name="colorTweenMode">Color tween mode.</param>
			/// <param name="textStyle">Text style.</param>
			/// <param name="verticalOffset">Vertical Offset.</param>
			public Link(
				UnityEngine.UI.ColorBlock colors,
				ColorTintMode colorTintMode,
				ColorTween.Mode colorTweenMode,
				RichTextStyle textStyle,
				float verticalOffset
			) : this()
			{
				m_Colors = colors;
				m_ColorTintMode = colorTintMode;
				m_ColorTweenMode = colorTweenMode;
				m_TextStyle = textStyle;
				m_VerticalOffset = verticalOffset;
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
			/// <see cref="HyperTextStyles.Link"/>.
			/// </summary>
			/// <param name="obj">
			/// The <see cref="System.Object"/> to compare with the current <see cref="HyperTextStyles.Link"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="HyperTextStyles.Link"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public override bool Equals(object obj)
			{
				return ObjectX.Equals(ref this, obj);
			}

			/// <summary>
			/// Determines whether the specified <see cref="HyperTextStyles.Link"/> is equal to the
			/// current <see cref="HyperTextStyles.Link"/>.
			/// </summary>
			/// <param name="other">
			/// The <see cref="HyperTextStyles.Link"/> to compare with the current <see cref="HyperTextStyles.Link"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="HyperTextStyles.Link"/> is equal to
			/// the current <see cref="HyperTextStyles.Link"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public bool Equals(Link other)
			{
				return GetHashCode() == other.GetHashCode();
			}

			/// <summary>
			/// Serves as a hash function for a <see cref="HyperTextStyles.Link"/> object.
			/// </summary>
			/// <returns>
			/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as
			/// a hash table.
			/// </returns>
			public override int GetHashCode()
			{
				return ObjectX.GenerateHashCode(
					m_Colors.GetHashCode(),
					m_ColorTintMode.GetHashCode(),
					m_ColorTweenMode.GetHashCode(),
					m_TextStyle.GetHashCode(),
					m_VerticalOffset.GetHashCode()
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
		/// A structure for storing a style for a subclass of link.
		/// </summary>
		[System.Serializable]
		public struct LinkSubclass : IIdentifiable<string>, IPropertyBackingFieldCompatible<LinkSubclass>
		{
			#region Backing Fields
			[SerializeField, StatusProperty(ValidationStatus.Error, "Class name must be specified.", 0, "", 0, null)]
			private string m_ClassName;
			[SerializeField, FlushChildrenAttribute]
			private Link m_Style;
			#endregion

			/// <summary>
			/// Gets the name of the class.
			/// </summary>
			/// <value>The name of the class.</value>
			public string ClassName { get { return m_ClassName ?? string.Empty; } }
			/// <summary>
			/// Gets the identifier.
			/// </summary>
			/// <value>The identifier.</value>
			public string Identifier { get { return this.ClassName; } }
			/// <summary>
			/// Gets the style.
			/// </summary>
			/// <value>The style.</value>
			public Link Style { get { return m_Style; } }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextStyles.LinkSubclass"/> struct.
			/// </summary>
			/// <param name="className">Class name.</param>
			/// <param name="style">Style.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if className is null.</exception>
			/// <exception cref="System.ArgumentException">Thrown if className is empty.</exception>
			public LinkSubclass(string className, Link style) : this()
			{
				if (className == null)
				{
					throw new System.ArgumentNullException("className", "Class name cannot be null or empty.");
				}
				else if (
#if UNITY_EDITOR
					Application.isPlaying &&
#endif
					className.Length == 0
				)
				{
					throw new System.ArgumentException("Class name cannot be null or empty", "className");
				}
				m_ClassName = className;
				m_Style = style;
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
			/// <see cref="HyperTextStyles.LinkSubclass"/>.
			/// </summary>
			/// <param name="obj">
			/// The <see cref="System.Object"/> to compare with the current <see cref="HyperTextStyles.LinkSubclass"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="HyperTextStyles.LinkSubclass"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public override bool Equals(object obj)
			{
				return ObjectX.Equals(ref this, obj);
			}

			/// <summary>
			/// Determines whether the specified <see cref="HyperTextStyles.LinkSubclass"/> is equal to the
			/// current <see cref="HyperTextStyles.LinkSubclass"/>.
			/// </summary>
			/// <param name="other">The <see cref="HyperTextStyles.LinkSubclass"/> to compare with the current <see cref="HyperTextStyles.LinkSubclass"/>.</param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="HyperTextStyles.LinkSubclass"/> is equal to the
			/// current <see cref="HyperTextStyles.LinkSubclass"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public bool Equals(LinkSubclass other)
			{
				return GetHashCode() == other.GetHashCode();
			}

			/// <summary>
			/// Serves as a hash function for a
			/// <see cref="HyperTextStyles.LinkSubclass"/> object.
			/// </summary>
			/// <returns>
			/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as
			/// a hash table.
			/// </returns>
			public override int GetHashCode()
			{
				return ObjectX.GenerateHashCode(this.ClassName.GetHashCode(), m_Style.GetHashCode());
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
		/// A structure for storing information about a quad style.
		/// </summary>
		[System.Serializable]
		public struct Quad : IIdentifiable<string>, IPropertyBackingFieldCompatible<Quad>
		{
			#region Backing Fields
			[SerializeField]
			private string m_LinkClassName;
			[SerializeField]
			private string m_LinkId;
			[SerializeField, StatusProperty(ValidationStatus.Error, "Class name must be specified.", 0, "", 0, null)]
			private string m_ClassName;
			[SerializeField]
			private bool m_ShouldRespectColorization;
			[SerializeField]
			private float m_SizeScalar;
			[SerializeField, TextureThumbnail]
			private Sprite m_Sprite;
			[SerializeField]
			private float m_VerticalOffset;
			#endregion

			/// <summary>
			/// Gets the name of the class.
			/// </summary>
			/// <value>The name of the class.</value>
			public string ClassName { get { return m_ClassName = m_ClassName ?? string.Empty; } }
			/// <summary>
			/// Gets the identifier.
			/// </summary>
			/// <value>The identifier.</value>
			public string Identifier { get { return this.ClassName; } }
			/// <summary>
			/// Gets the name of the link class.
			/// </summary>
			/// <value>The name of the link class.</value>
			public string LinkClassName { get { return m_LinkClassName = m_LinkClassName ?? string.Empty; } }
			/// <summary>
			/// Gets the link identifier.
			/// </summary>
			/// <value>The link identifier.</value>
			public string LinkId { get { return m_LinkId = m_LinkId ?? string.Empty; } }
			/// <summary>
			/// Gets a value indicating whether this <see cref="HyperTextStyles.Quad"/> should
			/// respect colorization.
			/// </summary>
			/// <value>
			/// <see langword="true"/> if glyph colorization should be applied to the quad's vertices; otherwise, 
			/// <see langword="false"/>.
			/// </value>
			public bool ShouldRespectColorization { get { return m_ShouldRespectColorization; } }
			/// <summary>
			/// Gets the size scalar.
			/// </summary>
			/// <value>The size scalar.</value>
			public float SizeScalar { get { return m_SizeScalar; } }
			/// <summary>
			/// Gets the sprite.
			/// </summary>
			/// <value>The sprite.</value>
			public Sprite Sprite { get { return m_Sprite; } }
			/// <summary>
			/// Gets the vertical offset as a percentage of surrounding line height.
			/// </summary>
			/// <value>The vertical offset as a percentage of surrounding line height.</value>
			public float VerticalOffset { get { return m_VerticalOffset; } }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextStyles.Quad"/> struct.
			/// </summary>
			/// <param name="sprite">Sprite.</param>
			/// <param name="className">Class name.</param>
			/// <param name="sizeScalar">Size scalar.</param>
			/// <param name="verticalOffset">Vertical offset.</param>
			/// <param name="shouldRespectColorization">
			/// If set to <see langword="true"/> glyph colorization will be applied to the quad's vertices.
			/// </param>
			/// <param name="linkId">If not null or empty, a link ID to use with each instance of this quad.</param>
			/// <param name="linkClassName">If not null or empty, the class name of a custom link style to use.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if className is null.</exception>
			/// <exception cref="System.ArgumentException">Thrown if className is empty.</exception>
			public Quad(
				Sprite sprite,
				string className,
				float sizeScalar,
				float verticalOffset,
				bool shouldRespectColorization,
				string linkId,
				string linkClassName
			) : this()
			{
				m_LinkId = linkId;
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
				m_LinkClassName = linkClassName ?? string.Empty;
				m_ShouldRespectColorization = shouldRespectColorization;
				m_SizeScalar = sizeScalar;
				m_Sprite = sprite;
				m_VerticalOffset = verticalOffset;
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
			/// <see cref="HyperTextStyles.Quad"/>.
			/// </summary>
			/// <param name="obj">
			/// The <see cref="System.Object"/> to compare with the current <see cref="HyperTextStyles.Quad"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="HyperTextStyles.Quad"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public override bool Equals(object obj)
			{
				return ObjectX.Equals(ref this, obj);
			}

			/// <summary>
			/// Determines whether the specified <see cref="HyperTextStyles.Quad"/> is equal to the
			/// current <see cref="HyperTextStyles.Quad"/>.
			/// </summary>
			/// <param name="other">
			/// The <see cref="HyperTextStyles.Quad"/> to compare with the current <see cref="HyperTextStyles.Quad"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="HyperTextStyles.Quad"/> is equal to the current
			/// <see cref="HyperTextStyles.Quad"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public bool Equals(Quad other)
			{
				return GetHashCode() == other.GetHashCode();
			}

			/// <summary>
			/// Serves as a hash function for a <see cref="HyperTextStyles.Quad"/> object.
			/// </summary>
			/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
			public override int GetHashCode()
			{
				return ObjectX.GenerateHashCode(
					this.ClassName.GetHashCode(),
					this.LinkClassName.GetHashCode(),
					this.LinkId.GetHashCode(),
					m_ShouldRespectColorization.GetHashCode(),
					m_SizeScalar.GetHashCode(),
					m_Sprite == null ? s_NullSpriteHash : m_Sprite.GetHashCode(),
					m_VerticalOffset.GetHashCode()
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

			/// <summary>
			/// Returns a <see cref="System.String"/> that represents the current
			/// <see cref="HyperTextStyles.Quad"/>.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String"/> that represents the current
			/// <see cref="HyperTextStyles.Quad"/>.
			/// </returns>
			public override string ToString()
			{
				return string.Format(
					"[Quad: Sprite={5}, ClassName={0}, SizeScalar={4}, VerticalOffset={6}, ShouldRespectColorization={3}, LinkId={2}, LinkClassName={1}]",
					this.ClassName,
					this.LinkClassName,
					this.LinkId,
					m_ShouldRespectColorization,
					m_SizeScalar,
					m_Sprite,
					m_VerticalOffset
				);
			}
		}

		/// <summary>
		/// A structure for storing information about a text style for a tag.
		/// </summary>
		[System.Serializable]
		public struct Text : IIdentifiable<string>, IPropertyBackingFieldCompatible<Text>
		{
			/// <summary>
			/// An allocation for the validation status message.
			/// </summary>
			private static string s_ValidationStatusMessage = null;
			/// <summary>
			/// The valid tag match pattern.
			/// </summary>
			private static readonly string s_ValidTagMatchPattern = @"^\w+$";
			/// <summary>
			/// The valid tag regular expression.
			/// </summary>
			private static readonly System.Text.RegularExpressions.Regex s_ValidTagRegex =
				new System.Text.RegularExpressions.Regex(s_ValidTagMatchPattern);

			/// <summary>
			/// Validates the supplied tag.
			/// </summary>
			/// <remarks>
			/// This version only exists for the status tooltip.
			/// </remarks>
			/// <returns>The tag.</returns>
			/// <param name="provider">The <see cref="HyperTextStyles"/> on which the tag is defined.</param>
			/// <param name="tag">Tag.</param>
			/// <param name="statusMessage">Status message.</param>
			private static ValidationStatus ValidateTag(object provider, object tag, out string statusMessage)
			{
				if (tag is string)
				{
					return ValidateTag((string)tag, out statusMessage);
				}
				else
				{
					throw new System.ArgumentException("Tag must be a string.", "tag");
				}
			}

			/// <summary>
			/// Validates the supplied tag.
			/// </summary>
			/// <returns>The tag.</returns>
			/// <param name="tag">Tag.</param>
			/// <param name="statusMessage">Status message.</param>
			public static ValidationStatus ValidateTag(string tag, out string statusMessage)
			{
				if (string.IsNullOrEmpty(tag))
				{
					statusMessage = "Tag cannot be null or empty.";
					return ValidationStatus.Error;
				}
				if (HyperTextProcessor.ReservedTags.Contains(tag))
				{
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					foreach (string t in HyperTextProcessor.ReservedTags)
					{
						sb.AppendFormat(", {0}", t);
					}
					statusMessage =
						string.Format("Supplied tag is in reserved tag list:{0}", sb.ToString().Substring(1));
					return ValidationStatus.Error;
				}
				if (!s_ValidTagRegex.IsMatch(tag))
				{
					statusMessage = string.Format(
						"Supplied tag contains invalid characters. Must match pattern: {0}", s_ValidTagMatchPattern
					);
					return ValidationStatus.Error;
				}
				statusMessage = string.Empty;
				return ValidationStatus.Okay;
			}
			
			#region Backing Fields
			[SerializeField, StatusProperty(typeof(Text), "ValidateTag")]
			private string m_Tag;
			[SerializeField, FlushChildren]
			private RichTextStyle m_TextStyle;
			[SerializeField]
			private float m_VerticalOffset;
			#endregion

			/// <summary>
			/// Gets the identifier.
			/// </summary>
			/// <value>The identifier.</value>
			public string Identifier { get { return this.Tag; } }
			/// <summary>
			/// Gets the tag used to indicate this style.
			/// </summary>
			/// <value>The tag.</value>
			public string Tag { get { return m_Tag = m_Tag ?? string.Empty; } }
			/// <summary>
			/// Gets the text style.
			/// </summary>
			/// <value>The text style.</value>
			public RichTextStyle TextStyle { get { return m_TextStyle; } }
			/// <summary>
			/// Gets the vertical offset as a percentage of surrounding line height.
			/// </summary>
			/// <value>The vertical offset as a percentage of surrounding line height.</value>
			public float VerticalOffset { get { return m_VerticalOffset; } }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextStyles.Text"/> struct.
			/// </summary>
			/// <param name="tag">Tag.</param>
			/// <param name="textStyle">Text style.</param>
			/// <param name="verticalOffset">Vertical offset.</param>
			/// <exception cref="System.ArgumentException">
			/// Thrown if tag is empty, in reserved tag list, or contains invalid characters.
			/// </exception>
			/// <exception cref="System.ArgumentNullException">Thrown if tag is null.</exception>
			public Text(string tag, RichTextStyle textStyle, float verticalOffset): this()
			{
#pragma warning disable 219
				ValidationStatus validationStatus = ValidateTag(tag, out s_ValidationStatusMessage);
#pragma warning restore 219
				if (tag == null)
				{
					throw new System.ArgumentNullException("tag", s_ValidationStatusMessage);
				}
#if !UNITY_EDITOR
				if (validationStatus == ValidationStatus.Error)
				{
					throw new System.ArgumentException(s_ValidationStatusMessage, "tag");
				}
#endif
				m_Tag = tag;
				m_TextStyle = textStyle;
				m_VerticalOffset = verticalOffset;
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
			/// <see cref="HyperTextStyles.Text"/>.
			/// </summary>
			/// <param name="obj">
			/// The <see cref="System.Object"/> to compare with the current <see cref="HyperTextStyles.Text"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="HyperTextStyles.Text"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public override bool Equals(object obj)
			{
				return ObjectX.Equals(ref this, obj);
			}

			/// <summary>
			/// Determines whether the specified <see cref="HyperTextStyles.Text"/> is equal to the
			/// current <see cref="HyperTextStyles.Text"/>.
			/// </summary>
			/// <param name="other">
			/// The <see cref="HyperTextStyles.Text"/> to compare with the current
			/// <see cref="HyperTextStyles.Text"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="HyperTextStyles.Text"/> is equal to
			/// the current <see cref="HyperTextStyles.Text"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public bool Equals(Text other)
			{
				return GetHashCode() == other.GetHashCode();
			}

			/// <summary>
			/// Serves as a hash function for a <see cref="HyperTextStyles.Text"/> object.
			/// </summary>
			/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
			public override int GetHashCode()
			{
				return ObjectX.GenerateHashCode(
					this.Tag.GetHashCode(), m_TextStyle.GetHashCode(), m_VerticalOffset.GetHashCode()
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
			
			/// <summary>
			/// Gets the vertical offset.
			/// </summary>
			/// <returns>The vertical offset.</returns>
			/// <param name="fontSize">Font size of surrounding text.</param>
			public float GetVerticalOffset(int fontSize)
			{
				return m_VerticalOffset * fontSize;
			}
		}
		#endregion

		/// <summary>
		/// Hash code to use for null sprites.
		/// </summary>
		private static readonly int s_NullSpriteHash = typeof(Sprite).GetHashCode();

		/// <summary>
		/// Occurs when a property on this instance changes.
		/// </summary>
		public event ChangedEventHandler Changed = null;

		/// <summary>
		/// A flag indicating whether or not lists of cascaded styles have been initialized.
		/// </summary>
		[System.NonSerialized]
		private bool m_AreCascadesInitialized = false;

		#region Backing Fields
		private readonly List<LinkSubclass> m_CascadedLinkStyles = new List<LinkSubclass>(64);
		private readonly List<Quad> m_CascadedQuadStyles = new List<Quad>(64);
		private readonly List<Text> m_CascadedTextStyles = new List<Text>(64);
		private readonly Dictionary<LinkSubclass, HyperTextStyles> m_InheritedLinkStyles =
			new Dictionary<LinkSubclass, HyperTextStyles>();
		private readonly Dictionary<Quad, HyperTextStyles> m_InheritedQuadStyles =
			new Dictionary<Quad, HyperTextStyles>();
		private readonly Dictionary<Text, HyperTextStyles> m_InheritedTextStyles =
			new Dictionary<Text, HyperTextStyles>();
		[SerializeField, PropertyBackingField]
		private List<Text> m_CustomTextStyles = new List<Text>(
			new []
			{
				new Text("sub", new RichTextStyle(0.5f, FontStyle.Normal), -0.1f),
				new Text("sup", new RichTextStyle(0.5f, FontStyle.Normal), 0.6f)
			}
		);
		[SerializeField, PropertyBackingField]
		private FontStyle m_DefaultFontStyle = UnityEngine.UI.FontData.defaultFontData.fontStyle;
		[SerializeField, PropertyBackingField]
		private Link m_DefaultLinkStyle = Link.DefaultStyle;
		[SerializeField, PropertyBackingField]
		private Color m_DefaultTextColor = new Color(0.197f, 0.197f, 0.197f, 1f); // default color for UI.Text
		[SerializeField, PropertyBackingField]
		private Font m_Font = null;
		[SerializeField, PropertyBackingField]
		private int m_FontSize = UnityEngine.UI.FontData.defaultFontData.fontSize;
		[SerializeField, PropertyBackingField]
		private List<HyperTextStyles> m_InheritedStyles = new List<HyperTextStyles>();
		[SerializeField, PropertyBackingField]
		private float m_LineSpacing = UnityEngine.UI.FontData.defaultFontData.lineSpacing;
		[SerializeField, PropertyBackingField]
		private ImmutableRectOffset m_LinkHitboxPadding = new ImmutableRectOffset(0, 0, 0, 0);
		[SerializeField, PropertyBackingField]
		private List<LinkSubclass> m_LinkStyles = new List<LinkSubclass>();
		[SerializeField, PropertyBackingField]
		private List<Quad> m_QuadStyles = new List<Quad>();
		[SerializeField, PropertyBackingField]
		private bool m_ShouldOverrideInheritedDefaultFontStyle = false;
		[SerializeField, PropertyBackingField]
		private bool m_ShouldOverrideInheritedDefaultLinkStyle = false;
		[SerializeField, PropertyBackingField]
		private bool m_ShouldOverrideInheritedDefaultTextColor = false;
		[SerializeField, PropertyBackingField]
		private bool m_ShouldOverrideInheritedFontSize = false;
		[SerializeField, PropertyBackingField]
		private bool m_ShouldOverrideInheritedLineSpacing = false;
		[SerializeField, PropertyBackingField]
		private bool m_ShouldOverrideInheritedLinkHitboxPadding = false;
		#endregion

		#region Event Handlers
		/// <summary>
		/// Raises the inherited style changed event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		private void OnInheritedStyleChanged(HyperTextStyles sender)
		{
			SetDirty();
		}
		#endregion

		#region Inspector Properties
		private Text[] GetCustomTextStyles()
		{
			return m_CustomTextStyles.ToArray();
		}

		private HyperTextStyles[] GetInheritedStyles()
		{
			return m_InheritedStyles.ToArray();
		}

		private LinkSubclass[] GetLinkStyles()
		{
			return m_LinkStyles.ToArray();
		}

		private Quad[] GetQuadStyles()
		{
			return m_QuadStyles.ToArray();
		}

		private void SetCustomTextStyles(Text[] styles)
		{
			SetCustomTextStyles(styles as IList<Text>);
		}

		private void SetInheritedStyles(HyperTextStyles[] styles)
		{
			SetInheritedStyles(styles as IList<HyperTextStyles>);
		}

		private void SetLinkStyles(LinkSubclass[] styles)
		{
			SetLinkStyles(styles as IList<LinkSubclass>);
		}

		private void SetQuadStyles(Quad[] styles)
		{
			SetQuadStyles(styles as IList<Quad>);
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets the cascaded font.
		/// </summary>
		/// <value>The cascaded font.</value>
		public Font CascadedFont
		{
			get
			{
				Font result = m_Font;
				if (result == null)
				{
					int i = m_InheritedStyles.Count;
					while (result == null && i > 0)
					{
						--i;
						if (m_InheritedStyles[i] != null)
						{
							result = m_InheritedStyles[i].CascadedFont;
						}
					}
				}
				return result;
			}
		}
		/// <summary>
		/// Gets the cascaded default font style.
		/// </summary>
		/// <value>The cascaded default font style.</value>
		public FontStyle CascadedDefaultFontStyle
		{
			get
			{
				FontStyle result = m_DefaultFontStyle;
				if (!m_ShouldOverrideInheritedDefaultFontStyle)
				{
					int i = m_InheritedStyles.Count;
					while (i > 0)
					{
						--i;
						if (m_InheritedStyles[i] != null)
						{
							result = m_InheritedStyles[i].CascadedDefaultFontStyle;
							break;
						}
					}
				}
				return result;
			}
		}
		/// <summary>
		/// Gets the cascaded default text color.
		/// </summary>
		/// <value>The cascaded default text color.</value>
		public Color CascadedDefaultTextColor
		{
			get
			{
				Color result = m_DefaultTextColor;
				if (!m_ShouldOverrideInheritedDefaultTextColor)
				{
					int i = m_InheritedStyles.Count;
					while (i > 0)
					{
						--i;
						if (m_InheritedStyles[i] != null)
						{
							result = m_InheritedStyles[i].CascadedDefaultTextColor;
							break;
						}
					}
				}
				return result;
			}
		}
		/// <summary>
		/// Gets the cascaded default link style.
		/// </summary>
		/// <value>The cascaded default link style.</value>
		public Link CascadedDefaultLinkStyle
		{
			get
			{
				Link result = m_DefaultLinkStyle;
				if (!m_ShouldOverrideInheritedDefaultLinkStyle)
				{
					int i = m_InheritedStyles.Count;
					while (i > 0)
					{
						--i;
						if (m_InheritedStyles[i] != null)
						{
							result = m_InheritedStyles[i].CascadedDefaultLinkStyle;
							break;
						}
					}
				}
				return result;
			}
		}
		/// <summary>
		/// Gets the cascaded font size.
		/// </summary>
		/// <value>The cascaded font size.</value>
		public int CascadedFontSize
		{
			get
			{
				int result = m_FontSize;
				if (!m_ShouldOverrideInheritedFontSize)
				{
					int i = m_InheritedStyles.Count;
					while (i > 0)
					{
						--i;
						if (m_InheritedStyles[i] != null)
						{
							result = m_InheritedStyles[i].CascadedFontSize;
							break;
						}
					}
				}
				return result;
			}
		}
		/// <summary>
		/// Gets the cascaded line spacing.
		/// </summary>
		/// <value>The cascaded line spacing.</value>
		public float CascadedLineSpacing
		{
			get
			{
				float result = m_LineSpacing;
				if (!m_ShouldOverrideInheritedLineSpacing)
				{
					int i = m_InheritedStyles.Count;
					while (i > 0)
					{
						--i;
						if (m_InheritedStyles[i] != null)
						{
							result = m_InheritedStyles[i].CascadedLineSpacing;
							break;
						}
					}
				}
				return result;
			}
		}
		/// <summary>
		/// Gets the cascaded link hitbox padding.
		/// </summary>
		/// <value>The cascaded link hitbox padding.</value>
		public ImmutableRectOffset CascadedLinkHitboxPadding
		{
			get
			{
				ImmutableRectOffset result = m_LinkHitboxPadding;
				{
					if (!m_ShouldOverrideInheritedLinkHitboxPadding)
					{
						int i = m_InheritedStyles.Count;
						while (i > 0)
						{
							--i;
							if (m_InheritedStyles[i] != null)
							{
								result = m_InheritedStyles[i].CascadedLinkHitboxPadding;
								break;
							}
						}
					}
					return result;
				}
			}
		}
		/// <summary>
		/// Gets or sets the default font style.
		/// </summary>
		/// <value>The default font style.</value>
		public FontStyle DefaultFontStyle
		{
			get { return m_DefaultFontStyle; }
			set
			{
				if (m_DefaultFontStyle != value)
				{
					m_DefaultFontStyle = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets the default color of the text.
		/// </summary>
		/// <value>The default color of the text.</value>
		public Color DefaultTextColor
		{
			get { return m_DefaultTextColor; }
			set
			{
				if (m_DefaultTextColor != value)
				{
					m_DefaultTextColor = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets the default link style.
		/// </summary>
		/// <value>The default link style.</value>
		public Link DefaultLinkStyle
		{
			get { return m_DefaultLinkStyle; }
			set
			{
				if (m_DefaultLinkStyle.GetSerializedPropertiesHash() != value.GetSerializedPropertiesHash())
				{
					m_DefaultLinkStyle = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets the font defined on this instance.
		/// </summary>
		/// <value>The font.</value>
		public Font Font
		{
			get { return m_Font; }
			set
			{
				if (m_Font != value)
				{
					m_Font = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets the size of the font.
		/// </summary>
		/// <value>The size of the font.</value>
		public int FontSize
		{
			get { return m_FontSize; }
			set
			{
				if (m_FontSize != value)
				{
					m_FontSize = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets the line spacing.
		/// </summary>
		/// <value>The line spacing.</value>
		public float LineSpacing
		{
			get { return m_LineSpacing; }
			set
			{
				if (m_LineSpacing != value)
				{
					m_LineSpacing = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets a value indicating the number of units on each side that link hitboxes should extend beyond the bounds
		/// of the glyph geometry. Use positive values to generate link hitboxes that are larger than their encapsulated
		/// geometry (for, e.g., small screen devices).
		/// </summary>
		/// <value>
		/// The number of units on each side that link hitboxes should extend beyond the bounds of the glyph geometry.
		/// </value>
		public ImmutableRectOffset LinkHitboxPadding
		{
			get { return m_LinkHitboxPadding; }
			set
			{
				if (!m_LinkHitboxPadding.Equals(value))
				{
					m_LinkHitboxPadding = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether this instance should override the inherited default font style.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if should override inherited default font style; otherwise, <see langword="false"/>.
		/// </value>
		public bool ShouldOverrideInheritedDefaultFontStyle
		{
			get { return m_ShouldOverrideInheritedDefaultFontStyle; }
			set
			{
				if (m_ShouldOverrideInheritedDefaultFontStyle != value)
				{
					m_ShouldOverrideInheritedDefaultFontStyle = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether this instance should override the inherited default text color.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if should override inherited default text color; otherwise, <see langword="false"/>.
		/// </value>
		public bool ShouldOverrideInheritedDefaultTextColor
		{
			get { return m_ShouldOverrideInheritedDefaultTextColor; }
			set
			{
				if (m_ShouldOverrideInheritedDefaultTextColor != value)
				{
					m_ShouldOverrideInheritedDefaultTextColor = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether this instance should override the inherited default link style.
		/// </summary>
		/// <value>
		/// <see langword="true"/> the inherited default link style should be overridden; otherwise,
		/// <see langword="false"/>.
		/// </value>
		public bool ShouldOverrideInheritedDefaultLinkStyle
		{
			get { return m_ShouldOverrideInheritedDefaultLinkStyle; }
			set
			{
				if (m_ShouldOverrideInheritedDefaultLinkStyle != value)
				{
					m_ShouldOverrideInheritedDefaultLinkStyle = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether this instance should override the inherited font size.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the inherited font size should be overridden; otherwise, <see langword="false"/>.
		/// </value>
		public bool ShouldOverrideInheritedFontSize
		{
			get { return m_ShouldOverrideInheritedFontSize; }
			set
			{
				if (m_ShouldOverrideInheritedFontSize != value)
				{
					m_ShouldOverrideInheritedFontSize = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether this instance should override the inherited line spacing.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the inherited line spacing should be overridden; otherwise,
		/// <see langword="false"/>.
		/// </value>
		public bool ShouldOverrideInheritedLineSpacing
		{
			get { return m_ShouldOverrideInheritedLineSpacing; }
			set
			{
				if (m_ShouldOverrideInheritedLineSpacing != value)
				{
					m_ShouldOverrideInheritedLineSpacing = value;
					OnPropertyChanged();
				}
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether this instance should override the inherited link hitbox padding.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the inherited link hitbox padding should be overriden; otherwise,
		/// <see langword="false"/>.
		/// </value>
		public bool ShouldOverrideInheritedLinkHitboxPadding
		{
			get { return m_ShouldOverrideInheritedLinkHitboxPadding; }
			set
			{
				if (m_ShouldOverrideInheritedLinkHitboxPadding != value)
				{
					m_ShouldOverrideInheritedLinkHitboxPadding = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets the cascaded custom text styles.
		/// </summary>
		/// <param name="text">Text.</param>
		public void GetCascadedCustomTextStyles(List<Text> text)
		{
			if (!m_AreCascadesInitialized)
			{
				RebuildCascadingAndInheritedStyles();
			}
			text.Clear();
			text.AddRange(m_CascadedTextStyles);
		}

		/// <summary>
		/// Gets the cascaded link styles.
		/// </summary>
		/// <param name="links">Links.</param>
		public void GetCascadedLinkStyles(List<LinkSubclass> links)
		{
			if (!m_AreCascadesInitialized)
			{
				RebuildCascadingAndInheritedStyles();
			}
			links.Clear();
			links.AddRange(m_CascadedLinkStyles);
		}

		/// <summary>
		/// Gets the cascaded quad styles.
		/// </summary>
		/// <param name="quads">Quads.</param>
		public void GetCascadedQuadStyles(List<Quad> quads)
		{
			if (!m_AreCascadesInitialized)
			{
				RebuildCascadingAndInheritedStyles();
			}
			quads.Clear();
			quads.AddRange(m_CascadedQuadStyles);
		}

		/// <summary>
		/// Gets the custom text styles.
		/// </summary>
		/// <param name="text">Text.</param>
		public void GetCustomTextStyles(List<Text> text)
		{
			text.Clear();
			text.AddRange(m_CustomTextStyles);
		}

		/// <summary>
		/// Gets a table of the inherited custom text styles and the sheets on which they are defined.
		/// </summary>
		/// <returns>The inherited custom text styles.</returns>
		public Dictionary<Text, HyperTextStyles> GetInheritedCustomTextStyles()
		{
			if (!m_AreCascadesInitialized)
			{
				RebuildCascadingAndInheritedStyles();
			}
			return new Dictionary<Text, HyperTextStyles>(m_InheritedTextStyles);
		}
		
		/// <summary>
		/// Gets a table of the inherited link styles and the sheets on which they are defined.
		/// </summary>
		/// <returns>The inherited link styles.</returns>
		public Dictionary<LinkSubclass, HyperTextStyles> GetInheritedLinkStyles()
		{
			if (!m_AreCascadesInitialized)
			{
				RebuildCascadingAndInheritedStyles();
			}
			return new Dictionary<LinkSubclass, HyperTextStyles>(m_InheritedLinkStyles);
		}
		
		/// <summary>
		/// Gets a table of the inherited quad styles and the sheets on which they are defined.
		/// </summary>
		/// <returns>The inherited quad styles.</returns>
		public Dictionary<Quad, HyperTextStyles> GetInheritedQuadStyles()
		{
			if (!m_AreCascadesInitialized)
			{
				RebuildCascadingAndInheritedStyles();
			}
			return new Dictionary<Quad, HyperTextStyles>(m_InheritedQuadStyles);
		}

		/// <summary>
		/// Gets the inherited styles.
		/// </summary>
		/// <param name="styles">Styles.</param>
		public void GetInheritedStyles(List<HyperTextStyles> styles)
		{
			styles.Clear();
			styles.AddRange(m_InheritedStyles);
		}

		/// <summary>
		/// Gets the link styles.
		/// </summary>
		/// <param name="links">Links.</param>
		public void GetLinkStyles(List<LinkSubclass> links)
		{
			links.Clear();
			links.AddRange(m_LinkStyles);
		}

		/// <summary>
		/// Gets the quad styles.
		/// </summary>
		/// <param name="quads">Quads.</param>
		public void GetQuadStyles(List<Quad> quads)
		{
			quads.Clear();
			quads.AddRange(m_QuadStyles);
		}

		/// <summary>
		/// Sets the custom text styles.
		/// </summary>
		/// <param name="styles">Custom text styles.</param>
		/// <remarks>
		/// If more than one <see cref="LinkSubclass" /> in the collection has the same class name, then subsequent
		/// entries will append sequential digits to make their names unique.
		/// </remarks>
		public void SetCustomTextStyles(IList<Text> styles)
		{
			if (
				BackingFieldUtility.SetKeyedListBackingFieldFromStringKeyedArray(
					m_CustomTextStyles,
					styles == null ? null : styles.ToArray(),
					(tag, t) => new Text(tag, t.TextStyle, t.VerticalOffset),
					ignoreCase: true
				)
			)
			{
				RebuildCascadingAndInheritedStyles();
				OnPropertyChanged();
			}
		}

		#pragma warning disable 109
		/// <summary>
		/// Sets this instance dirty in order to force a styles changed callback.
		/// </summary>
		new public void SetDirty()
		{
			RebuildCascadingAndInheritedStyles();
			OnPropertyChanged();
		}
		#pragma warning restore 109

		/// <summary>
		/// Sets the list of styles to inherit from. Any custom link classes, tags, or quads specified on these sheets
		/// will be inherited, with preference given to those specified later in the array. If this instance defines any
		/// of the items, they will be considered overrides.
		/// </summary>
		/// <param name="styles">Styles.</param>
		public void SetInheritedStyles(IList<HyperTextStyles> styles)
		{
			List<HyperTextStyles> newStyles = new List<HyperTextStyles>();
			if (styles != null)
			{
				newStyles.AddRange(styles);
			}
			if (m_InheritedStyles.SequenceEqual(newStyles))
			{
				return;
			}
			foreach (HyperTextStyles style in m_InheritedStyles)
			{
				if (style != null)
				{
					style.Changed -= OnInheritedStyleChanged;
				}
			}
			m_InheritedStyles.Clear();
			foreach (HyperTextStyles style in newStyles)
			{
				if (style == this)
				{
					continue;
				}
				m_InheritedStyles.Add(style);
				if (style != null)
				{
					style.Changed += OnInheritedStyleChanged;
				}
			}
			RebuildCascadingAndInheritedStyles();
			OnPropertyChanged();
		}

		/// <summary>
		/// Sets the link styles.
		/// </summary>
		/// <remarks>
		/// If more than one <see cref="LinkSubclass" /> in the collection has the same class name, then subsequent
		/// entries will append sequential digits to make their names unique.
		/// </remarks>
		/// <param name="styles">Link styles.</param>
		public void SetLinkStyles(IList<LinkSubclass> styles)
		{
			if (
				BackingFieldUtility.SetKeyedListBackingFieldFromStringKeyedArray(
					m_LinkStyles,
					styles == null ? null : styles.ToArray(),
					(className, subclass) => new LinkSubclass(className, subclass.Style),
					ignoreCase: true
				)
			)
			{
				RebuildCascadingAndInheritedStyles();
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Sets the quad styles.
		/// </summary>
		/// <remarks>
		/// If more than one <see cref="Quad" /> in the collection has the same class name, then subsequent entries will
		/// append sequential digits to make their names unique.
		/// </remarks>
		/// <param name="styles">Quad styles.</param>
		public void SetQuadStyles(IList<Quad> styles)
		{
			if (
				BackingFieldUtility.SetKeyedListBackingFieldFromStringKeyedArray(
					m_QuadStyles,
					styles == null ? null : styles.ToArray(),
					(className, q) => new Quad(
						q.Sprite,
						className,
						q.SizeScalar,
						q.VerticalOffset,
						q.ShouldRespectColorization,
						q.LinkId,
						q.LinkClassName
					), ignoreCase: true
				)
			)
			{
				RebuildCascadingAndInheritedStyles();
				OnPropertyChanged();
			}
		}
		#endregion

		#region Unity Messages
		/// <summary>
		/// Unsubscribe from change events on inherited styles.
		/// </summary>
		protected virtual void OnDestroy()
		{
			foreach (HyperTextStyles style in m_InheritedStyles)
			{
				if (style == this || style == null)
				{
					continue;
				}
				style.Changed -= OnInheritedStyleChanged;
			}
		}

		/// <summary>
		/// Subscribe to change events on all inherited styles.
		/// </summary>
		protected virtual void OnEnable()
		{
			foreach (HyperTextStyles style in m_InheritedStyles)
			{
				if (style == this || style == null)
				{
					continue;
				}
				style.Changed += OnInheritedStyleChanged;
			}
		}
		#endregion

		/// <summary>
		/// Adds items in the styles collection to the supplied cascade table.
		/// </summary>
		/// <param name="styles">Collection of styles and the sheets on which they are defined.</param>
		/// <param name="cascade">Table of styles representing the cascade.</param>
		/// <typeparam name="T">A <see cref="Candlelight.IIdentifiable{T}"/> style type.</typeparam>
		private void AddToCascade<T>(
			Dictionary<T, HyperTextStyles> styles, Dictionary<T, HyperTextStyles> cascade
		) where T: IIdentifiable<string>, new()
		{
			List<T> cascadeKeys = new List<T>(cascade.Keys.Count * 2);
			foreach (KeyValuePair<T, HyperTextStyles> kv in styles)
			{
				T existingKey = new T();
				cascadeKeys.Clear();
				cascadeKeys.AddRange(cascade.Keys);
				for (int i = 0; i < cascadeKeys.Count; ++i)
				{
					if (cascadeKeys[i].Identifier.ToLower() == kv.Key.Identifier.ToLower())
					{
						existingKey = cascadeKeys[i];
						break;
					}
				}
				if (existingKey.Identifier.ToLower() == kv.Key.Identifier.ToLower())
				{
					if (
						existingKey is IPropertyBackingFieldCompatible &&
						kv.Key is IPropertyBackingFieldCompatible &&
						(existingKey as IPropertyBackingFieldCompatible).GetSerializedPropertiesHash() !=
						(kv.Key as IPropertyBackingFieldCompatible).GetSerializedPropertiesHash()
					)
					{
						cascade.Remove(existingKey);
						cascade.Add(kv.Key, kv.Value);
					}
				}
				else
				{
					cascade.Add(kv.Key, kv.Value);
				}
			}
		}

		/// <summary>
		/// Raises the property changed event.
		/// </summary>
		private void OnPropertyChanged()
		{
			if (this.Changed != null)
			{
				this.Changed(this);
			}
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
		/// Populates the cascaded styles backing field.
		/// </summary>
		/// <param name="cascadedStylesBackingField">Cascaded styles backing field.</param>
		/// <param name="selfStylesBackingField">The backing field for the styles defined on this instance.</param>
		/// <param name="inheritedStylesBackingField">The backing field for the styles inherited from parents.</param>
		/// <typeparam name="T">A style type.</typeparam>
		private void PopulateCascadedStylesBackingField<T>(
			List<T> cascadedStylesBackingField,
			List<T> selfStylesBackingField,
			Dictionary<T, HyperTextStyles> inheritedStylesBackingField
		) where T: IIdentifiable<string>, new()
		{
			using (DictPool<T, HyperTextStyles>.Scope cascadedStyles = new DictPool<T, HyperTextStyles>.Scope())
			{
				AddToCascade(inheritedStylesBackingField, cascadedStyles.Dict);
				// ensure duplicate entries (via inspector) don't get double-added to dictionaries for this instance
				using (DictPool<T, HyperTextStyles>.Scope selfStyles = new DictPool<T, HyperTextStyles>.Scope())
				{
					foreach (T style in selfStylesBackingField)
					{
						if (selfStyles.Dict.ContainsKey(style))
						{
							continue;
						}
						selfStyles.Dict.Add(style, this);
					}
					AddToCascade(selfStyles.Dict, cascadedStyles.Dict);
					cascadedStylesBackingField.Clear();
					cascadedStylesBackingField.AddRange(cascadedStyles.Dict.Keys);
				}
			}
		}

		/// <summary>
		/// Rebuilds the cascading and inherited styles.
		/// </summary>
		private void RebuildCascadingAndInheritedStyles()
		{
			// rebuild inheritance tables
			m_InheritedLinkStyles.Clear();
			m_InheritedQuadStyles.Clear();
			m_InheritedTextStyles.Clear();
			foreach (HyperTextStyles styles in m_InheritedStyles)
			{
				if (styles == null)
				{
					continue;
				}
				if (!styles.m_AreCascadesInitialized)
				{
					styles.RebuildCascadingAndInheritedStyles();
				}
				AddToCascade(styles.m_InheritedLinkStyles, m_InheritedLinkStyles);
				AddToCascade(styles.m_LinkStyles.ToDictionary(k => k, v => styles), m_InheritedLinkStyles);
				AddToCascade(styles.m_InheritedQuadStyles, m_InheritedQuadStyles);
				AddToCascade(styles.m_QuadStyles.ToDictionary(k => k, v => styles), m_InheritedQuadStyles);
				AddToCascade(styles.m_InheritedTextStyles, m_InheritedTextStyles);
				AddToCascade(styles.m_CustomTextStyles.ToDictionary(k => k, v => styles), m_InheritedTextStyles);
			}
			// rebuild cascades
			PopulateCascadedStylesBackingField(m_CascadedLinkStyles, m_LinkStyles, m_InheritedLinkStyles);
			PopulateCascadedStylesBackingField(m_CascadedQuadStyles, m_QuadStyles, m_InheritedQuadStyles);
			PopulateCascadedStylesBackingField(m_CascadedTextStyles, m_CustomTextStyles, m_InheritedTextStyles);
			m_AreCascadesInitialized = true;
		}

		#region Obsolete
		[System.Obsolete("Use HyperTextStyles.StylesChanged", true)]
		public UnityEngine.Events.UnityEvent OnStylesChanged { get; private set; }
		[System.Obsolete("Use HyperTextStyles.GetCustomTextStyles(List<Text>)", true)]
		public void GetCustomTextStyles(ref List<Text> text) {}
		[System.Obsolete("Use HyperTextStyles.GetCascadedCustomTextStyles(List<Text>)", true)]
		public void GetCascadedCustomTextStyles(ref List<Text> text) {}
		[System.Obsolete("Use HyperTextStyles.GetCascadedLinkStyles(List<LinkSubclass>)", true)]
		public void GetCascadedLinkStyles(ref List<LinkSubclass> links) {}
		[System.Obsolete("Use HyperTextStyles.GetCascadedQuadStyles(List<Quad>)", true)]
		public void GetCascadedQuadStyles(ref List<Quad> quads) {}
		[System.Obsolete("Use HyperTextStyles.GetInheritedStyles(List<HyperTextStyles>)", true)]
		public void GetInheritedStyles(ref List<HyperTextStyles> styles) {}
		[System.Obsolete("Use HyperTextStyles.GetLinkStyles(List<LinkSubclass>)", true)]
		public void GetLinkStyles(ref List<LinkSubclass> links) {}
		[System.Obsolete("Use HyperTextStyles.GetQuadStyles(List<Quad>)", true)]
		public void GetQuadStyles(ref List<Quad> quads) {}
		[System.Obsolete("Use HyperTextStyles.SetCustomTextStyles(IList<Text>)", true)]
		public void SetCustomTextStyles(IEnumerable<Text> styles) {}
		[System.Obsolete("Use HyperTextStyles.SetInheritedStyles(IList<HyperTextStyles>)", true)]
		public void SetInheritedStyles(IEnumerable<HyperTextStyles> styles) {}
		[System.Obsolete("Use HyperTextStyles.SetLinkStyles(IList<LinkSubclass>)", true)]
		public void SetLinkStyles(IEnumerable<LinkSubclass> styles) {}
		[System.Obsolete("Use HyperTextStyles.SetQuadStyles(IList<Quad>)", true)]
		public void SetQuadStyles(IEnumerable<Quad> styles) {}
		#endregion
	}
}