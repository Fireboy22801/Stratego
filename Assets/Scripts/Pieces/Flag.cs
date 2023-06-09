using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : Piece
{
    public Flag()
    {
        Type = PieceType.Flag;
    }

    public override List<Vector2Int> GetAvailableMoves(Piece[,] board, int tileCountX, int tileCountY)
    {
        return null;
    }

    public override List<Vector2Int> UnlockMoves(Piece[,] board, int tileCountX, int tileCountY)
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
