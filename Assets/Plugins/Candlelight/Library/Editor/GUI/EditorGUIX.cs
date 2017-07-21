// 
// EditorGUIX.cs
// 
// Copyright (c) 2012-2016, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a non-redistributable part of a static class for working
// with editor GUI.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;

namespace Candlelight
{
	/// <summary>
	/// Editor GUI extensions.
	/// </summary>
	public static partial class EditorGUIX
	{
		#region Delegates
		/// <summary>
		/// A callback to set the value on a serialized property.
		/// </summary>
		public delegate void SetSerializedPropertyValueCallback(SerializedProperty property);
		/// <summary>
		/// A callback to test if the value represented by a mass property is equal to the value represented by an
		/// individual property.
		/// </summary>
		public delegate bool TestMassPropertyEqualityCallback(
			SerializedProperty massProperty, SerializedProperty individualProperty
		);
		/// <summary>
		/// A callback to test if all values represented by the specified property are equal.
		/// </summary>
		public delegate bool TestPropertyEqualityCallback(SerializedProperty property);
		#endregion

		/// <summary>
		/// Initializes the <see cref="EditorGUIX"/> class.
		/// </summary>
		static EditorGUIX()
		{
			foreach (System.Type type in ReflectionX.AllTypes)
			{
				if (!type.IsEnum)
				{
					continue;
				}
				using (ListPool<System.FlagsAttribute>.Scope attrs = new ListPool<System.FlagsAttribute>.Scope())
				{
					if (type.GetCustomAttributes<System.FlagsAttribute>(attrs.List) == 0)
					{
						continue;
					}
				}
				if (System.Enum.GetUnderlyingType(type) != typeof(int))
				{
					continue;
				}
				s_MaskEnumValues[type] = new ReadOnlyCollection<int>(System.Enum.GetValues(type).Cast<int>().ToArray());
			}
		}

		/// <summary>
		/// The label to use for handle size sliders.
		/// </summary>
		private static readonly GUIContent s_HandleSizeLabel = new GUIContent("Size");
		/// <summary>
		/// The pixels per indent level.
		/// </summary>
		public static readonly float pixelsPerIndentLevel = 15f;
		/// <summary>
		/// Unity's x handle color.
		/// </summary>
		public static readonly Color xHandleColor =
			GetColorFromUnityPreferences("Scene/X Axis", new Color(0.95f, 0.28f, 0.137f, 1f));
		/// <summary>
		/// Unity's y handle color.
		/// </summary>
		public static readonly Color yHandleColor =
			GetColorFromUnityPreferences("Scene/Y Axis", new Color(0.733f, 0.95f, 0.337f, 1f));
		/// <summary>
		/// Unity's z handle color.
		/// </summary>
		public static readonly Color zHandleColor =
			GetColorFromUnityPreferences("Scene/Z Axis", new Color(0.255f, 0.553f, 0.95f, 1f));
		/// <summary>
		/// The values for all enum types decorated with <see cref="System.Flags"/>, converted to integers.
		/// </summary>
		private static readonly Dictionary<System.Type, ReadOnlyCollection<int>> s_MaskEnumValues =
			new Dictionary<System.Type, ReadOnlyCollection<int>>();
		/// <summary>
		/// The maximum number of layers that can be defined.
		/// </summary>
		private static readonly int s_MaxNumLayers = 32;
		/// <summary>
		/// An empty icon to use for status fields with no status.
		/// </summary>
		private static Texture2D s_NoStatusIcon = null;
		/// <summary>
		/// The slider hash.
		/// </summary>
		private static readonly int s_SliderHash = (int)typeof(EditorGUI).GetStaticFieldValue<int>("s_SliderHash");

		#region MemberInfo
		private static readonly MethodInfo s_GetClosestPowerOfTen =
			typeof(MathUtils).GetStaticMethod("GetClosestPowerOfTen");
		private static readonly MethodInfo s_RoundBasedOnMinimumDifference = typeof(MathUtils).GetMethod(
			"RoundBasedOnMinimumDifference",
			ReflectionX.staticBindingFlags,
			null,
			new System.Type[] { typeof(float), typeof(float) },
			null
		);
		private static readonly MethodInfo s_RoundToMultipleOf = typeof(MathUtils).GetStaticMethod("RoundToMultipleOf");
		#endregion
		#region Shared Allocations
		private static readonly object[] s_Param1 = new object[1];
		private static readonly object[] s_Param2 = new object[2];
		private static readonly GUIContent s_ReusableLabel = new GUIContent();
		private static float s_SliderMin = 0f;
		private static float s_SliderMax = 5f;
		private static GUIContent s_ValidationStatusIcon = new GUIContent();
		#endregion

		/// <summary>
		/// The width of the input fields for min/max sliders.
		/// </summary>
		/// <value>The width of the input fields for min/max sliders.</value>
		private static float MinMaxFloatFieldWidth { get { return EditorGUIUtility.wideMode ? 48f : 32f; } }
		/// <summary>
		/// Gets the width of the narrow inline button.
		/// </summary>
		/// <value>The width of the narrow inline button.</value>
		public static float NarrowInlineButtonWidth { get { return k_NarrowButtonWidth; } }
		/// <summary>
		/// The width of the thumb widget in a reorderable list
		/// </summary>
		/// <value>The width of the thumb widget in a reorderable list.</value>
		public static float ReorderableListThumbWidth { get { return 18f; } }
		/// <summary>
		/// Gets the width of the wide inline button.
		/// </summary>
		/// <value>The width of the wide inline button.</value>
		public static float WideInlineButtonWidth { get { return k_WideButtonWidth; } }
		
		/// <summary>
		/// Begins the scene GUI controls area.
		/// </summary>
		/// <returns><see langword="true"/> if the scene GUI is enabled; otherwise, <see langword="false"/>.</returns>
		public static bool BeginSceneGUIControlsArea()
		{
			EditorGUILayout.BeginVertical(EditorStylesX.SceneGUIInspectorBackground);
			DisplaySceneGUIToggle();
			++EditorGUI.indentLevel;
			return SceneGUI.IsEnabled;
		}
		
		/// <summary>
		/// Create an array of buttons in the editor GUI layout.
		/// </summary>
		/// <returns>The index of the button pressed; otherwise, -1.</returns>
		/// <param name="labels">Labels.</param>
		/// <param name="buttonEnabledStates">Optional array to specify enabled states for buttons in the array.</param>
		/// <param name="style">Optional style to use.</param>
		public static int DisplayButtonArray(string[] labels, bool[] buttonEnabledStates = null, GUIStyle style = null)
		{
			GUIContent[] gcLabels = labels == null ? null : new GUIContent[labels.Length];
			if (labels != null)
			{
				for (int i=0; i<gcLabels.Length; ++i)
				{
					gcLabels[i] = new GUIContent(labels[i]);
				}
			}
			return DisplayButtonArray(gcLabels, buttonEnabledStates, style);
		}
		
		/// <summary>
		/// Create an array of buttons in the editor GUI layout.
		/// </summary>
		/// <returns>The index of the button pressed; otherwise, -1.</returns>
		/// <param name="labels">Labels.</param>
		/// <param name="buttonEnabledStates">Optional array to specify enabled states for buttons in the array.</param>
		/// <param name="style">Optional style to use.</param>
		public static int DisplayButtonArray(
			GUIContent[] labels, bool[] buttonEnabledStates = null, GUIStyle style = null
		)
		{
			return DisplayButtonArray(
				GUILayoutUtility.GetRect(0f, InlineButtonHeight + EditorGUIUtility.standardVerticalSpacing),
				labels,
				buttonEnabledStates,
				style
			);
		}

