namespace Atma
{
    /*
        Based on the Public Domain MaxRectsBinPack.cpp source by Jukka Jylänki
        https://github.com/juj/RectangleBinPack/

        Ported to C# by Sven Magnus
        This version is also public domain - do whatever you want with it.
    */


    /*

    I have better code for this somewhere, ideally we would want to just set a width constraint
    and then keep adding and let it figure out how to pack everything, once done it will tell us 
    the height we need. TBFL (To Be Found Later)

    */
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("MinX: {MinX}, MinY: {MinY}, MaxX: {MaxX}, MaxY: {MaxY}")]
    public struct PackRect
    {
        public int MinX;
        public int MinY;
        public int MaxX;
        public int MaxY;

        public int X
        {
            get { return MinX; }
            set { MinX = value; }
        }

        public int Y
        {
            get { return MinY; }
            set { MinY = value; }
        }

        public int Width
        {
            get { return MaxX - MinX; }
            set { MaxX = MinX + value; }
        }

        public int Height
        {
            get { return MaxY - MinY; }
            set { MaxY = MinY + value; }
        }

        public PackRect(int x, int y, int w, int h)
        {
            MinX = x;
            MinY = y;
            MaxX = x + w;
            MaxY = y + h;
        }

        public override string ToString() => $"{{ MinX: {MinX}, MinY: {MinY}, MaxX: {MaxX}, MaxY: {MaxY} }}";
    }

    public enum FreeRectChoiceHeuristic
    {
        RectBestShortSideFit, //< -BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best.
        RectBestLongSideFit, //< -BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best.
        RectBestAreaFit, //< -BAF: Positions the rectangle into the smallest free rect into which it fits.
        RectBottomLeftRule, //< -BL: Does the Tetris placement.
        RectContactPointRule //< -CP: Choosest the placement where the rectangle touches other rects as much as possible.
    }

    public class MaxRectsBinPack
    {
        public int BinWidth { get; private set; } = 0;
        public int BinHeight { get; private set; } = 0;
        public bool allowRotations { get; private set; } = false;

        private List<PackRect> _usedRectangles = new List<PackRect>();
        private List<PackRect> _freeRectangles = new List<PackRect>();

        public MaxRectsBinPack(int width, int height, bool rotations = true)
        {
            Init(width, height, rotations);
        }

        private void Init(int width, int height, bool rotations = true)
        {
            BinWidth = width;
            BinHeight = height;
            allowRotations = rotations;

            PackRect n = new PackRect();
            n.X = 0;
            n.Y = 0;
            n.Width = width;
            n.Height = height;

            _usedRectangles.Clear();

            _freeRectangles.Clear();
            _freeRectangles.Add(n);
        }

        public PackRect Insert(int width, int height, FreeRectChoiceHeuristic method)
        {
            PackRect newNode = new PackRect();
            int score1 = 0; // Unused in this function. We don't need to know the score after finding the position.
            int score2 = 0;
            switch (method)
            {
                case FreeRectChoiceHeuristic.RectBestShortSideFit: newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2); break;
                case FreeRectChoiceHeuristic.RectBottomLeftRule: newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2); break;
                case FreeRectChoiceHeuristic.RectContactPointRule: newNode = FindPositionForNewNodeContactPoint(width, height, ref score1); break;
                case FreeRectChoiceHeuristic.RectBestLongSideFit: newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1); break;
                case FreeRectChoiceHeuristic.RectBestAreaFit: newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2); break;
            }

            if (newNode.Height == 0)
                return newNode;

            int numRectanglesToProcess = _freeRectangles.Count;
            for (int i = 0; i < numRectanglesToProcess; ++i)
            {
                if (SplitFreeNode(_freeRectangles[i], ref newNode))
                {
                    _freeRectangles.RemoveAt(i);
                    --i;
                    --numRectanglesToProcess;
                }
            }

            PruneFreeList();

            _usedRectangles.Add(newNode);
            return newNode;
        }

        public void Insert(Span<PackRect> rects, Span<PackRect> dst, FreeRectChoiceHeuristic method)
        {
            var remaining = rects.Length;
            var completed = new bool[rects.Length];
            while (remaining > 0)
            {
                int bestScore1 = int.MaxValue;
                int bestScore2 = int.MaxValue;
                int bestRectIndex = -1;
                PackRect bestNode = new PackRect();

                for (int i = 0; i < rects.Length; ++i)
                {
                    if (!completed[i])
                    {
                        int score1 = 0;
                        int score2 = 0;
                        PackRect newNode = ScoreRect((int)rects[i].Width, (int)rects[i].Height, method, ref score1, ref score2);

                        if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2))
                        {
                            bestScore1 = score1;
                            bestScore2 = score2;
                            bestNode = newNode;
                            bestRectIndex = i;
                        }
                    }
                }

                if (bestRectIndex == -1)
                    return;

                PlaceRect(bestNode);
                completed[bestRectIndex] = true;
                dst[bestRectIndex] = bestNode;
                remaining--;
            }
        }

        void PlaceRect(PackRect node)
        {
            int numRectanglesToProcess = _freeRectangles.Count;
            for (int i = 0; i < numRectanglesToProcess; ++i)
            {
                if (SplitFreeNode(_freeRectangles[i], ref node))
                {
                    _freeRectangles.RemoveAt(i);
                    --i;
                    --numRectanglesToProcess;
                }
            }

            PruneFreeList();

            _usedRectangles.Add(node);
        }

        PackRect ScoreRect(int width, int height, FreeRectChoiceHeuristic method, ref int score1, ref int score2)
        {
            PackRect newNode = new PackRect();
            score1 = int.MaxValue;
            score2 = int.MaxValue;
            switch (method)
            {
                case FreeRectChoiceHeuristic.RectBestShortSideFit: newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2); break;
                case FreeRectChoiceHeuristic.RectBottomLeftRule: newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2); break;
                case FreeRectChoiceHeuristic.RectContactPointRule:
                    newNode = FindPositionForNewNodeContactPoint(width, height, ref score1);
                    score1 = -score1; // Reverse since we are minimizing, but for contact point score bigger is better.
                    break;
                case FreeRectChoiceHeuristic.RectBestLongSideFit: newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1); break;
                case FreeRectChoiceHeuristic.RectBestAreaFit: newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2); break;
            }

            // Cannot fit the current rectangle.
            if (newNode.Height == 0)
            {
                score1 = int.MaxValue;
                score2 = int.MaxValue;
            }

            return newNode;
        }

        PackRect FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
        {
            PackRect bestNode = new PackRect();
            //memset(bestNode, 0, sizeof(ImRect));

            bestY = int.MaxValue;

            for (int i = 0; i < _freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    int topSideY = (int)_freeRectangles[i].Y + height;
                    if (topSideY < bestY || (topSideY == bestY && _freeRectangles[i].X < bestX))
                    {
                        bestNode.X = _freeRectangles[i].X;
                        bestNode.Y = _freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestY = topSideY;
                        bestX = (int)_freeRectangles[i].X;
                    }
                }
                if (allowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    int topSideY = (int)_freeRectangles[i].Y + width;
                    if (topSideY < bestY || (topSideY == bestY && _freeRectangles[i].X < bestX))
                    {
                        bestNode.X = _freeRectangles[i].X;
                        bestNode.Y = _freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestY = topSideY;
                        bestX = (int)_freeRectangles[i].X;
                    }
                }
            }
            return bestNode;
        }

        PackRect FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
        {
            PackRect bestNode = new PackRect();
            //memset(&bestNode, 0, sizeof(ImRect));

            bestShortSideFit = int.MaxValue;

            for (int i = 0; i < _freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    int leftoverHoriz = Math.Abs((int)_freeRectangles[i].Width - width);
                    int leftoverVert = Math.Abs((int)_freeRectangles[i].Height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (shortSideFit < bestShortSideFit || (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
                    {
                        bestNode.X = _freeRectangles[i].X;
                        bestNode.Y = _freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (allowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    int flippedLeftoverHoriz = Math.Abs((int)_freeRectangles[i].Width - height);
                    int flippedLeftoverVert = Math.Abs((int)_freeRectangles[i].Height - width);
                    int flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                    int flippedLongSideFit = Math.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                    if (flippedShortSideFit < bestShortSideFit || (flippedShortSideFit == bestShortSideFit && flippedLongSideFit < bestLongSideFit))
                    {
                        bestNode.X = _freeRectangles[i].X;
                        bestNode.Y = _freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestShortSideFit = flippedShortSideFit;
                        bestLongSideFit = flippedLongSideFit;
                    }
                }
            }
            return bestNode;
        }

        PackRect FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
        {
            PackRect bestNode = new PackRect();
            //memset(&bestNode, 0, sizeof(ImRect));

            bestLongSideFit = int.MaxValue;

            for (int i = 0; i < _freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    int leftoverHoriz = Math.Abs((int)_freeRectangles[i].Width - width);
                    int leftoverVert = Math.Abs((int)_freeRectangles[i].Height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.X = _freeRectangles[i].X;
                        bestNode.Y = _freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (allowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    int leftoverHoriz = Math.Abs((int)_freeRectangles[i].Width - height);
                    int leftoverVert = Math.Abs((int)_freeRectangles[i].Height - width);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.X = _freeRectangles[i].X;
                        bestNode.Y = _freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }
            }
            return bestNode;
        }

        PackRect FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit)
        {
            PackRect bestNode = new PackRect();
            //memset(&bestNode, 0, sizeof(ImRect));

            bestAreaFit = int.MaxValue;

            for (int i = 0; i < _freeRectangles.Count; ++i)
            {
                int areaFit = (int)_freeRectangles[i].Width * (int)_freeRectangles[i].Height - width * height;

                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    int leftoverHoriz = Math.Abs((int)_freeRectangles[i].Width - width);
                    int leftoverVert = Math.Abs((int)_freeRectangles[i].Height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.X = _freeRectangles[i].X;
                        bestNode.Y = _freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }

                if (allowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    int leftoverHoriz = Math.Abs((int)_freeRectangles[i].Width - height);
                    int leftoverVert = Math.Abs((int)_freeRectangles[i].Height - width);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.X = _freeRectangles[i].X;
                        bestNode.Y = _freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }
            }
            return bestNode;
        }

        /// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
        int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
        {
            if (i1end < i2start || i2end < i1start)
                return 0;
            return Math.Min(i1end, i2end) - Math.Max(i1start, i2start);
        }

        int ContactPointScoreNode(int x, int y, int width, int height)
        {
            int score = 0;

            if (x == 0 || x + width == BinWidth)
                score += height;
            if (y == 0 || y + height == BinHeight)
                score += width;

            for (int i = 0; i < _usedRectangles.Count; ++i)
            {
                if (_usedRectangles[i].X == x + width || _usedRectangles[i].X + _usedRectangles[i].Width == x)
                    score += CommonIntervalLength((int)_usedRectangles[i].Y, (int)_usedRectangles[i].Y + (int)_usedRectangles[i].Height, y, y + height);
                if (_usedRectangles[i].Y == y + height || _usedRectangles[i].Y + _usedRectangles[i].Height == y)
                    score += CommonIntervalLength((int)_usedRectangles[i].X, (int)_usedRectangles[i].X + (int)_usedRectangles[i].Width, x, x + width);
            }
            return score;
        }

        PackRect FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore)
        {
            PackRect bestNode = new PackRect();
            //memset(&bestNode, 0, sizeof(ImRect));

            bestContactScore = -1;

            for (int i = 0; i < _freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    int score = ContactPointScoreNode((int)_freeRectangles[i].X, (int)_freeRectangles[i].Y, width, height);
                    if (score > bestContactScore)
                    {
                        bestNode.X = (int)_freeRectangles[i].X;
                        bestNode.Y = (int)_freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestContactScore = score;
                    }
                }
                if (allowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    int score = ContactPointScoreNode((int)_freeRectangles[i].X, (int)_freeRectangles[i].Y, height, width);
                    if (score > bestContactScore)
                    {
                        bestNode.X = (int)_freeRectangles[i].X;
                        bestNode.Y = (int)_freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestContactScore = score;
                    }
                }
            }
            return bestNode;
        }

        bool SplitFreeNode(PackRect freeNode, ref PackRect usedNode)
        {
            // Test with SAT if the rectangles even intersect.
            if (usedNode.X >= freeNode.X + freeNode.Width || usedNode.X + usedNode.Width <= freeNode.X ||
                usedNode.Y >= freeNode.Y + freeNode.Height || usedNode.Y + usedNode.Height <= freeNode.Y)
                return false;

            if (usedNode.X < freeNode.X + freeNode.Width && usedNode.X + usedNode.Width > freeNode.X)
            {
                // New node at the top side of the used node.
                if (usedNode.Y > freeNode.Y && usedNode.Y < freeNode.Y + freeNode.Height)
                {
                    PackRect newNode = freeNode;
                    newNode.Height = usedNode.Y - newNode.Y;
                    _freeRectangles.Add(newNode);
                }

                // New node at the bottom side of the used node.
                if (usedNode.Y + usedNode.Height < freeNode.Y + freeNode.Height)
                {
                    PackRect newNode = freeNode;
                    newNode.Y = usedNode.Y + usedNode.Height;
                    newNode.Height = freeNode.Y + freeNode.Height - (usedNode.Y + usedNode.Height);
                    _freeRectangles.Add(newNode);
                }
            }

            if (usedNode.Y < freeNode.Y + freeNode.Height && usedNode.Y + usedNode.Height > freeNode.Y)
            {
                // New node at the left side of the used node.
                if (usedNode.X > freeNode.X && usedNode.X < freeNode.X + freeNode.Width)
                {
                    PackRect newNode = freeNode;
                    newNode.Width = usedNode.X - newNode.X;
                    _freeRectangles.Add(newNode);
                }

                // New node at the right side of the used node.
                if (usedNode.X + usedNode.Width < freeNode.X + freeNode.Width)
                {
                    PackRect newNode = freeNode;
                    newNode.X = usedNode.X + usedNode.Width;
                    newNode.Width = freeNode.X + freeNode.Width - (usedNode.X + usedNode.Width);
                    _freeRectangles.Add(newNode);
                }
            }

            return true;
        }

        void PruneFreeList()
        {
            for (int i = 0; i < _freeRectangles.Count; ++i)
                for (int j = i + 1; j < _freeRectangles.Count; ++j)
                {
                    if (IsContainedIn(_freeRectangles[i], _freeRectangles[j]))
                    {
                        _freeRectangles.RemoveAt(i);
                        --i;
                        break;
                    }
                    if (IsContainedIn(_freeRectangles[j], _freeRectangles[i]))
                    {
                        _freeRectangles.RemoveAt(j);
                        --j;
                    }
                }
        }

        bool IsContainedIn(PackRect a, PackRect b)
        {
            return a.X >= b.X && a.Y >= b.Y
                && a.X + a.Width <= b.X + b.Width
                && a.Y + a.Height <= b.Y + b.Height;
        }

    }
}