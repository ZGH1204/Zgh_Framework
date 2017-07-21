// 
// OrientationParameterTestData.cs
// 
// Copyright (c) 2015-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Collections.Generic;

namespace Candlelight
{
	/// <summary>
	/// A class for defining data associated with an <see cref="UnityEngine.Animator"/> parameter of type
	/// <see cref="UnityEngine.AnimatorControllerParameterType.Bool"/> that indicates whether or not a specified
	/// (average) local axis on a collection of <see cref="UnityEngine.Transform"/> objects is currently facing up.
	/// </summary>
	[System.Serializable]
	public class OrientationParameterTestData
	{
		#region Backing Fields
		[SerializeField]
		private bool m_IncludeAllBodies = true;
		[SerializeField, StatusProperty]
		private List<Transform> m_IncludedBodies = new List<Transform>();
		[SerializeField]
		private Vector3 m_TestAxis = Vector3.forward;
		[SerializeField, PropertyBackingField(typeof(RangeAttribute), 0f, 180f)]
		private float m_TestAngle = 90f;
		#endregion
		/// <summary>
		/// Gets a value indicating whether this instance should include all bodies in the hierarchy being tested.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if include all bodies in the hierarchy should be tested; otherwise,
		/// <see langword="false"/>.
		/// </value>
		public bool IncludeAllBodies { get { return m_IncludeAllBodies; } }
		/// <summary>
		/// Gets the test angle.
		/// </summary>
		/// <value>The test angle.</value>
		public float TestAngle
		{
			get { return m_TestAngle; }
			private set { m_TestAngle = Mathf.Clamp(value, 0f, 180f); }
		}
		/// <summary>
		/// Gets the local axis to average and test.
		/// </summary>
		/// <value>The local axis to average and test.</value>
		public Vector3 TestAxis { get { return m_TestAxis; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="OrientationParameter"/> class.
		/// </summary>
		/// <param name="testAxis">Local axis to average and test.</param>
		/// <param name="testAngle">
		/// The angle from <see cref="UnityEngine.Vector3.up"/> within which to test <paramref name="testAxis"/>.
		/// </param>
		/// <param name="includedBodies">
		/// Optional collection of specific bodies to include in the calculation.
		/// </param>
		public OrientationParameterTestData(
			Vector3 testAxis, float testAngle = 90f, IEnumerable<Transform> includedBodies = null
		)
		{
			m_IncludeAllBodies = includedBodies == null;
			m_IncludedBodies = includedBodies == null ? null : new List<Transform>(includedBodies);
			m_TestAxis = testAxis.normalized;
			this.TestAngle = testAngle;
		}

		/// <summary>
		/// Gets the bodies that should be included for consideration.
		/// </summary>
		/// <returns>
		/// <see langword="true"/>, if the returned list specifies particular bodies; otherwise, <see langword="false"/>
		/// if all bodies in the hierarchy in question ought to be taken into consideration.
		/// </returns>
		/// <param name="bodies">A list of <see cref="UnityEngine.Transform"/>s to populate.</param>
		public bool GetIncludedBodies(List<Transform> bodies)
		{
			bodies.Clear();
			if (m_IncludedBodies != null)
			{
				bodies.AddRange(m_IncludedBodies);
			}
			return !m_IncludeAllBodies;
		}
	}
}