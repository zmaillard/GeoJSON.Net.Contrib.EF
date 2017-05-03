using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoJSON.Net.Contrib.EF
{
    public static class Utils
    {
        static public DbGeography FeatureToDbGeography(GeoJSON.Net.Feature.Feature p_Feature)
        {
            return GeometryToDbGeography(p_Feature.Geometry);
        }

        static public DbGeography GeometryToDbGeography(GeoJSON.Net.Geometry.IGeometryObject p_Geometry)
        {
            return DbGeography.FromBinary(WkbEncode.Encode(p_Geometry));
        }

        static public DbGeometry FeatureToDbGeometry(GeoJSON.Net.Feature.Feature p_Feature)
        {
            return GeometryToDbGeometry(p_Feature.Geometry);
        }

        static public DbGeometry GeometryToDbGeometry(GeoJSON.Net.Geometry.IGeometryObject p_Geometry)
        {
            return DbGeometry.FromBinary(WkbEncode.Encode(p_Geometry));
        }


        static public byte[] GeometryToWkb(GeoJSON.Net.Geometry.IGeometryObject p_Geometry)
        {
            return WkbEncode.Encode(p_Geometry);
        }

        static public GeoJSON.Net.Feature.Feature FeatureFromDbGeography(DbGeography p_DbGeography, object p_Properties = null, string p_Id = null)
        {
            var v_geo = WkbDecode.Decode(p_DbGeography.AsBinary());
            return new GeoJSON.Net.Feature.Feature(v_geo, p_Properties, p_Id);
        }

        static public GeoJSON.Net.Geometry.IGeometryObject GeometryFromDbGeography(DbGeography p_DbGeography)
        {
            return WkbDecode.Decode(p_DbGeography.AsBinary());
        }

        static public GeoJSON.Net.Geometry.IGeometryObject GeometryFromWkb(byte[] p_Wkb)
        {
            return WkbDecode.Decode(p_Wkb);
        }

    }
}
