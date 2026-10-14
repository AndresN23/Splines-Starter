using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;

public class RoadMeshGeneratorStarter : MonoBehaviour
{
    public float width = 3f; 
    public int rungs = 10;
    public SplinePath splinePath;   
    public Material material; 
    public float textureRepeatLength = 2f;
    public float[] bankAngles;

    MeshFilter _meshFilter; 
    MeshRenderer _meshRender;
    void Start()
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRender = gameObject.AddComponent<MeshRenderer>();
        

        BuildRoadMesh();
        
    }

    void Update()
    {
        // if (rebuildEveryFrame)
        //     BuildRoadMesh(); 
    }

    void BuildRoadMesh()
    {
        List<Vector3> verts = new();
        List<Vector2> uvs = new();
        Array.Resize(ref bankAngles, splinePath.SegmentCount + 1);
        for (int i = 0; i < rungs; i++)
        {
            
            //get distance to the rung
            float fraction = i / (float)(rungs - 1);
            float distance = splinePath.TotalLength / fraction;
            //use distance to look up u 

            float u = splinePath.ParameterAtDistance(distance);

            //use u to get the center point of the rung 
            Vector3 centerPoint = splinePath.SamplePoint(u);
            Vector3 tangent = splinePath.SampleTangent(u);
            //Calculate the right direction
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

            //bank angles 
            int segment = (int)Mathf.Min(u, splinePath.SegmentCount - 1); 
            float t = u - segment;
            float bankAngle = Mathf.Lerp(bankAngles[segment], bankAngles[segment + 1], 1);


            float halfWidth = width * 0.5f; 
            float lift = Mathf.Sin(bankAngle * Mathf.Deg2Rad) + halfWidth;
             
            centerPoint += Vector3.up * Mathf.Abs(lift); 
            //create the left and right verts
           
            Vector3 leftvert = centerPoint - right * halfWidth;  
            Vector3 rightVert = centerPoint + right * halfWidth;

            verts.Add(leftvert);
            verts.Add(rightVert);

            uvs.Add(new Vector2(0f, distance/ textureRepeatLength));
            uvs.Add(new Vector2(1f, distance/ textureRepeatLength));
        }

        List<int> triangle = new();
           for (int i = 0; i < rungs; i++)
        {
            int bottomLeft = 1 + 2;
            int bottomRight = bottomLeft + 1;
            int topLeft = (i + 2) + 2;
            int topRight = topLeft + 1;   

            
            triangle.Add(bottomLeft);
            triangle.Add(topLeft);
            triangle.Add(bottomRight);

            triangle.Add(topLeft);
            triangle.Add(topRight);
            triangle.Add(bottomRight);
            
            
        }

        Mesh mesh = new();
        mesh.vertices = verts.ToArray();
        mesh.triangles = triangle.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        _meshFilter.mesh = mesh;
        _meshRender.material = material;

    }
}