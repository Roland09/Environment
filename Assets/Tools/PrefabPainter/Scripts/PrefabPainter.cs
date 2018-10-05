using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PrefabPainter
{
    /// <summary>
    /// Prefab painter data structure
    /// </summary>
    public class PrefabPainter : MonoBehaviour
    {

        public enum Mode { Paint, Spline }

        /// <summary>
        /// The parent of the instantiated prefabs 
        /// </summary>
        [HideInInspector]
        public GameObject container;

        [HideInInspector]
        public Mode mode;

        /// <summary>
        /// The diameter of the brush
        /// </summary>
        [HideInInspector]
        public float brushSize = 2.0f;

        /// <summary>
        /// The prefab which should be instanted and placed at the brush position
        /// </summary>
        [HideInInspector]
        public GameObject prefab;

        /// <summary>
        /// The offset that should be added to the instantiated gameobjects position
        /// </summary>
        [HideInInspector]
        public Vector3 positionOffset;

        /// <summary>
        /// Randomize rotation
        /// </summary>
        [HideInInspector]
        public bool randomRotation;

        /// <summary>
        /// Randomize Scale Minimum
        /// </summary>
        [HideInInspector]
        public bool randomScale = false;

        /// <summary>
        /// Randomize Scale Minimum
        /// </summary>
        [HideInInspector]
        public float randomScaleMin = 0.5f;

        /// <summary>
        /// Randomize Scale Maximum
        /// </summary>
        [HideInInspector]
        public float randomScaleMax = 1.5f;

        /// <summary>
        /// Instance of PhysicsSimulation.
        /// Keeping the object here allows us to navigate away from the PrefabPainter gameobject
        /// and return to it and keep the phyics settings. Otherwise the physics settings would always be reset
        /// </summary>
        [HideInInspector]
        public PhysicsSimulation physicsSimulation;

        /// <summary>
        /// Container for copied positions and rotations
        /// </summary>
        [HideInInspector]
        public Dictionary<int, Geometry> copyPasteGeometryMap = new Dictionary<int, Geometry>();

    }
}