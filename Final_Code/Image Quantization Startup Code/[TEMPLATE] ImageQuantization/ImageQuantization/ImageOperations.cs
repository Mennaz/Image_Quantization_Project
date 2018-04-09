using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
  
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        //--------------------------------------------------------------------------------------------------------------//
        public static List<RGBPixel> Distinct = new List<RGBPixel>();

        //Finds the Distinct Colors in an image using a unique ID.
        public static void Find_Distinct(RGBPixel[,] ImageMatrix)
        {
            int Image_width = GetWidth(ImageMatrix);
            int Image_Height = GetHeight(ImageMatrix);
            bool[] check = new bool[16777217];
            long ID = new long();


            for (int i = 0; i < Image_Height; i++)
            {
                for (int j = 0; j < Image_width; j++)
                {
                   //Gets a unique ID for each color.
                    ID = ImageMatrix[i, j].red + ImageMatrix[i, j].green * 256 + ImageMatrix[i, j].blue * 256 * 256; //------> O(1)

                    if (check[ID] == false) //------> O(1)
                    {
                        Distinct.Add(ImageMatrix[i, j]); //------> O(1)
                        check[ID] = true; //------> O(1)
                    }
                }
            }

            MessageBox.Show("Distinct Colors: " + Distinct.Count.ToString());
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Function to construct the minimum spanning tree, extract clusters and replace colors.
        public static void Quantize_Image(RGBPixel[,] ImageMatrix, int ClusteringValue, ref PictureBox PicBox)
        {


            bool[] visited = new bool[Distinct.Count];
            int[] Nodes = new int[Distinct.Count];
            double[] weights = new double[Distinct.Count];
            double MST_Sum = 0;
            for (int i = 0; i < Distinct.Count; i++)
            {
                weights[i] = 1e9;  //------> O(1)
            }

            weights[0] = 0;  //------> O(1)
            Nodes[0] = 0;    //------> O(1)

            for (int j = 0; j < Distinct.Count; j++)
            {
                double Minimumvalue = 1e9;   //------> O(1)
                int minimumindex = 0;     //------> O(1)
                for (int k = 0; k < Distinct.Count; k++)
                {
                    if (visited[k] == false && weights[k] < Minimumvalue)   //------> O(1)
                    {
                        Minimumvalue = weights[k];  //------> O(1)
                        minimumindex = k;  //------> O(1)
                    }
                }

                visited[minimumindex] = true;  //------> O(1)

                double distance;
                int r, g, b;
                for (int M = 0; M < Distinct.Count; M++)
                {
                    r = (Distinct[M].red - Distinct[minimumindex].red) * (Distinct[M].red - Distinct[minimumindex].red); //------> O(1)
                   
                    g = (Distinct[M].green - Distinct[minimumindex].green) * (Distinct[M].green - Distinct[minimumindex].green); //------> O(1)
                    
                    b = (Distinct[M].blue - Distinct[minimumindex].blue) * (Distinct[M].blue - Distinct[minimumindex].blue); //------> O(1)
                    
                    distance = r + g + b; //------> O(1)
                    distance = Math.Sqrt(distance); //------> O(1)
                    if (distance > 0 && visited[M] == false && distance < weights[M])
                    {
                        Nodes[M] = minimumindex; //------> O(1)
                        weights[M] = distance; //------> O(1)
                    }
                }

            }

            for (int i = 1; i < Distinct.Count; i++)
            {
                MST_Sum += weights[i]; //------> O(1)
            }


            MessageBox.Show("MST " + MST_Sum.ToString());

            double max = 0;

            // Clustering part involves looping on the distinct colors K-1 times


            for (int k = 0; k < ClusteringValue - 1; k++)
            {
                int temp = 0;
                max = 0;
                for (int h = 0; h < Distinct.Count; h++)
                {

                    if (weights[h] > max) //------> O(1)
                    {

                        temp = h; //------> O(1)
                        max = weights[h]; //------> O(1)
                    }
                }
                weights[temp] = -1; //------> O(1)
                Nodes[temp] = temp; //------> O(1)
                 
            }
            //Constructing the adjacency list.
            List<int>[] adj = new List<int>[Distinct.Count];
            for (int s = 0; s < Distinct.Count; s++)
            {
                adj[s] = new List<int>(Distinct.Count); //------> O(1)
            }

            for (int w = 0; w < Distinct.Count; w++)
            {
                if (Nodes[w] != w)
                {

                    adj[w].Add(Nodes[w]); //------> O(1)
                    adj[Nodes[w]].Add(w); //------> O(1)    
                }
            }
            vis = new bool[Distinct.Count];
            RGBPixel[] map = new RGBPixel[16777217];
            RGBPixel tmp;
            for (int i = 0; i < Distinct.Count; i++)
            {
                List<int> ColorsSum = new List<int>(Distinct.Count);

                if (vis[i]==false )
                {

                    DFS(adj, i, ref ColorsSum);
                    int sumred = 0;     
                    int sumgreen = 0;
                    int sumblue = 0;

                    for (int j = 0; j < ColorsSum.Count; j++)
                    {
                        sumred += Distinct[ColorsSum[j]].red;
                        sumgreen += Distinct[ColorsSum[j]].green;
                        sumblue += Distinct[ColorsSum[j]].blue;
                    }
                    sumgreen /= (ColorsSum.Count);  //------> O(1)
                    sumblue /= (ColorsSum.Count); //------> O(1)
                    sumred /= (ColorsSum.Count); //------> O(1)
                    tmp.red = (byte)sumred; //------> O(1)
                    tmp.green = (byte)sumgreen; //------> O(1)
                    tmp.blue = (byte)sumblue; //------> O(1)
                    for (int j = 0; j < ColorsSum.Count; j++)
                    {
                        int ID = Distinct[ColorsSum[j]].red + Distinct[ColorsSum[j]].green * 256 + Distinct[ColorsSum[j]].blue * 256 * 256; //------> O(1)
                        map[ID] = tmp; //------> O(1)
                    }
                }

            }
            Distinct.Clear();
            DisplayImagefinall(ImageMatrix,PicBox,map);
        }
        // DFS 
        private static bool[] vis;
        private static void DFS(List<int>[] input, int U, ref List<int> Colorssum)
        {

            Colorssum.Add(U);
            vis[U] = true;

            for (int i = 0; i < input[U].Count; i++)
            {
                if (!vis[input[U][i]])
                {
                    DFS(input, input[U][i], ref Colorssum);
                }
            }
        }
        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }

        public static void DisplayImagefinall(RGBPixel[,] ImageMatrix, PictureBox PicBox, RGBPixel[] f)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        int ID = ImageMatrix[i, j].red + ImageMatrix[i, j].green * 256 + ImageMatrix[i, j].blue * 256 * 256;

                        p[2] = f[ID].red;
                        p[1] = f[ID].green;
                        p[0] = f[ID].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }



    }
}
