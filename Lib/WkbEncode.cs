using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GeoJSON.Net.Geometry;

namespace GeoJSON.Net.Contrib.EF
{
    internal static class WkbEncode
    {
        static internal byte s_WKBXDR = 0x00;       // Big Endian
        static internal byte s_WKBNDR = 0x01;       // Little Endian

        static void Point(BinaryWriter p_bw, IGeometryObject p_Geometry)
        {
            var v_Point = p_Geometry as Point;

            p_bw.Write(s_WKBNDR);
            p_bw.Write((Int32)WkbGeometryType.WkbPoint);
            var v_gp = v_Point.Coordinates as GeographicPosition;
            p_bw.Write(v_gp.Longitude);
            p_bw.Write(v_gp.Latitude);
        }

        static void MultiPoint(BinaryWriter p_bw, IGeometryObject p_Geometry)
        {
            var v_MultiPoint = p_Geometry as MultiPoint;

            p_bw.Write(s_WKBNDR);
            p_bw.Write((Int32)WkbGeometryType.WkbMultiPoint);
            p_bw.Write((Int32)v_MultiPoint.Coordinates.Count);

            v_MultiPoint.Coordinates.ForEach(p_point => Point(p_bw, p_point));
        }

        static void Pointold(BinaryWriter p_bw, GeographicPosition p_Location)
        {
            p_bw.Write(s_WKBNDR);
            p_bw.Write((Int32)WkbGeometryType.WkbPoint);
            p_bw.Write(p_Location.Longitude);
            p_bw.Write(p_Location.Latitude);
        }

        static void Points(BinaryWriter p_bw, List<IPosition> p_Points)
        {
            foreach (var v_Point in p_Points)
            {
                var v_gp = v_Point as GeographicPosition;
                p_bw.Write(v_gp.Longitude);
                p_bw.Write(v_gp.Latitude);
            }
        }

        static void Polyline(BinaryWriter p_bw, IGeometryObject p_Geometry)
        {
            var v_Polyline = p_Geometry as LineString;

            p_bw.Write(s_WKBNDR);
            p_bw.Write((Int32)WkbGeometryType.WkbLineString);
            p_bw.Write((Int32)v_Polyline.Coordinates.Count);

            Points(p_bw, v_Polyline.Coordinates);
        }

        static void MultiPolyline(BinaryWriter p_bw, IGeometryObject p_Geometry)
        {
            var v_MultiPolyLine = p_Geometry as MultiLineString;

            p_bw.Write(s_WKBNDR);
            p_bw.Write((Int32)WkbGeometryType.WkbMultiLineString);
            p_bw.Write((Int32)v_MultiPolyLine.Coordinates.Count);

            v_MultiPolyLine.Coordinates.ForEach(p_ls => Polyline(p_bw, p_ls));
        }

        static void Polygon(BinaryWriter p_bw, IGeometryObject p_Geometry)
        {
            var v_Polygon = p_Geometry as Polygon;

            p_bw.Write(s_WKBNDR);
            p_bw.Write((Int32)WkbGeometryType.WkbPolygon);

            var v_numRings = v_Polygon.Coordinates.Count;
            p_bw.Write(v_numRings);

            foreach (var v_ls in v_Polygon.Coordinates)
            {
                p_bw.Write((Int32)v_ls.Coordinates.Count);
                Points(p_bw, v_ls.Coordinates);
            }
        }

        static void MultiPolygon(BinaryWriter p_bw, IGeometryObject p_Geometry)
        {
            var v_MultiPolygon = p_Geometry as MultiPolygon;

            p_bw.Write(s_WKBNDR);
            p_bw.Write((Int32)WkbGeometryType.WkbMultiPolygon);
            p_bw.Write((Int32)v_MultiPolygon.Coordinates.Count);

            v_MultiPolygon.Coordinates.ForEach(v_g => Polygon(p_bw, v_g));
        }

        static void GeometryCollection(BinaryWriter p_bw, IGeometryObject p_Geometry)
        {
            var v_GeometryCollection = p_Geometry as GeometryCollection;

            p_bw.Write(s_WKBNDR);
            p_bw.Write((Int32)WkbGeometryType.WkbGeometryCollection);
            p_bw.Write((Int32)v_GeometryCollection.Geometries.Count);

            v_GeometryCollection.Geometries.ForEach(p_g => Encode(p_bw, p_g));
        }

        static internal byte[] Encode(GeoJSON.Net.Feature.Feature p_Feature)
        {
            return Encode(p_Feature.Geometry);
        }

        static internal void Encode(BinaryWriter p_bw, IGeometryObject p_Geometry)
        {
            switch (p_Geometry.Type)
            {
                case GeoJSONObjectType.Point:
                    Point(p_bw, p_Geometry);
                    break;

                case GeoJSONObjectType.MultiPoint:
                    MultiPoint(p_bw, p_Geometry);
                    break;

                case GeoJSONObjectType.Polygon:
                    Polygon(p_bw, p_Geometry);
                    break;

                case GeoJSONObjectType.MultiPolygon:
                    MultiPolygon(p_bw, p_Geometry);
                    break;

                case GeoJSONObjectType.LineString:
                    Polyline(p_bw, p_Geometry);
                    break;

                case GeoJSONObjectType.MultiLineString:
                    MultiPolyline(p_bw, p_Geometry);
                    break;

                case GeoJSONObjectType.GeometryCollection:
                    GeometryCollection(p_bw, p_Geometry);
                    break;
            }
        }

        static internal byte[] Encode(IGeometryObject p_Geometry)
        {
            using (var v_ms = new MemoryStream())
            {
                using (var v_bw = new BinaryWriter(v_ms))
                {
                    Encode(v_bw, p_Geometry);

                    var v_length = (int)v_ms.Length;
                    v_bw.Close();
                    var v_buffer = v_ms.GetBuffer();
                    Array.Resize(ref v_buffer, v_length);
                    v_ms.Close();
                    return v_buffer;
                }
            }
        }

    }
}