		/// <summary>
		/// Create an array of buttons
		/// </summary>
		/// <returns>The button array.</returns>
		/// <param name="position">Position.</param>
		/// <param name="labels">Labels.</param>
		/// <param name="buttonEnabledStates">Optional array to specify enabled states for buttons in the array.</param>
		/// <param name="style">Optional style to use.</param>
		public static int DisplayButtonArray(
			Rect position, GUIContent[] labels, bool[] buttonEnabledStates = null, GUIStyle style = null
		)
		{
			int result = -1;
			Color oldColor = GUI.color;
			if (labels == null || labels.Length == 0)
			{
				GUI.color = Color.red;
				EditorGUI.LabelField(position, "No button labels supplied.");
				GUI.color = oldColor;
				return result;
			}
			if (buttonEnabledStates == null)
			{
				buttonEnabledStates = new bool[labels.Length];
				buttonEnabledStates.Populate(true);
			}
			GUI.color = TintedGUIColor;
			position = EditorGUI.IndentedRect(position);
			position.height -= EditorGUIUtility.standardVerticalSpacing;
			position.width -= EditorGUIUtility.standardVerticalSpacing * 0.5f * (labels.Length - 1);
			position.width = position.width / labels.Length;
			for (int i = 0; i < labels.Length; ++i)
			{
				EditorGUI.BeginDisabledGroup(!buttonEnabledStates[i]);
				{
					if (DisplayEditorButton(position, labels[i], style, false))
					{
						result = i;
					}
				}
				EditorGUI.EndDisabledGroup();
				position.x += position.width + EditorGUIUtility.standardVerticalSpacing * 0.5f;
			}
			GUI.color = oldColor;
			return result;
		}

		/// <summary>
		/// Displays an enum mask popup.
		/// </summary>
		/// <remarks>
		/// By default, EditorGUI.EnumMaskPopup() assumes each enumeration represents a unique value incremented by
		/// powers of 2, starting from 1. This method assists when serializing enums that start from 0 and/or that have
		/// mixed values for different enumerations (e.g., 1 | 2).
		/// </remarks>
		/// <returns>The value of the enum mask popup.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="intValue"><see cref="System.Int32"/> value to be serialized.</param>
		/// <param name="enumType">Enum type decorated with <see cref="System.Flags"/>.</param>
		public static int DisplayEnumMaskPopup(Rect position, GUIContent label, int intValue, System.Type enumType)
		{
			ReadOnlyCollection<int> enumValues;
			try { enumValues = s_MaskEnumValues[enumType]; }
			catch (KeyNotFoundException)
			{
				GUIContent errorMessage = new GUIContent(
					"Incompatible Type",
					string.Format("{0} must be an enum marked with {1}.", enumType, typeof(System.FlagsAttribute))
				);
				EditorGUIX.DisplayLabelFieldWithStatus(
					position, label, errorMessage, ValidationStatus.Error, errorMessage.tooltip
				);
				return intValue;
			}
			System.Enum oldEnumValue = (System.Enum)System.Enum.ToObject(enumType, intValue);
			int unityIntValue = 0;
			for (int i = 0; i < enumValues.Count; ++i)
			{
				if ((intValue & enumValues[i]) == enumValues[i])
				{
					unityIntValue |= 2 << (i - 1);
				}
			}
			oldEnumValue = (System.Enum)System.Enum.ToObject(enumType, unityIntValue);
			object newEnumValue;
			EditorGUI.BeginChangeCheck();
			{
				newEnumValue = EditorGUI.EnumMaskField(position, label ?? GUIContent.none, oldEnumValue);
			}
			if (EditorGUI.EndChangeCheck())
			{
				unityIntValue = System.Convert.ToInt32(newEnumValue);
				int result = 0;
				for (int i = 0; i < enumValues.Count; ++i)
				{
					if ((unityIntValue & (2 << (i - 1))) == (2 << (i - 1)))
					{
						result |= System.Convert.ToInt32(enumValues[i]);
					}
				}
				return result;
			}
			else
			{
				return intValue;
			}
		}

		/// <summary>
		/// Displays a selection grid for an enumerated type.
		/// </summary>
		/// <returns>The currently selected value.</returns>
		/// <param name="currentValue">The currently selected value.</param>
		/// <param name="labels">Labels.</param>
		/// <param name="xCount">Number of buttons in each grid row.</param>
		/// <param name="style">Optional style override.</param>
		/// <typeparam name="T">An enumerated type.</typeparam>
		public static T DisplayEnumSelectionGrid<T>(
			T currentValue, GUIContent[] labels = null, int xCount = 0, GUIStyle style = null
		) where T : struct, System.IComparable, System.IConvertible, System.IFormattable
		{
			if (!typeof(T).IsEnum) 
			{
				string message = "T must be an enumerated type";
				Debug.LogException(new System.ArgumentException(message, "T"));
				EditorGUILayout.HelpBox(message, MessageType.Error);
				return currentValue;
			}
			labels = labels ??
				(from name in System.Enum.GetNames(typeof(T)) select new GUIContent(name.ToWords())).ToArray();
			return (T)(object)DisplaySelectionGrid(
				System.Convert.ToInt32(currentValue), labels, xCount, style
			);
		}
		
		/// <summary>
		/// Displays a tab group for an enumerated type.
		/// </summary>
		/// <returns>The current tab.</returns>
		/// <param name="currentTab">Current tab.</param>
		/// <param name="tabContents">GUI callbacks to invoke for each tab.</param>
		/// <param name="labels">Labels.</param>
		/// <param name="xCount">Number of tabs to draw in each row.</param>
		/// <typeparam name="T">An enumerated type.</typeparam>
		public static T DisplayEnumTabGroup<T>(
			T currentTab, Dictionary<T, System.Action> tabContents, GUIContent[] labels = null, int xCount = 0
		) where T : struct, System.IComparable, System.IConvertible, System.IFormattable
		{
			if (!typeof(T).IsEnum) 
			{
				string message = "T must be an enumerated type";
				Debug.LogException(new System.ArgumentException(message, "T"));
				EditorGUILayout.HelpBox(message, MessageType.Error);
				return currentTab;
			}
			labels = labels ?? (
				from name in System.Enum.GetNames(typeof(T)) select new GUIContent(name.ToWords(), name.ToWords())
			).ToArray();
			Dictionary<int, System.Action> contents = new Dictionary<int, System.Action>();
			List<int> values = new List<int>((int[])System.Enum.GetValues(typeof(T)));
			foreach (KeyValuePair<T, System.Action> kv in tabContents)
			{
				contents.Add(values.IndexOf(System.Convert.ToInt32(kv.Key)), kv.Value);
			}
			return (T)(object)DisplayTabGroup(System.Convert.ToInt32(currentTab), labels, contents, xCount);
		}
		
		/// <summary>
		/// Displays a field using the current GUI skin, if a default skin is not being used.
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="value">Value</param>
		/// <param name="drawMethod"></param>
		public static T DisplayField<T>(GUIContent label, T value, System.Func<Rect, GUIContent, T, T> drawMethod)
		{
			return DisplayField<T>(EditorGUILayout.GetControlRect(), label, value, drawMethod);
		}

