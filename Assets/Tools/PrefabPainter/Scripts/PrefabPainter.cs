using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Prefab painter data structure
/// </summary>
public class PrefabPainter : MonoBehaviour {

    /// <summary>
    /// The parent of the instantiated prefabs 
    /// </summary>
    public GameObject container;

    /// <summary>
    /// The prefab which should be instanted and placed at the brush position
    /// </summary>
    public GameObject prefab;

    /// <summary>
    /// The diameter of the brush
    /// </summary>
    public float brushSize = 2.0f;

    /// <summary>
    /// The offset that should be added to the instantiated gameobjects position
    /// </summary>
    public Vector3 positionOffset;

    /// <summary>
    /// Randomize rotation
    /// </summary>
    public bool randomRotation;

    /// <summary>
    /// Randomize Scale Minimum
    /// </summary>
    public bool randomScale = false;

    /// <summary>
    /// Randomize Scale Minimum
    /// </summary>
    public float randomScaleMin = 0.5f;

    /// <summary>
    /// Randomize Scale Maximum
    /// </summary>
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
