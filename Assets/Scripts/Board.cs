using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using Photon.Realtime;
using TMPro;
using System.Linq;
using JetBrains.Annotations;

public class Board : MonoBehaviourPunCallbacks
{
    public static Board Instance;

    [Header("Art stuff")]
    [SerializeField] private Material tileMaterialWhite;
    [SerializeField] private Material tileMaterialBlack;
    [SerializeField] private Material tileMaterialLake;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float dragOffset = 1f;
    [SerializeField] private float timeToHide = 3f;
    [SerializeField] private int timeBeforeStart = 20;

    [Header("Prefabs & Materials")]

    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material enemyMaterial;
    [SerializeField] private Mesh enemyMesh;

    [SerializeField] public int[] NumberOfPieces = { 1, 8, 5, 4, 4, 4, 3, 2, 1, 1, 6, 1 };
    private GameUI gameUI;

    private new PhotonView photonView;

    private Piece[,] pieces;

    public List<Vector2Int> availableMoves = new List<Vector2Int>();

    private List<Piece> deadWhites = new List<Piece>();
    private List<Piece> deadBlacks = new List<Piece>();

    private Piece currentlyDragging;

    public bool beforeGame = true;

    private const int TILE_COUNT_X = 10;
    private const int TILE_COUNT_Y = 10;

    private GameObject[,] tiles;

    private GameObject selectedPiece;

    private Camera currentCamera;

    private Vector2Int currentHover;

    private Vector3 bounds;

    private int playerCount = 1;

    public bool mineTurn;

    private void Awake()
    {
        Instance = this;

        mineTurn = true;

        pieces = new Piece[TILE_COUNT_X, TILE_COUNT_Y];

        GenerateAllTiles(TILE_COUNT_X, TILE_COUNT_Y);

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        photonView = GetComponent<PhotonView>();

        gameUI = GameUI.Instance;
        gameUI.scrollbar.SetActive(false);

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player Entered");
        playerCount++;
        if (playerCount == 2)
        {
            mineTurn = true;

            StartCoroutine(Countdown(timeBeforeStart));
            gameUI.scrollbar.SetActive(true);

            photonView.RPC("CountdownPhoton", RpcTarget.Others);
            photonView.RPC("ShowScrollBar", RpcTarget.Others);

            StartCoroutine(StartGame(timeBeforeStart));
        }
    }

