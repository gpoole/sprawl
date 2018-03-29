using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NodeFollow))]
public class NodeFollowEditor : Editor
{
    #region Reference to the script this custom editor was made for
    NodeFollow script;
    #endregion

    #region Reference to the EnemyAI Image
    private Texture2D nodeFollowLabel;
    #endregion

    #region Reference to the EnemyAI Font
    private Font nodeFollowFont;
    #endregion

    void OnEnable()
    {
        #region The target for custom editor
        script = (NodeFollow)target;
        #endregion

        #region Node Follow Image
        nodeFollowLabel = Resources.Load("NodeFollow Label") as Texture2D;
        #endregion

        #region Node Follow Font
        nodeFollowFont = Resources.Load("NodeFollow Font") as Font;
        #endregion
    }

    public override void OnInspectorGUI()
    {
        #region Update serialized object
        serializedObject.Update();
        #endregion

        #region Font Style "GUIStyle"
        var fontStyle = new GUIStyle();
        fontStyle.font = Resources.Load("NodeFollow Font") as Font;
        fontStyle.normal.textColor = new Color32(50, 50, 50, 255);
        fontStyle.fontSize = 12;
        #endregion

        #region Button Style "GUIStyle"
        var buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.font = Resources.Load("NodeFollow Font") as Font;
        buttonStyle.normal.textColor = new Color32(0, 0, 0, 255);
        buttonStyle.fontSize = 12;
        #endregion

        #region Background Box
        GUI.backgroundColor = script.backgroundColor;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        #endregion

        #region Moving object "Gameobject"
        GUILayout.Space(2);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Moving Object",fontStyle);
        GUILayout.FlexibleSpace();
        script.movingObject = (GameObject)EditorGUILayout.ObjectField(script.movingObject, typeof(GameObject), true, GUILayout.MinWidth(150), GUILayout.MaxWidth(150), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Moving towards node "Int"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Moving Towards Node: ", fontStyle);
        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(true);
        script.pointSelection = EditorGUILayout.IntField(script.pointSelection, GUILayout.MinWidth(25), GUILayout.MaxWidth(25), GUILayout.ExpandWidth(false));
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Speed "Float"
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Speed", fontStyle);
        GUILayout.FlexibleSpace();
        script.moveSpeed = EditorGUILayout.FloatField(script.moveSpeed, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Rotation speed "Float" 
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Rotation Speed", fontStyle);
        GUILayout.FlexibleSpace();
        script.rotationSpeed = EditorGUILayout.FloatField(script.rotationSpeed, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Stop time "Float"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Stop Time", fontStyle);
        GUILayout.FlexibleSpace();
        script.stopTime = EditorGUILayout.FloatField(script.stopTime, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Use trigger to start movement "Bool"
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Use Trigger To Start Movement", fontStyle);
        GUILayout.FlexibleSpace();
        script.useTriggerForStart = EditorGUILayout.Toggle(script.useTriggerForStart, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Move to start at end "Bool"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Move To Start At End", fontStyle);
        GUILayout.FlexibleSpace();
        script.moveToStartAtEnd = EditorGUILayout.Toggle(script.moveToStartAtEnd, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Loop movement "Bool"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Loop Movement", fontStyle);
        GUILayout.FlexibleSpace();
        script.loop = EditorGUILayout.Toggle(script.loop, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Rotate towards next node "Bool"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Rotate Towards Next Node", fontStyle);
        GUILayout.FlexibleSpace();
        script.rotateTowardsNextNode = EditorGUILayout.Toggle(script.rotateTowardsNextNode, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Moving object direction "Enum"
        if (script.rotateTowardsNextNode == true)
        {
            GUI.backgroundColor = Color.white;
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Moving Object Direction", fontStyle);
            GUILayout.FlexibleSpace();
            script.objectDirection = (NodeFollow.Direction)EditorGUILayout.EnumPopup(script.objectDirection, GUILayout.MinWidth(120), GUILayout.MaxWidth(120), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.backgroundColor = script.backgroundColor;
        }
        else if(script.rotateTowardsNextNode == false)
        {
            GUILayout.EndVertical();
        }
        #endregion

        #region Draw lines "Bool"
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Draw Lines", fontStyle);
        GUILayout.FlexibleSpace();
        script.drawLines = EditorGUILayout.Toggle(script.drawLines, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Draw dot lines "Bool"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Draw Dot Lines", fontStyle);
        GUILayout.FlexibleSpace();
        script.drawDotLine = EditorGUILayout.Toggle(script.drawDotLine, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Draw line from last to first "Bool"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Draw Line From Last To First", fontStyle);
        GUILayout.FlexibleSpace();
        script.drawLastToFirst = EditorGUILayout.Toggle(script.drawLastToFirst, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Show handles "Bool"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Show Handles", fontStyle);
        GUILayout.FlexibleSpace();
        script.showHandles = EditorGUILayout.Toggle(script.showHandles, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Handles size "Slider"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Handles Size", fontStyle);
        GUILayout.FlexibleSpace();
        script.handlesSize = EditorGUILayout.Slider(script.handlesSize, 0.1f, 5f, GUILayout.MinWidth(200), GUILayout.MaxWidth(200), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Line color "Enum"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Line Color", fontStyle);
        GUILayout.FlexibleSpace();
        script.HandlesColor = (NodeFollow.SetHandlesColor)EditorGUILayout.EnumPopup(script.HandlesColor, GUILayout.MinWidth(200), GUILayout.MaxWidth(200), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Background color "Color"
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Background color", fontStyle);
        GUILayout.FlexibleSpace();
        script.backgroundColor = EditorGUILayout.ColorField(script.backgroundColor, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Reset positions "Button"
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reset Positions", buttonStyle, GUILayout.Width(150), GUILayout.Height(25)) && script.nodes.Length != 0)
        {
            for (int i = 0; i < script.nodes.Length; i++)
            {
                script.nodes[i].Set(script.transform.position.x, script.transform.position.y + 2, script.transform.position.z);
            }
        }
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Node
        GUI.backgroundColor = Color.white;
        EditorList.Show(serializedObject.FindProperty("nodes"), true, true, "Node ", true);
        GUI.backgroundColor = script.backgroundColor;
        #endregion

        #region Stop at node 
        GUI.backgroundColor = Color.white;
        EditorList.Show(serializedObject.FindProperty("stopNodes"), true, true, "Stop At Node: ", false);
        GUILayout.EndVertical();
        GUILayout.EndVertical();
        #endregion

        #region Apply modifications to serialized object
        serializedObject.ApplyModifiedProperties();
        #endregion

        #region Undo
        Undo.RecordObject(target, "Node Follow Changes");
        #endregion
    }

    void OnSceneGUI()
    {
        #region Updating all node positions in case transform is moved
        if (script.nodes != null)
        {
            script.currentPosition = script.transform.position;

            if (script.currentPosition != script.lastPosition)
            {
                for (int i = 0; i < script.nodes.Length; i++)
                {
                    var differenceX = script.lastPosition.x - script.currentPosition.x;
                    var differenceY = script.lastPosition.y - script.currentPosition.y;
                    var differenceZ = script.lastPosition.z - script.currentPosition.z;

                    script.newX = script.nodes[i].x - differenceX;
                    script.newY = script.nodes[i].y - differenceY;
                    script.newZ = script.nodes[i].z - differenceZ;

                    script.nodes[i] = new Vector3(script.newX, script.newY, script.nodes[i].z);
                }

                script.lastPosition = script.currentPosition;
            }
        }
        #endregion

        #region Setting handles color and switching gizmo icon color to match handles color
        switch (script.HandlesColor)
        {
            case NodeFollow.SetHandlesColor.white:
                Handles.color = Color.white;
                script.useWhiteGizmo = true;
                script.useBlueGizmo = false;
                script.useRedGizmo = false;
                script.useGreenGizmo = false;
                script.useYellowGizmo = false;
                break;
            case NodeFollow.SetHandlesColor.blue:
                Handles.color = Color.blue;
                script.useWhiteGizmo = false;
                script.useBlueGizmo = true;
                script.useRedGizmo = false;
                script.useGreenGizmo = false;
                script.useYellowGizmo = false;
                break;
            case NodeFollow.SetHandlesColor.red:
                Handles.color = Color.red;
                script.useWhiteGizmo = false;
                script.useBlueGizmo = false;
                script.useRedGizmo = true;
                script.useGreenGizmo = false;
                script.useYellowGizmo = false;
                break;
            case NodeFollow.SetHandlesColor.green:
                Handles.color = Color.green;
                script.useWhiteGizmo = false;
                script.useBlueGizmo = false;
                script.useRedGizmo = false;
                script.useGreenGizmo = true;
                script.useYellowGizmo = false;
                break;
            case NodeFollow.SetHandlesColor.yellow:
                Handles.color = Color.yellow;
                script.useWhiteGizmo = false;
                script.useBlueGizmo = false;
                script.useRedGizmo = false;
                script.useGreenGizmo = false;
                script.useYellowGizmo = true;
                break;
        }
        #endregion

        #region Show handles
        if (script.showHandles == true && script.nodes != null)
        {
            for (int i = 0; i < script.nodes.Length; i++)
            {
                script.nodes[i] = Handles.FreeMoveHandle(script.nodes[i], Quaternion.identity, script.handlesSize, new Vector3(0.05f, 0.05f, 0), Handles.CircleHandleCap);
            }
        }
        #endregion

        #region Check if lines should be drawn
        if (script.drawLines == false || script.nodes == null)
        {
            return;
        }
        #endregion

        #region Label style
        for (int i = 0; i < script.nodes.Length - 1; i++)
        {
            var labelStyle = new GUIStyle();
            labelStyle.normal.background = nodeFollowLabel;
            labelStyle.font = nodeFollowFont;
            labelStyle.fontSize = 12;
            labelStyle.normal.textColor = Color.white;
            labelStyle.contentOffset = new Vector2(2, 0);

            Vector3 labelOffset = new Vector3(0, 1.75f); 

            Handles.Label(script.nodes[i] + labelOffset, " Node " + i + "  ", labelStyle);
            Handles.Label(script.nodes[i + 1] + labelOffset, " Node " + (i + 1) + "  ", labelStyle);
        }
        #endregion

        #region Line type selection
        if (script.drawDotLine == true)
        {
            for (int i = 0; i < script.nodes.Length - 1; i++)
            {
                Handles.DrawDottedLine(script.nodes[i], script.nodes[(int)Mathf.Repeat(i + 1, script.nodes.Length)], 20f);
            }
        }
        else
        {
            for (int i = 0; i < script.nodes.Length - 1; i++)
            {
                Handles.DrawLine(script.nodes[i], script.nodes[(int)Mathf.Repeat(i + 1, script.nodes.Length)]);
            }
        }
        #endregion

        #region Draw line from last node to the first
        if (script.drawDotLine == true)
        {
            if (script.drawLastToFirst == true && script.nodes.Length != 0)
            {
                for (int i = 0; i < script.nodes.Length - 1; i++)
                {
                    Handles.DrawDottedLine(script.nodes[script.nodes.Length - 1], script.nodes[0], 20f);
                }
            }
        }
        else
        {
            if (script.drawLastToFirst == true && script.nodes.Length != 0)
            {
                for (int i = 0; i < script.nodes.Length - 1; i++)
                {
                    Handles.DrawLine(script.nodes[script.nodes.Length - 1], script.nodes[0]);
                }
            }
        }
        #endregion

        #region Undo
        Undo.RecordObject(target, "Node Follow Changes");
        #endregion
    }
}