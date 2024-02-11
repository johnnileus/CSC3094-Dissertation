using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MeshChunkOld : MonoBehaviour{
    public int ID;
    public int detailLevel;
    public bool hasChildren;
    public MeshChunkOld[] children;
    public Vector3 worldPos;

    public MeshChunkOld(int id, Vector3 pos){
        ID = id;
        worldPos = pos;
    }
}
