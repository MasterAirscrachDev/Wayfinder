using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour
{
    [SerializeField] bool isStart = false;
    [SerializeField] float wallHeight = 3.0f; // Height of the wall in meters
    [SerializeField] WallGenerator nextPoint;
    // Start is called before the first frame update

    List<Vector3> meshVertices;
    List<int> meshTriangles;
    List<Vector2> points;
    void Start()
    {
        if(isStart){
            points = new List<Vector2>();
            meshVertices = new List<Vector3>();
            meshTriangles = new List<int>();
            points.Add(GetPoint());
            while(nextPoint != null){
                points.Add(nextPoint.GetPoint());
                nextPoint = nextPoint.GetNextPoint();
            }
            for(int i = 0; i < points.Count - 1; i++){
                AddPointPairToMesh(points[i], points[i + 1]);
            }
            GenerateMeshFromLists();
        }
    }
    WallGenerator GetNextPoint(){
        return nextPoint;
    }
    Vector2 GetPoint(){
        return new Vector2(transform.position.x, transform.position.z);
    }

    void AddPointPairToMesh(Vector2 point1, Vector2 point2){
        Vector3[] points = {new Vector3(point1.x, 0, point1.y), new Vector3(point2.x, 0, point2.y), new Vector3(point2.x, wallHeight, point2.y), new Vector3(point1.x, wallHeight, point1.y)};
        for(int i = 0; i < points.Length; i++){ meshVertices.Add(points[i]); }
        int baseIndex = meshVertices.Count - points.Length;
        
        int[] triangles = {baseIndex, 1 + baseIndex, 2 + baseIndex, baseIndex, 2 + baseIndex, 3 + baseIndex};
        for(int i = 0; i < triangles.Length; i++){ meshTriangles.Add(triangles[i]); }
    }
    void GenerateMeshFromLists(){
        Vector3[] vertices = new Vector3[meshVertices.Count];
        for(int i = 0; i < meshVertices.Count; i++){
            vertices[i] = transform.InverseTransformPoint(meshVertices[i]);
        }
        int[] triangles = meshTriangles.ToArray();
        Mesh mesh = new Mesh { vertices = vertices, triangles = triangles };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void CreateNextPoint()
    {
        GameObject newObject = Instantiate(gameObject, transform.position, Quaternion.identity);
        WallGenerator newWallGenerator = newObject.GetComponent<WallGenerator>();
        this.nextPoint = newWallGenerator;
        //if this object ends with a (number) then increment the number
        //if this object ends with a letter then add a 1 to the end
        if (gameObject.name.EndsWith(")"))
        {
            int index = gameObject.name.LastIndexOf("(");
            string newName = gameObject.name.Substring(0, index);
            int number = int.Parse(gameObject.name.Substring(index + 1, gameObject.name.Length - index - 2));
            newObject.name = newName + "(" + (number + 1) + ")";
        }
        else
        {
            newObject.name = newObject.name + " 1";
        }
        //select the new object in editor
        UnityEditor.Selection.activeObject = newObject;
    }
}
