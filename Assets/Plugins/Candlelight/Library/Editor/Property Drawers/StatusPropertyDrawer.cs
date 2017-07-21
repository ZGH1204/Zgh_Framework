// 
// StatusPropertyDrawer.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Candlelight
{
	/// <summary>
	/// A property drawer to display some field with a status icon.
	/// </summary>
	[CustomPropertyDrawer(typeof(StatusPropertyAttribute), true)]
	public class StatusPropertyDrawer : PropertyDrawer
	{
		/// <summary>
		/// The method signature for <see cref="StatusPropertyAttribute.GetStatusCallback"/> 
		/// </summary>
		private static readonly System.Type[] s_GetStatusCallbackMethodSignature = (
			from p in typeof(StatusPropertyAttribute.GetStatusCallback).GetMethod("Invoke").GetParameters()
			select p.ParameterType
		).ToArray();

		/// <summary>
		/// Gets the status callback with the specified name on the specified <paramref name="property"/>'s class.
		/// </summary>
		/// <returns>The status callback.</returns>
		/// <param name="property">Property.</param>
		/// <param name="methodName">Method name.</param>
		public static StatusPropertyAttribute.GetStatusCallback GetStatusCallback(
			SerializedProperty property, string methodName
		)
		{
			System.Reflection.FieldInfo field;
			object provider = property.GetProvider(out field);
			StatusPropertyAttribute.GetStatusCallback result = null;
			if (!string.IsNullOrEmpty(methodName))
			{
				System.Reflection.MethodInfo method =
					provider.GetType().GetStaticMethod(methodName, s_GetStatusCallbackMethodSignature);
				if (method != null)
				{
					result = System.Delegate.CreateDelegate(
						typeof(StatusPropertyAttribute.GetStatusCallback), null, method
					) as StatusPropertyAttribute.GetStatusCallback;
				}
			}
			return result;
		}

		/// <summary>
		/// The current status tooltip.
		/// </summary>
		public string m_CurrentStatusTooltip = "";

		/// <summary>
		/// Gets the status property attribute associated with this drawer.
		/// </summary>
		/// <value>The status property attribute associated with this drawer.</value>
		private StatusPropertyAttribute Attribute { get { return this.attribute as StatusPropertyAttribute; } }

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}
		
		/// <summary>
		/// Gets the status by testing the supplied value.
		/// </summary>
		/// <returns>The status of the supplied value.</returns>
		/// <param name="provider">Object on which the field exists.</param>
		/// <param name="testValue">Test value.</param>
		private ValidationStatus GetStatus(Object provider, IValidatable testValue)
		{
			return this.Attribute.ValidationMethod == null ?
				testValue.GetValidationStatus(out m_CurrentStatusTooltip) :
				this.Attribute.ValidationMethod(provider, testValue, out m_CurrentStatusTooltip);
		}
		
		/// <summary>
		/// Gets the status by testing the supplied value.
		/// </summary>
		/// <returns>The status of the supplied value.</returns>
		/// <param name="provider">Object on which the field exists.</param>
		/// <param name="testValue">Test value.</param>
		private ValidationStatus GetStatus(Object provider, System.IComparable testValue)
		{
			m_CurrentStatusTooltip = string.Empty;
			if (this.Attribute.ValidationMethod == null)
			{
				foreach (StatusPropertyAttribute.Comparison comparison in this.Attribute.GetComparisons())
				{
					try
					{
						if (
							(testValue == null && comparison.TestValue == null && comparison.BadComparisonResult == 0) ||
							(testValue.CompareTo(comparison.TestValue) == comparison.BadComparisonResult)
						)
						{
							m_CurrentStatusTooltip = this.Attribute.BadStatusTooltip;
							return this.Attribute.BadStatusIcon;
						}
					}
					catch (System.ArgumentException e)
					{
						m_CurrentStatusTooltip = e.Message;
						return ValidationStatus.Error;
					}
				}
				return ValidationStatus.Okay;	
			}
			else
			{
				return this.Attribute.ValidationMethod(provider, testValue, out m_CurrentStatusTooltip);
			}
		}
		
		/// <summary>
		/// Gets the status by testing the supplied value.
		/// </summary>
		/// <returns>The status of the supplied value.</returns>
		/// <param name="provider">Object on which the field exists.</param>
		/// <param name="testValue">Test value.</param>
		public ValidationStatus GetStatus(Object provider, object testValue)
		{
			m_CurrentStatusTooltip = string.Empty;
			if (this.Attribute.ValidationMethod == null)
			{
				foreach (StatusPropertyAttribute.Comparison comparison in this.Attribute.GetComparisons())
				{
					try
					{
						if (
							comparison.BadComparisonResult != 0 ||
							(testValue == null && comparison.TestValue == null) ||
							(testValue.Equals(comparison.TestValue))
						)
						{
							m_CurrentStatusTooltip = this.Attribute.BadStatusTooltip;
							return this.Attribute.BadStatusIcon;
						}
					}
					catch (System.ArgumentException e)
					{
						m_CurrentStatusTooltip = e.Message;
						return ValidationStatus.Error;
					}
				}
				return ValidationStatus.Okay;
			}
			else
			{
				return this.Attribute.ValidationMethod(provider, testValue, out m_CurrentStatusTooltip);
			}
		}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			ValidationStatus status = ValidationStatus.None;
			bool canDraw = true;
			object value = property.GetValue();
			if (value is IValidatable)
			{
				status = GetStatus(property.serializedObject.targetObject, value as IValidatable);
			}
			else
			{
				switch (property.propertyType)
				{
				case SerializedPropertyType.Generic:
					canDraw = false;
					break;
				case SerializedPropertyType.AnimationCurve:
					canDraw = Attribute.ValidationMethod != null;
					if (canDraw)
					{
						status = GetStatus(property.serializedObject.targetObject, property.animationCurveValue);
					}
					break;
				case SerializedPropertyType.Gradient:
					canDraw = false;
					break;
				case SerializedPropertyType.ArraySize:
				case SerializedPropertyType.Character:
				case SerializedPropertyType.Enum:
				case SerializedPropertyType.Float:
				case SerializedPropertyType.Integer:
				case SerializedPropertyType.String:
					status = GetStatus(property.serializedObject.targetObject, (System.IComparable)property.GetValue());
					break;
				case SerializedPropertyType.Boolean:
				case SerializedPropertyType.Bounds:
				case SerializedPropertyType.Color:
				case SerializedPropertyType.LayerMask:
				case SerializedPropertyType.ObjectReference:
				case SerializedPropertyType.Quaternion:
				case SerializedPropertyType.Rect:
				case SerializedPropertyType.Vector2:
				case SerializedPropertyType.Vector3:
				case SerializedPropertyType.Vector4:
					status = GetStatus(property.serializedObject.targetObject, property.GetValue());
					break;
				}
			}
			if (!canDraw)
			{
				Color oldColor = GUI.color;
				GUI.color = Color.red;
				EditorGUI.LabelField(
					position,
					label,
					new GUIContent(
						string.Format(
							"Unable to perform comparison of SerializedPropertyType.{0} ({1}.{2}).",
							property.propertyType, property.serializedObject.targetObject.name, property.propertyPath
						)
					)
				);
				GUI.color = oldColor;
			}
			else
			{
				EditorGUIX.DisplayPropertyFieldWithStatus(
					position, property, status, label, true, m_CurrentStatusTooltip
				);
			}
		}
	}
}