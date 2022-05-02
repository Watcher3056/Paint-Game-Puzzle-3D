using System;
using UnityEngine;


namespace TeamAlpha.Source
{

    public class MathHelper
    {

        public class Matrix2x2
        {
            // здесь немного про матрицы 2х2: http://www.dr-lex.be/random/matrix-inv.html
            private float _m00, _m01, _m10, _m11;

            public float Determinant
            {
                get
                {
                    return _m00 * _m11 - _m01 * _m10;
                }
            }

            public Matrix2x2 Inverse
            {
                get
                {
                    float d = 1 / Determinant;
                    return new Matrix2x2(_m11 * d, -_m01 * d, -_m10 * d, _m00 * d);
                }
            }

            public Matrix2x2(float m00, float m01, float m10, float m11)
            {
                _m00 = m00;
                _m01 = m01;
                _m10 = m10;
                _m11 = m11;
            }

            public Vector2 MultiplyPoint(Vector2 point)
            {
                Vector2 r = new Vector2();
                r.x = _m00 * point.x + _m01 * point.y;
                r.y = _m10 * point.x + _m11 * point.y;
                return r;
            }

            public override string ToString()
            {
                return string.Format("{0} {1}\n{2} {3}", _m00, _m01, _m10, _m11);
            }

        }

        public static Vector2 ProjectPoint(Vector2 p, Vector2 p0, Vector2 p1)
        {
            // угловой коэффициент: https://ru.wikipedia.org/wiki/Угловой_коэффициент
            // уравнение прямой с угловым коэффициентом, проходящей через заданную точку: http://www.cleverstudents.ru/line_and_plane/slope_intercept_equation_of_line.html
            // решение СЛАУ матричным методом: http://www.cleverstudents.ru/systems/matrix_method.html

            if (Mathf.Approximately(p0.y, p1.y))
            {
                Vector2 r = new Vector2();
                r.x = Mathf.Clamp(p.x, Mathf.Min(p0.x, p1.x), Mathf.Max(p0.x, p1.x));
                r.y = p0.y;
                return r;
            }
            else
            {
                Vector2 d = p1 - p0;
                float k = -1 / Mathf.Tan(Mathf.Atan2(d.y, d.x));
                Matrix2x2 m = new Matrix2x2(-k, 1, p0.y - p1.y, p1.x - p0.x);
                Vector2 v = new Vector2(-k * p.x + p.y, p1.x * p0.y - p0.x * p1.y);
                Vector2 r = m.Inverse.MultiplyPoint(v);
                r.x = Mathf.Clamp(r.x, Mathf.Min(p0.x, p1.x), Mathf.Max(p0.x, p1.x));
                r.y = Mathf.Clamp(r.y, Mathf.Min(p0.y, p1.y), Mathf.Max(p0.y, p1.y));
                return r;
            }
        }

        public static float Angle(Vector2 p)
        {
            return Mathf.Repeat(Mathf.Atan2(p.y, p.x), Mathf.PI * 2f);
        }
        public static float Angle2D(Vector2 center, Vector2 to, Vector2 from)
        {
            return Vector2.Angle(to - center, from - center);
        }
        public static float SignedAngle2D(Vector2 center, Vector2 to, Vector2 from)
        {
            return Vector2.SignedAngle(to - center, from - center);
        }
        public static Vector3 SignedAngle(Vector3 center, Vector3 to, Vector3 from)
        {
            Vector3 result = Vector3.zero;
            result.x = Vector3.SignedAngle(from - center, to - center, Vector3.right);
            result.y = Vector3.SignedAngle(from - center, to - center, Vector3.up);
            result.z = Vector3.SignedAngle(from - center, to - center, Vector3.forward);
            return result;
        }
        public static float Angle(Vector3 center, Vector3 to, Vector3 from)
        {
            return Vector3.Angle(to - center, from - center);
        }
        public static int Coordinates2Index(int column, int row, int columns, int rows)
        {
            if (column > columns || row > rows || column < 0 || row < 0) throw new Exception("Index out of range");
            return row * columns + column;
        }
        public static int EnumToInt(object someEnum)
        {
            if (someEnum.GetType().IsEnum)
                return (int)someEnum;
            else
                return -1;
        }

        // The main function that returns true if line segment 'p1q1' 
        // and 'p2q2' intersect. 
        public static bool Intersects(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
            {
                if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
                    q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
                    return true;

                return false;
            }
            int Orientation(Vector2 p, Vector2 q, Vector2 r)
            {
                // See https://www.geeksforgeeks.org/orientation-3-ordered-points/ 
                // for details of below formula. 
                float val = (q.y - p.y) * (r.x - q.x) -
                        (q.x - p.x) * (r.y - q.y);

                if (val == 0) return 0; // colinear 

                return (val > 0) ? 1 : 2; // clock or counterclock wise 
            }
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }
    }

}