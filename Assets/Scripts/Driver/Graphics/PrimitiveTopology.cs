using System;

namespace Nofun.Driver.Graphics
{
    public enum PrimitiveTopology
    {
        /// <summary>
        /// Normal triangle list, each 3 vertices make a triangle.
        /// </summary>
        TriangleList = 0,

        /// <summary>
        /// Previous triangle edge is an edge of the next triangle.
        /// </summary>
        TriangleStrip = 0x10,

        /// <summary>
        /// The first vertice is considered as a fan handle, which is the top vertice of the consequenced triangles.
        /// Format: v0 v1 v2 v3 v4
        /// 
        /// Example result:
        ///    
        ///  v3  v4
        ///    v0
        ///  v1  v2
        /// 
        /// </summary>
        TriangleFan = 0x20,
    }
}
