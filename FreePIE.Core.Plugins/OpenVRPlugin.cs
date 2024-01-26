using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.OculusVR;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(OpenVRGlobal))]
    public class OpenVRPlugin : Plugin
    {
        public override object CreateGlobal()
        {
            return new OpenVRGlobal(this);
        }

        public override string FriendlyName
        {
            get { return "Open VR"; }
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            return false;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            return true;
        }

        public override Action Start()
        {
            if (!Api.Init())
                throw new Exception("Open VR SDK failed to init");

            return null;
        }

        public override void Stop()
        {
            Api.Dispose();
        }

        public override void DoBeforeNextExecute()
        {
            Api.Read(out _data);
            OnUpdate();
        }

        public void Center()
        {
            Api.Center();
        }

        public void TriggerHapticPulse(uint controllerIndex, uint durationMicroSec, float frequency, float amplitude)
        {
            Api.TriggerHapticPulse(controllerIndex, durationMicroSec, frequency, amplitude);
        }

        private OpenVrData _data;

        public OpenVr6Dof HeadPose => _data.HeadPose;
        public OpenVr6Dof LeftTouchPose => _data.LeftTouchPose;
        public OpenVr6Dof RightTouchPose=> _data.RightTouchPose;

        public float LeftTrigger => _data.LeftTrigger;
        public float RightTrigger => _data.RightTrigger;
        public float LeftGrip => _data.LeftGrip;
        public float RightGrip => _data.RightGrip;

        public Pointf LeftStick => _data.LeftStick;
        public Pointf RightStick => _data.RightStick;

        public float A => _data.A;
        public float B => _data.B;
        public float X => _data.X;
        public float Y => _data.Y;
        public float LThumb => _data.LThumb;
        public float RThumb => _data.RThumb;
        public float Menu => _data.Menu;
        public float Home => _data.Home;

        public uint RightTouchStatus => _data.RightTouchStatus;
        public uint LeftTouchStatus => _data.LeftTouchStatus;
        public uint HeadStatus => _data.HeadStatus;

        public bool IsHmdMounted { get { return _data.IsHmdMounted > 0; } }
    }

    [Global(Name = "openVR")]
    public class OpenVRGlobal : UpdateblePluginGlobal<OpenVRPlugin>
    {
        public OpenVRGlobal(OpenVRPlugin plugin) : base(plugin){}

        public OpenVr6Dof headPose => plugin.HeadPose;
        public OpenVr6Dof leftTouchPose => plugin.LeftTouchPose;
        public OpenVr6Dof rightTouchPose => plugin.RightTouchPose;
                
        public uint headStatus => plugin.HeadStatus;
        public uint leftTouchStatus => plugin.LeftTouchStatus;
        public uint rightTouchStatus => plugin.RightTouchStatus;

        public bool isMounted => plugin.IsHmdMounted;
        public bool isHeadTracking => plugin.HeadStatus > 1;
        public bool isLeftTouchTracking => plugin.LeftTouchStatus > 1;
        public bool isRightTouchTracking => plugin.RightTouchStatus > 1;

        public float leftTrigger => plugin.LeftTrigger;
        public float rightTrigger => plugin.RightTrigger;

        public float leftGrip => plugin.LeftGrip;
        public float rightGrip => plugin.RightGrip;

        public Pointf leftStick => plugin.LeftStick;
        public Pointf rightStick => plugin.RightStick;

        public bool a => plugin.A > 0.6;
        public bool b => plugin.B > 0.6;
        public bool x => plugin.X > 0.6;
        public bool y => plugin.Y > 0.6;
        public bool lThumb => plugin.LThumb > 0.6;
        public bool rThumb => plugin.RThumb > 0.6;
        public bool menu => plugin.Menu > 0.6;
        public bool home => plugin.Home > 0.6;

        public bool touchingA => plugin.A > 0.1 && plugin.A < 0.6;
        public bool touchingB => plugin.B > 0.1 && plugin.B < 0.6;
        public bool touchingX => plugin.X > 0.1 && plugin.X < 0.6;
        public bool touchingY => plugin.Y > 0.1 && plugin.Y < 0.6;
        public bool touchingLThumb => plugin.LThumb > 0.1 && plugin.LThumb < 0.6;
        public bool touchingRThumb => plugin.RThumb > 0.1 && plugin.RThumb < 0.6;
        public bool touchingMenu => plugin.Menu > 0.1 && plugin.Menu < 0.6;
        public bool touchingHome => plugin.Home > 0.1 && plugin.Home < 0.6;

        public void center()
        {
            plugin.Center();
        }

        public void triggerHapticPulse(uint controllerIndex, uint durationMicroSec, float frequency, float amplitude)
        {
            plugin.TriggerHapticPulse(controllerIndex, durationMicroSec, frequency, amplitude);
        }
    }
}
