using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Prefab Painter allows you to paint prefabs in the scene
/// </summary>
[ExecuteInEditMode()]
[CanEditMultipleObjects]
[CustomEditor(typeof(PrefabPainter))]
public class PrefabPainterEditor : Editor
{
    private bool mousePosValid = false;
    private Vector3 mousePos;
    private PrefabPainter gizmo;

    public void OnEnable()
    {
        this.gizmo = target as PrefabPainter;

        if (this.gizmo.physicsSimulation == null)
        {
            this.gizmo.physicsSimulation = ScriptableObject.CreateInstance<PhysicsSimulation>();
        }
    }
    public override void OnInspectorGUI()
    {

        // draw default inspector elements
        DrawDefaultInspector();

        ///
        /// draw custom components
        /// 

        // separator
        addGUISeparator();

        EditorGUILayout.LabelField("Physics Settings");
        // EditorGUI.indentLevel++;

        this.gizmo.physicsSimulation.maxIterations = EditorGUILayout.IntField("Max Iterations", this.gizmo.physicsSimulation.maxIterations);
        this.gizmo.physicsSimulation.forceMinMax = EditorGUILayout.Vector2Field("Force Min/Max", this.gizmo.physicsSimulation.forceMinMax);
        this.gizmo.physicsSimulation.forceAngleInDegrees = EditorGUILayout.FloatField("Force Angle (Degrees)", this.gizmo.physicsSimulation.forceAngleInDegrees);
        this.gizmo.physicsSimulation.randomizeForceAngle = EditorGUILayout.Toggle("Randomize Force Angle", this.gizmo.physicsSimulation.randomizeForceAngle);

        EditorGUI.indentLevel--;

        if (GUILayout.Button("Run Simulation"))
        {
            RunSimulation();
        }

        if (GUILayout.Button("Undo Last Simulation"))
        {
            ResetAllBodies();
        }

        // separator
        addGUISeparator();

        // transform copy/paste
        if (GUILayout.Button("Copy Transforms"))
        {
            CopyTransforms();
        }
        else if (GUILayout.Button("Paste Transforms"))
        {
            PasteTransforms();
        }


        // separator
        addGUISeparator();

        // draw custom components
        if (GUILayout.Button("Remove Container Children"))
        {
            RemoveContainerChildren();
        }
    }


    private void addGUISeparator()
    {
        // space
        GUILayout.Space(10);

        // separator line
        GUIStyle separatorStyle = new GUIStyle(GUI.skin.box);
        separatorStyle.stretchWidth = true;
        separatorStyle.fixedHeight = 2;
        GUILayout.Box("", separatorStyle);
    }

    private void OnSceneGUI()
    {
        this.gizmo = target as PrefabPainter;

        if (this.gizmo == null)
            return;

        float radius = gizmo.brushSize / 2f;

        int controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
        {
            mousePos = hit.point;
            mousePosValid = true;

            Handles.color = Color.red;
            Handles.DrawWireDisc(mousePos, hit.normal, radius);


            ///
            /// process mouse events
            ///

            // control key pressed
            if (Event.current.control)
            {
                // mouse wheel up/down changes the radius
                if (Event.current.type == EventType.ScrollWheel)
                {

                    if (Event.current.delta.y > 0)
                    {
                        gizmo.brushSize++;
                        Event.current.Use();
                    }
                    else if (Event.current.delta.y < 0)
                    {
                        gizmo.brushSize--;

                        // TODO: slider
                        if (gizmo.brushSize < 1)
                            gizmo.brushSize = 1;

                        Event.current.Use();
                    }
                }
            }

            if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown)
            {
                // left button = 0; right = 1; middle = 2
                if (Event.current.button == 0)
                {
                    PaintPrefab();
                }
            }
        }
        else
        {
            mousePosValid = false;
        }

        if (Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(controlId);
        }


        // examples about how to show ui info
        // note: Handles.BeginGUI and EndGUI are important, otherwise the default gizmos aren't drawn
        Handles.BeginGUI();
        ShowHandleInfo();
        ShowGuiInfo();
        Handles.EndGUI();


