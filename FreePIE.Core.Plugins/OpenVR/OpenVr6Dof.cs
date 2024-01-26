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

        public Pointf LeftStick;
        public Pointf RightStick;

        public float A;
        public float B;
        public float X;
        public float Y;
        public float LThumb;
        public float RThumb;
        public float Menu;
        public float Home;

        public uint HeadStatus;
        public uint LeftTouchStatus;
        public uint RightTouchStatus;

        public uint IsHmdMounted;
    }
}
