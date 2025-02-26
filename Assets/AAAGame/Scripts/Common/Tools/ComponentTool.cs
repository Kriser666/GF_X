using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class ComponentTool
{
    /// <summary>
    /// 复制组件到游戏物体上，必须要有[Serializable]的字段
    /// </summary>
    /// <typeparam name="T">组件</typeparam>
    /// <param name="source">原始组件</param>
    /// <param name="destination">目标</param>
    /// <param name="removeFirst">是否先移除原有的T组件</param>
    /// <returns></returns>
    public static T CloneComponentBySerializable<T>(T source, GameObject destination, bool removeFirst = false) where T : Component
    {
        if (removeFirst)
            RemoveComponent<T>(destination);
        // 序列化为JSON
        string json = JsonConvert.SerializeObject(source);
        // 添加新组件并反序列化
        T newComponent = destination.AddComponent<T>();
        JsonUtility.FromJsonOverwrite(json, newComponent);

        return newComponent;
    }
    public static T CloneComponentByReflection<T>(T source, GameObject destination, bool removeFirst = false) where T : Component
    {
        if (removeFirst)
            RemoveComponent<T>(destination);
        T newCom = destination.AddComponent<T>();
        CopyComponentValues(source, newCom);
        return newCom;
    }
    public static void CopyComponentValues<T>(T source, T target) where T : Component
    {
        // 获取组件类型
        Type type = typeof(T);

        // 复制字段
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            try
            {
                field.SetValue(target, field.GetValue(source));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Field {field.Name} 复制失败: {e.Message}");
            }
        }

        // 复制属性（跳过索引器、静态属性、无setter的属性）
        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanWrite || prop.GetIndexParameters().Length > 0 || prop.Name == "name")
                continue;

            try
            {
                prop.SetValue(target, prop.GetValue(source));
            }
            catch (TargetException)
            {
                // 特殊处理需要实例目标的属性（如某些Unity内部属性）
                Debug.LogWarning($"属性 {prop.Name} 需要特殊处理，已跳过");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"属性 {prop.Name} 复制失败: {e.Message}");
            }
        }
    }

    public static void RemoveComponent<T>(GameObject gameObject) where T : Component
    {
        if (gameObject.TryGetComponent(out T component))
        {
            UnityEngine.Object.Destroy(component);
        }
    }
    public static void RemoveAllComponentsFromGameObject(GameObject whichObject)
    {
        List<Component> components = new (whichObject.GetComponents<Component>());

        foreach (Component component in components)
        {
            if (component is not Transform) // 保留Transform组件
            {
                UnityEngine.Object.Destroy(component);
            }
        }
    }
}
