using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshChunk{
    public GameObject MeshGO;
    public MeshChunk ParentChunk;
    
    public List<int> NorthPath = new List<int>();
    public List<int> EastPath = new List<int>();
    public List<int> SouthPath = new List<int>();
    public List<int> WestPath = new List<int>();
    
    public int DetailLevel;
    public Vector3 Pos; //bottom left
    
    public bool HasChildren;
    public MeshChunk[] Children;

    public MeshChunk(int detailLevel, Vector3 pos){
        DetailLevel = detailLevel;
        Pos = pos;
        HasChildren = false;
        Children = null;
    }
}
