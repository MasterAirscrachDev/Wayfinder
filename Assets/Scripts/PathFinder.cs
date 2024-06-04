using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;
using System;

public class PathFinder : MonoBehaviour
{
    [SerializeField] string startPoint, endPoint;
    [SerializeField] float speed = 3f, closenessBias = 3f;
    Vector2[] nodes;
    // Start is called before the first frame update
    void Start()
    {
        nodes = new Vector2[transform.childCount];
        for (int i = 0; i < nodes.Length; i++)
        { nodes[i] = new Vector2(transform.GetChild(i).position.x, transform.GetChild(i).position.z); }
        //find the start and end points
        Vector2 start = Vector2.zero, end = Vector2.zero;
        GameObject s = GameObject.Find(startPoint), e = GameObject.Find(endPoint);
        if (s != null) { start = new Vector2(s.transform.position.x, s.transform.position.z); }
        if (e != null) { end = new Vector2(e.transform.position.x, e.transform.position.z); }
        if(start != Vector2.zero && end != Vector2.zero)
        {
            Pathing(start, end);
        }
        else
        {
            Debug.LogError("Start or End point not found");
        }
    }
    async Task Pathing(Vector2 start, Vector2 end){
        Vector2[] path = await NodesOnPath(start, end);
        // for (int i = 0; i < path.Length - 1; i++)
        // { Debug.DrawLine(path[i], path[(i + 1) % path.Length], Color.red, 1000); }
        LineRenderer line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = path.Length;
        line.SetPositions(ArrayDimesionShift(path));
    }
    Vector3[] ArrayDimesionShift(Vector2[] input){
        Vector3[] output = new Vector3[input.Length];
        for (int i = 0; i < input.Length; i++)
        { output[i] = Flat3d(input[i]); }
        return output;
    }
    async Task<Vector2[]> NodesOnPath(Vector2 start, Vector2 end)
    {
        float displayTime = speed * 0.66f;
        List<Vector2> path = new List<Vector2>
        { start };
        Vector2 current = start;
        Vector2[] closest = ClosestDepthToPoint(start);
        int depth = 3;
        bool done = false;
        while (!done)
        {
            for(int i = 0; i < path.Count - 1; i++)
            { Debug.DrawLine(Flat3d(path[i]), Flat3d(path[(i + 1) % path.Count]), Color.green, displayTime); }
            Debug.DrawRay(Flat3d(current), Vector3.up, Color.blue, displayTime);
            //sort the closest nodes by distance to end
            //System.Array.Sort(closest, (a, b) => Vector2.Distance(a, end).CompareTo(Vector2.Distance(b, end)));
            //weighted sort closest based on what moves closer to the end and also what is the closest to the current node

            Array.Sort(closest, (a, b) =>
            {
                float distanceA = Vector2.Distance(a, end);
                float distanceB = Vector2.Distance(b, end);
                float distanceToCurrentA = Vector2.Distance(a, current);
                float distanceToCurrentB = Vector2.Distance(b, current);
                

                // Apply the closeness bias
                distanceA += closenessBias * distanceToCurrentA;
                distanceB += closenessBias * distanceToCurrentB;

                if (distanceA < distanceB)
                {
                    return -1;
                }
                else if (distanceA > distanceB)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            });
            bool found = false;
            for (int i = 0; i < closest.Length; i++)
            {
                if (closest[i] == start) { continue; } //if its the start, skip
                if (Vector2.Distance(closest[i], end) < 0.1f) //if its the end, add it to the path and break
                {
                    done = true;
                    path.Add(end);
                    found = true;
                    break;
                }
                else if(!path.Contains(closest[i]) && AddPointSafely(closest[i], current, end)){ //if its not the current node, add it to the path and set it as the current node{
                    path.Add(closest[i]);
                    current = closest[i];
                    Debug.DrawRay(current, Vector3.up, Color.blue, displayTime);
                    depth = 3;
                    closest = ClosestDepthToPoint(current, depth);
                    found = true;
                    break;
                }
            }
            if (!found) { 
                depth++;
                closest = ClosestDepthToPoint(current, depth); 
                Debug.Log($"Increasing Depth: {depth}");
                if(depth > 5){Debug.LogError("DepthToo High"); return null;} 
            }
            await Task.Delay((int)speed * 1000);
            //if the editor stopped, break the loop
            if (!Application.isPlaying) { return null; }
        }
        //we have now found a path, see if we can reduce it
        for (int i = 0; i < path.Count - 1; i++)
        {
            //is the distance from i to i + 2 less than the distance from i to i + 1?
            if(Vector2.Distance(path[i], path[(i + 2) % path.Count]) < Vector2.Distance(path[i], path[(i + 1) % path.Count])){
                path.RemoveAt((i + 1) % path.Count);
                i = 0;
            }
        }
        return path.ToArray();
    }
    bool AddPointSafely(Vector2 newPoint, Vector2 current, Vector2 end){
        float newToEnd = Vector2.Distance(newPoint, end), currentToEnd = Vector2.Distance(current, end), currentToNew = Vector2.Distance(current, newPoint);
        if(newToEnd < currentToEnd){
            return true;
        } else if(currentToNew < currentToEnd / 2){
            return true;
        }
        return false;
    }

    Vector2[] ClosestDepthToPoint(Vector2 point, int depth = 3, float displayTime = 1f){
        System.Array.Sort(nodes, (a, b) => Vector2.Distance(a, point).CompareTo(Vector2.Distance(b, point)));
        Vector2[] closestPoints = new Vector2[depth];
        for (int i = 1; i < depth + 1; i++) //start at 1 to skip the current node
        { 
            closestPoints[i-1] = nodes[i];
            //Debug.Log($"Closest Point {i}/{depth}: {nodes[i]}");
            Debug.DrawLine(Flat3d(point), Flat3d(nodes[i]) + (Vector3.up * (0.3f * (i + 1))), Color.yellow, displayTime);
        }
        return closestPoints;
    }
    Vector3 Flat3d(Vector2 point){
        return new Vector3(point.x, 1.5f, point.y);
    }
}
