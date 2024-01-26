using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.OculusVR
{
    //Note: Oculus SDK reports on xinput controller as well as remote, i've commented them out for simplicity but should we?
    [Flags]
    public enum OpenVrButton:uint
    {
        System = 0,
        A = 1,
        B = 2,
    }

    public enum OpenVrControllerType : uint
    {
        None = 0x00,
        LTouch = 0x01,
        RTouch = 0x02,
        Touch = 0x03,
        Remote = 0x04,
        XBox = 0x10,
        Active = 0xff,      //Operate on or query whichever controller is active.

    }

    public static class OpenVrStatusExtension
    {
        public static bool IsTracking(this OpenVrStatus status)
        {
            switch (status)
            {
                case OpenVrStatus.TrackingResult_Running_OK:
                case OpenVrStatus.TrackingResult_Running_OutOfRange:
                case OpenVrStatus.TrackingResult_Fallback_RotationOnly:
                    return true;
            }

            return false;
        }
    }
    public enum OpenVrStatus : uint
    {
        TrackingResult_Uninitialized = 1,

        TrackingResult_Calibrating_InProgress = 100,
        TrackingResult_Calibrating_OutOfRange = 101,

        TrackingResult_Running_OK = 200,
        TrackingResult_Running_OutOfRange = 201,

        TrackingResult_Fallback_RotationOnly = 300,
    }

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

        public uint LeftTouches;
        public uint LeftButtons;

        public uint RightTouches;
        public uint RightButtons;

        /// The type of the controller this state is for.
        public uint ControllerType;

        public uint HeadStatus;
        public uint LeftTouchStatus;
        public uint RightTouchStatus;

        public uint IsHmdMounted;
    }
}
