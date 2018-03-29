using UnityEditor;
using UnityEngine;

// This is for showing lists in custom editor at the way they are shown
public static class EditorList
{
    public static void Show(SerializedProperty list, bool showListSize = true, bool showListLabel = true, string elementName = "", bool showNumber = false, bool showButtons = true)
    {
        if (showListLabel)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(list);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel += 1;

        if (!showListLabel || list.isExpanded)
        {
            if (showListSize)
            {
                EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
            }
            for (int i = 0; i < list.arraySize; i++)
            {
                if (showButtons)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                string number;
                if (showNumber == true)
                {
                    number = (i).ToString();
                }
                else
                {
                    number = "";
                }
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent(elementName + (number)));

                if (showButtons)
                {
                    ShowButtons(list,i);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        if (showListLabel)
        {
            EditorGUI.indentLevel -= 1;
        }
    }

    private static GUIContent deleteButtonContent = new GUIContent("-");

    private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

    private static void ShowButtons(SerializedProperty list, int index)
    {
        if(GUILayout.Button(deleteButtonContent, EditorStyles.miniButton, miniButtonWidth))
        {
            int oldSize = list.arraySize;
            list.DeleteArrayElementAtIndex(index);
            if(list.arraySize == oldSize)
            {
                list.DeleteArrayElementAtIndex(index);
            }
        }      
    }
}