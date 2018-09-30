using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for position & rotation
/// </summary>
public class Geometry {

    readonly Vector3 originalPosition;
    readonly Quaternion originalRotation;

    public Geometry( Transform transform)
    {
        this.originalPosition = transform.position;
        this.originalRotation = transform.rotation;
    }

    public Vector3 getPosition()
    {
        return originalPosition;
    }

    public Quaternion getRotation()
    {
        return originalRotation;
    }

}
