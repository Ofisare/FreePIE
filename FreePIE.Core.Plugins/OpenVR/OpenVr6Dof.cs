using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.OculusVR
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vectorf
    {
        public float x, y, z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OpenVr6Dof
    {
        public Vectorf left;
        public Vectorf up;
        public Vectorf forward;
        public Vectorf position;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Pointf
    {
        public float x, y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OpenVrData
    {
        public OpenVr6Dof HeadPose;
        public OpenVr6Dof LeftTouchPose;
        public OpenVr6Dof RightTouchPose;

        public float LeftTrigger;
        public float RightTrigger;

        public float LeftGrip;
        public float RightGrip;

        public Pointf LeftStickAxes;
        public Pointf RightStickAxes;

        public ulong LeftButtonsPressed;
        public ulong LeftButtonsTouched;

        public ulong RightButtonsPressed;
        public ulong RightButtonsTouched;

        public uint HeadStatus;
        public uint LeftTouchStatus;
        public uint RightTouchStatus;

        public uint IsHmdMounted;
    }

    public struct OpenVrMapping
    {
        public ulong A;
        public ulong B;
        public ulong X;
        public ulong Y;
        public ulong LeftStick;
        public ulong RightStick;
        public ulong LeftThumb;
        public ulong RightThumb;
        public ulong Menu;
        public ulong Home;
    }
}
