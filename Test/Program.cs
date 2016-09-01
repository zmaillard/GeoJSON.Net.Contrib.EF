using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoJSON.Net.Contrib.EF.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Test("POINT (30 10)");
            Test("LINESTRING (30 10, 10 30, 40 40)");
            Test("POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))");
            Test("POLYGON ((35 10, 45 45, 15 40, 10 20, 35 10),(20 30, 35 35, 30 20, 20 30))");
            Test("MULTIPOINT ((10 40), (40 30), (20 20), (30 10))");
            Test("MULTIPOINT (10 40, 40 30, 20 20, 30 10)");
            Test("MULTILINESTRING ((10 10, 20 20, 10 40),(40 40, 30 30, 40 20, 30 10))");
            Test("MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)),((15 5, 40 10, 10 20, 5 10, 15 5)))");
            Test("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)),((20 35, 10 30, 10 10, 30 5, 45 20, 20 35),(30 20, 20 15, 20 25, 30 20)))");
            Test("GEOMETRYCOLLECTION(POINT(4 6),LINESTRING(4 6,7 10))");
        }

        static void Test(string p_wkt)
        {
            try
            {
                var v_geo1 = DbGeography.FromText(p_wkt);
                var v_wkb1 = v_geo1.AsBinary();
                var v_geo2 = Utils.GeometryFromWkb(v_wkb1);
                var v_wkb2 = Utils.GeometryToWkb(v_geo2);
                var v_geo3 = DbGeography.FromBinary(v_wkb2);
                var v_IsEqual = System.Data.Entity.Spatial.DbSpatialServices.Default.SpatialEquals(v_geo1, v_geo3);
                Console.WriteLine("passed: " + p_wkt);
            }
            catch
            {
                Console.WriteLine("failed: " + p_wkt);
            }
        }
    }
}
