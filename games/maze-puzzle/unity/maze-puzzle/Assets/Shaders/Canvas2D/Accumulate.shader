Shader "Hidden/Canvas2D/Accumulate"
{
    // Exact-area signed coverage accumulation. Every polygon edge draws one quad from its left-most pixel to the
    // right bound of the draw region; each fragment adds the signed area of the part of its pixel that lies to the
    // right of the edge. The sum over all edges is the winding-weighted area of the pixel inside the polygon.
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass // 0: accumulate
        {
            Blend One One
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #include "UnityCG.cginc"
            #include "Canvas2DCommon.cginc"

            StructuredBuffer<float4> _C2D_Edges;

            struct v2f
            {
                float4 pos : SV_POSITION;
                nointerpolation float4 edge : TEXCOORD0;
            };

            v2f vert(uint vid : SV_VertexID)
            {
                float4 ep = C2D_P(IDX_EDGE);
                uint e = vid / 6;
                uint c = vid % 6;
                float4 ed = _C2D_Edges[(uint)ep.x + e];
                float x0 = floor(min(ed.x, ed.z));
                float x1 = max(ep.y, x0 + 1.0);
                float y0 = floor(min(ed.y, ed.w));
                float y1 = ceil(max(ed.y, ed.w));
                v2f o;
                float2 p = C2D_QuadCorner(c, float4(x0, y0, x1, y1));
                o.pos = float4(C2D_PixelToNdc(p), 0, 1);
                o.edge = ed;
                return o;
            }

            float fclamp(float x, float px) { return saturate(px + 1.0 - x); }

            float4 frag(v2f i) : SV_Target
            {
                float2 pc = C2D_FragToCanvas(i.pos);
                float px = floor(pc.x);
                float py = floor(pc.y);
                float x0 = i.edge.x, y0 = i.edge.y, x1 = i.edge.z, y1 = i.edge.w;
                float dir = 1.0;
                if (y0 > y1)
                {
                    float tx = x0; x0 = x1; x1 = tx;
                    float ty = y0; y0 = y1; y1 = ty;
                    dir = -1.0;
                }
                float dy = y1 - y0;
                if (dy <= 0.0) return 0;
                float ya = max(y0, py);
                float yb = min(y1, py + 1.0);
                if (yb <= ya) return 0;
                float slope = (x1 - x0) / dy;
                float xa = x0 + (ya - y0) * slope;
                float xb = x0 + (yb - y0) * slope;
                float h = yb - ya;
                float dx = xb - xa;
                float integral;
                if (abs(dx) < 1e-7)
                {
                    integral = fclamp(xa, px);
                }
                else
                {
                    float inv = 1.0 / dx;
                    float tA = saturate((px - xa) * inv);
                    float tB = saturate((px + 1.0 - xa) * inv);
                    float tlo = min(tA, tB);
                    float thi = max(tA, tB);
                    float f0 = fclamp(xa, px);
                    float f1 = fclamp(xa + dx * tlo, px);
                    float f2 = fclamp(xa + dx * thi, px);
                    float f3 = fclamp(xb, px);
                    integral = 0.5 * (f0 + f1) * tlo + 0.5 * (f1 + f2) * (thi - tlo) + 0.5 * (f2 + f3) * (1.0 - thi);
                }
                return dir * h * integral;
            }
            ENDHLSL
        }

        Pass // 1: clear rect to zero
        {
            Blend Off
            HLSLPROGRAM
            #pragma vertex C2D_RectVert
            #pragma fragment frag
            #pragma target 4.5
            #include "UnityCG.cginc"
            #include "Canvas2DCommon.cginc"

            float4 frag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
