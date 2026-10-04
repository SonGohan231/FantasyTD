#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FantasyTD.CI
{
    internal static class GameViewResolutionHelper
    {
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        internal static bool SetFixedResolution(int width, int height, string label, out string details)
        {
            try
            {
                Assembly editorAssembly = typeof(EditorWindow).Assembly;
                Type gameViewType = editorAssembly.GetType("UnityEditor.GameView");
                Type sizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
                Type sizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
                Type sizeKindType = editorAssembly.GetType("UnityEditor.GameViewSizeType");
                Type groupType = editorAssembly.GetType("UnityEditor.GameViewSizeGroupType");

                if (gameViewType == null || sizesType == null || sizeType == null ||
                    sizeKindType == null || groupType == null)
                {
                    details = "UnityEditor GameView reflection types unavailable.";
                    return false;
                }

                Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                PropertyInfo instanceProperty = singletonType.GetProperty("instance", AnyStatic);
                object sizes = instanceProperty != null ? instanceProperty.GetValue(null) : null;
                if (sizes == null)
                {
                    details = "GameViewSizes singleton unavailable.";
                    return false;
                }

                object standalone = Enum.Parse(groupType, "Standalone");
                MethodInfo getGroup = sizesType.GetMethod("GetGroup", AnyInstance);
                object group = getGroup != null ? getGroup.Invoke(sizes, new[] { standalone }) : null;
                if (group == null)
                {
                    details = "Standalone GameView size group unavailable.";
                    return false;
                }

                Type groupRuntimeType = group.GetType();
                MethodInfo getBuiltinCount = groupRuntimeType.GetMethod("GetBuiltinCount", AnyInstance);
                MethodInfo getCustomCount = groupRuntimeType.GetMethod("GetCustomCount", AnyInstance);
                MethodInfo getGameViewSize = groupRuntimeType.GetMethod("GetGameViewSize", AnyInstance);
                MethodInfo addCustomSize = groupRuntimeType.GetMethod("AddCustomSize", AnyInstance);

                if (getBuiltinCount == null || getCustomCount == null ||
                    getGameViewSize == null || addCustomSize == null)
                {
                    details = "GameView size group methods unavailable.";
                    return false;
                }

                int builtinCount = (int)getBuiltinCount.Invoke(group, null);
                int customCount = (int)getCustomCount.Invoke(group, null);
                int selectedIndex = -1;

                for (int i = 0; i < builtinCount + customCount; i++)
                {
                    object size = getGameViewSize.Invoke(group, new object[] { i });
                    if (size == null) continue;

                    int existingWidth = ReadIntMember(size, "width");
                    int existingHeight = ReadIntMember(size, "height");
                    if (existingWidth == width && existingHeight == height)
                    {
                        selectedIndex = i;
                        break;
                    }
                }

                if (selectedIndex < 0)
                {
                    object fixedResolution = Enum.Parse(sizeKindType, "FixedResolution");
                    ConstructorInfo constructor = sizeType.GetConstructor(
                        AnyInstance, null,
                        new[] { sizeKindType, typeof(int), typeof(int), typeof(string) },
                        null);
                    if (constructor == null)
                    {
                        details = "GameViewSize fixed-resolution constructor unavailable.";
                        return false;
                    }

                    object newSize = constructor.Invoke(new object[]
                    {
                        fixedResolution, width, height, label
                    });
                    addCustomSize.Invoke(group, new[] { newSize });

                    customCount = (int)getCustomCount.Invoke(group, null);
                    selectedIndex = builtinCount + customCount - 1;
                }

                EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
                if (gameView == null)
                {
                    details = "GameView window unavailable.";
                    return false;
                }

                PropertyInfo selectedProperty = gameViewType.GetProperty("selectedSizeIndex", AnyInstance);
                if (selectedProperty == null)
                {
                    details = "GameView.selectedSizeIndex unavailable.";
                    return false;
                }

                selectedProperty.SetValue(gameView, selectedIndex);
                gameView.Focus();
                gameView.Repaint();

                details = "selectedIndex=" + selectedIndex +
                          ",requested=" + width + "x" + height +
                          ",label=" + label;
                return true;
            }
            catch (Exception ex)
            {
                details = ex.GetType().Name + ": " + ex.Message;
                return false;
            }
        }

        private static int ReadIntMember(object instance, string name)
        {
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperty(name, AnyInstance);
            if (property != null && property.PropertyType == typeof(int))
                return (int)property.GetValue(instance);

            FieldInfo field = type.GetField(name, AnyInstance);
            if (field != null && field.FieldType == typeof(int))
                return (int)field.GetValue(instance);

            return -1;
        }
    }
}
#endif

