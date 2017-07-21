// 
// Typewriter.cs
// 
// Copyright (c) 2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;

namespace Candlelight.UI
{
	/// <summary>
	/// An <see cref="ITextSource"/> component that spells out one letter at a time. To use it with a
	/// <see cref="HyperText"/> component, add it to the <see cref="HyperText"/> and assign it as the
	/// <see cref="HyperText.InputTextSource"/>.
	/// </summary>
	[AddComponentMenu("UI/Candlelight/Effects/Typewriter"), ExecuteInEditMode]
	public class Typewriter : MonoBehaviour, ITextSource
	{
		/// <summary>
		/// Typewriter event.
		/// </summary>
		[System.Serializable]
		public class TypewriterEvent : UnityEngine.Events.UnityEvent<Typewriter> {}
		/// <summary>
		/// Typewriter character event.
		/// </summary>
		[System.Serializable]
		public class TypewriterCharacterEvent : UnityEngine.Events.UnityEvent<Typewriter, char> {}

		/// <summary>
		/// Gets the input text source status.
		/// </summary>
		/// <returns>The input text source status.</returns>
		/// <param name="provider">Provider.</param>
		/// <param name="value">Value.</param>
		/// <param name="tooltip">Tooltip.</param>
		private static ValidationStatus GetInputTextSourceStatus(object provider, object value, out string tooltip)
		{
			if (value != null)
			{
				tooltip = "Assigning a text input source overrides the text on this object.";
				return ValidationStatus.Warning;
			}
			tooltip = string.Empty;
			return ValidationStatus.None;
		}

		/// <summary>
		/// Occurs whenever the text on this instance has changed.
		/// </summary>
		public event ITextSourceEventHandler BecameDirty;

		/// <summary>
		/// The timer for typing out the next character.
		/// </summary>
		private float m_CharacterTimer = 0f;
		/// <summary>
		/// The current character index typed out.
		/// </summary>
		[SerializeField, HideInInspector]
		private int m_CurrentIndex = -1;
		/// <summary>
		/// The timer for blinking the cursor.
		/// </summary>
		private float m_CursorBlinkTimer = 0f;
		/// <summary>
		/// Flag specifying the cursor's current state.
		/// </summary>
		private bool m_IsCursorOn = true;

		#region Backing Fields
		[Tooltip("Specifies the target text to output.")]
		[SerializeField, PropertyBackingField(typeof(TextAreaAttribute), 5, 10)]
		private string m_Text = "";
		[Tooltip("Assigning a text input source overrides the text on this object.")]
		[SerializeField, PropertyBackingField(
			typeof(StatusPropertyAttribute), typeof(Typewriter), "GetInputTextSourceStatus"
		)]
		private Object m_OverrideTextSource = null;
		private ITextSource m_InputTextSource;
		[Header("Animation")]
#pragma warning disable 414
		[Tooltip("Specifies the progress of this object typing out its text.")]
		[SerializeField, PropertyBackingField(typeof(RangeAttribute), 0f, 1f)]
		private float m_Progress = 0f;
#pragma warning restore 414
		[Tooltip("Specifies whether character and cursor animation should be affected by time scale.")]
		[SerializeField]
		private bool m_UseUnscaledTime = true;
		[Tooltip("Specifies the number of seconds to wait between typing out characters.")]
		[SerializeField, PropertyBackingField(typeof(RangeAttribute), 0f, 5f)]
		private float m_CharacterDelay = 0.02f;
		[Tooltip("Specifies an optional cursor to append to the output text.")]
		[SerializeField, PropertyBackingField]
		private string m_Cursor = "_";
		[Tooltip("Specifies the frequency of the cursor blink in seconds.")]
		[SerializeField, PropertyBackingField(typeof(RangeAttribute), 0f, 5f)]
		private float m_CursorBlink = 0.5f;
		[Header("Events")]
		[SerializeField]
		private TypewriterCharacterEvent m_OnTypeCharacter = new TypewriterCharacterEvent();
		[SerializeField]
		private TypewriterEvent m_OnFinish = new TypewriterEvent();
		#endregion

