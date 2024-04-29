using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour
{
    [SerializeField] float wallHeight = 3.0f; // Height of the wall in meters
    [Header("Green side")]
    [SerializeField] WallGenerator GreenNextWall, GreenPreviousWall;
    [Header("Red side")]
    [SerializeField] WallGenerator RedNextWall, RedPreviousWall;
    // Start is called before the first frame update
    void Start()
    {
        if(GreenPreviousWall == null && GreenNextWall != null){
            Debug.Log($"Using Points {GreenSide()} and {GreenNextWall.GreenSide()}");
            MakeQuadMeshUsingPoints(GreenSide(), GreenNextWall.GreenSide());
        }
    }
    public Vector2 GreenSide(){
        //get a point -0.5 on the local x axis accounting for scale
        Vector3 point1 = new Vector3(-0.5f * transform.localScale.x, 0, 0);
        point1 = transform.TransformPoint(point1);
        //Debug.DrawRay(transform.TransformPoint(point1), Vector3.up, Color.blue, 1000f);
        return new Vector2(point1.x, point1.z);
    }
    public Vector2 RedSide(){
        //get a point 0.5 on the local x axis accounting for scale
        Vector2 point1 = new Vector2(0.5f * transform.localScale.x, 0);
        return transform.TransformPoint(point1);
    }

    void MakeQuadMeshUsingPoints(Vector2 point1, Vector2 point2){
        Vector3[] points = {new Vector3(point1.x, 0, point1.y), new Vector3(point2.x, 0, point2.y), new Vector3(point2.x, wallHeight, point2.y), new Vector3(point1.x, wallHeight, point1.y)};
        for(int i = 0; i < points.Length; i++){ points[i] = transform.InverseTransformPoint(points[i]); }
        int[] triangles = {0, 1, 2, 0, 2, 3};
        Mesh mesh = new Mesh { vertices = points, triangles = triangles };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        GetComponent<MeshFilter>().mesh = mesh;
        Debug.Log("Mesh created");
    }
}
