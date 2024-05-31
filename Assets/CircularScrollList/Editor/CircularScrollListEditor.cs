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
        RegisterSerializedProperty("Column", ()=> scrollList.scrollType == CircularScrollList.ScrollType.Vertical);
        RegisterSerializedProperty("Row", () => scrollList.scrollType == CircularScrollList.ScrollType.Horizontal);
        RegisterSerializedProperty("use_prefab_size");
        RegisterSerializedProperty("cellSize", () => !scrollList.use_prefab_size);
        RegisterSerializedProperty("Space");
        RegisterSerializedProperty("element_count");
        RegisterSerializedProperty("element_prefab");
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
    }

    /// <summary>
    /// ע����inspector�б༭���ֶ�
    /// </summary>
    /// <param name="property_name">�ֶ���</param>
    /// <param name="show_condition">չʾ����</param>
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