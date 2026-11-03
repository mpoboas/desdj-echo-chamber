using UnityEngine;

namespace EchoChamber
{
    /// <summary>
    /// Maps the logical 1000x640 prototype world to the 3D street.
    /// X is mirrored so the scene matches the prototype render (glTF import mirrors X).
    /// </summary>
    public static class WorldMap
    {
        public const float GroundY = -1f;
        public const float WidthUnits = 9f;
        public const float DepthUnits = 9f;
        public const float ZOrigin = -17f;
        public const float LogicalWidth = 1000f;
        public const float LogicalHeight = 640f;

        /// <summary>World units per logical unit along X.</summary>
        public const float UnitsPerLogical = WidthUnits / LogicalWidth;

        /// <summary>Prototype (three.js) X before mirroring.</summary>
        public static float ProtoX(float lx) => (lx / LogicalWidth - 0.5f) * WidthUnits;

        public static float X(float lx) => -ProtoX(lx);

        public static float Z(float ly) => ZOrigin + ly / LogicalHeight * DepthUnits;

        public static Vector3 ToWorld(float lx, float ly) => new Vector3(X(lx), GroundY, Z(ly));

        public static Vector3 ToWorld(float lx, float ly, float height) => new Vector3(X(lx), GroundY + height, Z(ly));

        /// <summary>Converts prototype three.js coordinates to Unity world space (mirrors X).</summary>
        public static Vector3 FromProto(float x, float y, float z) => new Vector3(-x, y, z);

        public static float LogicalX(float worldX) => (-worldX / WidthUnits + 0.5f) * LogicalWidth;

        public static float LogicalY(float worldZ) => (worldZ - ZOrigin) / DepthUnits * LogicalHeight;
    }
}
