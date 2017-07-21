// 
// ITextSource.cs
// 
// Copyright (c) 2015-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

namespace Candlelight
{
	/// <summary>
	/// A delegate type for an event on a <see cref="ITextSource"/>.
	/// </summary>
	public delegate void ITextSourceEventHandler(ITextSource sender);

	/// <summary>
	/// An interface to specify an object is a source of text.
	/// </summary>
	public interface ITextSource
	{
		/// <summary>
		/// Occurs whenever the text on this instance has changed.
		/// </summary>
		event ITextSourceEventHandler BecameDirty;
		/// <summary>
		/// Gets the output text.
		/// </summary>
		/// <value>The output text.</value>
		string OutputText { get; }
	}
}