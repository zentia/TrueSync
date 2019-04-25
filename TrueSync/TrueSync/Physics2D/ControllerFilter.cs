namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ControllerFilter
    {
        public ControllerType ControllerFlags;
        public void IgnoreController(ControllerType controller)
        {
            this.ControllerFlags |= controller;
        }

        public void RestoreController(ControllerType controller)
        {
            this.ControllerFlags &= ~controller;
        }

        public bool IsControllerIgnored(ControllerType controller)
        {
            return ((this.ControllerFlags & controller) == controller);
        }
    }
}

