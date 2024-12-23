using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RSL;
using System;
using System.Reflection;

[CustomEditor(typeof(RecyclingScrollList))]
public class RecyclingScrollListEditor : Editor
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

    private RecyclingScrollList scrollList;

    private List<SerializedPropertyExtend> properties;

    private void OnEnable()
    {
        scrollList = (RecyclingScrollList)target;
        properties = new List<SerializedPropertyExtend>();
        RegisterSerializedProperty("scrollType");
        RegisterSerializedProperty("Reverse");
        RegisterSerializedProperty("SiblingOrderReverse");
        RegisterSerializedProperty("Column", ()=> scrollList.scrollType == RecyclingScrollList.ScrollType.Vertical);
        RegisterSerializedProperty("Row", () => scrollList.scrollType == RecyclingScrollList.ScrollType.Horizontal);
        RegisterSerializedProperty("UsePrefabSize", () => scrollList.AutoFitCellSize == false);
        RegisterSerializedProperty("AutoFitCellSize", () => scrollList.UsePrefabSize == false);
        RegisterSerializedProperty("cellSize", () => !scrollList.UsePrefabSize);
        RegisterSerializedProperty("Space");
        RegisterSerializedProperty("ElementPrefab");
        RegisterSerializedProperty("enableCurve");
        RegisterSerializedProperty("positionOffsetCurve", ()=>scrollList.enableCurve);
        RegisterSerializedProperty("scaleCurve", () => scrollList.enableCurve);
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
            scrollList.ReloadAll();
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
        FieldInfo field = typeof(RecyclingScrollList).GetField(property_name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Type type = field.FieldType;
        properties.Add(
            new SerializedPropertyExtend(
                property, type, show_condition
                )
            );
    }
}
