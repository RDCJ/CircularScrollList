using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SCL;
using System;
using System.Reflection;

[CustomEditor(typeof(CircularScrollList))]
public class CircularScrollListEditor : Editor
{
    public delegate bool PropertyShowDelegate();
    class SerializedPropertyExtend
    {
        public SerializedProperty property;
        public Type property_type;
        public PropertyShowDelegate show_condition;

        public SerializedPropertyExtend(SerializedProperty property, Type property_type, PropertyShowDelegate show_condition)
        {
            this.property = property;
            this.property_type = property_type;
            this.show_condition = show_condition;
        }
    }

    private CircularScrollList scrollList;

    private List<SerializedPropertyExtend> properties;

    private void OnEnable()
    {
        scrollList = (CircularScrollList)target;
        properties = new List<SerializedPropertyExtend>();
        RegisterSerializedProperty("scrollType");
        RegisterSerializedProperty("Reverse");
        RegisterSerializedProperty("SiblingOrderReverse");
        RegisterSerializedProperty("Column", ()=> scrollList.scrollType == CircularScrollList.ScrollType.Vertical);
        RegisterSerializedProperty("Row", () => scrollList.scrollType == CircularScrollList.ScrollType.Horizontal);
        RegisterSerializedProperty("UsePrefabSize", () => scrollList.AutoFitCellSize == false);
        RegisterSerializedProperty("AutoFitCellSize", () => scrollList.UsePrefabSize == false);
        RegisterSerializedProperty("cellSize", () => !scrollList.UsePrefabSize);
        RegisterSerializedProperty("Space");
        RegisterSerializedProperty("elementCount");
        RegisterSerializedProperty("ElementPrefab");
        RegisterSerializedProperty("dataBank");
        RegisterSerializedProperty("enable_curve");
        RegisterSerializedProperty("position_offset_curve", ()=>scrollList.enable_curve);
        RegisterSerializedProperty("scale_curve", () => scrollList.enable_curve);
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        foreach (var property in properties)
        {
            if (property.show_condition == null || property.show_condition())
                EditorGUILayout.PropertyField(property.property);
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Refresh Grid"))
        {
            scrollList.ClearGrid();
            scrollList.RefreshGrid();
            scrollList.ScrollToElement(0, 0);
        }

        if (GUILayout.Button("Clear Grid"))
        {
            scrollList.ClearGrid();
        }
    }

    /// <summary>
    /// 注册在inspector中编辑的字段
    /// </summary>
    /// <param name="property_name">字段名</param>
    /// <param name="show_condition">展示条件</param>
    private void RegisterSerializedProperty(string property_name, PropertyShowDelegate show_condition = null)
    {
        SerializedProperty property = serializedObject.FindProperty(property_name);
        if (property == null)
        {
            Debug.Log($"[CircularScrollListEditor.RegisterSerializedProperty] property doesn't exist: {property_name}");
            return;
        }
        FieldInfo field = typeof(CircularScrollList).GetField(property_name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Type type = field.FieldType;
        properties.Add(
            new SerializedPropertyExtend(
                property, type, show_condition
                )
            );
    }
}
