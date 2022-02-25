using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertedCircle : Shape {

    public float minRadius;
    public float maxRadius;
    public Vector2 spawn;

    public InvertedCircle(Vector2 spawn, float minRadius, float maxRadius) {
        this.spawn = spawn;
        this.minRadius = minRadius;
        this.maxRadius = maxRadius;
    }

    public override Vector3 randomPoint() {
        float angle = 2.0f * Mathf.PI * Random.Range(0.0f, 1.0f);

        float mineX = spawn.x + maxRadius * Mathf.Cos(angle);
        float mineZ = spawn.y + maxRadius * Mathf.Sin(angle);

        int attempts = 0;

        while (Vector2.Distance(new Vector2(mineX, mineZ), new Vector2(spawn.x, spawn.y)) <= minRadius) {
            angle = 2.0f * Mathf.PI * Random.Range(0.0f, 1.0f);

            mineX = spawn.x + maxRadius * Mathf.Cos(angle);
            mineZ = spawn.y + maxRadius * Mathf.Sin(angle);

            attempts++;

            if (attempts > 10) {
                break;
            }
        }

        return new Vector3(mineX, 1, mineZ);
    }

}
