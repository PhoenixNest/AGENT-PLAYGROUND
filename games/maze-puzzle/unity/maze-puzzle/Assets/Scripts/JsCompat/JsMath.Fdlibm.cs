/*
 * ====================================================
 * Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
 *
 * Developed at SunSoft, a Sun Microsystems, Inc. business.
 * Permission to use, copy, modify, and distribute this
 * software is freely granted, provided that this notice
 * is preserved.
 * ====================================================
 *
 * atan ported from V8 src/base/ieee754.cc (v8 13.6.233.17). Used only to seed the\n * correctly-rounded atan in JsMath.Libm.cs.
 */

using System;

namespace OrbitalSalvage.JsCompat
{
    public static partial class JsMath
    {
        private static readonly double[] AtanHi =
        {
            4.63647609000806093515e-01,
            7.85398163397448278999e-01,
            9.82793723247329054082e-01,
            1.57079632679489655800e+00,
        };

        private static readonly double[] AtanLo =
        {
            2.26987774529616870924e-17,
            3.06161699786838301793e-17,
            1.39033110312309984516e-17,
            6.12323399573676603587e-17,
        };

        private static readonly double[] AT =
        {
            3.33333333333329318027e-01,
            -1.99999999998764832476e-01,
            1.42857142725034663711e-01,
            -1.11111104054623557880e-01,
            9.09088713343650656196e-02,
            -7.69187620504482999495e-02,
            6.66107313738753120669e-02,
            -5.83357013379057348645e-02,
            4.97687799461593236017e-02,
            -3.65315727442169155270e-02,
            1.62858201153657823623e-02,
        };

        internal static double FdlibmAtan(double x)
        {
            const double one = 1.0;
            double w, s1, s2, z;
            int ix, hx, id;

            hx = HighWord(x);
            ix = hx & 0x7FFFFFFF;
            if (ix >= 0x44100000)
            {
                uint low = LowWord(x);
                if (ix > 0x7FF00000 || (ix == 0x7FF00000 && low != 0)) return x + x;
                if (hx > 0) return AtanHi[3] + AtanLo[3];
                return -AtanHi[3] - AtanLo[3];
            }
            if (ix < 0x3FDC0000)
            {
                if (ix < 0x3E400000)
                {
                    return x;
                }
                id = -1;
            }
            else
            {
                x = Abs(x);
                if (ix < 0x3FF30000)
                {
                    if (ix < 0x3FE60000)
                    {
                        id = 0;
                        x = (2.0 * x - one) / (2.0 + x);
                    }
                    else
                    {
                        id = 1;
                        x = (x - one) / (x + one);
                    }
                }
                else
                {
                    if (ix < 0x40038000)
                    {
                        id = 2;
                        x = (x - 1.5) / (one + 1.5 * x);
                    }
                    else
                    {
                        id = 3;
                        x = -1.0 / x;
                    }
                }
            }
            z = x * x;
            w = z * z;
            s1 = z * (AT[0] + w * (AT[2] + w * (AT[4] + w * (AT[6] + w * (AT[8] + w * AT[10])))));
            s2 = w * (AT[1] + w * (AT[3] + w * (AT[5] + w * (AT[7] + w * AT[9]))));
            if (id < 0)
            {
                return x - x * (s1 + s2);
            }
            z = AtanHi[id] - ((x * (s1 + s2) - AtanLo[id]) - x);
            return hx < 0 ? -z : z;
        }
    }
}
