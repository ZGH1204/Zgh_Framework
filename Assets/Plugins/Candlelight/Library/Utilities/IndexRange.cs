// 
// IndexRange.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Candlelight
{
	/// <summary>
	/// A class for describing a range of indices.
	/// </summary>
	public class IndexRange : System.ICloneable, IEnumerable<int>
	{
		/// <summary>
		/// Gets the number of elements encompassed by this instance.
		/// </summary>
		/// <value>The number of elements encompassed by this instance.</value>
		public int Count { get { return Mathf.Abs(this.EndIndex - this.StartIndex) + 1; } }
		/// <summary>
		/// The direction of the range, positive or negative.
		/// </summary>
		private int Direction { get { return this.EndIndex >= this.StartIndex ? 1 : -1; } }
		/// <summary>
		/// Gets or sets the end index.
		/// </summary>
		/// <value>The end index.</value>
		public int EndIndex { get; set; }
		/// <summary>
		/// Gets or sets the start index.
		/// </summary>
		/// <value>The start index.</value>
		public int StartIndex { get; set; }
		/// <summary>
		/// Gets the <see cref="System.Int32"/> at the specified index in the range.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <value>The <see cref="System.Int32"/> at the specified index in the range.</value>
		public int this[int index] { get { return this.StartIndex + index * this.Direction; } }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="IndexRange"/> class.
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		public IndexRange(int start, int end)
		{
			this.StartIndex = start;
			this.EndIndex = end;
		}

		/// <summary>
		/// Clone this instance.
		/// </summary>
		/// <returns>A clone of this instance.</returns>
		public object Clone()
		{
			return new IndexRange(this.StartIndex, this.EndIndex);
		}

		/// <summary>
		/// Determines whether or not this instance contains the specified index.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if this instance contains the specified index; otherwise <see langword="false"/>.
		/// </returns>
		/// <param name="index">Index.</param>
		public bool Contains(int index)
		{
			return this.Direction > 0 ?
				index >= this.StartIndex && index <= this.EndIndex :
				index <= this.StartIndex && index >= this.EndIndex;
		}

		/// <summary>
		/// Determines whether or not this instance contains the specified other <see cref="IndexRange"/>.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if this instance contains the specified other <see cref="IndexRange"/>; otherwise
		/// <see langword="false"/>.
		/// </returns>
		/// <param name="other">Other.</param>
		public bool Contains(IndexRange other)
		{
			return Contains(other.StartIndex) && Contains(other.EndIndex);
		}
		
		/// <summary>
		/// Gets an enumerator.
		/// </summary>
		/// <returns>An enumerator.</returns>
		public IEnumerator<int> GetEnumerator()
		{
			return (
				from i in Enumerable.Range(0, this.Count) select this.StartIndex + i * this.Direction
			).GetEnumerator();
		}
		
		/// <summary>
		/// Gets an enumerator.
		/// </summary>
		/// <returns>An enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Offset the indices in this instance based on the specified <paramref name="delta"/> over the specified
		/// <paramref name="range"/>.
		/// </summary>
		/// <param name="range">The range of indices that has shifted.</param>
		/// <param name="delta">The amount that the specified <paramref name="range"/> has shifted.</param>
		public void Offset(IndexRange range, int delta)
		{
			if (delta == 0)
			{
				return;
			}
			int direction = this.Direction;
			if (direction < 0)
			{
				Reverse();
			}
			int deltaEnd = Mathf.Max(range.StartIndex, range.EndIndex);
			int deltaStart = Mathf.Min(range.StartIndex, range.EndIndex);
			if (deltaEnd <= this.StartIndex)		// ...  |-------|
			{
				this.StartIndex += delta;
				this.EndIndex += delta;
			}
			else if (Contains(deltaStart))			// |--.----|.....
			{
				if (deltaStart == this.StartIndex)	// .-------|.....
				{
					this.StartIndex += delta;
				}
				this.EndIndex += delta;
			}
			else if (Contains(deltaEnd))			// .....|--.----|
			{
				this.StartIndex += delta;
				this.EndIndex += delta;
			}
			else if (								// ...|-------|..
				range.Contains(this.StartIndex) && range.Contains(this.EndIndex)
			)
			{
				this.StartIndex += delta;
				this.EndIndex += delta;
			}
			if (direction < 0)
			{
				Reverse();
			}
		}

		/// <summary>
		/// Reverse this instance.
		/// </summary>
		public void Reverse()
		{
			int start = this.StartIndex;
			this.StartIndex = this.EndIndex;
			this.EndIndex = start;
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="IndexRange"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="IndexRange"/>.</returns>
		public override string ToString()
		{
			return string.Format("[{0}, {1}]", this.StartIndex, this.EndIndex);
		}

		#region Obsolete
		/// <summary>
		/// Obsolete
		/// </summary>
		/// <param name="deltaValues">A collection delta values for each interval in the old range.</param>
		[System.Obsolete("Use IndexRange.Offset(IndexRange, int)")]
		public void Offset(Dictionary<IndexRange, int> deltaValues)
		{
			foreach (KeyValuePair<IndexRange, int> kv in deltaValues)
			{
				Offset(kv.Key, kv.Value);
			}
		}
		#endregion
	}
}