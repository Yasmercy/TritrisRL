using System;
using System.Text;

namespace Game;
public static class Globals
{
    // note: changing the statics requires changing To2D
    public static int NUM_ROWS = 6;
    public static int NUM_COLS = 4;
    public static int SIZE = NUM_ROWS * NUM_COLS;
}

public enum Action
{
    Drop = 0,
    Left = 1,
    Right = 2,
}
public record State(bool[] Board, int PieceType, int PieceRow, int PieceCol)
{
    #region StaticMembers
    public static int SpawnRow = 4;
    public static int SpawnCol = 2;
    static int[,,] RelPieceIndex = {
            { {-1, 0}, {0, 0}, {1, 0}},
            { {0, -1}, {0 ,0}, {0, 1}},
            { {0, 0}, {0, 1}, {1, 0}},
            { {0, -1}, {0, 0}, {0, 1}},
            { {-1, 0}, {0, -1}, {0, 0}},
            { {-1, 0}, {0 ,0}, {0, 1}}
    };
    #endregion
    #region FieldDescription
    /* 
    Board
        - 1d array (Globals.SIZE), representing a 2d board (8x4)
        - fills up the board from bottom to top, left to right
        - supports an index function, State.index(row, col)
    PieceType
        - 0: |, 1: ---
        - 2: L, 3: ⅃, 4: ⅂, 5: Γ 
    PieceRow, PieceCol
        - integers representing the center of the piece
        - line pieces center is the middle
        - elbow pieces center is in the bend
    */
    #endregion

    #region FieldHelpers
    public State(State state, int dR, int dC) : this(state.Board, state.PieceType, state.PieceRow + dR, state.PieceCol + dC) { }
    public State() : this(new bool[Globals.SIZE], (new Random()).Next(0, 6), SpawnRow, SpawnCol) {}
    public override string ToString()
    {
        (int, int)[] pos = {
            (RelPieceIndex[PieceType, 0, 0], RelPieceIndex[PieceType, 0, 1]),
            (RelPieceIndex[PieceType, 1, 0], RelPieceIndex[PieceType, 1, 1]),
            (RelPieceIndex[PieceType, 2, 0], RelPieceIndex[PieceType, 2, 1])
        };

        StringBuilder sb = new StringBuilder(Globals.SIZE + Globals.NUM_ROWS);
        for (var r = Globals.NUM_ROWS - 1; r >= 0; --r) {
            for (var c = 0; c < Globals.NUM_COLS; ++c) {
                if (pos.Any(m => m == (r - PieceRow, c - PieceCol)))
                    sb.Append("x ");
                //     sb.Append("□ ");
                else if (IndexBoard(r, c) ?? true)
                    sb.Append("■ ");
                else
                    sb.Append("- ");
                //     sb.Append("⬚ ");
            }
            if (r != 0)
                sb.Append('\n');
        }
        return sb.ToString();

    }
    bool? IndexBoard(int row, int col) => 
        (row < 0 || row >= Globals.NUM_ROWS || col < 0 || col >= Globals.NUM_COLS) 
        ? null : Board[row * Globals.NUM_COLS + col];

    static bool[] From2D(bool[][] board) => board.SelectMany(i => i).ToArray<bool>();

    static bool[][] To2D(bool[] board) => 
    [
        [board[0], board[1], board[2], board[3]],
        [board[4], board[5], board[6], board[7]],
        [board[8], board[9], board[10], board[11]],
        [board[12], board[13], board[14], board[15]],
        [board[16], board[17], board[18], board[19]],
        [board[20], board[21], board[22], board[23]],
        // [board[24], board[25], board[26], board[27]],
        // [board[28], board[29], board[30], board[31]],
    ];

    static void ShiftDown(bool[][] board, int row) {
        for (var r = row; r < Globals.NUM_ROWS - 1; ++r)
        {
            for (var c = 0; c < Globals.NUM_COLS; ++c)
            {
                board[r][c] = board[r + 1][c];
                board[r + 1][c] = false;
            }
        }
    }

    static bool FullRow(bool[][] board, int row) => board[row].All(m => m);

    public double[] FeatureVector()
    {
        double[] ret = new double[Globals.SIZE + 8];

        // Globals.SIZE len board bitvector
        for (var i = 0; i < Globals.SIZE; ++i)
            ret[i] = Board[i] ? 1 : 0;

        // 6 len piece bitvector
        ret[Globals.SIZE + PieceType] = 1;

        // 2 len row/col
        ret[Globals.SIZE + 6] = PieceRow;
        ret[Globals.SIZE + 7] = PieceCol;

        return ret;
    }
    #endregion

    #region Actions
    bool InvalidMove(int dR, int dC) =>
        (IndexBoard(RelPieceIndex[PieceType, 0, 0] + PieceRow + dR, RelPieceIndex[PieceType, 0, 1] + PieceCol + dC) ?? true) ||
        (IndexBoard(RelPieceIndex[PieceType, 1, 0] + PieceRow + dR, RelPieceIndex[PieceType, 1, 1] + PieceCol + dC) ?? true) ||
        (IndexBoard(RelPieceIndex[PieceType, 2, 0] + PieceRow + dR, RelPieceIndex[PieceType, 2, 1] + PieceCol + dC) ?? true);


    State? DoLeft() => !InvalidMove(0, -1) ? new State(this, 0, -1) : null;
    State? DoRight() => !InvalidMove(0, 1) ? new State(this, 0, 1) : null;
    State? DoDrop()
    {
        State cpy = this;
        while (!cpy.InvalidMove(-1, 0))
            cpy = new State(cpy, -1, 0);

        // modify board with cpy
        bool[][] board = To2D(cpy.Board);
        board[RelPieceIndex[cpy.PieceType, 0, 0] + cpy.PieceRow][RelPieceIndex[cpy.PieceType, 0, 1] + cpy.PieceCol] = true;
        board[RelPieceIndex[cpy.PieceType, 1, 0] + cpy.PieceRow][RelPieceIndex[cpy.PieceType, 1, 1] + cpy.PieceCol] = true;
        board[RelPieceIndex[cpy.PieceType, 2, 0] + cpy.PieceRow][RelPieceIndex[cpy.PieceType, 2, 1] + cpy.PieceCol] = true;

        // clear lines
        for (var r = 0; r < Globals.NUM_ROWS; ++r) {
            if (FullRow(board, r)) {
                ShiftDown(board, r);
                --r;
            }
        }

        State ret = new State(From2D(board), (new Random()).Next(0, 6), SpawnRow, SpawnCol);
        return (ret.InvalidMove(0, 0)) ? null : ret;
    }

    State? DoGravity() => 
        InvalidMove(-1, 0) ? DoDrop() : new State(this, -1, 0);

    public State? DoAction(Action action)
    {
        switch (action)
        {
            case Action.Left:
                return DoLeft()?.DoGravity();
            case Action.Right:
                return DoRight()?.DoGravity();
            default:
                return DoDrop();
        }
    }

    public bool IsTerminal() => InvalidMove(0, 0);
    #endregion
}