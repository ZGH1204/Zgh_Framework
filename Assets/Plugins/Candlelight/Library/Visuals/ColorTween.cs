// 
// ColorTween.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Collections;

namespace Candlelight.ColorTween
{
	/// <summary>
	/// Possible color tween modes.
	/// </summary>
	/// <remarks>
	/// This enum mirrors <see cref="UnityEngine.UI.CoroutineTween.ColorTween.ColorTweenMode"/>.
	/// </remarks>
	public enum Mode
	{
		/// <summary>
		/// Interpolate all color channels.
		/// </summary>
		All,
		/// <summary>
		/// Interpolate only the red, blue, and green color channels.
		/// </summary>
		RGB,
		/// <summary>
		/// Interpolate only the alpha channel.
		/// </summary>
		Alpha
	}

	/// <summary>
	/// Info for a color tween.
	/// </summary>
	/// <remarks>
	/// This struct mirrors functionality found in <see cref="UnityEngine.UI.CoroutineTween.ColorTween"/>.
	/// </remarks>
	internal class Info
	{
		/// <summary>
		/// A color change callback.
		/// </summary>
		internal delegate void Callback(Info sender, Color color);

		/// <summary>
		/// Occurs when the color changed.
		/// </summary>
		public event Callback ColorChanged;
		/// <summary>
		/// Gets or sets the duration of the tween.
		/// </summary>
		/// <value>The duration of the tween.</value>
		public float Duration { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Info"/> should ignore time scale.
		/// </summary>
		/// <value><see langword="true"/> if time scale should be ignored; otherwise, <see langword="false"/>.</value>
		public bool IgnoreTimeScale { get; set; }
		/// <summary>
		/// Gets or sets the start color.
		/// </summary>
		/// <value>The start color.</value>
		public Color StartColor { get; set; }
		/// <summary>
		/// Gets or sets the target color.
		/// </summary>
		/// <value>The target color.</value>
		public Color TargetColor { get; set; }
		/// <summary>
		/// Gets or sets the tween mode.
		/// </summary>
		/// <value>The tween mode.</value>
		public Mode TweenMode { get; set; }

		/// <summary>
		/// Interpolates between <see cref="Info.StartColor"/> and <see cref="Info.TargetColor"/> with the specified
		/// percentage value and invokes the <see cref="Info.ColorChanged"/> callback.
		/// </summary>
		/// <param name="percentage">Percentage.</param>
		public void TweenValue(float percentage)
		{
			Color result = Color.Lerp(this.StartColor, this.TargetColor, percentage);
			switch (this.TweenMode)
			{
			case Mode.Alpha:
				result.r = this.StartColor.r;
				result.g = this.StartColor.g;
				result.b = this.StartColor.b;
				break;
			case Mode.RGB:
				result.a = this.StartColor.a;
				break;
			}
			if (ColorChanged != null)
			{
				ColorChanged(this, result);
			}
		}
	}

	/// <summary>
	/// An iterator to invoke as a coroutine.
	/// </summary>
	internal class Iterator : IEnumerator
	{
		/// <summary>
		/// The elapsed time.
		/// </summary>
		private float m_ElapsedTime = 0f;
		/// <summary>
		/// The percentage completion.
		/// </summary>
		private float m_Percentage = 0f;
		/// <summary>
		/// The tween info.
		/// </summary>
		private readonly Info m_TweenInfo;

		/// <summary>
		/// Initializes a new instance of the <see cref="Iterator"/> class.
		/// </summary>
		/// <param name="tweenInfo">Tween info.</param>
		public Iterator(Info tweenInfo)
		{
			m_TweenInfo = tweenInfo;
		}

		/// <summary>
		/// Gets the current value.
		/// </summary>
		/// <value>The current value.</value>
		public object Current { get { return null; } }

		/// <summary>
		/// Moves to the next item in the iterator.
		/// </summary>
		/// <returns><see langword="true"/>, if the iterator advanced; otherwise, <see langword="false"/>.</returns>
		public bool MoveNext()
		{
			if (m_ElapsedTime < m_TweenInfo.Duration)
			{
				m_ElapsedTime += !m_TweenInfo.IgnoreTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
				m_Percentage = Mathf.Clamp01(m_ElapsedTime / m_TweenInfo.Duration);
				m_TweenInfo.TweenValue(m_Percentage);
				return true;
			}
			m_TweenInfo.TweenValue(1f);
			return false;
		}

		/// <summary>
		/// Reset this instance.
		/// </summary>
		public void Reset()
		{
			throw new System.NotSupportedException();
		}
	}

	/// <summary>
	/// A class to run a color tween.
	/// </summary>
	/// <remarks>
	/// This class mirrors functionality found in <see cref="UnityEngine.UI.CoroutineTween.TweenRunner{T}"/>.
	/// </remarks>
	internal class Runner
	{
		/// <summary>
		/// The iterator.
		/// </summary>
		private IEnumerator m_Iterator;

		/// <summary>
		/// Gets the <see cref="UnityEngine.MonoBehavior"/> that will run the coroutine.
		/// </summary>
		/// <value>The coroutine container.</value>
		public MonoBehaviour CoroutineContainer { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Runner"/> class.
		/// </summary>
		/// <param name="container">Container.</param>
		public Runner(MonoBehaviour container)
		{
			this.CoroutineContainer = container;
		}

		/// <summary>
		/// Starts the tween.
		/// </summary>
		/// <param name="colorTweenInfo">Color tween info.</param>
		public void StartTween(Info colorTweenInfo)
		{
			if (m_Iterator != null)
			{
				this.CoroutineContainer.StopCoroutine(m_Iterator);
				m_Iterator = null;
			}
			if (!this.CoroutineContainer.gameObject.activeInHierarchy)
			{
				colorTweenInfo.TweenValue(1f);
				return;
			}
			m_Iterator = new Iterator(colorTweenInfo);
			this.CoroutineContainer.StartCoroutine(m_Iterator);
		}
	}
}