    private void Update()
    {
        Application.targetFrameRate = 60;

        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        if (beforeGame)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
                {
                    Tile tile = hit.transform.GetComponent<Tile>();

                    if (selectedPiece != null)
                    {
                        if (tile.CanCreate(selectedPiece))
                        {
                            pieces[tile.IndexInMatrix.x, tile.IndexInMatrix.y] = tile.CreatePiece(selectedPiece);
                            pieces[tile.IndexInMatrix.x, tile.IndexInMatrix.y].CurrentXIndex = tile.IndexInMatrix.x;
                            pieces[tile.IndexInMatrix.x, tile.IndexInMatrix.y].CurrentYIndex = tile.IndexInMatrix.y;
                        }
                    }
                }
            }
        }
        else
        {
            RaycastHit info;
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
            {
                Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

                if (currentHover == -Vector2Int.one)
                {
                    currentHover = hitPosition;
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                }

                if (currentHover != hitPosition)
                {
                    tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                    currentHover = hitPosition;
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (pieces[hitPosition.x, hitPosition.y] != null)
                    {
                        if (pieces[hitPosition.x, hitPosition.y].Team == 0 && mineTurn)
                        {
                            currentlyDragging = pieces[hitPosition.x, hitPosition.y];

                            if (currentlyDragging.Type != PieceType.Bomb && currentlyDragging.Type != PieceType.Flag)
                                availableMoves = currentlyDragging.GetAvailableMoves(pieces, TILE_COUNT_X, TILE_COUNT_Y);
                        }
                        HighlightTiles();
                    }
                }

                if (currentlyDragging != null && Input.GetMouseButtonUp(0))
                {
                    Vector2Int previousPosition = new Vector2Int(currentlyDragging.CurrentXIndex, currentlyDragging.CurrentYIndex);

                    bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                    if (!validMove)
                        currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));

                    MoveToCoordinates(previousPosition.x, previousPosition.y, hitPosition.x, hitPosition.y);
                    currentlyDragging = null;
                    RemoveHighlightTiles();
                }
            }
            else
            {
                if (currentHover != -Vector2Int.one)
                {
                    tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                    currentHover = -Vector2Int.one;
                }

                if (currentlyDragging && Input.GetMouseButtonUp(0))
                {
                    currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.CurrentXIndex, currentlyDragging.CurrentYIndex));
                    currentlyDragging = null;
                    RemoveHighlightTiles();
                }
            }

            if (currentlyDragging)
            {
                Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
                float distance;
                if (horizontalPlane.Raycast(ray, out distance))
                    currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }
    }

    private void GenerateAllTiles(int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountY / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];

        for (int y = 0; y < tileCountY; y++)
        {
            for (int x = 0; x < tileCountX; x++)
            {

                if (x == 2 && y == 4 || x == 2 && y == 5 || x == 3 && y == 4 || x == 3 && y == 5
                    || x == 6 && y == 4 || x == 6 && y == 5 || x == 7 && y == 4 || x == 7 && y == 5)
                {
                    tiles[x, y] = GenerateSingleTile(tileSize, x, y, LayerMask.NameToLayer("Lake"), tileMaterialLake);
                    pieces[x, y] = SpawnSinglePiece(PieceType.Lake, 2);
                    PositionSinglePiece(x, y, true);
                }

                else if ((x + y) % 2 == 0)
                    tiles[x, y] = GenerateSingleTile(tileSize, x, y, LayerMask.NameToLayer("Tile"), tileMaterialWhite);

                else
                    tiles[x, y] = GenerateSingleTile(tileSize, x, y, LayerMask.NameToLayer("Tile"), tileMaterialBlack);
            }
        }

        if (beforeGame)
            ChangeTilesLayer(tileCountX, tileCountY, LayerMask.NameToLayer("Unavailable"));
    }

    private void ChangeTilesLayer(int tileCountX, int tileCountY, LayerMask layer)
    {
        for (int y = tileCountY / 2 - 1; y < tileCountY; y++)
        {
            for (int x = 0; x < tileCountX; x++)
            {
                if (x == 2 && y == 4 || x == 2 && y == 5 || x == 3 && y == 4 || x == 3 && y == 5
                    || x == 6 && y == 4 || x == 6 && y == 5 || x == 7 && y == 4 || x == 7 && y == 5)
                {
                    continue;
                }
                else
                    ChangeTileLayer(x, y, layer);
            }
        }
    }

    private void ChangeTileLayer(int x, int y, LayerMask layer)
    {
        tiles[x, y].layer = layer;
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y, LayerMask layer, Material material)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;
        tileObject.transform.localPosition = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = material;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0, 0, tileSize);
        vertices[2] = new Vector3(tileSize, 0, 0);
        vertices[3] = new Vector3(tileSize, 0, tileSize);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = layer;
        tileObject.AddComponent<BoxCollider>();

        tileObject.AddComponent<Tile>().IndexInMatrix = new Vector2Int(x, y);

        return tileObject;
    }

    private Piece SpawnSinglePiece(PieceType type, int team)
    {
        Piece piece;

        piece = Instantiate(prefabs[(int)type], transform).GetComponent<Piece>();

        piece.Type = type;
        piece.Team = team;

        if (team == 1)
        {
            piece.GetComponent<MeshFilter>().mesh = enemyMesh;
            piece.GetComponent<MeshRenderer>().material = enemyMaterial;

            MeshFilter[] meshFilters = piece.GetComponentsInChildren<MeshFilter>();

            if (meshFilters.Length > 1)
            {
                meshFilters[1].GetComponent<MeshRenderer>().enabled = false;
            }
        }

        return piece;
    }


    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        pieces[x, y].CurrentXIndex = x;
        pieces[x, y].CurrentYIndex = y;
        pieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private void PositionPiecesInRandomPlaces()
    {
        for (int i = 0; i < NumberOfPieces.Length; i++)
        {
            while (NumberOfPieces[i] != 0)
            {
                int x;
                int y;

                x = Random.Range(0, TILE_COUNT_X);
                y = Random.Range(0, TILE_COUNT_Y / 2 - 1);

                if (pieces[x, y] == null)
                {
                    pieces[x, y] = SpawnSinglePiece((PieceType)(i + 1), 0);
                    PositionSinglePiece(x, y, true);
                    NumberOfPieces[i]--;
                }
            }
        }
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        availableMoves.Clear();
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int position)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == position.x && moves[i].y == position.y)
                return true;

        return false;
    }
    private bool MoveTo(Piece piece, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
            return false;

        Vector2Int previousPosition = new Vector2Int(piece.CurrentXIndex, piece.CurrentYIndex);

        if (pieces[x, y] != null)
        {
            Piece otherPiece = pieces[x, y];

            if (piece.Team == otherPiece.Team)
                return false;

            if (piece.Type != PieceType.Miner && otherPiece.Type == PieceType.Bomb)
            {
                pieces[previousPosition.x, previousPosition.y] = null;

                AddToDeadList(piece);
                ShowPiece(piece);
                ShowPiece(otherPiece);

                HidePiece(otherPiece);

                if (!beforeGame)
                    mineTurn = !mineTurn;

                return true;
            }
            else if (piece.Type == PieceType.Miner && otherPiece.Type == PieceType.Bomb)
            {
                AddToDeadList(otherPiece);
                ShowPiece(piece);
                ShowPiece(otherPiece);

                HidePiece(piece);

                pieces[x, y] = piece;
                pieces[previousPosition.x, previousPosition.y] = null;

                PositionSinglePiece(x, y);

                if (!beforeGame)
                    mineTurn = !mineTurn;

                return true;
            }
            else if (piece.Type == PieceType.Spy && otherPiece.Type == PieceType.Marshal)
            {
                AddToDeadList(otherPiece);
                ShowPiece(piece);
                ShowPiece(otherPiece);

                HidePiece(piece);

                pieces[x, y] = piece;
                pieces[previousPosition.x, previousPosition.y] = null;

                PositionSinglePiece(x, y);

                if (!beforeGame)
                    mineTurn = !mineTurn;

                return true;
            }

            if (otherPiece.Type == PieceType.Flag)
            {
                AddToDeadList(otherPiece);
                ShowPiece(piece);
                ShowPiece(otherPiece);

                pieces[x, y] = piece;
                pieces[previousPosition.x, previousPosition.y] = null;

                PositionSinglePiece(x, y);

                if (!beforeGame)
                    mineTurn = !mineTurn;

                GameOver();

                return true;
            }

            if (piece.Type > otherPiece.Type)
            {
                AddToDeadList(otherPiece);

                ShowPiece(piece);
                ShowPiece(otherPiece);

                HidePiece(piece);

                pieces[x, y] = piece;
                pieces[previousPosition.x, previousPosition.y] = null;

                PositionSinglePiece(x, y);

                if (!beforeGame)
                    mineTurn = !mineTurn;

                return true;
            }
            else if (piece.Type < otherPiece.Type)
            {
                pieces[previousPosition.x, previousPosition.y] = null;

                AddToDeadList(piece);

                ShowPiece(piece);
                ShowPiece(otherPiece);

                HidePiece(otherPiece);

                if (!beforeGame)
                    mineTurn = !mineTurn;

                return true;
            }
            else
            {
                ShowPiece(piece);
                ShowPiece(otherPiece);

                AddToDeadList(otherPiece);
                AddToDeadList(piece);

                pieces[x, y] = null;
                pieces[previousPosition.x, previousPosition.y] = null;

                if (!beforeGame)
                    mineTurn = !mineTurn;

                return true;
            }
        }

        pieces[x, y] = piece;
        pieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        if (!beforeGame)
            mineTurn = !mineTurn;

        return true;
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int y = 0; y < TILE_COUNT_Y; y++)
            for (int x = 0; x < TILE_COUNT_X; x++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
        return -Vector2Int.one;
    }

    private void AddToDeadList(Piece piece)
    {
        if (piece.Team == 0)
        {
            deadWhites.Add(piece);
            piece.SetScale(Vector3.one * deathSize);
            piece.SetPosition(
                new Vector3(TILE_COUNT_X * tileSize, yOffset, -1 * tileSize)
                - bounds
                + new Vector3(tileSize / 2, 0, tileSize / 2)
                + (Vector3.forward * deathSpacing) * deadWhites.Count);
            piece.transform.Rotate(0, -90, 0);
        }
        else
        {
            deadBlacks.Add(piece);
            piece.SetScale(Vector3.one * deathSize);
            piece.SetPosition(
                new Vector3(-1 * tileSize, yOffset, TILE_COUNT_X * tileSize)
                - bounds
                + new Vector3(tileSize / 2, 0, tileSize / 2)
                + (Vector3.back * deathSpacing) * deadBlacks.Count);
            piece.transform.Rotate(0, 270, 0);
        }
    }

    public void MoveToCoordinates(int x, int y, int finalX, int finalY)
    {
        photonView.RPC("MovePhoton", RpcTarget.Others, x, y, finalX, finalY);
    }

    public void ChangeTurn()
    {
        photonView.RPC("ChangeTurnPhoton", RpcTarget.Others);
    }

    [PunRPC]
    private void MovePhoton(int x, int y, int finalX, int finalY)
    {
        x = TILE_COUNT_X - x - 1;
        y = TILE_COUNT_Y - y - 1;
        finalX = TILE_COUNT_X - finalX - 1;
        finalY = TILE_COUNT_Y - finalY - 1;

        if (pieces[x, y] != null)
        {
            if (pieces[x, y].Type != PieceType.Bomb && pieces[x, y].Type != PieceType.Flag)
                availableMoves = pieces[x, y].GetAvailableMoves(pieces, TILE_COUNT_X, TILE_COUNT_Y);

            MoveTo(pieces[x, y], finalX, finalY);

            RemoveHighlightTiles();
        }

    }

    [PunRPC]
    private void ChangeTurnPhoton()
    {
        mineTurn = !mineTurn;
    }

    [PunRPC]
    private void StartGamePhoton()
    {
        PositionPiecesInRandomPlaces();

        gameUI.scrollbar.SetActive(false);

        beforeGame = false;

        mineTurn = false;

        StartCoroutine(Countdown(5));

        //Wait 5 seconds
        Invoke("StartGameAfterDelay", 5);
    }


    [PunRPC]
    private void HidePiecePhoton(int x, int y)
    {
        x = TILE_COUNT_X - x - 1;
        y = TILE_COUNT_Y - y - 1;

        if (pieces[x, y] != null)
        {
            if (pieces[x, y].Team == 1)
            {
                pieces[x, y].GetComponent<MeshFilter>().mesh = enemyMesh;
                pieces[x, y].GetComponent<MeshRenderer>().material = enemyMaterial;

                MeshFilter[] meshFilters = pieces[x, y].GetComponentsInChildren<MeshFilter>();

                if (meshFilters.Length > 1)
                {
                    meshFilters[1].GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
    }

    [PunRPC]
    private void CountdownPhoton()
    {
        StartCoroutine(Countdown(timeBeforeStart));
    }

    [PunRPC]
    private void ShowScrollBar()
    {
        gameUI.scrollbar.SetActive(true);
    }

    [PunRPC]
    private void PlaceEnemyPhoton(int x, int y, int type)
    {
        x = TILE_COUNT_X - x - 1;
        y = TILE_COUNT_Y - y - 1;
        pieces[x, y] = SpawnSinglePiece((PieceType)type, 1);
        pieces[x, y].transform.Rotate(0, 180, 0);
        PositionSinglePiece(x, y, true);
    }

    private void GameOver()
    {
        Time.timeScale = 0f;
    }

    private void ShowPiece(Piece piece)
    {
        piece.GetComponent<MeshFilter>().mesh = prefabs[(int)piece.Type].GetComponent<MeshFilter>().sharedMesh;
        piece.GetComponent<MeshRenderer>().material = prefabs[(int)piece.Type].GetComponent<MeshRenderer>().sharedMaterial;

        MeshFilter[] meshFilters = piece.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length > 1)
        {
            meshFilters[1].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    private void HidePiece(Piece piece)
    {
        StartCoroutine(HidePieceDelayed(piece));
    }

    IEnumerator HidePieceDelayed(Piece piece)
    {
        yield return new WaitForSeconds(timeToHide);
        photonView.RPC("HidePiecePhoton", RpcTarget.Others, piece.CurrentXIndex, piece.CurrentYIndex);
    }

    IEnumerator StartGame(int delay)
    {
        yield return new WaitForSeconds(delay);

        ChangeTilesLayer(TILE_COUNT_X, TILE_COUNT_Y, LayerMask.NameToLayer("Tile"));

        gameUI.scrollbar.SetActive(false);

        PositionPiecesInRandomPlaces();

        photonView.RPC("StartGamePhoton", RpcTarget.Others);

        beforeGame = false;

        mineTurn = false;

        StartCoroutine(Countdown(5));

        mineTurn = true;

        for (int y = 0; y < pieces.GetLength(0) / 2 - 1; y++)
        {
            for (int x = 0; x < pieces.GetLength(1); x++)
            {
                if (pieces[x, y] != null)
                {
                    photonView.RPC("PlaceEnemyPhoton", RpcTarget.Others, x, y, (int)pieces[x, y].Type);
                }
            }
        }

    }

    IEnumerator Countdown(int seconds)
    {
        while (seconds > 0)
        {
            gameUI.timer.text = seconds.ToString();
            yield return new WaitForSeconds(1);
            seconds--;
        }
        gameUI.timer.text = "0";
    }

    private void StartGameAfterDelay()
    {
        for (int y = 0; y < pieces.GetLength(0) / 2 - 1; y++)
        {
            for (int x = 0; x < pieces.GetLength(1); x++)
            {
                if (pieces[x, y] != null)
                {
                    photonView.RPC("PlaceEnemyPhoton", RpcTarget.Others, x, y, (int)pieces[x, y].Type);
                }
            }
        }
    }

    public void SelectSpy()
    {
        SelectPiece(prefabs[1]);
    }
    public void SelectScout()
    {
        SelectPiece(prefabs[2]);
    }
    public void SelectMiner()
    {
        SelectPiece(prefabs[3]);
    }
    public void SelectSergeant()
    {
        SelectPiece(prefabs[4]);
    }
    public void SelectLieutenant()
    {
        SelectPiece(prefabs[5]);
    }
    public void SelectCaptain()
    {
        SelectPiece(prefabs[6]);
    }
    public void SelectMajor()
    {
        SelectPiece(prefabs[7]);
    }
    public void SelectColonel()
    {
        SelectPiece(prefabs[8]);
    }
    public void SelectGeneral()
    {
        SelectPiece(prefabs[9]);
    }
    public void SelectMarshal()
    {
        SelectPiece(prefabs[10]);
    }
    public void SelectBomb()
    {
        SelectPiece(prefabs[11]);
    }
    public void SelectFlag()
    {
        SelectPiece(prefabs[12]);
    }


    private void SelectPiece(GameObject piece)
    {
        selectedPiece = piece;
    }
}
