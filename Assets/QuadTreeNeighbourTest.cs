using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuadTreeNeighbourTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        List<int> path = new List<int> { 3, 2, 0 };
        FindQuadTreeNeighbour(path, 'N');
        FindQuadTreeNeighbour(path, 'E');
        FindQuadTreeNeighbour(path, 'S');
        FindQuadTreeNeighbour(path, 'W');
        
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
    private List<int> FindQuadTreeNeighbour(List<int> path, char dir) {

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
            newPath.Add(binX[i] + 2 * binY[i]);
        }

        //print new path
        string str = "";
        for (int i = 0; i < newPath.Count; i++) {
            str += newPath[i];
        }
        print($"{dir} {str}");
        
        
        return newPath;






    }
}
    