using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour {
    [SerializeField] [Min(1)]
    private int initialSize = 11;
    [SerializeField]
    private GameObject tileGen;
    [SerializeField]
    private LayerMask groundLayer;
    private List<(int x, int y)> edges = new List<(int x, int y)>();
    private (int x, int y) offset;

    private void Start() {
        offset = ((int)(transform.position.x - (initialSize / 2)), (int)(transform.position.z - (initialSize / 2)));
        SetupLevel();
    }

    private void SetupLevel() {
        for (int i = 0; i < initialSize; i++) {
            for (int j = 0; j < initialSize; j++) {
                (int, int) pos = (i + offset.x, j + offset.y);
                if (pos != (0,0)) edges.Add(pos);
            }
        }

        // Fill from center out OR Fill at random
        Tile t = Instantiate(tileGen, Vector3.zero, Quaternion.identity, this.transform).GetComponent<Tile>();
        t.CreateTile(1, 1, 1, 1);
        while (edges.Count > 0) {
            edges = StructureDefinitions.Shuffle(edges);
            SpawnTile(edges[0].x, edges[0].y);
            edges.RemoveAt(0);
		}

        // Clear out edges and fill with all tiles directly outside of instatiation area
        edges.Clear();
        for (int i = 0; i < initialSize; i++) {
            edges.Add((i + offset.x, offset.y - 1));
            edges.Add((i + offset.x, -offset.y + 1));
            edges.Add((-offset.x + 1, i + offset.y));
            edges.Add((offset.x - 1, i + offset.y));
        }
    }

    private void SpawnTile(int x, int y) {
        Vector3 pos = new Vector3(x, 0, y);
        Tile t = Instantiate(tileGen, pos, Quaternion.identity, this.transform).GetComponent<Tile>();
        int[] neighbours = { -1, -1, -1, -1 };
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.back + Vector3.up, Vector3.down, out hit, Mathf.Infinity, groundLayer)) {
            var data = hit.collider.gameObject.GetComponent<TileData>();
            if (data != null) {
                neighbours[0] = data.down;
            }
        }
        if (Physics.Raycast(pos + Vector3.left + Vector3.up, Vector3.down, out hit, Mathf.Infinity, groundLayer)) {
            var data = hit.collider.gameObject.GetComponent<TileData>();
            if (data != null) {
                neighbours[1] = data.left;
            }
        }
        if (Physics.Raycast(pos + Vector3.forward + Vector3.up, Vector3.down, out hit, Mathf.Infinity, groundLayer)) {
            var data = hit.collider.gameObject.GetComponent<TileData>();
            if (data != null) {
                neighbours[2] = data.up;
            }
        }
        if (Physics.Raycast(pos + Vector3.right + Vector3.up, Vector3.down, out hit, Mathf.Infinity, groundLayer)) {
            var data = hit.collider.gameObject.GetComponent<TileData>();
            if (data != null) {
                neighbours[3] = data.right;
            }
        }
        t.CreateTile(neighbours[0], neighbours[1], neighbours[2], neighbours[3]); // placeholder
    }

    private void Update() {
        // If the player has moved far enough, store their position, then start a new chunk of generation (filling in edges)
    }
}
