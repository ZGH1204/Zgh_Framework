// 
// UnityVersion.cs
// 
// Copyright (c) 2013-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Text.RegularExpressions;

namespace Candlelight
{
	/// <summary>
	/// A struct for getting and comparing Unity versions.
	/// </summary>
	public struct UnityVersion : System.IEquatable<UnityVersion>
	{
		/// <summary>
		/// Initializes the <see cref="UnityVersion"/> struct.
		/// </summary>
		static UnityVersion()
		{
			string[] tokens = Application.unityVersion.Split('.');
			s_Current = new UnityVersion(
				int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(new Regex(@"\d+").Match(tokens[2]).Value)
			);
		}

		#region Backing Fields
		private static UnityVersion s_Current;
		#endregion

		/// <summary>
		/// Gets the currently running version.
		/// </summary>
		/// <value>The current running version.</value>
		public static UnityVersion Current { get { return s_Current; } }

		/// <summary>
		/// Gets the maintenance version.
		/// </summary>
		/// <value>The maintenance version.</value>
		public int MaintenanceVersion { get; private set; }
		/// <summary>
		/// Gets the major version.
		/// </summary>
		/// <value>The major version.</value>
		public int MajorVersion { get; private set; }
		/// <summary>
		/// Gets the minor version.
		/// </summary>
		/// <value>The minor version.</value>
		public int MinorVersion { get; private set; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="UnityVersion"/> struct.
		/// </summary>
		/// <param name="major">Major.</param>
		/// <param name="minor">Minor.</param>
		/// <param name="maintenance">Maintenance.</param>
		public UnityVersion(int major, int minor, int maintenance) : this()
		{
			this.MajorVersion = major;
			this.MinorVersion = minor;
			this.MaintenanceVersion = maintenance;
		}

		/// <summary>
		/// Compares this instance to the supplied other.
		/// </summary>
		/// <returns>-1 if less than, 1 if greater than, 0 if equal.</returns>
		/// <param name="other">Other version.</param>
		public int CompareTo(UnityVersion other)
		{
			int cmp = this.MajorVersion.CompareTo(other.MajorVersion);
			if (cmp != 0)
			{
				return cmp;
			}
			cmp = this.MinorVersion.CompareTo(other.MinorVersion);
			if (cmp != 0)
			{
				return cmp;
			}
			return this.MaintenanceVersion.CompareTo(other.MaintenanceVersion);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="UnityVersion"/>.
		/// </summary>
		/// <param name="obj">
		/// The <see cref="System.Object"/> to compare with the current <see cref="UnityVersion"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="UnityVersion"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public override bool Equals(object obj)
		{
			return ObjectX.Equals(ref this, obj);
		}

		/// <summary>
		/// Determines whether the specified <see cref="UnityVersion"/> is equal to the current
		/// <see cref="UnityVersion"/>.
		/// </summary>
		/// <param name="other">
		/// The <see cref="UnityVersion"/> to compare with the current <see cref="UnityVersion"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the specified <see cref="UnityVersion"/> is equal to the current
		/// <see cref="UnityVersion"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public bool Equals(UnityVersion other)
		{
			return GetHashCode() == other.GetHashCode();
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="UnityVersion"/> object.
		/// </summary>
		/// <returns>
		/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
		/// hash table.
		/// </returns>
		public override int GetHashCode()
		{
			return ObjectX.GenerateHashCode(
				this.MajorVersion.GetHashCode(), this.MinorVersion.GetHashCode(), this.MaintenanceVersion.GetHashCode()
			);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the <see cref="UnityVersion"/> instance.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the <see cref="UnityVersion"/> instance.</returns>
		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}", this.MajorVersion, this.MinorVersion, this.MaintenanceVersion);
		}

		/// <summary>
		/// Gets a value indicating whether one <see cref="UnityVersion"/> is older than another.
		/// </summary>
		/// <param name="v1">The first <see cref="UnityVersion"/></param>
		/// <param name="v2">The second <see cref="UnityVersion"/></param>
		/// <returns>
		/// <see langword="true"/> if the first <see cref="UnityVersion"/> is older than the second; otherwise,
		/// <see langword="false"/>.
		/// </returns>
		public static bool operator <(UnityVersion v1, UnityVersion v2)
		{
			return v1.CompareTo(v2) < 0;
		}

		/// <summary>
		/// Gets a value indicating whether one <see cref="UnityVersion"/> is newer than another.
		/// </summary>
		/// <param name="v1">The first <see cref="UnityVersion"/></param>
		/// <param name="v2">The second <see cref="UnityVersion"/></param>
		/// <returns>
		/// <see langword="true"/> if the first <see cref="UnityVersion"/> is newer than the second; otherwise,
		/// <see langword="false"/>.
		/// </returns>
		public static bool operator >(UnityVersion v1, UnityVersion v2)
		{
			return v1.CompareTo(v2) > 0;
		}

		/// <summary>
		/// Gets a value indicating whether one <see cref="UnityVersion"/> is equal to another.
		/// </summary>
		/// <param name="v1">The first <see cref="UnityVersion"/></param>
		/// <param name="v2">The second <see cref="UnityVersion"/></param>
		/// <returns>
		/// <see langword="true"/> if the first <see cref="UnityVersion"/> is equal to the second; otherwise,
		/// <see langword="false"/>.
		/// </returns>
		public static bool operator ==(UnityVersion v1, UnityVersion v2)
		{
			return v1.CompareTo(v2) == 0;
		}

		/// <summary>
		/// Gets a value indicating whether one <see cref="UnityVersion"/> is unequal to another.
		/// </summary>
		/// <param name="v1">The first <see cref="UnityVersion"/></param>
		/// <param name="v2">The second <see cref="UnityVersion"/></param>
		/// <returns>
		/// <see langword="true"/> if the first <see cref="UnityVersion"/> is unequal to the second; otherwise,
		/// <see langword="false"/>.
		/// </returns>
		public static bool operator !=(UnityVersion v1, UnityVersion v2)
		{
			return v1.CompareTo(v2) != 0;
		}

		/// <summary>
		/// Gets a value indicating whether one <see cref="UnityVersion"/> is older than or equal to another.
		/// </summary>
		/// <param name="v1">The first <see cref="UnityVersion"/></param>
		/// <param name="v2">The second <see cref="UnityVersion"/></param>
		/// <returns>
		/// <see langword="true"/> if the first <see cref="UnityVersion"/> is older than or equal to the second;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		public static bool operator <=(UnityVersion v1, UnityVersion v2)
		{
			return v1.CompareTo(v2) <= 0;
		}

		/// <summary>
		/// Gets a value indicating whether one <see cref="UnityVersion"/> is newer than or equal to another.
		/// </summary>
		/// <param name="v1">The first <see cref="UnityVersion"/></param>
		/// <param name="v2">The second <see cref="UnityVersion"/></param>
		/// <returns>
		/// <see langword="true"/> if the first <see cref="UnityVersion"/> is newer than or equal to the second;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		public static bool operator >=(UnityVersion v1, UnityVersion v2)
		{
			return v1.CompareTo(v2) >= 0;
		}
	}
}