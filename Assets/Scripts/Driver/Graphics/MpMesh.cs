using Nofun.Module.VMGP3D;
using System;

namespace Nofun.Driver.Graphics
{
    public ref struct MpMesh
    {
        public Span<NativeVector3D> vertices;
        public Span<NativeUV> uvs;
        public Span<NativeDiffuseColor> diffuses;
        public Span<NativeSpecularColor> speculars;
        public Span<NativeVector3D> normals;
        public Span<short> indices;
        public PrimitiveTopology topology;
    }
}
