using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class meshgen : MonoBehaviour{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    
    [SerializeField] private int meshWidth;
    [SerializeField] private int meshHeight;
    
    // Start is called before the first frame update
    void Start(){
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        GenMesh();
        UpdateMesh();
    }

    void GenMesh(){
        vertices = new Vector3[(meshWidth + 1) * (meshHeight + 1)];
        
        //create vertices

        for (int y = 0; y < meshHeight + 1; y++) {
            for (int x = 0; x < meshWidth + 1; x++) {
                vertices[y * (meshWidth + 1) + x] = new Vector3(x, 0, y);
            }
        }
        
        triangles = new int[meshHeight * meshWidth * 6];

        for (int y = 0; y < meshHeight; y++) {
            for (int x = 0; x < meshWidth; x++) {

                int tileNum =( meshWidth * y + x) * 6;
                int rootVert = (meshWidth + 1) * y + x;
                triangles[tileNum]     = rootVert + meshWidth + 1;
                triangles[tileNum + 1] = rootVert + 1;
                triangles[tileNum + 2] = rootVert;
                triangles[tileNum + 3] = rootVert + meshWidth + 1;
                triangles[tileNum + 4] = rootVert + meshWidth + 2;
                triangles[tileNum + 5] = rootVert + 1;
            }
        }



    }
    

    void UpdateMesh(){
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos(){
        if (vertices == null) {
            return;
        }
        
        for (int i = 0; i < vertices.Length; i++) {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }
}
