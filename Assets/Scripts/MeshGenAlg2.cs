using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MeshGenAlg2 : MonoBehaviour{
    [SerializeField] private float RootMeshWidth;
    [SerializeField] private int MeshCellCount;
    [SerializeField] private float skirtHeight;

    private float CellSize;
    private MeshChunk RootChunk;

    private GameObject player;
    public GameObject testObj;

    private Dictionary<int, float> detailDistances = new Dictionary<int, float>() {
        { 0, 256f },
        { 1, 128f },
        { 2, 64f },
        { 3, 32f },
        { 4, 16f },
        { 5, 8f },
        { 6, 4f },
        { 7, 2f }
    };
    
    private void CheckChunkDistance(MeshChunk chunk){
        
        float offset = RootMeshWidth / MathF.Pow(2, chunk.DetailLevel + 1);
        Vector3 chunkCenter = chunk.Pos + new Vector3(offset, 0, offset);
        float dist = Vector2.Distance(new Vector2(chunkCenter.x, chunkCenter.z), new Vector2(player.transform.position.x, player.transform.position.z));
        
        if (chunk.HasChildren) {
            if (dist > detailDistances[chunk.DetailLevel]*32) {
                MergeMesh(chunk);
            } else {
                for (int i = 0; i < 4; i++) {
                    CheckChunkDistance(chunk.Children[i]);
                }
            }
        }
        else {
            if (chunk.DetailLevel < detailDistances.Count && dist < detailDistances[chunk.DetailLevel]*32) {
                SplitMesh(chunk);
            }
        }
    }
    
    private void Update(){
        //CheckChunkDistance(RootChunk);
    }
    
    void Start(){

        player = GameObject.FindWithTag("Player");
        
        RootChunk = new MeshChunk(0, new Vector3(0,0,0), CellSize);
        CellSize = RootMeshWidth / MeshCellCount;

        GameObject meshObj = new GameObject("root");
        meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
        meshObj.AddComponent<MeshFilter>().sharedMesh = GenMesh(0, Vector2.zero);
        
        meshObj.transform.parent = transform;
        RootChunk.MeshGO = meshObj;
        
        SplitMesh(RootChunk);
        SplitMesh(RootChunk.Children[0]);
        SplitMesh(RootChunk.Children[0].Children[0]);
        
        //MergeMesh(RootChunk.Children[0]);

    }
    //splits chunk into 4 child meshes
    private void SplitMesh(MeshChunk chunk){
        if (!chunk.HasChildren) {
            chunk.HasChildren = true;
            chunk.Children = new MeshChunk[4];
            chunk.MeshGO.GetComponent<MeshRenderer>().enabled = false;
        
            float offset = RootMeshWidth / MathF.Pow(2, chunk.DetailLevel + 1);

            Dictionary<int, Vector3> cellOffsets = new Dictionary<int, Vector3>() {
                {0, Vector3.zero},
                {1, new Vector3(offset, 0, 0)},
                {2, new Vector3(0, 0, offset)},
                {3, new Vector3(offset, 0, offset)},
            };

            for (int i = 0; i < 4; i++) {
                GameObject newObj = new GameObject($"mesh {i}");
                
                Vector3 newPos = chunk.Pos + cellOffsets[i];
                
                newObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                newObj.AddComponent<MeshFilter>().sharedMesh = GenMesh(chunk.DetailLevel + 1, new Vector2(newPos.x, newPos.z));

                newObj.transform.parent = chunk.MeshGO.transform;

                
                newObj.transform.position = chunk.Pos + cellOffsets[i];
                
                MeshChunk newChunk = new MeshChunk(chunk.DetailLevel + 1, newPos, CellSize);
            
                newChunk.MeshGO = newObj;
                newChunk.ParentChunk = chunk;
                chunk.Children[i] = newChunk;
            
            }

        }
        
    }
    //merges all children meshes into parent mesh
    private void MergeMesh(MeshChunk chunk){
        if (chunk.HasChildren) {
            chunk.HasChildren = false;
            for (int i = 0; i < 4; i++) {
                MergeMesh(chunk.Children[i]);
                Destroy(chunk.Children[i].MeshGO);
                chunk.Children[i] = null;
            }

            chunk.Children = null;
            chunk.MeshGO.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    private float GetMeshHeight(float x, float z){
        return 10*(Mathf.PerlinNoise(x/30, z/30)*5 + Mathf.PerlinNoise(x/300, z/300)*100 + Mathf.PerlinNoise(x/5, z/5));
    }

    private Mesh GenMesh(int detailLevel, Vector2 globalPos){
        Mesh m = new Mesh();
        
        int skirtVertexOffset = (MeshCellCount + 1) * (MeshCellCount + 1);
        Vector3[] vertices = new Vector3[skirtVertexOffset + 8*(MeshCellCount+1)];
        for (int y = 0; y < MeshCellCount + 1; y++) {
            for (int x = 0; x < MeshCellCount + 1; x++) {
                float scale = MathF.Pow(2, detailLevel);

        
                float xPos = x * CellSize / scale;
                float yPos = y * CellSize / scale;
                float height = GetMeshHeight(globalPos.x + xPos, globalPos.y + yPos);
                
                
                
                if (y == 0) { //bottom row
                    vertices[skirtVertexOffset + 2 * x] = new Vector3(xPos, height, yPos);
                    vertices[skirtVertexOffset + 2 * x + 1] = new Vector3(xPos, height - skirtHeight, yPos);
                } 
                if (y == MeshCellCount) { //top row
                    vertices[skirtVertexOffset + 4 * (MeshCellCount + 1) + 2 * x] = new Vector3(xPos, height, yPos);
                    vertices[skirtVertexOffset + 4 * (MeshCellCount + 1) + 2 * x + 1] = new Vector3(xPos, height - skirtHeight, yPos);

                }
                if (x == 0) { // left side
                    vertices[skirtVertexOffset + 2 * (MeshCellCount + 1) + 2 * y] = new Vector3(xPos, height, yPos);
                    vertices[skirtVertexOffset + 2 * (MeshCellCount + 1) + 2 * y + 1] = new Vector3(xPos, height - skirtHeight, yPos);

                } 
                if (x == MeshCellCount) { // right
                    vertices[skirtVertexOffset + 6 * (MeshCellCount + 1) + 2 * y] = new Vector3(xPos, height, yPos);
                    vertices[skirtVertexOffset + 6 * (MeshCellCount + 1) + 2 * y + 1] = new Vector3(xPos, height - skirtHeight, yPos);
                }
                
                vertices[y * (MeshCellCount + 1) + x] = new Vector3(xPos, height, yPos);
            }
        }

        int skirtTriangleOffset = MeshCellCount * MeshCellCount * 6;
        int[] triangles = new int[skirtTriangleOffset + 24 * MeshCellCount];
        for (int y = 0; y < MeshCellCount; y++) {
            for (int x = 0; x < MeshCellCount; x++) {

                int tileNum = (MeshCellCount * y + x) * 6;
                int rootVert = (MeshCellCount + 1) * y + x;
                triangles[tileNum] = rootVert + MeshCellCount + 1;
                triangles[tileNum + 1] = rootVert + 1;
                triangles[tileNum + 2] = rootVert;
                
                triangles[tileNum + 3] = rootVert + MeshCellCount + 1;
                triangles[tileNum + 4] = rootVert + MeshCellCount + 2;
                triangles[tileNum + 5] = rootVert + 1;

                if (y == 0) { //south
                    triangles[skirtTriangleOffset + 6 * x + 0] = skirtVertexOffset + 2 * x + 0;
                    triangles[skirtTriangleOffset + 6 * x + 1] = skirtVertexOffset + 2 * x + 3;
                    triangles[skirtTriangleOffset + 6 * x + 2] = skirtVertexOffset + 2 * x + 1;
                    
                    triangles[skirtTriangleOffset + 6 * x + 3] = skirtVertexOffset + 2 * x + 0;
                    triangles[skirtTriangleOffset + 6 * x + 4] = skirtVertexOffset + 2 * x + 2;
                    triangles[skirtTriangleOffset + 6 * x + 5] = skirtVertexOffset + 2 * x + 3;
                }
                if (y == MeshCellCount - 1) { //north
                    int o = 2*(MeshCellCount + 1) * 2;
                    int s = 6*MeshCellCount * 2;
                    triangles[skirtTriangleOffset + s + 6 * x + 0] = skirtVertexOffset + o + 2 * x + 2;
                    triangles[skirtTriangleOffset + s + 6 * x + 1] = skirtVertexOffset + o + 2 * x + 1;
                    triangles[skirtTriangleOffset + s + 6 * x + 2] = skirtVertexOffset + o + 2 * x + 3;
                    
                    triangles[skirtTriangleOffset + s + 6 * x + 3] = skirtVertexOffset + o + 2 * x + 2;
                    triangles[skirtTriangleOffset + s + 6 * x + 4] = skirtVertexOffset + o + 2 * x + 0;
                    triangles[skirtTriangleOffset + s + 6 * x + 5] = skirtVertexOffset + o + 2 * x + 1;
                }

                if (x == 0) { //west
                    int o = 2 * (MeshCellCount + 1) * 1;
                    int s = 6 * MeshCellCount * 1;
                    triangles[skirtTriangleOffset + s + 6 * y + 0] = skirtVertexOffset + o + 2 * y + 2;
                    triangles[skirtTriangleOffset + s + 6 * y + 1] = skirtVertexOffset + o + 2 * y + 1;
                    triangles[skirtTriangleOffset + s + 6 * y + 2] = skirtVertexOffset + o + 2 * y + 3;

                    triangles[skirtTriangleOffset + s + 6 * y + 3] = skirtVertexOffset + o + 2 * y + 2;
                    triangles[skirtTriangleOffset + s + 6 * y + 4] = skirtVertexOffset + o + 2 * y + 0;
                    triangles[skirtTriangleOffset + s + 6 * y + 5] = skirtVertexOffset + o + 2 * y + 1;
                }
                if (x == MeshCellCount - 1) { //east
                    int o = 2 * (MeshCellCount + 1) * 3;
                    int s = 6 * MeshCellCount * 3;
                    triangles[skirtTriangleOffset + s + 6 * y + 0] = skirtVertexOffset + o + 2 * y + 0;
                    triangles[skirtTriangleOffset + s + 6 * y + 1] = skirtVertexOffset + o + 2 * y + 3;
                    triangles[skirtTriangleOffset + s + 6 * y + 2] = skirtVertexOffset + o + 2 * y + 1;

                    triangles[skirtTriangleOffset + s + 6 * y + 3] = skirtVertexOffset + o + 2 * y + 0;
                    triangles[skirtTriangleOffset + s + 6 * y + 4] = skirtVertexOffset + o + 2 * y + 2;
                    triangles[skirtTriangleOffset + s + 6 * y + 5] = skirtVertexOffset + o + 2 * y + 3;
                }
            }
        }

        m.vertices = vertices;
        m.triangles = triangles;
        m.RecalculateNormals();
        return m;
    }
}
