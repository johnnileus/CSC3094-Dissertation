using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


// moves vertices to match neighbour
public class MeshGenAlg1 : MonoBehaviour{
    [SerializeField] private float RootMeshWidth;
    [SerializeField] private int MeshCellCount;

    private float CellSize;
    private MeshChunk RootChunk;

    private GameObject player;

    [SerializeField] private bool testGenMesh = true;

    private Dictionary<int, float> detailDistances = MeshGenCommon.detailDistances;
    
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
    
    public void TestGenMesh() {
        float startTime = Time.realtimeSinceStartup;


        // private Mesh GenMesh(int detailLevel, Vector2 globalPos, MeshChunk chunk) {
        for (int i = 0; i < 10000; i++) {
            SplitMesh(RootChunk);
            MergeMesh(RootChunk);
        }
        
        float timeTaken = Time.realtimeSinceStartup - startTime;
        print($"{timeTaken/10000 * 1000}ms"); 

    }
    
    private void Update(){
        CheckChunkDistance(RootChunk);
        if (testGenMesh) {
            testGenMesh = false;
            TestGenMesh();
        } else {
            CheckChunkDistance(RootChunk);
        }
    }
    
    private int ListToInt(List<int> lst) {
        int num = 0;
        for (int i = 0; i < lst.Count; i++) {
            num <<= 1;
            if (lst[i] == 1) {
                num++;
            }
        }

        return num;
    }
    private List<int> IntToList(int num, int detailLevel) {
        List<int> lst = new List<int>();

        for (int i = 0; i < detailLevel; i++) {
            lst.Add(num%2);
            num >>= 1;
        }

        lst.Reverse();


        return lst;
    }
    private List<int> FindQuadTreeNeighbourPath(MeshChunk chunk, int dir) {
        List<int> path = chunk.Path;
        int detailLevel = path.Count;
        List<int> newPath = new List<int>();
        List<int> binX = new List<int>();
        List<int> binY = new List<int>();
        
        //convert path to x,y coords in binary
        for (int i = 0; i < path.Count; i++) {
            int val = path[i];

            binX.Add(val % 2);
            binY.Add((val >> 1)%2);

        }

        int yCoord = ListToInt(binY);
        int xCoord = ListToInt(binX);
        
        
        switch (dir) {
            case 0: { //n
                if (yCoord >= Mathf.Pow(2,detailLevel)-1) {
                    //print("no north neighbour");
                    return null;
                }
                binY = IntToList( yCoord + 1, detailLevel);
                break;
            }case 1: { //e
                if (xCoord >= Mathf.Pow(2,detailLevel)-1) {
                    //print("no east neighbour");
                    return null;
                }
                binX = IntToList(xCoord + 1, detailLevel);
                break;
            }case 2: { //s
                if (yCoord <=0 ) {
                    //print("no south neighbour");
                    return null;
                }
                binY = IntToList( yCoord - 1, detailLevel);
                break;
            }case 3: { //w
                if (xCoord <=0 ) {
                    //print("no west neighbour");
                    return null;
                }
                binX = IntToList(xCoord - 1, detailLevel);
                break;
            }
        }

        for (int i = 0; i < detailLevel; i++) {
            newPath.Add(binX[i] + 2 * binY[i]);
        }
        return newPath;

    }
    
    
    void Start(){

        player = GameObject.FindWithTag("Player");
        
        RootChunk = new MeshChunk(0, new Vector3(0,0,0), CellSize);
        RootChunk.Path = new List<int>{ };
        CellSize = RootMeshWidth / MeshCellCount; //reorder?
        GameObject meshObj = new GameObject("root");
        meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
        meshObj.AddComponent<MeshFilter>().sharedMesh = GenMesh(0, Vector2.zero, RootChunk);
        
        meshObj.transform.parent = transform;
        RootChunk.MeshGO = meshObj;
        
        
        
        // SplitMesh(RootChunk);
        // SplitMesh(RootChunk.Children[0]);
        // SplitMesh(RootChunk.Children[0].Children[2]);
        // SplitMesh(RootChunk.Children[0].Children[3]);
        

    }

