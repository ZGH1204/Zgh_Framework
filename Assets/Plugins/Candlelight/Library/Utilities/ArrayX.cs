// 
// ArrayX.cs
// 
// Copyright (c) 2014-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

#if UNITY_WINRT || UNITY_WINRT_8_0 || UNITY_WINRT_8_1
#define WINRT
#endif

using System.Collections.Generic;

namespace Candlelight
{
	/// <summary>
	/// An extension class for <see cref="System.Array"/> and <see cref="System.Collections.Generic.List{T}"/>.
	/// </summary>
	public static class ArrayX
	{
		/// <summary>
		/// A reusable random number generator.
		/// </summary>
		private static readonly System.Random s_RandomNumberGenerator = new System.Random();

		/// <summary>
		/// Compares two types by depth.
		/// </summary>
		/// <remarks>
		/// Note that this method only performs name-based comparison when targeting WinRT-based platforms.
		/// </remarks>
		/// <returns>
		/// 1 if <paramref name="t1"/> has more ancestors than <paramref name="t2"/>; -1 if the opposite is true;
		/// otherwise, the result of comparing the names of <paramref name="t1"/> and <paramref name="t2"/>.
		/// </returns>
		/// <param name="t1">The first <see cref="System.Type"/>.</param>
		/// <param name="t2">the second <see cref="System.Type"/>.</param>
		private static int CompareTypesByDepth(System.Type t1, System.Type t2)
		{
			int depth1 = 0;
			int depth2 = 0;
#if !WINRT
			System.Type baseType = t1.BaseType;
			while (baseType != null)
			{
				baseType = baseType.BaseType;
				++depth1;
			}
			baseType = t2.BaseType;
			while (baseType != null)
			{
				baseType = baseType.BaseType;
				++depth2;
			}
#endif
			return depth1 > depth2 ? 1 : (depth2 > depth1 ? -1 : t1.Name.CompareTo(t2.Name));
		}

		/// <summary>
		/// Populate the specified array with the given value.
		/// </summary>
		/// <param name="array">Array.</param>
		/// <param name="value">Value.</param>
		/// <typeparam name="T">The element type.</typeparam>
		public static void Populate<T>(this IList<T> array, T value)
		{
			for (int i = 0; i < array.Count; ++i)
			{
				array[i] = value;
			}
		}

		/// <summary>
		/// Scrolls the index of the array to wrap on ends.
		/// </summary>
		/// <returns>The array index.</returns>
		/// <param name="currentIndex">Current index.</param>
		/// <param name="length">Length of the array.</param>
		/// <param name="scrollAmount">Scroll amount.</param>
		public static int ScrollArrayIndex(int currentIndex, int length, int scrollAmount)
		{
			currentIndex += scrollAmount;
			if (currentIndex < 0)
			{
				while (currentIndex < 0)
				{
					currentIndex = (length) + currentIndex;
				}
			}
			else if (currentIndex > length - 1)
			{
				while (currentIndex > length - 1)
				{
					currentIndex -= (length);
				}
			}
			return currentIndex;
		}

		/// <summary>
		/// Perform a Fisher-Yates shuffle on the specified <paramref name="array"/>.
		/// </summary>
		/// <remarks>See http://stackoverflow.com/questions/273313/randomize-a-listt/1262619#1262619</remarks>
		/// <param name="array">Array.</param>
		/// <typeparam name="T">The element type.</typeparam>
		public static void Shuffle<T>(this IList<T> array)
		{
			int n = array.Count;
			while (n > 1)
			{
				--n;
				int k = s_RandomNumberGenerator.Next(n + 1);
				T value = array[k];
				array[k] = array[n];
				array[n] = value;
			}
		}

		/// <summary>
		/// Sorts a list of types hierarchically.
		/// </summary>
		/// <remarks>Note that this method does nothing when targeting WinRT-based platforms.</remarks>
		/// <param name="types">Types.</param>
		public static void SortTypesHierarchically(this List<System.Type> types)
		{
#if !WINRT
			using (ListPool<System.Type>.Scope sortedItems = new ListPool<System.Type>.Scope())
			{
				types.Sort(CompareTypesByDepth);
				foreach (System.Type item in types)
				{
					System.Type baseType = item.BaseType;
					int baseTypeIndex = sortedItems.List.FindIndex(i => i == baseType);
					while (baseTypeIndex < 0 && baseType != null)
					{
						baseType = baseType.BaseType;
						baseTypeIndex = sortedItems.List.FindIndex(i => i == baseType);
					}
					if (baseTypeIndex < 0)
					{
						sortedItems.List.Add(item);
					}
					else
					{
						List<System.Type> siblingItems = sortedItems.List.FindAll(i => i.BaseType == baseType);
						int insertionIndex = baseTypeIndex;
						if (siblingItems.Count > 0)
						{
							foreach (System.Type siblingItem in siblingItems)
							{
								if (siblingItem.Name.CompareTo(item.Name) > 0)
								{
									insertionIndex = sortedItems.List.IndexOf(siblingItem);
									break;
								}
							}
						}
						sortedItems.List.Insert(UnityEngine.Mathf.Max(0, insertionIndex), item);
					}
				}
				types.Clear();
				types.AddRange(sortedItems.List);
			}
#endif
		}
	}
}