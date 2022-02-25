using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : Shape {

    public Vector2 A, B, C;

    public Triangle(Vector2 A, int orientation, float angle, float maxDistance) {
        this.A = A;

        float x = maxDistance;
        float y = maxDistance * Mathf.Sin(angle);

        if (orientation == 2 || orientation == 3) {
            x *= -1;
        }

        if (orientation % 2 == 0) {
            B = new Vector2(x + A.x, y + A.y);
            C = new Vector2(x + A.x, -y + A.y);
        } else {
            B = new Vector2(y + A.x, x + A.y);
            C = new Vector2(-y + A.x, x + A.y);
        }
    }

    public Triangle(Vector2 A, Vector2 B, Vector3 C) {
        this.A = A;
        this.B = B;
        this.C = C;
    }

    public Triangle(Vector2[] A) {
        this.A = A[0];
        this.B = A[1];
        this.C = A[2];
    }

    public override Vector3 randomPoint() {
        float p = Random.Range(0.0f, 1.0f);
        float q = Random.Range(0.0f, 1.0f);

        if (p + q > 1) {
            p = 1 - p;
            q = 1 - q;
        }

        // A + AB * p + BC * q
        float x = A.x + (B.x - A.x) * p + (C.x - A.x) * q;
        float y = A.y + (B.y - A.y) * p + (C.y - A.y) * q;

        return new Vector3(x, 0.5f, y);
    }
}
