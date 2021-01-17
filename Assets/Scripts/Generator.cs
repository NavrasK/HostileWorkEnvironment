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
    private Transform player;
    private (float x, float y) playerpos;

    private void Start() {
        player = GameObject.Find("ECM_FirstPerson").transform;
        playerpos = (0, 0);
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

    private void SpawnTile(int x, int y, bool addEdges = false) {
        Vector3 pos = new Vector3(x * 10, 0, y * 10);
        Tile t = Instantiate(tileGen, pos, Quaternion.identity, this.transform).GetComponent<Tile>();
        int[] neighbours = { -1, -1, -1, -1 };

        TileData n0 = neighbourRaycast(pos + new Vector3(0, 12, 10));
        if (n0 == null) {
            AddEdge(addEdges, x, y + 1);
		} else {
            neighbours[0] = n0.down;
		}

        TileData n1 = neighbourRaycast(pos + new Vector3(10, 12, 0));
        if (n1 == null) {
            AddEdge(addEdges, x + 1, y);
		} else {
            neighbours[1] = n1.left;
		}

        TileData n2 = neighbourRaycast(pos + new Vector3(0, 12, -10));
        if (n2 == null) {
            AddEdge(addEdges, x, y - 1);
		} else {
            neighbours[2] = n2.up;
		}

        TileData n3 = neighbourRaycast(pos + new Vector3(-10, 12, 0));
        if (n3 == null) {
            AddEdge(addEdges, x - 1, y);
		} else {
            neighbours[3] = n3.right;
		}

        t.CreateTile(neighbours[0], neighbours[1], neighbours[2], neighbours[3]);
    }

    private TileData neighbourRaycast(Vector3 pos) {
        RaycastHit hit;
        if (Physics.Raycast(pos, Vector3.down, out hit, Mathf.Infinity, groundLayer)) {
            TileData data = hit.collider.gameObject.GetComponent<TileData>();
            if (data != null) {
                return data;
            }
        }
        return null;
    }

    private void AddEdge(bool add, int x, int y) {
        if (add && !edges.Contains((x, y))) {
            Debug.Log($"Adding edge at ({x}, {y})");
            edges.Add((x, y));
        }
	}

    private void Update() {
        // If the player has moved far enough, store their position, then start a new chunk of generation (filling in edges)
        Vector2 delta = new Vector2(player.position.x - playerpos.x, player.position.z - playerpos.y);
        if (delta.magnitude > 10) {
            Debug.Log(">> GENERATING");
            playerpos = (player.position.x, player.position.z);
            TileData currTile = neighbourRaycast(player.position + Vector3.up * 12);
            Debug.Log($"{currTile.x}, {currTile.y}");
            (int x, int y) reprojectOffset = ((int)(currTile.x - initialSize / 2), (int)(currTile.y - initialSize / 2));
            for (int i = 0; i < initialSize; i++) {
                for (int j = 0; j < initialSize; j++) {
                    (int x, int y) pos = (i + reprojectOffset.x, j + reprojectOffset.y);
                    if (edges.Contains(pos)) {
                        Debug.Log($"Checking edge at {pos.x}, {pos.y}");
                        SpawnTile(pos.x, pos.y, true);
                        edges.RemoveAll(p => p == pos);
                    }
                }
            }
        }
    }
}
