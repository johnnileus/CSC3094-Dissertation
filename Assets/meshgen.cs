using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class meshgen : MonoBehaviour{

    private MeshChunk[] meshChunks;
    private GameObject[] meshGOs;
    
    [SerializeField] private int meshNodeCount;
    [SerializeField] private int meshCount;
    [SerializeField] private float cellSize;

    // Start is called before the first frame update
    void Start(){
        meshGOs = new GameObject[meshCount^2];

        for (int y = 0; y < meshCount; y++) {
            for (int x = 0; x < meshCount; x++) {
                int chunkID = y * meshCount + x;
                
                Mesh mesh = GenMesh();
                GameObject meshObj = new GameObject($"mesh {chunkID}");
                meshObj.transform.parent = transform;
                meshObj.transform.position = new Vector3(x*(cellSize*meshNodeCount),0,y*(cellSize*meshNodeCount));

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;
            }
        }
    }

    private Mesh GenMesh(){
        Mesh m = new Mesh();
        Vector3[] vertices;
        int[] triangles;
        vertices = new Vector3[(meshNodeCount + 1) * (meshNodeCount + 1)];

        //create vertices

        for (int y = 0; y < meshNodeCount + 1; y++) {
            for (int x = 0; x < meshNodeCount + 1; x++) {
                vertices[y * (meshNodeCount + 1) + x] = new Vector3(x*cellSize, 0, y*cellSize);
            }
        }

        triangles = new int[meshNodeCount * meshNodeCount * 6];

        for (int y = 0; y < meshNodeCount; y++) {
            for (int x = 0; x < meshNodeCount; x++) {

                int tileNum = (meshNodeCount * y + x) * 6;
                int rootVert = (meshNodeCount + 1) * y + x;
                triangles[tileNum] = rootVert + meshNodeCount + 1;
                triangles[tileNum + 1] = rootVert + 1;
                triangles[tileNum + 2] = rootVert;

                triangles[tileNum + 3] = rootVert + meshNodeCount + 1;
                triangles[tileNum + 4] = rootVert + meshNodeCount + 2;
                triangles[tileNum + 5] = rootVert + 1;
            }
        }

        m.vertices = vertices;
        m.triangles = triangles;
        m.RecalculateNormals();
        return m;
    }
}
