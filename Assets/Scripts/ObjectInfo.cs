using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInfo : MonoBehaviour {

    public bool hasMaterialOnChildren = false;

    public List<Collider> colliders = new List<Collider>();

    private void OnCollisionEnter(Collision collision) {
        if (!collision.gameObject.name.Equals("Ground") && !collision.gameObject.name.Equals(name))
            colliders.Add(collision.collider);
    }

    private void OnCollisionExit(Collision collision) {
        colliders.Remove(collision.collider);
    }

}
