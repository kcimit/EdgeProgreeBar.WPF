#region Header
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-03-05 21:21:11 +0100 (Do, 05 Mrz 2015) $
//   Changed in:        $Revision: 113194 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapFillExtensions.cs $
//   Id:                $Id: WriteableBitmapFillExtensions.cs 113194 2015-03-05 20:21:11Z unknown $
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//
#endregion
#define WPF
using System;
using System.Collections.Generic;

namespace System.Windows.Media.Imaging

{
    /// <summary>
    /// Collection of extension methods for the WriteableBitmap class.
    /// </summary>
    public
#if WPF
        unsafe
#endif
        static partial class WriteableBitmapExtensions
    {
        /// <summary>
        /// Draws a filled polygon with or without alpha blending (default = false). 
        /// Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points of the polygon in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="color">The color for the line.</param>
        /// <param name="doAlphaBlend">True if alpha blending should be performed or false if not.</param>
        public static void FillPolygonAlpha(this WriteableBitmap bmp, int[] points, Color color)
        {
            using (var context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;

                int sa = color.A;
                int sr = color.R;
                int sg = color.G;
                int sb = color.B;

                var pixels = context.Pixels;
                int pn = points.Length;
                int pnh = points.Length >> 1;
                int[] intersectionsX = new int[pnh];

                // Find y min and max (slightly faster than scanning from 0 to height)
                int yMin = h;
                int yMax = 0;
                for (int i = 1; i < pn; i += 2)
                {
                    int py = points[i];
                    if (py < yMin) yMin = py;
                    if (py > yMax) yMax = py;
                }

                if (yMin < 0) yMin = 0;
                if (yMax >= h) yMax = h - 1;


                // Scan line from min to max
                for (int y = yMin; y <= yMax; y++)
                {
                    // Initial point x, y
                    float vxi = points[0];
                    float vyi = points[1];

                    // Find all intersections
                    // Based on http://alienryderflex.com/polygon_fill/
                    int intersectionCount = 0;
                    for (int i = 2; i < pn; i += 2)
                    {
                        // Next point x, y
                        float vxj = points[i];
                        float vyj = points[i + 1];

                        // Is the scanline between the two points
                        if (vyi < y && vyj >= y
                            || vyj < y && vyi >= y)
                        {
                            // Compute the intersection of the scanline with the edge (line between two points)
                            intersectionsX[intersectionCount++] = (int) (vxi + (y - vyi) / (vyj - vyi) * (vxj - vxi));
                        }

                        vxi = vxj;
                        vyi = vyj;
                    }

                    // Sort the intersections from left to right using Insertion sort 
                    // It's faster than Array.Sort for this small data set
                    int t, j;
                    for (int i = 1; i < intersectionCount; i++)
                    {
                        t = intersectionsX[i];
                        j = i;
                        while (j > 0 && intersectionsX[j - 1] > t)
                        {
                            intersectionsX[j] = intersectionsX[j - 1];
                            j = j - 1;
                        }

                        intersectionsX[j] = t;
                    }

                    // Fill the pixels between the intersections
                    for (int i = 0; i < intersectionCount - 1; i += 2)
                    {
                        int x0 = intersectionsX[i];
                        int x1 = intersectionsX[i + 1];

                        // Check boundary
                        if (x1 > 0 && x0 < w)
                        {
                            if (x0 < 0) x0 = 0;
                            if (x1 >= w) x1 = w - 1;

                            // Fill the pixels
                            for (int x = x0; x <= x1; x++)
                            {
                                int idx = y * w + x;

                                pixels[idx] = AlphaBlendColors(pixels[idx], sa, sr, sg, sb);
                            }
                        }
                    }
                }
            }
        }
    }
}
