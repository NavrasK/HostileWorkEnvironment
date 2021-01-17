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
        // Start with an X by X square centered at (0,0)
        offset = ((int)(transform.position.x - (initialSize / 2)), (int)(transform.position.z - (initialSize / 2)));
        SetupLevel();

        // Set clear out edges and fill with any tiles outside of instatiation area
        edges.Clear();
        // TODO
    }

    private void SetupLevel() {
        for (int i = 0; i < initialSize; i++) {
            for (int j = 0; j < initialSize; j++) {
                edges.Add((i - offset.x, j - offset.y));
            }
        }
        
        // Fill from center out OR Fill at random
        
    }

    private void SpawnTile(int x, int y) {
        Vector3 pos = new Vector3(x, 0, y);
        Tile t = Instantiate(tileGen, pos, Quaternion.identity, this.transform).GetComponent<Tile>();
        int[] neighbours = { -1, -1, -1, -1 };
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.back, Vector3.down, out hit, Mathf.Infinity, groundLayer)) {
            Debug.Log("Up");
            var data = hit.collider.gameObject.GetComponent<TileData>();
            if (data != null) {
                neighbours[0] = data.down;
			}
		}
        if (Physics.Raycast(pos + Vector3.left, Vector3.down, out hit, Mathf.Infinity, groundLayer)) {
            Debug.Log("Right");
            var data = hit.collider.gameObject.GetComponent<TileData>();
            if (data != null) {
                neighbours[1] = data.left;
            }
        }
        if (Physics.Raycast(pos + Vector3.forward, Vector3.down, out hit, Mathf.Infinity, groundLayer)) {
            Debug.Log("Down");
            var data = hit.collider.gameObject.GetComponent<TileData>();
            if (data != null) {
                neighbours[2] = data.up;
            }
        }
        if (Physics.Raycast(pos + Vector3.right, Vector3.down, out hit, Mathf.Infinity, groundLayer)) {
            Debug.Log("Left");
            var data = hit.collider.gameObject.GetComponent<TileData>();
            if (data != null) {
                neighbours[3] = data.right;
            }
        }
        t.CreateTile(neighbours[0], neighbours[1], neighbours[2], neighbours[3]); // placeholder
	}

    private void Update() {
        // If the player has moved far enough, start a new chunk of generation and store the current position for next check
    }
}