    private List<MeshChunk> FindChunksOnBorder(MeshChunk chunk, int dir) {
        List<MeshChunk> output = new List<MeshChunk>();
        List<MeshChunk> listToCheck = new List<MeshChunk>();
        listToCheck.Add(chunk);
        
        while (listToCheck.Count > 0) {
            
            MeshChunk chunkToCheck = listToCheck[0];
            if (chunkToCheck.HasChildren) {
                switch (dir) {
                    case 2: {
                        listToCheck.Add(chunkToCheck.Children[2]);
                        listToCheck.Add(chunkToCheck.Children[3]);
                        break;
                    }case 3: {
                        listToCheck.Add(chunkToCheck.Children[1]);
                        listToCheck.Add(chunkToCheck.Children[3]);
                        break;
                    }case 0: {
                        listToCheck.Add(chunkToCheck.Children[0]);
                        listToCheck.Add(chunkToCheck.Children[1]);
                        break;
                    }case 1: {
                        listToCheck.Add(chunkToCheck.Children[0]);
                        listToCheck.Add(chunkToCheck.Children[2]);
                        break;
                    }
                }
            } else {
                output.Add(chunkToCheck);
            }
            listToCheck.RemoveAt(0);
        }
        
        
        return output;
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
                


                newObj.transform.parent = chunk.MeshGO.transform;

                
                newObj.transform.position = chunk.Pos + cellOffsets[i];
                
                MeshChunk newChunk = new MeshChunk(chunk.DetailLevel + 1, newPos, CellSize);
                newChunk.Path = new List<int>(chunk.Path);
                newChunk.Path.Add(i);
                
                newObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
            
                newChunk.MeshGO = newObj;
                newChunk.ParentChunk = chunk;
                chunk.Children[i] = newChunk;
            }

            for (int i = 0; i < 4; i++) {
                Vector3 newPos = chunk.Pos + cellOffsets[i];
                MeshChunk childChunk = chunk.Children[i];
                childChunk.MeshGO.AddComponent<MeshFilter>().sharedMesh = GenMesh(chunk.DetailLevel + 1,
                    new Vector2(newPos.x, newPos.z), childChunk);
            }
            
            // recalculate neighbour meshes
             for (int i = 0; i < 4; i++) {
                 List<int> path = FindQuadTreeNeighbourPath(chunk, i); //combine with genmesh for efficiency
                 if (path != null) {
                     MeshChunk neighbour = GetChunkFromPath(path);// possible error with being larger than node then looping over non-bordering children?
                     List<MeshChunk> children = FindChunksOnBorder(neighbour, i);
                     foreach (var child in children) {
                         child.MeshGO.GetComponent<MeshFilter>().mesh = GenMesh(child.DetailLevel, new Vector2(child.Pos.x, child.Pos.z), child);
                     }
                 }
                 
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
        
        // //recalculate neighbour meshes
        for (int i = 0; i < 4; i++) {
            List<int> path = FindQuadTreeNeighbourPath(chunk, i);
            if (path != null) {
                MeshChunk neighbour = GetChunkFromPath(path);
                List<MeshChunk> children = FindChunksOnBorder(neighbour, i);
                foreach (var child in children) {
                    child.MeshGO.GetComponent<MeshFilter>().mesh = GenMesh(child.DetailLevel, new Vector2(child.Pos.x, child.Pos.z), child);
                }
            }
                
        }
    }
    


    private MeshChunk GetChunkFromPath(List<int> path) {
        if (path.Count == 0) {
            return RootChunk;
        }
        string str = "";
        for (int i = 0; i < path.Count; i++) {
            str += path[i];
        }

        MeshChunk chunk = RootChunk;
        for (int i = 0; i < path.Count; i++) {
            if (chunk.HasChildren) {
                MeshChunk newChunk = chunk.Children[path[i]];
                chunk = newChunk;
        
            } else {
                break;
            }
        }
        return chunk;
    }
    
