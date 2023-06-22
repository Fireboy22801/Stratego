using Photon.Pun.Demo.Cockpit;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    Lake = 0,
    Spy = 1,
    Scout = 2,
    Miner = 3,
    Sergeant = 4,
    Lieutenant = 5,
    Captain = 6,
    Major = 7,
    Colonel = 8,
    General = 9,
    Marshal = 10,
    Bomb = 11,
    Flag = 12

}


public class Piece : MonoBehaviour
{
    public int Team;

    public int CurrentXIndex;
    public int CurrentYIndex;

    public PieceType Type;

    public int speed = 2;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    private void Start()
    {
        desiredPosition = new Vector3(transform.position.x, 0, transform.position.z);
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * speed);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * speed);
    }

    public virtual List<Vector2Int> GetAvailableMoves(Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        //Right
        if (CurrentXIndex + 1 <= tileCountX)
        {
            if (board[CurrentXIndex + 1, CurrentYIndex] == null)
                moves.Add(new Vector2Int(CurrentXIndex + 1, CurrentYIndex));
            else if (board[CurrentXIndex + 1, CurrentYIndex].Team != this.Team && board[CurrentXIndex + 1, CurrentYIndex].Type != 0)
                moves.Add(new Vector2Int(CurrentXIndex + 1, CurrentYIndex));
        }

        //Left
        if (CurrentXIndex - 1 >= 0)
        {
            if (board[CurrentXIndex - 1, CurrentYIndex] == null)
                moves.Add(new Vector2Int(CurrentXIndex - 1, CurrentYIndex));
            else if (board[CurrentXIndex - 1, CurrentYIndex].Team != this.Team && board[CurrentXIndex - 1, CurrentYIndex].Type != 0)
                moves.Add(new Vector2Int(CurrentXIndex - 1, CurrentYIndex));
        }

        //Up
        if (CurrentYIndex + 1 < tileCountY)
        {
            if (board[CurrentXIndex, CurrentYIndex + 1] == null)
                moves.Add(new Vector2Int(CurrentXIndex, CurrentYIndex + 1));
            else
                if (board[CurrentXIndex, CurrentYIndex + 1].Team != this.Team && board[CurrentXIndex, CurrentYIndex + 1].Type != 0)
                moves.Add(new Vector2Int(CurrentXIndex, CurrentYIndex + 1));
        }

        //Down

        if (CurrentYIndex - 1 >= 0)
        {
            if (board[CurrentXIndex, CurrentYIndex - 1] == null)
                moves.Add(new Vector2Int(CurrentXIndex, CurrentYIndex - 1));
            else
                if (board[CurrentXIndex, CurrentYIndex - 1].Team != this.Team && board[CurrentXIndex, CurrentYIndex - 1].Type != 0)
                moves.Add(new Vector2Int(CurrentXIndex, CurrentYIndex - 1));
        }

        return moves;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
            transform.position = desiredPosition;
    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
            transform.localScale = desiredScale;
    }

    public virtual List<Vector2Int> UnlockMoves(Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        for (int y = 0; y < tileCountY / 2 - 1; y++)
        {
            for (int x = 0; x < tileCountX; x++)
            {
                if (board[x, y] == null)
                    moves.Add(new Vector2Int(x, y));
            }
        }

        return moves;
    }
}
