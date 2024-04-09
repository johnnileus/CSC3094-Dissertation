using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuadTreeNeighbourTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        int[] path = { 3,1,1 };
        FindQuadTreeNeighbour(path, 'N');
        FindQuadTreeNeighbour(path, 'E');
        FindQuadTreeNeighbour(path, 'W');
        FindQuadTreeNeighbour(path, 'S');
        
    }

    private int ListToInt(int[] lst) {
        int num = 0;
        for (int i = 0; i < lst.Length; i++) {
            num <<= 1;
            if (lst[i] == 1) {
                num++;
            }
        }

        return num;
    }
    private int[] IntToList(int num, int detailLevel) {
        List<int> lst = new List<int>();

        for (int i = 0; i < detailLevel; i++) {
            lst.Add(num%2);
            num >>= 1;
        }
        
        int[] output = new int[lst.Count];
        lst.Reverse();
        for (int i = 0; i < lst.Count; i++) {
            output[i] = lst[i];
        }

        return output;
    }
    private int[] FindQuadTreeNeighbour(int[] path, char dir) {

        int detailLevel = path.Length;
        int[] newPath = new int[detailLevel];
        int[] binX = new int[detailLevel];
        int[] binY = new int[detailLevel];
        
        //convert path to x,y coords in binary
        for (int i = 0; i < path.Length; i++) {
            int val = path[i];

            binX[i] = val % 2;
            binY[i] = (val >> 1)%2;

        }

        int yCoord = ListToInt(binY);
        int xCoord = ListToInt(binX);
        
        
        switch (dir) {
            case 'N': {
                if (yCoord <=0 ) {
                    print("no north neighbour");
                    return null;
                }
                binY = IntToList( yCoord - 1, detailLevel);
                break;
            }case 'E': {
                if (xCoord >= Mathf.Pow(2,detailLevel)-1) {
                    print("no east neighbour");
                    return null;
                }
                print($"xcoord = {xCoord}");
                binX = IntToList(xCoord + 1, detailLevel);
                break;
            }case 'S': {
                if (yCoord >= Mathf.Pow(2,detailLevel)-1) {
                    print("no south neighbour");
                    return null;
                }
                binY = IntToList( yCoord + 1, detailLevel);
                break;
            }case 'W': {
                if (xCoord <=0 ) {
                    print("no west neighbour");
                    return null;
                }
                binX = IntToList(xCoord - 1, detailLevel);
                break;
            }
        }

        for (int i = 0; i < detailLevel; i++) {
            newPath[i] = binX[i] + 2 * binY[i];
        }

        //print new path
        string str = "";
        for (int i = 0; i < newPath.Length; i++) {
            str += newPath[i];
        }
        print(str);
        
        
        return newPath;






    }
}
    