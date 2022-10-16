using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathController.Manager
{
    public static class PathControllerExtension {
        static Dictionary<ushort, SegmentPath> SegmentPaths { get; } = new Dictionary<ushort, SegmentPath>();
        static Dictionary<ushort, NodePath> NodePaths { get; } = new Dictionary<ushort, NodePath>();
    }
}
