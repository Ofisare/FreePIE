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

        public void TriggerHapticPulse(uint controllerIndex, uint axis, uint durationMicroSec)
        {
            Api.TriggerHapticPulse(controllerIndex, axis, durationMicroSec);
        }

        private OpenVrData _data;

        public OpenVr6Dof HeadPose { get { return _data.HeadPose; } }

        public OpenVr6Dof LeftTouchPose{ get { return _data.LeftTouchPose; } }

        public OpenVr6Dof RightTouchPose{ get { return _data.RightTouchPose; } }

        public float LeftTrigger{ get { return _data.LeftTrigger; } }

        public float RightTrigger{ get { return _data.RightTrigger; } }

        public float LeftGrip{ get { return _data.LeftGrip; } }

        public float RightGrip{ get { return _data.RightGrip; } }

        public Pointf LeftStick{ get { return _data.LeftStick; } }

        public Pointf RightStick{ get { return _data.RightStick; } }

        public OpenVrStatus RightTouchStatus { get { return (OpenVrStatus) _data.RightTouchStatus; } }
        public OpenVrStatus LeftTouchStatus { get { return (OpenVrStatus) _data.LeftTouchStatus; } }
        public OpenVrStatus HeadStatus { get { return (OpenVrStatus) _data.HeadStatus; } }

        public bool IsHmdMounted { get { return _data.IsHmdMounted > 0; } }

        public OpenVrButton LeftButtons { get { return (OpenVrButton) _data.LeftButtons; } }
        public OpenVrButton LeftTouches { get { return (OpenVrButton) _data.LeftTouches; } }

        public OpenVrButton RightButtons { get { return (OpenVrButton)_data.RightButtons; } }
        public OpenVrButton RightTouches { get { return (OpenVrButton)_data.RightTouches; } }
    }

    [Global(Name = "openVR")]
    public class OpenVRGlobal : UpdateblePluginGlobal<OpenVRPlugin>
    {
        public OpenVRGlobal(OpenVRPlugin plugin) : base(plugin){}

        public OpenVrButton leftButtons { get { return plugin.LeftButtons; } }
        public OpenVrButton leftTouches { get { return plugin.LeftTouches; } }
        
        public OpenVrButton rightButtons { get { return plugin.RightButtons; } }
        public OpenVrButton rightTouches { get { return plugin.RightTouches; } }

        public OpenVr6Dof headPose { get { return plugin.HeadPose; } }
        public OpenVr6Dof leftTouchPose{ get { return plugin.LeftTouchPose; } }
        public OpenVr6Dof rightTouchPose{ get { return plugin.RightTouchPose; } }

        
        public OpenVrStatus headStatus { get { return plugin.HeadStatus; } }
        public OpenVrStatus leftTouchStatus { get { return plugin.LeftTouchStatus; } }
        public OpenVrStatus rightTouchStatus { get { return plugin.RightTouchStatus; } }

        public bool isMounted { get { return plugin.IsHmdMounted; } }
        public bool isHeadTracking { get { return plugin.HeadStatus.IsTracking(); } }
        public bool isLeftTouchTracking { get { return plugin.LeftTouchStatus.IsTracking(); } }

        public bool isRightTouchTracking { get { return plugin.RightTouchStatus.IsTracking(); } }

        #region Buttons
        public bool a { get { return (plugin.RightButtons & OpenVrButton.A) > 0; } }
        public bool b { get { return (plugin.RightButtons & OpenVrButton.B) > 0; } }
        public bool x { get { return (plugin.LeftButtons & OpenVrButton.A) > 0; } }
        public bool y { get { return (plugin.LeftButtons & OpenVrButton.B) > 0; } }

        public bool leftThumb { get { return (plugin.LeftButtons & OpenVrButton.System) > 0; } }

        public bool rightThumb { get { return (plugin.RightButtons & OpenVrButton.System) > 0; } }
        
        public float leftTrigger { get { return plugin.LeftTrigger; } }
        public float rightTrigger { get { return plugin.RightTrigger; } }

        public float leftGrip { get { return plugin.LeftGrip; } }
        public float rightGrip { get { return plugin.RightGrip; } }

        public float leftStickX { get { return plugin.LeftStick.x; } }
        public float rightStickX { get { return plugin.RightStick.x; } }

        public float leftStickY { get { return -plugin.LeftStick.y; } }
        public float rightStickY{ get { return -plugin.RightStick.y; } }


        public bool system { get { return (plugin.LeftButtons & OpenVrButton.System) > 0; } }
        
        public bool home { get { return (plugin.RightButtons & OpenVrButton.System) > 0; } }

        #endregion //Buttons

        #region Touches      

        public bool touchingA { get { return (plugin.RightTouches & OpenVrButton.A) > 0; } }
        public bool touchingB { get { return (plugin.RightTouches & OpenVrButton.B) > 0; } }
        public bool touchingX { get { return (plugin.LeftTouches & OpenVrButton.A) > 0; } }
        public bool touchingY { get { return (plugin.LeftTouches & OpenVrButton.B) > 0; } }
        
        #endregion // Touches
        public void center()
        {
            plugin.Center();
        }

        public void triggerHapticPulse(uint controllerIndex, uint axis, uint durationMicroSec)
        {
            plugin.TriggerHapticPulse(controllerIndex, axis, durationMicroSec);
        }
    }
}
