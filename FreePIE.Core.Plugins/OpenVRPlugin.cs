using System;
using System.Collections.Generic;
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
            Api.Read(out Data);
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

        public OpenVrData Data;
    }

    [Global(Name = "openVR")]
    public class OpenVRGlobal : UpdateblePluginGlobal<OpenVRPlugin>
    {
        public OpenVRGlobal(OpenVRPlugin plugin) : base(plugin) { }

        public OpenVr6Dof headPose => plugin.Data.HeadPose;
        public OpenVr6Dof leftTouchPose => plugin.Data.LeftTouchPose;
        public OpenVr6Dof rightTouchPose => plugin.Data.RightTouchPose;

        public uint headStatus => plugin.Data.HeadStatus;
        public uint leftTouchStatus => plugin.Data.LeftTouchStatus;
        public uint rightTouchStatus => plugin.Data.RightTouchStatus;

        public bool isMounted => plugin.Data.IsHmdMounted > 0;
        public bool isHeadTracking => plugin.Data.HeadStatus > 1;
        public bool isLeftTouchTracking => plugin.Data.LeftTouchStatus > 1;
        public bool isRightTouchTracking => plugin.Data.RightTouchStatus > 1;

        public float leftTrigger => plugin.Data.LeftTrigger;
        public float rightTrigger => plugin.Data.RightTrigger;

        public float leftGrip => plugin.Data.LeftGrip;
        public float rightGrip => plugin.Data.RightGrip;

        public Pointf leftStickAxes => plugin.Data.LeftStickAxes;
        public Pointf rightStickAxes => plugin.Data.RightStickAxes;

        public float a => plugin.Data.A;
        public float b => plugin.Data.B;
        public float x => plugin.Data.X;
        public float y => plugin.Data.Y;
        public float leftStick => plugin.Data.LeftStick;
        public float rightStick => plugin.Data.RightStick;
        public float leftThumb => plugin.Data.LeftThumb;
        public float rightThumb => plugin.Data.RightThumb;
        public float menu => plugin.Data.Menu;
        public float home => plugin.Data.Home;

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