        SceneView.RepaintAll();
    }

    private void ShowHandleInfo()
    {

        if (!mousePosValid)
            return;

        // example about how to show info at the gizmo
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.blue;
        string text = "Mouse Postion: " + mousePos;
        text += "\n";
        text += "Children: " + GetChildCount();
        Handles.Label(mousePos, text, style);
    }

    private int GetChildCount()
    {
        if (gizmo.container == null)
            return 0;

        return gizmo.container.transform.childCount;

    }


    private Transform[] getContainerChildren()
    {
        if (gizmo.container == null)
            return new Transform[0];

        Transform[] children = gizmo.container.transform.Cast<Transform>().ToArray();

        return children;
    }

    // show info in the ui
    private void ShowGuiInfo()
    {

        float windowWidth = Screen.width;
        float windowHeight = Screen.height;
        float panelWidth = 500;
        float panelHeight = 100;
        float panelX = windowWidth * 0.5f - panelWidth * 0.5f;
        float panelY = windowHeight - panelHeight;
        Rect infoRect = new Rect(panelX, panelY, panelWidth, panelHeight);

        Color textColor = Color.white;
        Color backgroundColor = Color.red;

        var defaultColor = GUI.backgroundColor;
        GUI.backgroundColor = backgroundColor;

        string inputInfo = "Use ctrl + mousewheel to adjust the brush size\nPress left mouse button and drag to paint prefabs";
        string childrenInfo = "Children: " + GetChildCount();

        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };
        labelStyle.normal.textColor = textColor;

        GUILayout.BeginArea(infoRect);
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label(inputInfo, labelStyle);
                GUILayout.Label(childrenInfo, labelStyle);
            }
            EditorGUILayout.EndVertical();
        }
        GUILayout.EndArea();

        GUI.backgroundColor = defaultColor;
    }

    #region Paint Prefabs

    /// <summary>
    /// Check if the distance 
    /// </summary>
    private void PaintPrefab()
    {
        bool prefabExists = false;

        // check if a gameobject is already within the brush size
        // allow only 1 instance per bush size
        GameObject container = gizmo.container as GameObject;

        foreach (Transform child in container.transform)
        {
            float dist = Vector3.Distance(mousePos, child.transform.position);

            if (dist <= gizmo.brushSize)
            {
                prefabExists = true;
                break;
            }

        }

        if (!prefabExists)
        {

            GameObject prefab = PrefabUtility.InstantiatePrefab(gizmo.prefab) as GameObject;

            // size
            if (gizmo.randomScale)
            {
                prefab.transform.localScale = Vector3.one * Random.Range(gizmo.randomScaleMin, gizmo.randomScaleMax);
            }

            // position
            prefab.transform.position = new Vector3(mousePos.x, mousePos.y, mousePos.z);

            // add offset
            prefab.transform.position += gizmo.positionOffset;

            // rotation
            Quaternion rotation;
            if( gizmo.randomRotation)
            {
                rotation = Random.rotation;
            } else
            {
                rotation = new Quaternion(0, 0, 0, 0);
            }
            prefab.transform.rotation = rotation;

            // attach as child of container
            prefab.transform.parent = container.transform;

            Undo.RegisterCreatedObjectUndo(prefab, "Instantiate Prefab");

        }
    }

    #endregion Paint Prefabs

    #region Remove Container Children

    private void RemoveContainerChildren()
    {
        GameObject container = gizmo.container as GameObject;

        List<Transform> list = new List<Transform>();
        foreach (Transform child in container.transform)
        {
            list.Add(child);
        }

        foreach (Transform child in list)
        {
            GameObject go = child.gameObject;

            DestroyImmediate(go);

        }
    }

    #endregion Remove Container Children

    #region Physics Simulation

    private void RunSimulation()
    {

        this.gizmo.physicsSimulation.RunSimulation( getContainerChildren());

    }

    private void ResetAllBodies()
    {
        this.gizmo.physicsSimulation.UndoSimulation();
    }

    #endregion Physics Simulation


    #region Copy/Paste Transforms

    private void CopyTransforms()
    {
        gizmo.copyPasteGeometryMap.Clear();

        GameObject container = gizmo.container as GameObject;

        foreach (Transform child in container.transform)
        {
            GameObject go = child.gameObject;

            if (go == null)
                continue;

            gizmo.copyPasteGeometryMap.Add(go.GetInstanceID(), new Geometry( go.transform));

        }

        // logging
        Debug.Log("Copying transforms & rotations: " + gizmo.copyPasteGeometryMap.Keys.Count);
    }


    private void PasteTransforms()
    {
        // logging
        Debug.Log("Pasting transforms & rotations: " + gizmo.copyPasteGeometryMap.Keys.Count);
        
        GameObject container = gizmo.container as GameObject;

        foreach (Transform child in container.transform)
        {
            GameObject go = child.gameObject;

            if (go == null)
                continue;

            Geometry geometry = null;

            if (gizmo.copyPasteGeometryMap.TryGetValue(go.GetInstanceID(), out geometry))
            {
                go.transform.position = geometry.getPosition();
                go.transform.rotation = geometry.getRotation();
            }

        }
    }

    #endregion Copy/Paste Transforms

}