    //todo fix corner cracks
    private Mesh GenMesh(int detailLevel, Vector2 globalPos, MeshChunk chunk) {
        //print(detailLevel);
        Mesh m = new Mesh();

        MeshChunk[] neighbours = new MeshChunk[4];

        for (int i = 0; i < 4; i++) {
            List<int> path = FindQuadTreeNeighbourPath(chunk, i);
            if (path == null) {
                neighbours[i] = null;
            } else {
                neighbours[i] = GetChunkFromPath(path);
            }
            
        }
        
        
        Vector3[] vertices = new Vector3[(MeshCellCount + 1) * (MeshCellCount + 1)];
        bool attachNorth = true;
        bool attachEast = true;
        bool attachSouth = true;
        bool attachWest = true;
        
        float chunkCellSize = CellSize / Mathf.Pow(2, detailLevel);
        
        if (neighbours[0] == null || detailLevel - neighbours[0].DetailLevel <= 0) {
            attachNorth = false;
        }
        if (neighbours[1] == null || detailLevel - neighbours[1].DetailLevel <= 0) {
            attachEast = false;
        }
        if (neighbours[2] == null || detailLevel - neighbours[2].DetailLevel <= 0) {
            attachSouth = false;
        }
        if (neighbours[3] == null || detailLevel - neighbours[3].DetailLevel <= 0) {
            attachWest = false;
        }
        
        
        
        
        for (int y = 0; y < MeshCellCount + 1; y++) {
            for (int x = 0; x < MeshCellCount + 1; x++) {
            
                float scale = MathF.Pow(2, detailLevel);
                float xPos = x * CellSize / scale;
                float yPos = y * CellSize / scale;
                float height;
                
                if (y == MeshCellCount && attachNorth) {
                    float xCoord = x * chunkCellSize + chunk.Pos[0] - neighbours[0].Pos[0];
                    //floating point error, cant use %
                    float chunkProgress = xCoord / neighbours[0].CellSize;
                    int node = Mathf.FloorToInt(chunkProgress);
                    float progress = chunkProgress - node;
                    
                    float h1 = MeshGenCommon.GetMeshHeight(neighbours[0].Pos[0] + node * neighbours[0].CellSize, chunk.Pos[2] + yPos);
                    float h2 = MeshGenCommon.GetMeshHeight(neighbours[0].Pos[0] + (node+1) * neighbours[0].CellSize, chunk.Pos[2] + yPos);
                    height = Mathf.Lerp(h1, h2, progress);
                }else if (x == MeshCellCount && attachEast) {
                        
                    float yCoord = y * chunkCellSize + chunk.Pos[2] - neighbours[1].Pos[2];
                
                    float chunkProgress = yCoord / neighbours[1].CellSize;
                    int node = Mathf.FloorToInt(chunkProgress);
                    float progress = chunkProgress - node;
                    
                    float h1 = MeshGenCommon.GetMeshHeight(chunk.Pos[0] + xPos, neighbours[1].Pos[2] + node * neighbours[1].CellSize);
                    float h2 = MeshGenCommon.GetMeshHeight(chunk.Pos[0] + xPos, neighbours[1].Pos[2] + (node+1) * neighbours[1].CellSize);
                    height = Mathf.Lerp(h1, h2, progress);
                }
                else if (y == 0 && attachSouth) {
                    float xCoord = x * chunkCellSize + chunk.Pos[0] - neighbours[2].Pos[0];
                    //floating point error, cant use %
                    float chunkProgress = xCoord / neighbours[2].CellSize;
                    int node = Mathf.FloorToInt(chunkProgress);
                    float progress = chunkProgress - node;
                    
                    float h1 = MeshGenCommon.GetMeshHeight(neighbours[2].Pos[0] + node * neighbours[2].CellSize, chunk.Pos[2]);
                    float h2 = MeshGenCommon.GetMeshHeight(neighbours[2].Pos[0] + (node+1) * neighbours[2].CellSize, chunk.Pos[2]);
                    height = Mathf.Lerp(h1, h2, progress);
                } 
                else if (x == 0 && attachWest) {
                    
                    float yCoord = y * chunkCellSize + chunk.Pos[2] - neighbours[3].Pos[2];
                
                    float chunkProgress = yCoord / neighbours[3].CellSize;
                    int node = Mathf.FloorToInt(chunkProgress);
                    float progress = chunkProgress - node;
                    
                    float h1 = MeshGenCommon.GetMeshHeight(chunk.Pos[0], neighbours[3].Pos[2] + node * neighbours[3].CellSize);
                    float h2 = MeshGenCommon.GetMeshHeight(chunk.Pos[0], neighbours[3].Pos[2] + (node+1) * neighbours[3].CellSize);
                    height = Mathf.Lerp(h1, h2, progress);
                }
                else {
                    height = MeshGenCommon.GetMeshHeight(globalPos.x + xPos, globalPos.y + yPos);
                }
                
                

                //height = GetMeshHeight(globalPos.x + xPos, globalPos.y + yPos);
                
                
                
                vertices[y * (MeshCellCount + 1) + x] = new Vector3(xPos, height, yPos);
            }
        }

        
        int[] triangles = new int[MeshCellCount * MeshCellCount * 6];
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
            }
        }

        m.vertices = vertices;
        m.triangles = triangles;
        m.RecalculateNormals();
        return m;
    }
}

