using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PuzzleGame
{
    public partial class MainForm : Form
    {
        private const int GridSpace = 30;
        private const int GameEdgeLeft = 150;
        private const int GameEdgeRight = 450;

        private readonly System.Windows.Forms.Timer _timer;
        private readonly Random _random = new Random();

        private PlayPiece _fallingPiece = null!;
        private readonly List<Square> _gridPieces = new();

        private int _currentScore = 0;
        private int _currentLevel = 1;
        private int _linesCleared = 0;

        private int _ticks = 0;
        private int _updateEvery = 15;
        private int _updateEveryCurrent = 15;
        private float _fallSpeed = GridSpace * 0.5f;
        private bool _pauseGame = false;
        private bool _gameOver = false;

        private readonly Color[] _colors = new[]
        {
            ColorTranslator.FromHtml("#dca3ff"),
            ColorTranslator.FromHtml("#ff90a0"),
            ColorTranslator.FromHtml("#80ffb4"),
            ColorTranslator.FromHtml("#ff7666"),
            ColorTranslator.FromHtml("#70b3f5"),
            ColorTranslator.FromHtml("#b2e77d"),
            ColorTranslator.FromHtml("#ffd700"),
        };

        public MainForm()
        {
            // Ensure pixel-perfect canvas like index.html (600x540)
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(600, 540);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Text = "Game XEPHINH";

            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += (s, e) =>
            {
                if (!_pauseGame)
                {
                    _ticks++;
                    if (_ticks >= _updateEvery)
                    {
                        _ticks = 0;
                        _fallingPiece.Fall(_fallingPiece, _fallSpeed);
                    }
                }
                Invalidate();
            };

            ResetGame();
            _timer.Start();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_pauseGame && keyData != Keys.R) return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Left:
                    _fallingPiece.Input(Direction.Left);
                    return true;
                case Keys.Right:
                    _fallingPiece.Input(Direction.Right);
                    return true;
                case Keys.Up:
                    _fallingPiece.Input(Direction.Up);
                    return true;
                case Keys.Down:
                    _updateEvery = 2;
                    return true;
                case Keys.R:
                    ResetGame();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                _updateEvery = _updateEveryCurrent;
            }
            base.OnKeyUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var colorDark = ColorTranslator.FromHtml("#0d0d0d");
            var colorLight = ColorTranslator.FromHtml("#304550");
            var colorBackground = ColorTranslator.FromHtml("#e1eeb0");

            g.Clear(colorBackground);

            using (var brushSide = new SolidBrush(Color.FromArgb(25, 25, 25)))
            {
                g.FillRectangle(brushSide, GameEdgeRight, 0, 150, Height);
                g.FillRectangle(brushSide, 0, 0, GameEdgeLeft, Height);
            }

            using (var b = new SolidBrush(colorBackground))
            {
                g.FillRectangle(b, 450, 80, 150, 70);
                g.FillRectangle(b, 460, 405, 130, 130);
                g.FillRectangle(b, 460, 210, 130, 60);
                g.FillRectangle(b, 460, 280, 130, 60);
                g.FillRectangle(b, 460, 60, 130, 35);
            }

            using (var pen = new Pen(colorLight, 1))
            {
                g.FillRectangle(new SolidBrush(colorLight), 450, 85, 150, 20);
                g.FillRectangle(new SolidBrush(colorLight), 450, 110, 150, 4);
                g.FillRectangle(new SolidBrush(colorLight), 450, 140, 150, 4);

                using var p3 = new Pen(colorLight, 3);
                g.DrawRoundedRectangle(p3, new Rectangle(465, 65, 120, 25), 5);
                g.DrawRoundedRectangle(p3, new Rectangle(465, 410, 120, 120), 5);
                g.DrawRoundedRectangle(p3, new Rectangle(465, 215, 120, 50), 5);
                g.DrawRoundedRectangle(p3, new Rectangle(465, 285, 120, 50), 5);
            }

            using (var f = new Font("Segoe UI", 12, FontStyle.Regular))
            using (var fb = new SolidBrush(Color.Black))
            using (var fw = new SolidBrush(Color.White))
            {
                var sfCenter = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("Score", new Font("Segoe UI", 14, FontStyle.Bold), fb, new RectangleF(450, 70, 150, 30), sfCenter);
                g.DrawString("Level", new Font("Segoe UI", 14, FontStyle.Bold), fb, new RectangleF(450, 220, 150, 30), sfCenter);
                g.DrawString("Lines", new Font("Segoe UI", 14, FontStyle.Bold), fb, new RectangleF(450, 290, 150, 30), sfCenter);

                var sfRight = new StringFormat { Alignment = StringAlignment.Far };
                g.DrawString(_currentScore.ToString(), f, fb, new RectangleF(450, 120, 150, 30), sfRight);
                g.DrawString(_currentLevel.ToString(), f, fb, new RectangleF(450, 245, 150, 30), sfRight);
                g.DrawString(_linesCleared.ToString(), f, fb, new RectangleF(450, 315, 150, 30), sfRight);

                // Controls text
                g.DrawString("Controls:\n↑\n← ↓ →\n", f, Brushes.White, new RectangleF(0, 120, 150, 60), sfCenter);
                g.DrawString("Left and Right:\nmove side to side", f, fw, new RectangleF(0, 200, 150, 40), sfCenter);
                g.DrawString("Up:\nrotate", f, fw, new RectangleF(0, 250, 150, 40), sfCenter);
                g.DrawString("Down:\nfall faster", f, fw, new RectangleF(0, 300, 150, 40), sfCenter);
                g.DrawString("R:\nreset game", f, fw, new RectangleF(0, 350, 150, 40), sfCenter);
            }

            using (var penDark = new Pen(colorDark, 1))
            {
                g.DrawLine(penDark, GameEdgeRight, 0, GameEdgeRight, Height);
            }

            _fallingPiece.Show(g);

            foreach (var gp in _gridPieces)
            {
                gp.Show(g);
            }

            if (_gameOver)
            {
                using var fgo = new Font("Segoe UI", 36, FontStyle.Bold);
                using var bgo = new SolidBrush(colorDark);
                var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("Game Over!", fgo, bgo, new RectangleF(0, 0, Width, Height), sfCenter);
            }

            using (var border = new Pen(ColorTranslator.FromHtml("#304550"), 3))
            {
                g.DrawRectangle(border, new Rectangle(1, 1, Width - 2, Height - 2));
            }
        }

        private void ResetGame()
        {
            _fallingPiece = new PlayPiece(this);
            _fallingPiece.ResetPiece();
            _gridPieces.Clear();
            _currentScore = 0;
            _currentLevel = 1;
            _linesCleared = 0;
            _ticks = 0;
            _updateEvery = 15;
            _updateEveryCurrent = 15;
            _fallSpeed = GridSpace * 0.5f;
            _pauseGame = false;
            _gameOver = false;
        }

        private void AnalyzeGrid()
        {
            int score = 0;
            while (CheckLines())
            {
                score += 100;
                _linesCleared += 1;
                if (_linesCleared % 10 == 0)
                {
                    _currentLevel += 1;
                    if (_updateEveryCurrent > 2)
                    {
                        _updateEveryCurrent = Math.Max(2, _updateEveryCurrent - 10);
                    }
                }
            }
            if (score > 100) score *= 2;
            _currentScore += score;
        }

        private bool CheckLines()
        {
            for (int y = 0; y < Height; y += GridSpace)
            {
                int count = _gridPieces.Count(p => p.Pos.Y == y);
                if (count == 10)
                {
                    _gridPieces.RemoveAll(p => p.Pos.Y == y);
                    foreach (var p in _gridPieces)
                    {
                        if (p.Pos.Y < y)
                        {
                            p.Pos = new Point(p.Pos.X, p.Pos.Y + GridSpace);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool FutureCollision(PlayPiece piece, int dx, float dy, int? rotationOverride = null)
        {
            var points = rotationOverride.HasValue
                ? OrientPoints(piece.PieceType, rotationOverride.Value)
                : piece.Orientation;

            foreach (var pt in points)
            {
                int xx = piece.Pos.X + pt.X * GridSpace + dx;
                float yyF = piece.Pos.Y + pt.Y * GridSpace + dy;
                int yy = (int)Math.Round(yyF);

                if (xx < GameEdgeLeft || xx + GridSpace > GameEdgeRight || yy + GridSpace > Height) return true;

                foreach (var gp in _gridPieces)
                {
                    if (xx == gp.Pos.X)
                    {
                        if (yy >= gp.Pos.Y && yy < gp.Pos.Y + GridSpace) return true;
                        if (yy + GridSpace > gp.Pos.Y && yy + GridSpace <= gp.Pos.Y + GridSpace) return true;
                    }
                }
            }
            return false;
        }

        private List<Point> OrientPoints(int pieceType, int rotation)
        {
            return Pieces.OrientPoints(pieceType, rotation);
        }

        private Color ColorForType(int type) => _colors[type % _colors.Length];

        private class PlayPiece
        {
            private readonly MainForm _form;
            public Point Pos;
            public int Rotation;
            public int NextPieceType;
            public int PieceType;
            public List<Point> Orientation = new();
            public List<Square> Squares = new();
            public bool Fallen;

            public PlayPiece(MainForm form)
            {
                _form = form;
                Pos = new Point(0, 0);
                Rotation = 0;
                NextPieceType = form._random.Next(0, 7);
                PieceType = 0;
                Fallen = false;
            }

            public void ResetPiece()
            {
                Rotation = 0;
                Fallen = false;
                Pos = new Point(330, -60);
                PieceType = NextPieceType;
                NextPiece();
                NewPoints();
            }

            public void NextPiece()
            {
                NextPieceType = Pieces.PseudoRandom(_form._random, PieceType);
            }

            public void NewPoints()
            {
                Orientation = _form.OrientPoints(PieceType, Rotation);
                Squares = new List<Square>();
                foreach (var p in Orientation)
                {
                    Squares.Add(new Square(new Point(Pos.X + p.X * GridSpace, Pos.Y + p.Y * GridSpace), PieceType, _form.ColorForType(PieceType)));
                }
            }

            public void UpdatePoints()
            {
                Orientation = _form.OrientPoints(PieceType, Rotation);
                for (int i = 0; i < Squares.Count; i++)
                {
                    var p = Orientation[i];
                    Squares[i].Pos = new Point(Pos.X + p.X * GridSpace, Pos.Y + p.Y * GridSpace);
                }
            }

            public void AddPos(int dx, int dy)
            {
                Pos = new Point(Pos.X + dx, Pos.Y + dy);
                for (int i = 0; i < Squares.Count; i++)
                {
                    Squares[i].Pos = new Point(Squares[i].Pos.X + dx, Squares[i].Pos.Y + dy);
                }
            }

            public void Input(Direction dir)
            {
                switch (dir)
                {
                    case Direction.Left:
                        if (!_form.FutureCollision(this, -GridSpace, 0, Rotation)) AddPos(-GridSpace, 0);
                        break;
                    case Direction.Right:
                        if (!_form.FutureCollision(this, GridSpace, 0, Rotation)) AddPos(GridSpace, 0);
                        break;
                    case Direction.Up:
                        int newRot = Rotation + 1;
                        if (newRot > 3) newRot = 0;
                        if (!_form.FutureCollision(this, 0, 0, newRot))
                        {
                            Rotation = newRot;
                            UpdatePoints();
                        }
                        break;
                }
            }

            public void Rotate()
            {
                Rotation += 1;
                if (Rotation > 3) Rotation = 0;
                UpdatePoints();
            }

            public void Show(Graphics g)
            {
                foreach (var s in Squares) s.Show(g);
                // Next piece preview: reuse orientation at (525,490)
                var nextPoints = Pieces.OrientPoints(NextPieceType, 0);
                int xx = 525, yy = 490;
                if (NextPieceType != 0 && NextPieceType != 3 && NextPieceType != 5) xx += (int)(GridSpace * 0.5f);
                if (NextPieceType == 5) xx -= (int)(GridSpace * 0.5f);
                foreach (var p in nextPoints)
                {
                    var sq = new Square(new Point(xx + p.X * GridSpace, yy + p.Y * GridSpace), NextPieceType, _form.ColorForType(NextPieceType));
                    sq.Show(g);
                }
            }

            public void Fall(PlayPiece self, float amount)
            {
                if (!_form.FutureCollision(this, 0, amount, Rotation))
                {
                    AddPos(0, (int)Math.Round(amount));
                    Fallen = true;
                }
                else
                {
                    if (!Fallen)
                    {
                        _form._pauseGame = true;
                        _form._gameOver = true;
                    }
                    else
                    {
                        CommitShape();
                    }
                }
            }

            private void CommitShape()
            {
                foreach (var s in Squares) _form._gridPieces.Add(s);
                ResetPiece();
                _form.AnalyzeGrid();
            }
        }

        private class Square
        {
            public Point Pos;
            public int Type;
            private readonly Color _colorMid;

            public Square(Point pos, int type, Color color)
            {
                Pos = pos;
                Type = type;
                _colorMid = color;
            }

            public void Show(Graphics g)
            {
                using var stroke = new Pen(Color.FromArgb(25, 25, 25), 2);
                using var mid = new SolidBrush(_colorMid);
                g.FillRectangle(mid, Pos.X, Pos.Y, GridSpace - 1, GridSpace - 1);
                g.DrawRectangle(stroke, new Rectangle(Pos.X, Pos.Y, GridSpace - 1, GridSpace - 1));

                using var white = new SolidBrush(Color.White);
                g.FillRectangle(white, Pos.X + 6, Pos.Y + 6, 18, 2);
                g.FillRectangle(white, Pos.X + 6, Pos.Y + 6, 2, 16);

                using var dark = new SolidBrush(Color.FromArgb(25, 25, 25));
                g.FillRectangle(dark, Pos.X + 6, Pos.Y + 20, 18, 2);
                g.FillRectangle(dark, Pos.X + 22, Pos.Y + 6, 2, 16);
            }
        }

        private enum Direction { Left, Right, Up }
    }

    internal static class Pieces
    {
        public static int PseudoRandom(Random random, int previous)
        {
            int roll = random.Next(0, 8);
            if (roll == previous || roll == 7) roll = random.Next(0, 7);
            return roll;
        }

        public static List<Point> OrientPoints(int pieceType, int rotation)
        {
            List<Point> results = new();
            switch (pieceType)
            {
                case 0:
                    switch (rotation)
                    {
                        case 0: results = new() { new(-2, 0), new(-1, 0), new(0, 0), new(1, 0) }; break;
                        case 1: results = new() { new(0, -1), new(0, 0), new(0, 1), new(0, 2) }; break;
                        case 2: results = new() { new(-2, 1), new(-1, 1), new(0, 1), new(1, 1) }; break;
                        case 3: results = new() { new(-1, -1), new(-1, 0), new(-1, 1), new(-1, 2) }; break;
                    }
                    break;
                case 1:
                    switch (rotation)
                    {
                        case 0: results = new() { new(-2, -1), new(-2, 0), new(-1, 0), new(0, 0) }; break;
                        case 1: results = new() { new(-1, -1), new(-1, 0), new(-1, 1), new(0, -1) }; break;
                        case 2: results = new() { new(-2, 0), new(-1, 0), new(0, 0), new(0, 1) }; break;
                        case 3: results = new() { new(-1, -1), new(-1, 0), new(-1, 1), new(-2, 1) }; break;
                    }
                    break;
                case 2:
                    switch (rotation)
                    {
                        case 0: results = new() { new(-2, 0), new(-1, 0), new(0, 0), new(0, -1) }; break;
                        case 1: results = new() { new(-1, -1), new(-1, 0), new(-1, 1), new(0, 1) }; break;
                        case 2: results = new() { new(-2, 0), new(-2, 1), new(-1, 0), new(0, 0) }; break;
                        case 3: results = new() { new(-2, -1), new(-1, -1), new(-1, 0), new(-1, 1) }; break;
                    }
                    break;
                case 3:
                    results = new() { new(-1, -1), new(0, -1), new(-1, 0), new(0, 0) };
                    break;
                case 4:
                    switch (rotation)
                    {
                        case 0: results = new() { new(-1, -1), new(-2, 0), new(-1, 0), new(0, -1) }; break;
                        case 1: results = new() { new(-1, -1), new(-1, 0), new(0, 0), new(0, 1) }; break;
                        case 2: results = new() { new(-1, 0), new(-2, 1), new(-1, 1), new(0, 0) }; break;
                        case 3: results = new() { new(-2, -1), new(-2, 0), new(-1, 0), new(-1, 1) }; break;
                    }
                    break;
                case 5:
                    switch (rotation)
                    {
                        case 0: results = new() { new(-1, 0), new(0, 0), new(1, 0), new(0, -1) }; break;
                        case 1: results = new() { new(0, -1), new(0, 0), new(0, 1), new(1, 0) }; break;
                        case 2: results = new() { new(-1, 0), new(0, 0), new(1, 0), new(0, 1) }; break;
                        case 3: results = new() { new(0, -1), new(0, 0), new(0, 1), new(-1, 0) }; break;
                    }
                    break;
                case 6:
                    switch (rotation)
                    {
                        case 0: results = new() { new(-2, -1), new(-1, -1), new(-1, 0), new(0, 0) }; break;
                        case 1: results = new() { new(-1, 0), new(-1, 1), new(0, 0), new(0, -1) }; break;
                        case 2: results = new() { new(-2, 0), new(-1, 0), new(-1, 1), new(0, 1) }; break;
                        case 3: results = new() { new(-2, 0), new(-2, 1), new(-1, 0), new(-1, -1) }; break;
                    }
                    break;
            }
            return results;
        }
    }

    internal static class GraphicsExtensions
    {
        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle bounds, int radius)
        {
            int d = radius * 2;
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }
    }
}


