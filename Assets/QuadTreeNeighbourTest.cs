using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuadTreeNeighbourTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindNorthNeighbour();
        
    }

    private int listToInt(int[] lst) {
        int num = 0;
        for (int i = 0; i < lst.Length; i++) {
            num <<= 1;
            if (lst[i] == 1) {
                num++;
            }
        }

        return num;
    }
    private int[] intToList(int num, int detailLevel) {
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

    private int[] FindNorthNeighbour(int[] path, int dir) {

        int detailLevel = path.Length;
        int[] newPath = new int[detailLevel];
        int[] binX = new int[detailLevel];
        int[] binY = new int[detailLevel];
        
        //convert path to binary x and y
        for (int i = 0; i < path.Length; i++) {
            int val = path[i];

            binX[i] = val % 2;
            binY[i] = (val >> 1)%2;

        }

        int yCoord = listToInt(binY);
        if (yCoord <=0 ) {
            print("no north neighbour");
            return null;
        }

        binY = intToList( yCoord - 1, detailLevel);

        for (int i = 0; i < detailLevel; i++) {
            newPath[i] = binX[i] + 2 * binY[i];
        }

        string str = "";
        for (int i = 0; i < newPath.Length; i++) {
            str += newPath[i];
        }
        print(str);
        return newPath;






    }
}
    