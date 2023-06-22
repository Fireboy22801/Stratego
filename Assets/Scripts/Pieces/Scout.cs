using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scout : Piece
{
    public Scout()
    {
        Type = PieceType.Scout;
    }

    public override List<Vector2Int> GetAvailableMoves(Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        //Down
        for (int i = CurrentYIndex - 1; i >= 0; i--)
        {
            if (board[CurrentXIndex, i] == null)
                moves.Add(new Vector2Int(CurrentXIndex, i));

            else
            {
                if (board[CurrentXIndex, i].Team != this.Team && board[CurrentXIndex, i].Type != 0)
                    moves.Add(new Vector2Int(CurrentXIndex, i));

                break;
            }
        }

        //Up
        for (int i = CurrentYIndex + 1; i < tileCountY; i++)
        {
            if (board[CurrentXIndex, i] == null)
                moves.Add(new Vector2Int(CurrentXIndex, i));

            else
            {
                if (board[CurrentXIndex, i].Team != this.Team && board[CurrentXIndex, i].Type != 0)
                    moves.Add(new Vector2Int(CurrentXIndex, i));

                break;
            }
        }

        //Left
        for (int i = CurrentXIndex - 1; i >= 0; i--)
        {
            if (board[i, CurrentYIndex] == null)
                moves.Add(new Vector2Int(i, CurrentYIndex));

            else
            {
                if (board[i, CurrentYIndex].Team != this.Team && board[i, CurrentYIndex].Type != 0)
                    moves.Add(new Vector2Int(i, CurrentYIndex));

                break;
            }
        }

        //Right
        for (int i = CurrentXIndex + 1; i < tileCountX; i++)
        {
            if (board[i, CurrentYIndex] == null)
                moves.Add(new Vector2Int(i, CurrentYIndex));

            else
            {
                if (board[i, CurrentYIndex].Team != this.Team && board[i, CurrentYIndex].Type != 0)
                    moves.Add(new Vector2Int(i, CurrentYIndex));

                break;
            }
        }

        return moves;
    }
}
