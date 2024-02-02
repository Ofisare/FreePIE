using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.OculusVR;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(OpenVRGlobal))]
    public class OpenVRPlugin : Plugin
    {
        public OpenVrData Data;
        public OpenVrMapping Mapping;

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

            // oculus defaults
            Mapping.A = Mapping.X = (ulong)1 << 7;
            Mapping.B = Mapping.Y = (ulong)1 << 1;
            Mapping.LeftStick = Mapping.RightStick = (ulong)32 << 1;
            Mapping.Menu = Mapping.Home = (ulong)0 << 1;
            Mapping.LeftThumb = Mapping.RightThumb = (ulong)32 << 1;

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

        public float GetButtonState(bool left, ulong button)
        {
            ulong pressed = left ? Data.LeftButtonsPressed : Data.RightButtonsPressed;
            ulong touched = left ? Data.LeftButtonsTouched : Data.RightButtonsTouched;

            if ((pressed & button) == button)
                return 1.0f;

            if ((touched & button) == button)
                return 0.5f;

            return 0.0f;
        }
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

        public float a => plugin.GetButtonState(false, plugin.Mapping.A);
        public float b => plugin.GetButtonState(false, plugin.Mapping.B);
        public float x => plugin.GetButtonState(true, plugin.Mapping.X);
        public float y => plugin.GetButtonState(true, plugin.Mapping.Y);
        public float leftStick => plugin.GetButtonState(true, plugin.Mapping.LeftStick);
        public float rightStick => plugin.GetButtonState(false, plugin.Mapping.RightStick);
        public float leftThumb => plugin.GetButtonState(true, plugin.Mapping.LeftThumb);
        public float rightThumb => plugin.GetButtonState(false, plugin.Mapping.RightThumb);
        public float menu => plugin.GetButtonState(true, plugin.Mapping.Menu);
        public float home => plugin.GetButtonState(false, plugin.Mapping.Home);
        
        public float getButtonState(bool left, int buttonIndex)
        {
            return plugin.GetButtonState(left, (ulong)1 << buttonIndex);
        }

        public void center()
        {
            plugin.Center();
        }

        public void triggerHapticPulse(uint controllerIndex, uint durationMicroSec, float frequency, float amplitude)
        {
            plugin.TriggerHapticPulse(controllerIndex, durationMicroSec, frequency, amplitude);
        }

        public void mapButtons(int a, int b, int x, int y, int leftStick, int rightStick, int leftThumb, int rightThumb, int menu, int home)
        {
            plugin.Mapping.A = (ulong)1 << a;
            plugin.Mapping.B = (ulong)1 << b;
            plugin.Mapping.X = (ulong)1 << x;
            plugin.Mapping.Y = (ulong)1 << y;
            plugin.Mapping.LeftStick = (ulong)1 << leftStick;
            plugin.Mapping.RightStick = (ulong)1 << rightStick;
            plugin.Mapping.LeftThumb = (ulong)1 << leftThumb;
            plugin.Mapping.RightThumb = (ulong)1 << rightThumb;
            plugin.Mapping.Menu = (ulong)1 << menu;
            plugin.Mapping.Home = (ulong)1 << home;
        }
    }
}
