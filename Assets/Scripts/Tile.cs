using UnityEngine;

public class Tile : MonoBehaviour
{
    private Vector3 offSet = new Vector3(1, 0, 1);
    private bool hasPiece = false;
    private GameObject currentPiece;

    private Board board;
    private GameUI gameUI;

    public Vector2Int IndexInMatrix;

    private void Start()
    {
        board = Board.Instance;
        gameUI = GameUI.Instance;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (board.beforeGame)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    Tile tile = hit.transform.GetComponent<Tile>();
                    if (tile != null && tile.hasPiece)
                    {
                        int pieceIndex = (int)tile.currentPiece.GetComponent<Piece>().Type - 1;

                        board.NumberOfPieces[pieceIndex]++;
                        gameUI.ChangePieceNumber(pieceIndex, 1);

                        gameUI.ChangeTextColor(pieceIndex, Color.green);

                        Destroy(tile.currentPiece);

                        tile.hasPiece = false;
                        tile.currentPiece = null;
                    }
                }
            }
        }
    }

    public bool CanCreate(GameObject piece)
    {
        if (CreatePiece(piece) != null)
            return true;
        return false;
    }

    public Piece CreatePiece(GameObject piece)
    {
        int pieceIndex = (int)piece.GetComponent<Piece>().Type - 1;

        if (!hasPiece)
        {
            if (board.NumberOfPieces[pieceIndex] > 0)
            {
                board.NumberOfPieces[pieceIndex]--;
                gameUI.ChangePieceNumber(pieceIndex, -1);
            }
            else
                return null;

            currentPiece = Instantiate(piece, GetBuildPosition(), Quaternion.identity);
            hasPiece = true;
        }

        if (board.NumberOfPieces[pieceIndex] <= 0)
            gameUI.ChangeTextColor(pieceIndex, Color.red);
        else
            gameUI.ChangeTextColor(pieceIndex, Color.green);

        return currentPiece.GetComponent<Piece>();
    }

    public Vector3 GetBuildPosition()
    {
        return transform.position + offSet;
    }
}
