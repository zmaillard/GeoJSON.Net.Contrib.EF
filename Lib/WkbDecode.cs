using System;
using System.Net;
using System.Windows;

using GeoJSON.Net.Geometry;
using System.Collections.Generic;

namespace GeoJSON.Net.Contrib.EF
{
    internal enum WkbGeometryType : uint
    {
        WkbPoint = 1,
        WkbLineString = 2,
        WkbPolygon = 3,
        WkbMultiPoint = 4,
        WkbMultiLineString = 5,
        WkbMultiPolygon = 6,
        WkbGeometryCollection = 7
    };

    internal static class WkbDecode
    {
        static public byte s_WKBXDR = 0x00; // Big Endian
        static public byte s_WKBNDR = 0x01; // Little Endian

        static UInt32 GetUInt32(byte[] p_wkb, ref int p_pos)
        {
            var v_UInt32 = BitConverter.ToUInt32(p_wkb, p_pos);
            p_pos += 4;
            return v_UInt32;
        }

        static double GetDouble(byte[] p_wkb, ref int p_pos)
        {
            var v_dval = BitConverter.ToDouble(p_wkb, p_pos);
            p_pos += 8;
            return v_dval;
        }

        static GeographicPosition GetGeographicPosition(byte[] p_wkb, ref int p_pos)
        {
            var v_long = GetDouble(p_wkb, ref p_pos);
            var v_lat = GetDouble(p_wkb, ref p_pos);
            return new GeographicPosition(v_lat, v_long);
        }

        static UInt32 GetType(byte[] p_wkb, ref int p_pos)
        {
            if (p_wkb[p_pos] != s_WKBNDR)
            {
                throw new Exception("Only Little Endian format supported");
            }

            p_pos += 1;

            return GetUInt32(p_wkb, ref p_pos);
        }

        static Point ParsePoint(byte[] p_wkb, ref int p_pos)
        {
            var v_type = GetType(p_wkb, ref p_pos);

            if (v_type != (uint)WkbGeometryType.WkbPoint)
            {
                throw new Exception("Invalid object type");
            }

            var v_Point = GetGeographicPosition(p_wkb, ref p_pos);

            return new Point(v_Point);
        }

        static LineString ParseLineString(byte[] p_wkb, ref int p_pos)
        {
            var v_type = GetType(p_wkb, ref p_pos);

            if (v_type != (uint)WkbGeometryType.WkbLineString)
            {
                throw new Exception("Invalid object type");
            }

            var v_numPoints = GetUInt32(p_wkb, ref p_pos);
            var v_Points = new GeographicPosition[v_numPoints];

            for (var i = 0; i < v_numPoints; ++i)
            {
                v_Points[i] = GetGeographicPosition(p_wkb, ref p_pos);
            }

            return new LineString(v_Points);
        }

        static Polygon ParsePolygon(byte[] p_wkb, ref int p_pos)
        {
            var v_type = GetType(p_wkb, ref p_pos);

            if (v_type != (uint)WkbGeometryType.WkbPolygon)
            {
                throw new Exception("Invalid object type");
            }

            var v_numLineStrings = GetUInt32(p_wkb, ref p_pos);
            var v_LineStrings = new List<LineString>();

            for (var v_ls = 0; v_ls < v_numLineStrings; ++v_ls)
            {
                var v_numPoints = GetUInt32(p_wkb, ref p_pos);
                var v_Points = new GeographicPosition[v_numPoints];

                for (var i = 0; i < v_numPoints; ++i)
                {
                    v_Points[i] = GetGeographicPosition(p_wkb, ref p_pos);
                }

                v_LineStrings.Add(new LineString(v_Points));
            }

            return new Polygon(v_LineStrings);
        }

        static MultiPoint ParseMultiPoint(byte[] p_wkb, ref int p_pos)
        {
            var v_type = GetType(p_wkb, ref p_pos);

            if (v_type != (uint)WkbGeometryType.WkbMultiPoint)
            {
                throw new Exception("Invalid object type");
            }

            var v_numPoints = GetUInt32(p_wkb, ref p_pos);
            var v_Points = new List<Point>();

            for (var i = 0; i < v_numPoints; ++i)
            {
                v_Points.Add(ParsePoint(p_wkb, ref p_pos));
            }

            return new MultiPoint(v_Points);
        }

        static MultiLineString ParseMultiLineString(byte[] p_wkb, ref int p_pos)
        {
            var v_type = GetType(p_wkb, ref p_pos);

            if (v_type != (uint)WkbGeometryType.WkbMultiLineString)
            {
                throw new Exception("Invalid object type");
            }

            var v_numLineStrings = GetUInt32(p_wkb, ref p_pos);
            var v_LineStrings = new List<LineString>();

            for (var i = 0; i < v_numLineStrings; ++i)
            {
                v_LineStrings.Add(ParseLineString(p_wkb, ref p_pos));
            }

            return new MultiLineString(v_LineStrings);
        }

        static MultiPolygon ParseMultiPolygon(byte[] p_wkb, ref int p_pos)
        {
            var v_type = GetType(p_wkb, ref p_pos);

            if (v_type != (uint)WkbGeometryType.WkbMultiPolygon)
            {
                throw new Exception("Invalid object type");
            }

            var v_numPolygons = GetUInt32(p_wkb, ref p_pos);
            var v_Polygons = new List<Polygon>();

            for (var i = 0; i < v_numPolygons; ++i)
            {
                v_Polygons.Add(ParsePolygon(p_wkb, ref p_pos));
            }

            return new MultiPolygon(v_Polygons);
        }

        static GeometryCollection ParseGeometryCollection(byte[] p_wkb, ref int p_pos)
        {
            var v_type = GetType(p_wkb, ref p_pos);

            if (v_type != (uint)WkbGeometryType.WkbGeometryCollection)
            {
                throw new Exception("Invalid object type");
            }

            var v_numShapes = GetUInt32(p_wkb, ref p_pos);
            var v_Geometries = new List<IGeometryObject>();

            for (var i = 0; i < v_numShapes; ++i)
            {
                v_Geometries.Add(ParseShape(p_wkb, ref p_pos));
            }

            return new GeometryCollection(v_Geometries);
        }

        static internal IGeometryObject ParseShape(byte[] p_wkb, ref int p_pos)
        {
            var v_type = BitConverter.ToUInt32(p_wkb, p_pos + 1);

            switch (v_type)
            {
                case (uint)WkbGeometryType.WkbPoint:
                    return ParsePoint(p_wkb, ref p_pos);

                case (uint)WkbGeometryType.WkbLineString:
                    return ParseLineString(p_wkb, ref p_pos);

                case (uint)WkbGeometryType.WkbPolygon:
                    return ParsePolygon(p_wkb, ref p_pos);

                case (uint)WkbGeometryType.WkbMultiPoint:
                    return ParseMultiPoint(p_wkb, ref p_pos);

                case (uint)WkbGeometryType.WkbMultiLineString:
                    return ParseMultiLineString(p_wkb, ref p_pos);

                case (uint)WkbGeometryType.WkbMultiPolygon:
                    return ParseMultiPolygon(p_wkb, ref p_pos);

                case (uint)WkbGeometryType.WkbGeometryCollection:
                    return ParseGeometryCollection(p_wkb, ref p_pos);

                default:
                    throw new Exception("Unsupported type");
            }
        }

        static internal IGeometryObject Decode(byte[] p_wkb)
        {
            var v_pos = 0;
            return ParseShape(p_wkb, ref v_pos);
        }
    }

}