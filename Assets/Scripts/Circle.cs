using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : Shape {

    public float radius;
    public Vector2 spawn;

    public Circle(Vector2 spawn, float radius) {
        this.spawn = spawn;
        this.radius = radius;
    }

    public override Vector3 randomPoint() {
        float angle = 2.0f * Mathf.PI * Random.Range(0.0f, 1.0f);

        float mineX = spawn.x + radius * Mathf.Cos(angle);
        float mineZ = spawn.y + radius * Mathf.Sin(angle);

        return new Vector3(mineX, 0.5f, mineZ);
    }
}
