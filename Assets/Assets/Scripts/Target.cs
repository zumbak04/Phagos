using UnityEngine;

public class Target
{
    [SerializeField]
    public Vector3 vector = Vector3.zero;
    public int count = 0;

    public Target(Vector3 vector, int count = 0)
    {
        this.vector = vector;
        this.count = count;
    }
}
