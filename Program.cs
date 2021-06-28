using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using MaxRev.Gdal.Core;
using OSGeo.GDAL;

namespace gdal_test
{
    class Program
    {
        // src: https://www.worldpop.org/geodata/summary?id=49789#+
        // resolution of 3 arc(approximately 100m at the equator). 
        const string file = "ftp://ftp.worldpop.org/GIS/Population/Global_2000_2020_Constrained/2020/BSGM/DEU/deu_ppp_2020_constrained.tif";
        static void Main(string[] args)
        {
            GdalBase.ConfigureAll();
            Console.WriteLine("Gdal loaded");

            var ds = Gdal.Open(file, Access.GA_ReadOnly);

            // if the requested pixel is out of range return an error
            // for that you need the range.
            var rowSize = ds.RasterYSize;
            var colSize = ds.RasterXSize;

            double[] geoTransform = new double[6];
            ds.GetGeoTransform(geoTransform);

            var band = ds.GetRasterBand(1);

            // Get coord value

            // Marienplatz, 80331 München
            var lon = 11.576124;
            var lat = 48.137154;

            // See https://gdal.org/user/raster_data_model.html#affine-geotransform
            int col = Convert.ToInt32((lon - geoTransform[0]) / geoTransform[1]);
            int row = Convert.ToInt32((geoTransform[3] - lat) / -geoTransform[5]);


            var buffer = new float[1];

            band.ReadRaster(col, row, 1, 1, buffer, 1, 1, 0, 0);

            Console.WriteLine(buffer[0]);

            // Draw PNG 
            var width = colSize;
            var height = rowSize;

            // Creating a Bitmap to store the GDAL image in
            var bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            // Obtaining the bitmap buffer
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            try
            {
                int stride = bitmapData.Stride;
                IntPtr buf = bitmapData.Scan0;
                band.ReadRaster(0, 0, width, height, buf, width, height, DataType.GDT_Byte, 1, stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }

            bitmap.Save("./de_pop.png", ImageFormat.Png);
        }
    }
}
