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
    [SerializeField] float speed = 3f;
    Transform[] nodes;
    // Start is called before the first frame update
    void Start()
    {
        nodes = new Transform[transform.childCount];
        for (int i = 0; i < nodes.Length; i++)
        { nodes[i] = transform.GetChild(i); }
        //find the start and end points
        Transform start = null, end = null;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].name == startPoint)
            { start = nodes[i]; }
            if (nodes[i].name == endPoint)
            { end = nodes[i]; }
        }
        if(start != null && end != null)
        {
            Pathing(start, end);
        }
        else
        {
            Debug.LogError("Start or End point not found");
        }
    }
    async Task Pathing(Transform start, Transform end){
        Vector3[] path = await NodesOnPath(start, end);
        // for (int i = 0; i < path.Length - 1; i++)
        // { Debug.DrawLine(path[i], path[(i + 1) % path.Length], Color.red, 1000); }
        LineRenderer line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = path.Length;
        line.SetPositions(path);
    }
    async Task<Vector3[]> NodesOnPath(Transform start, Transform end)
    {
        float displayTime = speed * 0.66f;
        List<Vector3> path = new List<Vector3>
        { start.position };
        Vector3 current = start.position;
        Vector3[] closest = ClosestDepthToPoint(start.position);
        int depth = 3;
        bool done = false;
        while (!done)
        {
            for(int i = 0; i < path.Count - 1; i++)
            { Debug.DrawLine(path[i], path[(i + 1) % path.Count], Color.green, displayTime); }
            Debug.DrawRay(current, Vector3.up, Color.blue, displayTime);
            //sort the closest nodes by distance to end
            System.Array.Sort(closest, (a, b) => Vector3.Distance(a, end.position).CompareTo(Vector3.Distance(b, end.position)));
            bool found = false;
            for (int i = 0; i < closest.Length; i++)
            {
                if (closest[i] == start.position) { continue; } //if its the start, skip
                if (Vector3.Distance(closest[i], end.position) < 0.1f) //if its the end, add it to the path and break
                {
                    done = true;
                    path.Add(end.position);
                    found = true;
                    break;
                }
                else if(!path.Contains(closest[i]) && AddPointSafely(closest[i], current, end.position)){ //if its not the current node, add it to the path and set it as the current node{
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
                if(depth > 10){Debug.LogError("DepthToo High"); return null;} 
            }
            await Task.Delay((int)speed * 1000);
            //if the editor stopped, break the loop
            if (!Application.isPlaying) { return null; }
            
        }
        return path.ToArray();
    }
    bool AddPointSafely(Vector3 newPoint, Vector3 current, Vector3 end){
        float newToEnd = Vector3.Distance(newPoint, end), currentToEnd = Vector3.Distance(current, end), currentToNew = Vector3.Distance(current, newPoint);
        if(newToEnd < currentToEnd){
            return true;
        } else if(currentToNew < currentToEnd / 2){
            return true;
        }
        return false;
    }

    Vector3[] ClosestDepthToPoint(Vector3 point, int depth = 3, float displayTime = 1f){
        System.Array.Sort(nodes, (a, b) => Vector3.Distance(a.position, point).CompareTo(Vector3.Distance(b.position, point)));
        Vector3[] closestPoints = new Vector3[depth];
        for (int i = 1; i < depth + 1; i++) //start at 1 to skip the current node
        {Debug.Log($"ClosePointTo {point}: {nodes[i].position}"); closestPoints[i] = nodes[i].position; Debug.DrawLine(point, nodes[i].position + (Vector3.up * (0.3f * (i + 1))), Color.yellow, displayTime);}
        return closestPoints;
    }
}
