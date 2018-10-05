using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
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
            /// Version Info
            /// 
            EditorGUILayout.HelpBox("Prefab Painter v0.1 (Alpha)", MessageType.Info);

            /// 
            /// General settings
            /// 

            GUILayout.BeginVertical("box"); 

            EditorGUILayout.LabelField("General Settings", PrefabPainterStyles.BoxTitleStyle);

            this.gizmo.container = (GameObject)EditorGUILayout.ObjectField("Container", this.gizmo.container, typeof(GameObject), true);
            this.gizmo.mode = (PrefabPainter.Mode) EditorGUILayout.EnumPopup("Mode", this.gizmo.mode);

            GUILayout.EndVertical();

            ///
            /// draw custom components
            /// 

            /// 
            /// Mode dependent
            /// 

            switch (this.gizmo.mode)
            {
                case PrefabPainter.Mode.Paint:

                    GUILayout.BeginVertical("box");

                    EditorGUILayout.LabelField("Paint settings", PrefabPainterStyles.BoxTitleStyle);

                    this.gizmo.brushSize = EditorGUILayout.FloatField("Brush Size", this.gizmo.brushSize);

                    GUILayout.EndVertical();

                    break;

                case PrefabPainter.Mode.Spline:

                    GUILayout.BeginVertical("box");

                    EditorGUILayout.LabelField("Spline settings", PrefabPainterStyles.BoxTitleStyle);

                    EditorGUILayout.LabelField("Work in progress ...");

                    GUILayout.EndVertical();
                    break;
            }

            /// 
            /// Prefab
            /// 

            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Prefab", PrefabPainterStyles.BoxTitleStyle);

            this.gizmo.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", this.gizmo.prefab, typeof(GameObject), true);

            this.gizmo.positionOffset = EditorGUILayout.Vector3Field("Position Offset", this.gizmo.positionOffset);

            this.gizmo.randomRotation = EditorGUILayout.Toggle("Random Rotation", this.gizmo.randomRotation);
            this.gizmo.randomScale = EditorGUILayout.Toggle("Random Scale", this.gizmo.randomScale);

            this.gizmo.randomScaleMin = EditorGUILayout.FloatField("Random Scale Min", this.gizmo.randomScaleMin);
            this.gizmo.randomScaleMax = EditorGUILayout.FloatField("Random Scale Max", this.gizmo.randomScaleMax);

            GUILayout.EndVertical();

            /// 
            /// Physics
            /// 

            // separator
            GUILayout.BeginVertical("box");
            //addGUISeparator();

            EditorGUILayout.LabelField("Physics Settings", PrefabPainterStyles.BoxTitleStyle);

            this.gizmo.physicsSimulation.maxIterations = EditorGUILayout.IntField("Max Iterations", this.gizmo.physicsSimulation.maxIterations);
            this.gizmo.physicsSimulation.forceMinMax = EditorGUILayout.Vector2Field("Force Min/Max", this.gizmo.physicsSimulation.forceMinMax);
            this.gizmo.physicsSimulation.forceAngleInDegrees = EditorGUILayout.FloatField("Force Angle (Degrees)", this.gizmo.physicsSimulation.forceAngleInDegrees);
            this.gizmo.physicsSimulation.randomizeForceAngle = EditorGUILayout.Toggle("Randomize Force Angle", this.gizmo.physicsSimulation.randomizeForceAngle);

            if (GUILayout.Button("Run Simulation"))
            {
                RunSimulation();
            }

            if (GUILayout.Button("Undo Last Simulation"))
            {
                ResetAllBodies();
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Copy/Paste", PrefabPainterStyles.BoxTitleStyle);

            // transform copy/paste
            if (GUILayout.Button("Copy Transforms"))
            {
                CopyTransforms();
            }
            else if (GUILayout.Button("Paste Transforms"))
            {
                PasteTransforms();
            }

            EditorGUILayout.HelpBox("Use in combination with Physics to revert to another state than the previous one.", MessageType.Info);

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Container Tools", PrefabPainterStyles.BoxTitleStyle);

            // draw custom components
            if (GUILayout.Button("Remove Container Children"))
            {
                RemoveContainerChildren();
            }

            GUILayout.EndVertical();
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

            switch (this.gizmo.mode)
            {
                case PrefabPainter.Mode.Paint:
                    DrawPaintGUI();
                    break;

                case PrefabPainter.Mode.Spline:
                    break;
            }

            SceneView.RepaintAll();
        }

        private void DrawPaintGUI()
        {

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
                if (gizmo.randomRotation)
                {
                    rotation = Random.rotation;
                }
                else
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

            this.gizmo.physicsSimulation.RunSimulation(getContainerChildren());

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

                gizmo.copyPasteGeometryMap.Add(go.GetInstanceID(), new Geometry(go.transform));

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
}