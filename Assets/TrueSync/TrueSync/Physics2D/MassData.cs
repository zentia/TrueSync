namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct MassData : IEquatable<MassData>
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Area>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <Centroid>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Inertia>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Mass>k__BackingField;
        public FP Area { get; internal set; }
        public TSVector2 Centroid { get; internal set; }
        public FP Inertia { get; internal set; }
        public FP Mass { get; internal set; }
        public static bool operator ==(MassData left, MassData right)
        {
            return ((((left.Area == right.Area) && (left.Mass == right.Mass)) && (left.Centroid == right.Centroid)) && (left.Inertia == right.Inertia));
        }

        public static bool operator !=(MassData left, MassData right)
        {
            return !(left == right);
        }

        public bool Equals(MassData other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj.GetType() == typeof(MassData)))
            {
                return false;
            }
            return this.Equals((MassData) obj);
        }

        public override int GetHashCode()
        {
            int num = (this.Area.GetHashCode() * 0x18d) ^ this.Centroid.GetHashCode();
            num = (num * 0x18d) ^ this.Inertia.GetHashCode();
            return ((num * 0x18d) ^ this.Mass.GetHashCode());
        }
    }
}

