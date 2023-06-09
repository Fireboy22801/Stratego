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
        for (int i = CurrentY - 1; i >= 0; i--)
        {
            if (board[CurrentX, i] == null)
                moves.Add(new Vector2Int(CurrentX, i));

            else
            {
                if (board[CurrentX, i].Team != this.Team && board[CurrentX, i].Type != 0)
                    moves.Add(new Vector2Int(CurrentX, i));

                break;
            }
        }

        //Up
        for (int i = CurrentY + 1; i < tileCountY; i++)
        {
            if (board[CurrentX, i] == null)
                moves.Add(new Vector2Int(CurrentX, i));

            else
            {
                if (board[CurrentX, i].Team != this.Team && board[CurrentX, i].Type != 0)
                    moves.Add(new Vector2Int(CurrentX, i));

                break;
            }
        }

        //Left
        for (int i = CurrentX - 1; i >= 0; i--)
        {
            if (board[i, CurrentY] == null)
                moves.Add(new Vector2Int(i, CurrentY));

            else
            {
                if (board[i, CurrentY].Team != this.Team && board[i, CurrentY].Type != 0)
                    moves.Add(new Vector2Int(i, CurrentY));

                break;
            }
        }

        //Right
        for (int i = CurrentX + 1; i < tileCountX; i++)
        {
            if (board[i, CurrentY] == null)
                moves.Add(new Vector2Int(i, CurrentY));

            else
            {
                if (board[i, CurrentY].Team != this.Team && board[i, CurrentY].Type != 0)
                    moves.Add(new Vector2Int(i, CurrentY));

                break;
            }
        }

        return moves;
    }
}
