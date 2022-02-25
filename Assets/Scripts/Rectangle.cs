using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rectangle : Shape {

    public Vector2 center;
    public float width;
    public float length;
    public float height;

    public Rectangle(Vector2 center, float width, float length, float height) {
        this.center = center;
        this.width = width;
        this.length = length;
        this.height = height;
    }

    public override Vector3 randomPoint() {
        float x = Random.Range(center.x - width, center.x + width);
        float z = Random.Range(center.y - length, center.y + length);

        return new Vector3(x, height, z);
    }
}