		/// <summary>
		/// Displays a field using the current GUI skin, if a default skin is not being used.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Value</param>
		/// <param name="drawMethod"></param>
		public static T DisplayField<T>(
			Rect position, GUIContent label, T value, System.Func<Rect, GUIContent, T, T> drawMethod
		)
		{
			label = label ?? GUIContent.none;
#if !UNITY_4_6 && !UNITY_4_7
			if (EditorStylesX.IsUsingBuiltinSkin)
			{
				EditorGUI.PrefixLabel(position, label);
			}
			else
			{
				EditorGUI.PrefixLabel(position, label, GUI.skin.label);
			}
#else
			EditorGUI.PrefixLabel(position, label);
#endif
			position.width -= EditorGUIUtility.labelWidth;
			position.x += EditorGUIUtility.labelWidth;
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			value = drawMethod(position, GUIContent.none, value);
			EditorGUI.indentLevel = oldIndent;
			return value;
		}
		
		/// <summary>
		/// Displays a float slider using the current GUI skin, if a default skin is not being used.
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="size">Size.</param>
		/// <param name="min">Minimum size.</param>
		/// <param name="max">Maximum size.</param>
		public static float DisplayFloatSlider(GUIContent label, float value, float min, float max)
		{
			s_SliderMin = min;
			s_SliderMax = max;
			return DisplayField<float>(label, value, OnFloatSliderField);
		}

