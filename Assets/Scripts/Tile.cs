using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // Tile sides defined in a 2D int array, as above. Referenced by enum TileTypes
    protected int[,] TileSides = { { 0, 0, 0, 0 }, { 0, 0, 1, 0 }, { 1, 0, 1, 0 }, { 1, 1, 0, 0 }, { 1, 0, 1, 1 }, { 1, 1, 1, 1 } };
    // Order should be Empty, Deadend, Straight, Elbow, T, Cross
    [SerializeField] private GameObject[] tileObjects;

    protected static float _degPerRotation = 90;

    protected int[] ShiftFaceValues(int[] faceValues) {
        if (faceValues == null)
            return null;

        int size = faceValues.GetLength(0);
        int[] newValues = new int[size];

        for(int i = 1; i < size; i++)
            newValues[i] = faceValues[i - 1];
        newValues[0] = faceValues[size - 1];

        return newValues;
    }

    protected int[] ShiftFaceValues(int[] faceValues, int rotations)
    {
        if (faceValues == null)
            return null;

        int size = faceValues.GetLength(0);
        int[] newValues = new int[size];

        for(int rot = 0; rot < rotations; rot++)
            for (int i = 1; i < size; i++)
                newValues[i] = faceValues[i - 1];
        newValues[0] = faceValues[size - 1];

        return newValues;
    }


    protected int GetRotatedFace(int Rotation, int FaceSelection, StructureDefinitions.TileTypes tile)
    {
        int[] sides = GetTileFaces(tile);
        return ShiftFaceValues(sides, Rotation)[FaceSelection];
    }

    protected int[] GetTileFaces(StructureDefinitions.TileTypes tile)
    {
        int[] sides = new int[TileSides.GetLength(1)];
        for (int i = 0; i < TileSides.GetLength(1); i++)
            sides[i] = TileSides[(int)tile, i];
        return sides;
    }

    public void CreateTile(int up, int right, int down, int left)
    {
        int xPos = (int)transform.position.x;
        int yPos = (int)transform.position.z;
        //Generate adjacent tile states (wall: 0, path: 1, uninitalized: -1) (possible iteratively?)
        int[] outsideValues = { up, right, down, left };

        //pick a tile at random (shuffle bag method)
        List<StructureDefinitions.TileTypes> tileList = new List<StructureDefinitions.TileTypes>();
        tileList.Add(StructureDefinitions.TileTypes.Cross);
        tileList.Add(StructureDefinitions.TileTypes.Deadend);
        tileList.Add(StructureDefinitions.TileTypes.Elbow);
        tileList.Add(StructureDefinitions.TileTypes.Straight);
        tileList.Add(StructureDefinitions.TileTypes.T);
        tileList = StructureDefinitions.Shuffle(tileList);
        Queue<StructureDefinitions.TileTypes> tileQueue = new Queue<StructureDefinitions.TileTypes>(tileList);
        tileQueue.Enqueue(StructureDefinitions.TileTypes.Deadend);
        tileQueue.Enqueue(StructureDefinitions.TileTypes.Empty);

        bool fits = false;
        while (!fits)
        {
            var tile = tileQueue.Dequeue();
            int[] tileints = GetTileFaces(tile);
            //if tile is fully rotated thrice and none fit, throw out and grab new tile from bag
            for (int rot = 0; rot < 4; rot++)
            {
                //check if tile can fit into map
                if((outsideValues[0] == tileints[0] || outsideValues[0] == -1) && (outsideValues[1] == tileints[1] || outsideValues[1] == -1) 
                    && (outsideValues[2] == tileints[2] || outsideValues[2] == -1) && (outsideValues[3] == tileints[3] || outsideValues[3] == -1))
                {
                    fits = true;
                    TileData t = Instantiate(tileObjects[(int)tile], new Vector3(xPos, 0, yPos), Quaternion.Euler(0, rot * _degPerRotation, 0), this.transform.parent).GetComponent<TileData>();
                    t.x = xPos;
                    t.y = yPos;
                    t.up = tileints[0];
                    t.right = tileints[1];
                    t.down = tileints[2];
                    t.left = tileints[3];
                    Destroy(this.gameObject);
                    break;
                }
                tileints = ShiftFaceValues(tileints);
            }
            //repeat until valid tile is found
        }
    }
}