		/// <summary>
		/// Gets or sets the cursor to append at the end of the effect.
		/// </summary>
		/// <value>The cursor to append at the end of the effect.</value>
		public string Cursor
		{
			get { return m_Cursor; }
			set
			{
				value = value ?? string.Empty;
				if (value == m_Cursor)
				{
					return;
				}
				m_Cursor = value;
				UpdateOutputText(false);
			}
		}
		/// <summary>
		/// Gets or sets the cursor blink interval in seconds.
		/// </summary>
		/// <value>The cursor blink interval in seconds.</value>
		public float CursorBlink
		{
			get { return m_CursorBlink; }
			set
			{
				m_CursorBlink = Mathf.Max(0f, value);
				if (m_CursorBlink == 0f)
				{
					m_IsCursorOn = true;
				}
			}
		}
		/// <summary>
		/// Gets or sets the delay between characters.
		/// </summary>
		/// <value>The delay between characters.</value>
		public float CharacterDelay
		{
			get { return m_CharacterDelay; }
			set { m_CharacterDelay = Mathf.Max(0f, value); }
		}
		/// <summary>
		/// Gets or sets the input text source. If a value is assigned, its OutputText will be used in place of the
		/// value in the InputText property of this <see cref="HyperTextProcessor"/>.
		/// </summary>
		/// <value>The input text source.</value>
		public ITextSource InputTextSource
		{
			get { return BackingFieldUtility.GetInterfaceBackingField(ref m_InputTextSource, m_OverrideTextSource); }
			set
			{
				if (value as Typewriter == this)
				{
					Debug.LogError("Cannot assign this object as its own input text source.");
					return;
				}
				if (
					BackingFieldUtility.SetInterfaceBackingField(
						value,
						ref m_InputTextSource,
						ref m_OverrideTextSource,
						onAssign: t => t.BecameDirty += SetDirty,
						onUnassign: t => t.BecameDirty -= SetDirty
					)
				)
				{
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets the input text to use.
		/// </summary>
		/// <value>The input text to use.</value>
		private string InputTextToUse
		{
			get { return (this.InputTextSource != null ? m_InputTextSource.OutputText : m_Text) ?? string.Empty; }
		}
		/// <summary>
		/// Gets a callback for whenever the effect has finished.
		/// </summary>
		/// <value>A callback for whenever the effect has finished.</value>
		public TypewriterEvent OnFinish { get { return m_OnFinish; } }
		/// <summary>
		/// Gets a callback for whenever a character has been typed out.
		/// </summary>
		/// <remarks>Use this event to e.g., play a sound effect.</remarks>
		/// <value>A callback for whenever a character has been typed out.</value>
		public TypewriterCharacterEvent OnTypeCharacter { get { return m_OnTypeCharacter; } }
		/// <summary>
		/// Gets the output text.
		/// </summary>
		/// <value>The output text.</value>
		public string OutputText { get; private set; }
		/// <summary>
		/// Gets or sets the input text source object. This property only exists for the inspector.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <value>The input text source object.</value>
		private Object OverrideTextSource
		{
			get { return m_OverrideTextSource; }
			set
			{
				if (value == this)
				{
					Debug.LogError("Cannot assign this object as its own input text source.");
					return;
				}
				BackingFieldUtility.SetInterfaceBackingFieldObject<ITextSource>(
					value, ref m_OverrideTextSource, o => this.InputTextSource = o
				);
			}
		}
		/// <summary>
		/// Gets or sets the progress.
		/// </summary>
		/// <remarks>Set the value to 1.0 to complete the effect immediately.</remarks>
		/// <value>The progress.</value>
		public float Progress
		{
			get
			{
				string text = this.InputTextToUse;
				return m_Progress = text.Length == 0 ? 1f : (m_CurrentIndex + 1) / (float)(text.Length);
			}
			set
			{
				value = Mathf.Clamp01(value);
				int oldIndex = m_CurrentIndex;
				m_CurrentIndex = (int)(this.InputTextToUse.Length * value) - 1;
				if (oldIndex != m_CurrentIndex)
				{
					UpdateOutputText(true);
				}
				m_Progress = value;
			}
		}
		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <remarks>Setting this value will automatically restart the output.</remarks>
		/// <value>The text.</value>
		public string Text
		{
			get { return m_Text; }
			set
			{
				if (m_Text == value)
				{
					return;
				}
				m_Text = value;
				if (this.InputTextSource == null)
				{
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether this instance should use unscaled time.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this instance should use unscaled time; otherwise, <see langword="false"/>.
		/// </value>
		public bool ShouldUseUnscaledTime
		{
			get { return m_UseUnscaledTime; }
			set { m_UseUnscaledTime = value; }
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			UpdateOutputText(false);
		}

		/// <summary>
		/// Sets this instance dirty.
		/// </summary>
		/// <param name="textSource">The <see cref="ITextSource"/> that triggered the event.</param>
		private void SetDirty(ITextSource textSource = null)
		{
			m_CharacterTimer = 0f;
			m_CurrentIndex = Mathf.Min(m_CurrentIndex, this.InputTextToUse.Length - 1);
			m_Progress = this.Progress;
			UpdateOutputText(false);
		}

		/// <summary>
		/// Update this instance.
		/// </summary>
		protected virtual void Update()
		{
			float deltaTime = m_UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
			m_CursorBlinkTimer += deltaTime;
			bool didCursorChange = m_CursorBlinkTimer > m_CursorBlink;
			if (m_CursorBlink > 0f && didCursorChange)
			{
				m_IsCursorOn = !m_IsCursorOn;
				m_CursorBlinkTimer = 0f;
			}
			if (m_CurrentIndex >= this.InputTextToUse.Length)
			{
				if (didCursorChange)
				{
					UpdateOutputText(false);
				}
				return;
			}
			m_CharacterTimer += deltaTime;
			if (m_CharacterTimer >= m_CharacterDelay)
			{
				++m_CurrentIndex;
				m_CharacterTimer = 0f;
				UpdateOutputText(true);
			}
		}

		/// <summary>
		/// Updates the <see cref="Typewriter.OutputText"/>.
		/// </summary>
		/// <param name="didTypeCharacter">
		/// <see langword="true"/> if a new character was typed; otherwise, <see langword="false"/>.
		/// </param>
		private void UpdateOutputText(bool didTypeCharacter)
		{
			string text = this.InputTextToUse;
			this.OutputText = string.Format(
				"{0}{1}",
				m_CurrentIndex < text.Length ? text.Substring(0, m_CurrentIndex + 1) : text,
				m_IsCursorOn ? m_Cursor : string.Empty
			);
			if (this.BecameDirty != null)
			{
				this.BecameDirty(this);
			}
			didTypeCharacter &= m_CurrentIndex >= 0;
			if (!didTypeCharacter)
			{
				return;
			}
			if (m_CurrentIndex < text.Length)
			{
				m_OnTypeCharacter.Invoke(this, text[m_CurrentIndex]);
			}
			else
			{
				m_OnFinish.Invoke(this);
			}
		}
	}
}