		/// <summary>
		/// Displays the handle property editor.
		/// </summary>
		/// <param name="handleName">Handle name (e.g. "Vision").</param>
		/// <param name="toggle">Toggle.</param>
		/// <param name="size">Size.</param>
		/// <param name="minSize">Minimum size.</param>
		/// <param name="maxSize">Max size.</param>
		/// <typeparam name="TEditor">The 1st type parameter.</typeparam>
		public static void DisplayHandlePropertyEditor<TEditor>(
			string handleName,
			EditorPreference<bool, TEditor> toggle,
			EditorPreference<float, TEditor> size,
			float minSize = 0f,
			float maxSize = 5f
		)
		{
			EditorGUI.BeginChangeCheck();
			{
				toggle.CurrentValue = DisplayOnOffToggle(string.Format("{0} Handle", handleName), toggle.CurrentValue);
				if (toggle.CurrentValue)
				{
					++EditorGUI.indentLevel;
					size.CurrentValue = DisplayFloatSlider(s_HandleSizeLabel, size.CurrentValue, minSize, maxSize);
					--EditorGUI.indentLevel;
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}
		}

		/// <summary>
		/// Displays the handle property editor.
		/// </summary>
		/// <param name="handleName">Handle name (e.g. "Vision").</param>
		/// <param name="toggle">Toggle.</param>
		/// <param name="color">Color.</param>
		/// <param name="size">Optional size preference.</param>
		/// <param name="minSize">Minimum size.</param>
		/// <param name="maxSize">Maximum size.</param>
		/// <typeparam name="TEditor">The editor type.</typeparam>
		public static void DisplayHandlePropertyEditor<TEditor>(
			string handleName,
			EditorPreference<bool, TEditor> toggle,
			EditorPreference<Color, TEditor> color,
			EditorPreference<float, TEditor> size = null,
			float minSize = 0f,
			float maxSize = 5f
		)
		{
			EditorGUI.BeginChangeCheck();
			{
				toggle.CurrentValue = DisplayOnOffToggle(string.Format("{0} Handle", handleName), toggle.CurrentValue);
				if (toggle.CurrentValue)
				{
					++EditorGUI.indentLevel;
					Color col = color.CurrentValue;
					try
					{
						col = EditorGUILayout.ColorField(col);
					}
					catch (ExitGUIException)
					{
						col = color.CurrentValue;
					}
					color.CurrentValue = col;
					if (size != null)
					{
						size.CurrentValue = DisplayFloatSlider(s_HandleSizeLabel, size.CurrentValue, minSize, maxSize);
					}
					--EditorGUI.indentLevel;
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}
		}
		
		/// <summary>
		/// Displays the handle property editor.
		/// </summary>
		/// <param name="handleName">Handle name (e.g. "Vision").</param>
		/// <param name="toggle">Toggle.</param>
		/// <param name="color">Color.</param>
		/// <param name="size">Optional size preference.</param>
		/// <param name="minSize">Minimum size.</param>
		/// <param name="maxSize">Maximum size.</param>
		/// <typeparam name="TEditor">The editor type.</typeparam>
		public static void DisplayHandlePropertyEditor<TEditor>(
			string handleName,
			EditorPreference<bool, TEditor> toggle,
			EditorPreference<ColorGradient, TEditor> color,
			EditorPreference<float, TEditor> size = null,
			float minSize = 0f,
			float maxSize = 5f
		)
		{
			EditorGUI.BeginChangeCheck();
			{
				toggle.CurrentValue = DisplayOnOffToggle(string.Format("{0} Handle", handleName), toggle.CurrentValue);
				if (toggle.CurrentValue)
				{
					++EditorGUI.indentLevel;
					Color minColor = color.CurrentValue.MinColor;
					Color maxColor = color.CurrentValue.MaxColor;
					ColorInterpolationSpace interpolationSpace;
					Rect colorPickerPosition = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
					int indent = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 0;
					colorPickerPosition.width =
						(colorPickerPosition.width - EditorGUIX.StandardHorizontalSpacing) * 0.5f;
					try
					{
						minColor = EditorGUI.ColorField(colorPickerPosition, minColor);
					}
					catch (ExitGUIException)
					{
						minColor = color.CurrentValue.MinColor;
					}
					colorPickerPosition.x += colorPickerPosition.width + EditorGUIX.StandardHorizontalSpacing;
					try
					{
						maxColor = EditorGUI.ColorField(colorPickerPosition, maxColor);
					}
					catch (ExitGUIException)
					{
						maxColor = color.CurrentValue.MaxColor;
					}
					EditorGUI.indentLevel = indent;
					interpolationSpace = (ColorInterpolationSpace)EditorGUILayout.EnumPopup(
						"Interpolation", color.CurrentValue.InterpolationSpace
					);
					color.CurrentValue = new ColorGradient(minColor, maxColor, interpolationSpace);
					if (size != null)
					{
						size.CurrentValue = DisplayFloatSlider(s_HandleSizeLabel, size.CurrentValue, minSize, maxSize);
					}
					--EditorGUI.indentLevel;
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}
		}

		/// <summary>
		/// Displays a label field with a status icon.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="text">Text.</param>
		/// <param name="status">Status.</param>
		/// <param name="statusTooltip">Status tooltip.</param>
		public static void DisplayLabelFieldWithStatus(
			Rect position, GUIContent label, GUIContent text, ValidationStatus status, string statusTooltip = ""
		)
		{
			Rect iconRect;
			GetInlineIconRect(ref position, out iconRect);
			EditorGUI.LabelField(position, label, text);
			DisplayValidationStatusIcon(iconRect, status, statusTooltip);
		}

		/// <summary>
		/// Displays a layer selector popup.
		/// </summary>
		/// <returns>The currently selected layer.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="currentValue">Current value.</param>
		public static int DisplayLayerPopup(Rect position, GUIContent label, int currentValue)
		{
			using (ListPool<GUIContent>.Scope layerLabels = new ListPool<GUIContent>.Scope())
			{
				using (ListPool<int>.Scope layerValues = new ListPool<int>.Scope())
				{
					for (int i = 0; i < s_MaxNumLayers; ++i)
					{
						string layerName = LayerMask.LayerToName(i);
						if (!string.IsNullOrEmpty(layerName))
						{
							layerLabels.List.Add(new GUIContent(layerName));
							layerValues.List.Add(i);
						}
						else if (i == currentValue)
						{
							layerLabels.List.Add(new GUIContent(string.Format("Unnamed Layer {0}", i)));
							layerValues.List.Add(i);
						}
					}
					return EditorGUI.IntPopup(
						position, label, currentValue, layerLabels.List.ToArray(), layerValues.List.ToArray()
					);
				}
			}
		}

		/// <summary>
		/// Displays a reorderable list in a box followed by an editor for the currently selected element. Use this
		/// method with lists of objects that are <see cref="UnityEditor.SerializedPropertyType.Generic"/>.
		/// </summary>
		/// <param name="list">List.</param>
		/// <param name="forceExpand">
		/// If set to <see langword="true"/> then force expand the element if it has children.
		/// </param>
		public static void DisplayListWithElementEditor(ReorderableList list, bool forceExpand = false)
		{
			EditorGUILayout.BeginVertical(EditorStylesX.Box);
			{
				Rect rect = EditorGUILayout.GetControlRect(false, list.GetHeight());
				list.DoList(rect);
				if (list.count > 0)
				{
					int selected = Mathf.Clamp(list.index, 0, list.serializedProperty.arraySize - 1);
					EditorGUILayout.Space();
					SerializedProperty sp = list.serializedProperty.GetArrayElementAtIndex(selected);
					if (forceExpand && sp.hasChildren)
					{
						sp.isExpanded = true;
					}
					EditorGUILayout.PropertyField(sp, true);
				}
			}
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// Displays a min/max slider with input fields at either end.
		/// </summary>
		/// <param name="position">Control position.</param>
		/// <param name="label">Label.</param>
		/// <param name="minProp">Minimum value property.</param>
		/// <param name="maxProp">Maximum value property.</param>
		/// <param name="sliderMin">Minimum slider value.</param>
		/// <param name="sliderMax">Maximum slider value.</param>
		public static void DisplayMinMaxPropertiesWithSlider(
			Rect position,
			GUIContent label,
			SerializedProperty minProp,
			SerializedProperty maxProp,
			float sliderMin,
			float sliderMax
		)
		{
			Rect sliderPos = position;
			float indent = label == null || label == GUIContent.none ? 0f : EditorGUIUtility.labelWidth;
			sliderPos.x += indent + MinMaxFloatFieldWidth + StandardHorizontalSpacing;
			sliderPos.width -= indent + 2f * (MinMaxFloatFieldWidth + StandardHorizontalSpacing);
			Rect maxPos = sliderPos;
			maxPos.x += sliderPos.width + StandardHorizontalSpacing;
			maxPos.width = MinMaxFloatFieldWidth;
			Rect minPos = position;
			minPos.width -= sliderPos.width + maxPos.width + 2f * StandardHorizontalSpacing;
			bool drawSlider=  sliderPos.width > 32f;
			if (!drawSlider)
			{
				minPos.width = (position.width - StandardHorizontalSpacing) * 0.5f;
				maxPos.x -= minPos.width - maxPos.width;
				maxPos.width = minPos.width;
			}
			EditorGUI.BeginChangeCheck();
			{
				EditorGUI.PropertyField(minPos, minProp, label);
			}
			if (EditorGUI.EndChangeCheck())
			{
				maxProp.floatValue = Mathf.Max(minProp.floatValue, maxProp.floatValue);
			}
			float max = maxProp.floatValue;
			float min = minProp.floatValue;
			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			if (drawSlider)
			{
				EditorGUI.MinMaxSlider(
					GUIContent.none, sliderPos, ref min, ref max, Mathf.Min(sliderMin, min), Mathf.Max(sliderMax, max)
				);
			}
			if (maxProp.floatValue != max)
			{
				maxProp.floatValue = max;
				minProp.floatValue = Mathf.Min(min, max);
			}
			if (minProp.floatValue != min)
			{
				minProp.floatValue = min;
				maxProp.floatValue = Mathf.Max(min, max);
			}
			EditorGUI.BeginChangeCheck();
			{
				EditorGUI.PropertyField(maxPos, maxProp, GUIContent.none);
			}
			if (EditorGUI.EndChangeCheck())
			{
				minProp.floatValue = Mathf.Min(minProp.floatValue, maxProp.floatValue);
			}
			EditorGUI.indentLevel = indentLevel;
		}

		/// <summary>
		/// Displays the on/off toggle.
		/// </summary>
		/// <returns>The value of the on/off toggle.</returns>
		/// <param name="label">Label.</param>
		/// <param name="val">Value.</param>
		public static bool DisplayOnOffToggle(string label, bool val)
		{
			return DisplayOnOffToggle(new GUIContent(label), val);
		}

		/// <summary>
		/// Displays a property field using the current GUI skin, if a default skin is not being used.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <param name="includeChildren">If set to <see langword="true"/> include children.</param>
		/// <param name="label">Label.</param>
		public static void DisplayPropertyField(
			SerializedProperty property, bool includeChildren = true, GUIContent label = null
		)
		{
			DisplayPropertyField(EditorGUILayout.GetControlRect(), property, includeChildren, label);
		}

		/// <summary>
		/// Displays a property field using the current GUI skin, if a default skin is not being used.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="includeChildren">If set to <see langword="true"/> include children.</param>
		/// <param name="label">Label.</param>
		public static void DisplayPropertyField(
			Rect position, SerializedProperty property, bool includeChildren = true, GUIContent label = null
		)
		{
			label = label ?? new GUIContent(property.displayName);
			if (label != GUIContent.none)
			{
#if !UNITY_4_6 && !UNITY_4_7
				if (EditorStylesX.IsUsingBuiltinSkin)
				{
					EditorGUI.PrefixLabel(position, label);
				}
				else
				{
					EditorGUI.PrefixLabel(position, label, GUI.skin.label);
				}
#else
				EditorGUI.PrefixLabel(position, label);
#endif
				position.width -= EditorGUIUtility.labelWidth;
				position.x += EditorGUIUtility.labelWidth;
			}
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUI.PropertyField(position, property, GUIContent.none, includeChildren);
			EditorGUI.indentLevel = oldIndent;
		}

		/// <summary>
		/// Displays a property field with a button to apply its value to many items, if at least one item has a
		/// different value for the property of interest.
		/// </summary>
		/// <returns><see langword="true"/> if the button was pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="property">The <see cref="UnityEditor.SerializedProperty"/> used to set others.</param>
		/// <param name="label">Label.</param>
		/// <param name="affectedObjects">
		/// The <see cref="UnityEditor.SerializedProperty"/> for all objects affected by changes to the property.
		/// </param>
		/// <param name="buttonNarrow">Button width to use in narrow mode.</param>
		/// <param name="buttonWide">Button width to use in wide mode.</param>
		/// <param name="isMassPropertyEqualToAffectedProperties">
		/// If not <see langword="null"/> then specifies a method for testing equality of the values of the property and
		/// a potentially matching affected object; otherwise, the method simply compares their hash codes and, if they
		/// are enums, their type string as well.
		/// </param>
		/// <param name="areAffectedPropertiesEqual">
		/// If not <see langword="null"/> then specifies a method for testing the equality of the values on the affected
		/// objects. Otherwise, <see cref="UnityEditor.SerializedProperty.hasMultipleDifferentValues"/> will be used. 
		/// </param>
		/// <param name="onSetProperty">
		/// If not <see langword="null"/> then specifies a method to pass the new property value to, when the property
		/// changes or the button is clicked; otherwise, the property's new value is applied directly to all affected 
		/// objects. In the current implementation, if the property is of type Generic, then you must specify this
		/// method.
		/// </param>
		public static bool DisplayPropertyWithSetManyButton(
			SerializedProperty property,
			GUIContent label,
			SerializedProperty affectedObjects,
			string buttonText = "Set All",
			float buttonNarrow = k_NarrowButtonWidth,
			float buttonWide = k_NarrowButtonWidth,
			TestMassPropertyEqualityCallback isMassPropertyEqualToAffectedProperties = null,
			TestPropertyEqualityCallback areAffectedPropertiesEqual = null,
			SetSerializedPropertyValueCallback onSetProperty = null
		)
		{
			Rect position = EditorGUILayout.GetControlRect(label != null, EditorGUI.GetPropertyHeight(property));
			return DisplayPropertyWithSetManyButton(
				position, property, label, affectedObjects, buttonText,
				buttonNarrow, buttonWide,
				isMassPropertyEqualToAffectedProperties, areAffectedPropertiesEqual, onSetProperty
			);
		}

		/// <summary>
		/// Displays a property field with a button to apply its value to many items, if at least one item has a
		/// different value for the property of interest.
		/// </summary>
		/// <returns><see langword="true"/> if the button was pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="property">The <see cref="UnityEditor.SerializedProperty"/> used to set others.</param>
		/// <param name="label">Label.</param>
		/// <param name="affectedObjects">
		/// The <see cref="UnityEditor.SerializedProperty"/> for all objects affected by changes to the property.
		/// </param>
		/// <param name="buttonNarrow">Button width to use in narrow mode.</param>
		/// <param name="buttonWide">Button width to use in wide mode.</param>
		/// <param name="isMassPropertyEqualToAffectedProperties">
		/// If not <see langword="null"/> then specifies a method for testing equality of the values of the property and
		/// a potentially matching affected object; otherwise, the method simply compares their hash codes and, if they
		/// are enums, their type string as well.
		/// </param>
		/// <param name="areAffectedPropertiesEqual">
		/// If not <see langword="null"/> then specifies a method for testing the equality of the values on the affected
		/// objects. Otherwise, <see cref="UnityEditor.SerializedProperty.hasMultipleDifferentValues"/> will be used. 
		/// </param>
		/// <param name="onSetProperty">
		/// If not <see langword="null"/> then specifies a method to pass the new property value to, when the property
		/// changes or the button is clicked; otherwise, the property's new value is applied directly to all affected 
		/// objects. In the current implementation, if the property is of type Generic, then you must specify this
		/// method.
		/// </param>
		public static bool DisplayPropertyWithSetManyButton(
			Rect position,
			SerializedProperty property,
			GUIContent label,
			SerializedProperty affectedObjects,
			string buttonText = "Set All",
			float buttonNarrow = k_NarrowButtonWidth,
			float buttonWide = k_NarrowButtonWidth,
			TestMassPropertyEqualityCallback isMassPropertyEqualToAffectedProperties = null,
			TestPropertyEqualityCallback areAffectedPropertiesEqual = null,
			SetSerializedPropertyValueCallback onSetProperty = null
		)
		{
			using (ListPool<SerializedProperty>.Scope affected = new ListPool<SerializedProperty>.Scope())
			{
				affected.List.Add(affectedObjects);
				return DisplayPropertyWithSetManyButton(
					position, property, label, affected.List, buttonText,
					buttonNarrow, buttonWide,
					isMassPropertyEqualToAffectedProperties, areAffectedPropertiesEqual, onSetProperty
				);
			}
		}

		/// <summary>
		/// Displays a property field with a button to apply its value to many items, if at least one item has a
		/// different value for the property of interest.
		/// </summary>
		/// <returns><see langword="true"/> if the button was pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="property">The <see cref="UnityEditor.SerializedProperty"/> used to set others.</param>
		/// <param name="label">Label.</param>
		/// <param name="affectedObjects">
		/// A list of <see cref="UnityEditor.SerializedProperty"/> for all objects affected by changes to the property.
		/// </param>
		/// <param name="buttonNarrow">Button width to use in narrow mode.</param>
		/// <param name="buttonWide">Button width to use in wide mode.</param>
		/// <param name="isMassPropertyEqualToAffectedProperties">
		/// If not <see langword="null"/> then specifies a method for testing equality of the values of the property and
		/// a potentially matching affected object; otherwise, the method simply compares their hash codes and, if they
		/// are enums, their type string as well.
		/// </param>
		/// <param name="areAffectedPropertiesEqual">
		/// If not <see langword="null"/> then specifies a method for testing the equality of the values on the affected
		/// objects. Otherwise, <see cref="UnityEditor.SerializedProperty.hasMultipleDifferentValues"/> will be used. 
		/// </param>
		/// <param name="onSetProperty">
		/// If not <see langword="null"/> then specifies a method to pass the new property value to, when the property
		/// changes or the button is clicked; otherwise, the property's new value is applied directly to all affected 
		/// objects. In the current implementation, if the property is of type Generic, then you must specify this
		/// method.
		/// </param>
		public static bool DisplayPropertyWithSetManyButton(
			SerializedProperty property,
			GUIContent label,
			IList<SerializedProperty> affectedObjects,
			string buttonText = "Set All",
			float buttonNarrow = k_NarrowButtonWidth,
			float buttonWide = k_NarrowButtonWidth,
			TestMassPropertyEqualityCallback isMassPropertyEqualToAffectedProperties = null,
			TestPropertyEqualityCallback areAffectedPropertiesEqual = null,
			SetSerializedPropertyValueCallback onSetProperty = null
		)
		{
			Rect position = EditorGUILayout.GetControlRect(label != null, EditorGUI.GetPropertyHeight(property));
			return DisplayPropertyWithSetManyButton(
				position, property, label, affectedObjects, buttonText,
				buttonNarrow, buttonWide,
				isMassPropertyEqualToAffectedProperties, areAffectedPropertiesEqual, onSetProperty
			);
		}

		/// <summary>
		/// Displays a property field with a button to apply its value to many items, if at least one item has a
		/// different value for the property of interest.
		/// </summary>
		/// <returns><see langword="true"/> if the button was pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="property">The <see cref="UnityEditor.SerializedProperty"/> used to set others.</param>
		/// <param name="label">Label.</param>
		/// <param name="affectedObjects">
		/// A list of <see cref="UnityEditor.SerializedProperty"/> for all objects affected by changes to the property.
		/// </param>
		/// <param name="buttonNarrow">Button width to use in narrow mode.</param>
		/// <param name="buttonWide">Button width to use in wide mode.</param>
		/// <param name="isMassPropertyEqualToAffectedProperties">
		/// If not <see langword="null"/> then specifies a method for testing equality of the values of the property and
		/// a potentially matching affected object; otherwise, the method simply compares their hash codes and, if they
		/// are enums, their type string as well.
		/// </param>
		/// <param name="areAffectedPropertiesEqual">
		/// If not <see langword="null"/> then specifies a method for testing the equality of the values on the affected
		/// objects. Otherwise, <see cref="UnityEditor.SerializedProperty.hasMultipleDifferentValues"/> will be used. 
		/// </param>
		/// <param name="onSetProperty">
		/// If not <see langword="null"/> then specifies a method to pass the new property value to, when the property
		/// changes or the button is clicked; otherwise, the property's new value is applied directly to all affected 
		/// objects. In the current implementation, if the property is of type Generic, then you must specify this
		/// method.
		/// </param>
		public static bool DisplayPropertyWithSetManyButton(
			Rect position,
			SerializedProperty property,
			GUIContent label,
			IList<SerializedProperty> affectedObjects,
			string buttonText = "Set All",
			float buttonNarrow = k_NarrowButtonWidth,
			float buttonWide = k_NarrowButtonWidth,
			TestMassPropertyEqualityCallback isMassPropertyEqualToAffectedProperties = null,
			TestPropertyEqualityCallback areAffectedPropertiesEqual = null,
			SetSerializedPropertyValueCallback onSetProperty = null
		)
		{
			// determine if they all match (and hence whether button should be displayed)
			bool doAllMatch = affectedObjects == null || affectedObjects.Count < 1;
			if (!doAllMatch)
			{
				if (areAffectedPropertiesEqual != null)
				{
					doAllMatch = affectedObjects.All(p => areAffectedPropertiesEqual(p));
				}
				else
				{
					doAllMatch = affectedObjects.All(p => !p.hasMultipleDifferentValues);
				}
				if (doAllMatch)
				{
					if (isMassPropertyEqualToAffectedProperties != null)
					{
						doAllMatch &= isMassPropertyEqualToAffectedProperties(property, affectedObjects[0]);
					}
					else
					{
						bool isEnum = property.propertyType == SerializedPropertyType.Enum;
						doAllMatch &= affectedObjects.All(
							p => p.propertyType == property.propertyType && (isEnum ? p.type == property.type : true)
						);
						object propertyValue = property.GetValue(true);
						object compareValue = affectedObjects[0].GetValue(true);
						doAllMatch &= propertyValue == null ?
							compareValue == null :
							(compareValue == null ? false : propertyValue.GetHashCode() == compareValue.GetHashCode());
					}
				}
			}
			Rect controlPosition, buttonPosition;
			if (!doAllMatch)
			{
				GetRectsForControlWithInlineButton(
					position, out controlPosition, out buttonPosition, buttonNarrow, buttonWide
				);
			}
			else
			{
				controlPosition = position;
				buttonPosition = new Rect();
			}
			// display property field
			EditorGUI.BeginChangeCheck();
			{
				// wrap in Begin/EndProperty() so context menu works on label
				EditorGUI.BeginProperty(controlPosition, label, property);
				DisplayPropertyField(controlPosition, property, true, label);
				EditorGUI.EndProperty();
			}
			bool didChange = EditorGUI.EndChangeCheck();
			// display button if not all values match
			bool didClick = false;
			if (!doAllMatch)
			{
				didClick = DisplayButton(buttonPosition, buttonText);
			}
			// apply changes to all affected objects
			if (didChange || didClick)
			{
				property.serializedObject.ApplyModifiedProperties();
				if (onSetProperty != null)
				{
					using (ListPool<Object>.Scope undoObjects = new ListPool<Object>.Scope())
					{
						undoObjects.List.AddRange(property.serializedObject.targetObjects.Where(o => o != null));
						for (int i = 0; i < affectedObjects.Count; ++i)
						{
							undoObjects.List.AddRange(
								affectedObjects[i].serializedObject.targetObjects.Where(o => o != null)
							);
						}
						Undo.RecordObjects(
							undoObjects.List.ToArray(),
							string.Format("{0} {1}", buttonText, buttonText == "Set All" ? property.displayName : "")
						);
						onSetProperty(property);
						EditorUtilityX.SetDirty(undoObjects.List);
					}
				}
				else
				{
					object propertyValue = property.GetValue(true);
					for (int i = 0; i < affectedObjects.Count; ++i)
					{
						affectedObjects[i].SetValue(propertyValue);
						affectedObjects[i].serializedObject.ApplyModifiedProperties();
					}
				}
			}
			return didClick;
		}

		/// <summary>
		/// Displays a property field with a status icon using EditorGUILayout.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <param name="status">Status.</param>
		/// <param name="label">Label.</param>
		/// <param name="includeChildren">If set to <see langword="true"/> include children.</param>
		/// <param name="statusTooltip">Status tooltip.</param>
		public static void DisplayPropertyFieldWithStatus(
			SerializedProperty property, ValidationStatus status,
			GUIContent label = null, bool includeChildren = true, string statusTooltip = ""
		)
		{
			DisplayPropertyFieldWithStatus(
				EditorGUILayout.GetControlRect(), property, status, label, includeChildren, statusTooltip
			);
		}

		/// <summary>
		/// Displays a property field with a status icon.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="status">Status.</param>
		/// <param name="label">Label.</param>
		/// <param name="includeChildren">If set to <see langword="true"/> include children.</param>
		/// <param name="statusTooltip">Status tooltip.</param>
		public static void DisplayPropertyFieldWithStatus(
			Rect position, SerializedProperty property, ValidationStatus status,
			GUIContent label = null, bool includeChildren = true, string statusTooltip = ""
		)
		{
			if (status != ValidationStatus.None)
			{
				position.width -= EditorGUIUtility.singleLineHeight;
			}
			label = label ?? new GUIContent(property.displayName);
			bool hasTooltip = !string.IsNullOrEmpty(label.tooltip);
			if (!hasTooltip)
			{
				label.tooltip = statusTooltip;
			}
			EditorGUI.PropertyField(position, property, label, includeChildren);
			if (!hasTooltip)
			{
				label.tooltip = string.Empty;
			}
			position.x += position.width;
			position.width = position.height = EditorGUIUtility.singleLineHeight;
			if (status == ValidationStatus.None)
			{
				position.width = 0f;
			}
			DisplayValidationStatusIcon(position, status, statusTooltip);
		}
		
		/// <summary>
		/// Displays a property group.
		/// </summary>
		/// <returns><see langword="true"/> if the group is expanded; otherwise, <see langword="false"/>.</returns>
		/// <param name="label">Label for the group.</param>
		/// <param name="foldout">Foldout preference.</param>
		/// <param name="contents">Method to draw the contents of the group.</param>
		/// <typeparam name="TEditor">The editor type.</typeparam>
		public static bool DisplayPropertyGroup<TEditor>(
			string label, EditorPreference<bool, TEditor> foldout, System.Action contents
		)
		{
			s_ReusableLabel.text = label;
			s_ReusableLabel.image = null;
			s_ReusableLabel.tooltip = null;
			return DisplayPropertyGroup(s_ReusableLabel, foldout, contents);
		}
		
		/// <summary>
		/// Displays a property group.
		/// </summary>
		/// <returns><see langword="true"/> if the group is expanded; otherwise, <see langword="false"/>.</returns>
		/// <param name="label">Label for the group.</param>
		/// <param name="foldout">Foldout preference.</param>
		/// <param name="contents">Method to draw the contents of the group.</param>
		/// <typeparam name="TEditor">The editor type.</typeparam>
		public static bool DisplayPropertyGroup<TEditor>(
			GUIContent label, EditorPreference<bool, TEditor> foldout, System.Action contents
		)
		{
			foldout.CurrentValue = EditorGUILayout.Foldout(foldout.CurrentValue, label);
			if (foldout.CurrentValue)
			{
				int indent = EditorGUI.indentLevel;
				GUILayout.BeginHorizontal();
				{
					Rect position = EditorGUI.IndentedRect(new Rect());
					GUILayout.Space(position.x);
					EditorGUI.indentLevel = 0;
					EditorGUILayout.BeginVertical("box");
					{
						contents.Invoke();
					}
					EditorGUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				EditorGUI.indentLevel = indent;
			}
			return foldout.CurrentValue;
		}

		/// <summary>
		/// Displays a property with a toggle. Use this method for e.g., override properties that only take effect when
		/// the user explicitly enabled them.
		/// </summary>
		/// <returns><see langword="true"/> if the property is enabled; otherwise, <see langword="false"/>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="toggle">Property specifying whether or not the value will be used.</param>
		/// <param name="property">Property specifying the value.</param>
		public static bool DisplayPropertyWithToggle(
			Rect position, GUIContent label, SerializedProperty toggle, SerializedProperty property
		)
		{
			float totalWidth = position.width;
			position.width = EditorGUIUtility.labelWidth + 14f + StandardHorizontalSpacing;
			Rect togglePosition = position;
			togglePosition.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(togglePosition, toggle, label);
			EditorGUI.BeginDisabledGroup(!toggle.boolValue);
			{
				int indent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				if (property.propertyType != SerializedPropertyType.Generic) // NOTE: quick fix assuming FlushChildrenAttribute
				{
					position.x += position.width;
					position.width = totalWidth - position.width;
				}
				else
				{
					position.width = totalWidth;
				}
				EditorGUI.PropertyField(position, property, GUIContent.none);
				EditorGUI.indentLevel = indent;
			}
			EditorGUI.EndDisabledGroup();
			return toggle.boolValue;
		}
		
		/// <summary>
		/// Displays the scene GUI toggle.
		/// </summary>
		public static void DisplaySceneGUIToggle()
		{
			SceneGUI.IsEnabled = DisplayOnOffToggle("Scene GUI", SceneGUI.IsEnabled);
		}

		/// <summary>
		/// Displays a property field for a scriptable object with a button alongside to create a new instance or select
		/// the assigned instance.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if a new instance was created; otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		/// <typeparam name="T">The concrete type associated with this property.</typeparam>
		public static bool DisplayScriptableObjectPropertyFieldWithButton<T>(
			SerializedProperty property, GUIContent label = null
		) where T: ScriptableObject
		{
			return DisplayScriptableObjectPropertyFieldWithButton<T>(
				EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(property)), property, label
			);
		}

		/// <summary>
		/// Displays a property field for a scriptable object with a button alongside to create a new instance or select
		/// the assigned instance.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if a new instance was created; otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		/// <typeparam name="T">The concrete type associated with this property.</typeparam>
		public static bool DisplayScriptableObjectPropertyFieldWithButton<T>(
			Rect position, SerializedProperty property, GUIContent label = null
		) where T: ScriptableObject
		{
			bool didCreateNewAsset = false;
			if (property.propertyType != SerializedPropertyType.ObjectReference)
			{
				DisplayPropertyField(position, property, true);
			}
			else
			{
				Rect controlPosition, buttonPosition;
				GetRectsForControlWithInlineButton(
					position,
					out controlPosition,
					out buttonPosition,
					Mathf.Min(NarrowInlineButtonWidth, (position.width - EditorGUIUtility.labelWidth) * 0.5f),
					WideInlineButtonWidth
				);
				DisplayPropertyField(controlPosition, property, false, label);
				if (property.objectReferenceValue != null)
				{
					if (DisplayButton(buttonPosition, "Select"))
					{
						Selection.objects = new Object[] { property.objectReferenceValue };
					}
				}
				else if (
					DisplayButton(
						buttonPosition, buttonPosition.width > NarrowInlineButtonWidth ? "Create New" : "Create"
					)
				)
				{
					Object[] selection = Selection.objects;
					ScriptableObject newObject = AssetDatabaseX.CreateNewAssetInUserSpecifiedPath<T>();
					property.objectReferenceValue = newObject;
					Selection.objects = selection;
					didCreateNewAsset = true;
				}
			}
			return didCreateNewAsset;
		}

		/// <summary>
		/// Displays a slider whose type-in field allows values outside the slider range.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="property">Property.</param>
		/// <param name="sliderMin">Slider minimum.</param>
		/// <param name="sliderMax">Slider maximum.</param>
		public static void DisplaySoftSlider(
			Rect position, GUIContent label, SerializedProperty property, float sliderMin, float sliderMax
		)
		{
			switch (property.propertyType)
			{
			case SerializedPropertyType.Float:
				label = EditorGUI.BeginProperty(position, label, property);
				{
					float floatValue = property.floatValue;
					EditorGUI.BeginChangeCheck();
					{
						floatValue = DisplaySoftSlider(position, label, floatValue, sliderMin, sliderMax);
					}
					if (EditorGUI.EndChangeCheck())
					{
						property.floatValue = floatValue;
					}
				}
				EditorGUI.EndProperty();
				break;
			case SerializedPropertyType.Integer:
				label = EditorGUI.BeginProperty(position, label, property);
				{
					int intValue = property.intValue;
					EditorGUI.BeginChangeCheck();
					{
						intValue = DisplaySoftSlider(position, label, intValue, (int)sliderMin, (int)sliderMax);
					}
					if (EditorGUI.EndChangeCheck())
					{
						property.intValue = intValue;
					}
				}
				EditorGUI.EndProperty();
				break;
			default:
				DisplayPropertyFieldWithStatus(
					position,
					property,
					ValidationStatus.Error,
					label,
					true,
					string.Format(
						"Property must be of type {0} or {1} to use soft slider.",
						SerializedPropertyType.Float, SerializedPropertyType.Integer
					)
				);
				break;
			}
		}

		/// <summary>
		/// Displays a slider whose type-in field allows values outside the slider range.
		/// </summary>
		/// <returns>The soft slider.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Current slider value.</param>
		/// <param name="sliderMin">Slider minimum.</param>
		/// <param name="sliderMax">Slider maximum.</param>
		public static float DisplaySoftSlider(
			Rect position, GUIContent label, float value, float sliderMin, float sliderMax
		)
		{
			return DisplaySoftSlider(position, label, value, sliderMin, sliderMax, false);
		}

		/// <summary>
		/// Displays a slider whose type-in field allows values outside the slider range.
		/// </summary>
		/// <returns>The soft slider.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Current slider value.</param>
		/// <param name="sliderMin">Slider minimum.</param>
		/// <param name="sliderMax">Slider maximum.</param>
		public static int DisplaySoftSlider(Rect position, GUIContent label, int value, int sliderMin, int sliderMax)
		{
			return Mathf.RoundToInt(
				DisplaySoftSlider(position, label, (float)value, (float)sliderMin, (float)sliderMax)
			);
		}

		/// <summary>
		/// Displays a slider whose type-in field allows values outside the slider range.
		/// </summary>
		/// <returns>The soft slider.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Current slider value.</param>
		/// <param name="sliderMin">Slider minimum.</param>
		/// <param name="sliderMax">Slider maximum.</param>
		/// <param name="formatString">Format string for input field.</param>
		private static float DisplaySoftSlider(
			Rect position, GUIContent label, float value, float sliderMin, float sliderMax, bool isInt
		)
		{
			// modified from EditorGUI.DoSlider()
			if (position.width >= 65f + EditorGUIUtility.fieldWidth)
			{
				int id = GUIUtility.GetControlID(s_SliderHash, EditorGUIUtility.native, position);
				position = EditorGUI.PrefixLabel(position, id, label);
				position.width -= 5f + EditorGUIUtility.fieldWidth;
				EditorGUI.BeginChangeCheck();
				{
					value = GUI.Slider(
						position,
						value,
						0f,
						sliderMin,
						sliderMax,
						GUI.skin.horizontalSlider,
						(!EditorGUI.showMixedValue) ? GUI.skin.horizontalSliderThumb : "SliderMixed", true, id
					);
					if (GUIUtility.hotControl == id)
					{
						GUIUtility.keyboardControl = id;
					}
					if (
						GUIUtility.keyboardControl == id &&
						Event.current.type == EventType.KeyDown &&
						(Event.current.keyCode == KeyCode.LeftArrow || Event.current.keyCode == KeyCode.RightArrow)
					)
					{
						s_Param1[0] = Mathf.Abs((sliderMax - sliderMin) * 0.01f);
						float step = (float)s_GetClosestPowerOfTen.Invoke(null, s_Param1);
						if (isInt && step < 1f)
						{
							step = 1f;
						}
						if (Event.current.shift)
						{
							step *= 10f;
						}
						if (Event.current.keyCode == KeyCode.LeftArrow)
						{
							value -= step * 0.5001f;
						}
						else
						{
							value += step * 0.5001f;
						}
						s_Param2[0] = value;
						s_Param2[1] = step;
						value = (float)s_RoundToMultipleOf.Invoke(null, s_Param2);
						GUI.changed = true;
						Event.current.Use();
					}
				}
				if (EditorGUI.EndChangeCheck())
				{
					float f = (sliderMax - sliderMin) / (
						position.width -
						(float)GUI.skin.horizontalSlider.padding.horizontal -
						GUI.skin.horizontalSliderThumb.fixedWidth
					);
					s_Param2[0] = value;
					s_Param2[1] = Mathf.Abs(f);
					value = (float)s_RoundBasedOnMinimumDifference.Invoke(null, s_Param2);
					value = Mathf.Clamp(value, sliderMin, sliderMax);
				}
				position.x += position.width + 5f;
				position.width = EditorGUIUtility.fieldWidth;
				int indent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				value = isInt ? (float)EditorGUI.IntField(position, (int)value) : EditorGUI.FloatField(position, value);
				EditorGUI.indentLevel = indent;
			}
			else
			{
				position.width = Mathf.Min(EditorGUIUtility.fieldWidth, position.width);
				position.x = position.xMax - position.width;
				value = EditorGUI.FloatField(position, label, value);
			}
			return value;
		}

		/// <summary>
		/// Displays a title bar.
		/// </summary>
		/// <param name="label">Label.</param>
		public static void DisplayTitleBar(string label)
		{
			EditorGUILayout.LabelField(label, EditorStylesX.BoldTitleBar);
			GUILayoutUtility.GetRect(0f, EditorGUIUtility.standardVerticalSpacing);
		}
		
		/// <summary>
		/// Displays a validation status icon.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="status">Status.</param>
		/// <param name="tooltip">Tooltip (optional).</param>
		public static void DisplayValidationStatusIcon(Rect position, ValidationStatus status, string tooltip = "")
		{
			s_ValidationStatusIcon.image = null;
			s_ValidationStatusIcon.text = string.Empty;
			s_ValidationStatusIcon.tooltip = tooltip;
			switch (status)
			{
			case ValidationStatus.Error:
				s_ValidationStatusIcon.image = EditorStylesX.ErrorIcon;
				break;
			case ValidationStatus.Info:
				s_ValidationStatusIcon.image = EditorStylesX.InfoIcon;
				break;
			case ValidationStatus.None:
				// display empty icon to prevent control IDs changing when status icons dynamically update
				if (s_NoStatusIcon == null)
				{
					s_NoStatusIcon = new Texture2D(0, 0);
					s_NoStatusIcon.hideFlags = HideFlags.HideAndDontSave;
				}
				s_ValidationStatusIcon.image = s_NoStatusIcon;
				break;
			case ValidationStatus.Okay:
				s_ValidationStatusIcon.image = EditorStylesX.OkayIcon;
				break;
			case ValidationStatus.Warning:
				s_ValidationStatusIcon.image = EditorStylesX.WarningIcon;
				break;
			}
			if (s_ValidationStatusIcon.image != null)
			{
				GUI.Box(
					position,
					s_ValidationStatusIcon,
					status == ValidationStatus.Okay ? EditorStylesX.OkayStatusIconStyle : EditorStylesX.StatusIconStyle
				);
			}
		}
		
		/// <summary>
		/// Ends the scene GUI controls area.
		/// </summary>
		public static void EndSceneGUIControlsArea()
		{
			--EditorGUI.indentLevel;
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// Gets rects for a button and a status icon next to it.
		/// </summary>
		/// <param name="buttonRect">Button rect.</param>
		/// <param name="iconRect">Icon rect.</param>
		public static void GetButtonAndStatusRects(out Rect buttonRect, out Rect iconRect)
		{
			buttonRect = EditorGUI.IndentedRect(
				GUILayoutUtility.GetRect(0f, EditorGUIX.InlineButtonHeight + EditorGUIUtility.standardVerticalSpacing)
			);
			buttonRect.width -= EditorGUIUtility.singleLineHeight;
			buttonRect.height -= EditorGUIUtility.standardVerticalSpacing;
			iconRect = buttonRect;
			iconRect.x += buttonRect.width;
			iconRect.y += 0.5f * (EditorGUIX.InlineButtonHeight - EditorGUIUtility.singleLineHeight);
			iconRect.width = EditorGUIUtility.singleLineHeight;
		}
		
		/// <summary>
		/// Gets the color from Unity preferences.
		/// </summary>
		/// <remarks>
		/// Unity editor colors use their own, exotic serialization scheme.
		/// </remarks>
		/// <returns>The color from Unity preferences.</returns>
		/// <param name="propName">Property name.</param>
		/// <param name="defaultColor">Default color.</param>
		private static Color GetColorFromUnityPreferences(string propName, Color defaultColor)
		{
			string s = EditorPrefs.GetString(propName, "");
			if (string.IsNullOrEmpty(s))
			{
				return defaultColor;
			}
			string[] values = s.Split(';');
			try
			{
				return new Color(
					float.Parse(values[values.Length-4]),
					float.Parse(values[values.Length-3]),
					float.Parse(values[values.Length-2]),
					float.Parse(values[values.Length-1])
				);
			}
			catch (System.Exception)
			{
				return defaultColor;
			}
		}

		/// <summary>
		/// Gets a rect for an inline icon and reduces the size of the specific control rect to make room for it.
		/// </summary>
		/// <param name="controlRect">Control rect.</param>
		/// <param name="iconRect">Icon rect.</param>
		public static void GetInlineIconRect(ref Rect controlRect, out Rect iconRect)
		{
			controlRect.width -= EditorGUIUtility.singleLineHeight;
			iconRect = controlRect;
			iconRect.x += iconRect.width;
			iconRect.width = iconRect.height = EditorGUIUtility.singleLineHeight;
		}
		
		/// <summary>
		/// A wrapper for EditorGUI.Slider() that matches the method signature requirement of EditorGUIX.DisplayField().
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Value.</param>
		private static float OnFloatSliderField(Rect position, GUIContent label, float value)
		{
			return EditorGUI.Slider(position, label, value, s_SliderMin, s_SliderMax);
		}
	}
}