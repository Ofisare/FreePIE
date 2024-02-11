using FreePIE.Core.Contracts;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(VigemGlobal))]
    public class VigemPlugin : Plugin
    {
        public ViGEmClient Client { get; private set; }
        public string ErrorMessage { get; private set; }

        public IDualShock4Controller DualShockController { get; private set; }
        public IXbox360Controller XBoxController { get; private set; }

        public override string FriendlyName => "Vigem";

        public override object CreateGlobal()
        {
            return new VigemGlobal(this);
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
            try
            {
                Client = new ViGEmClient();
            }
            catch (Exception ex)
            {
                Client = null;
                ErrorMessage = "Vigem SDK failed to init: " + ex.Message;
            }

            return null;
        }

        public override void Stop()
        {
            if (DualShockController != null)
            {
                DualShockController.Disconnect();
                DualShockController.Dispose();
                DualShockController = null;
            }

            if (XBoxController != null)
            {
                XBoxController.Disconnect();
                XBoxController = null;
            }

            if (Client != null)
            {
                Client.Dispose();
                Client = null;
            }
        }

        public override void DoBeforeNextExecute()
        {
            if (DualShockController != null)
            {
                DualShockController.SubmitReport();
            }
            if (XBoxController != null)
            {
                XBoxController.SubmitReport();
            }
            OnUpdate();
        }

        public void CreateDualShockController()
        {
            if (Client == null)
                return;

            if (DualShockController != null)
                return;

            DualShockController = Client.CreateDualShock4Controller();
            DualShockController.Connect();
        }

        public void CreateXBoxController()
        {
            if (Client == null)
                return;

            if (XBoxController != null)
                return;

            XBoxController = Client.CreateXbox360Controller();
            XBoxController.Connect();
        }
    }

    [GlobalEnum]
    public enum VigemController
    {
        DualShockController,
        XBoxController,
    }

    [GlobalEnum]
    public enum VigemDPadDirection
    {
        None,
        Northwest,
        West,
        Southwest,
        South,
        Southeast,
        East,
        Northeast,
        North
    }

    [GlobalEnum]
    public enum VigemButton
    {
        ThumbRight,
        ThumbLeft,
        ShoulderRight,
        ShoulderLeft,

        Options,
        Share,

        Triangle,
        Circle,
        Cross,
        Square,
        
        Start,
        Back,
        Guide,

        A,
        B,
        X,
        Y    
    }

    [GlobalEnum]
    public enum VigemSide
    {
        Left,
        Right
    }

    [Global(Name = "vigem")]
    public class VigemGlobal : UpdateblePluginGlobal<VigemPlugin>
    {
        public VigemGlobal(VigemPlugin plugin) : base(plugin) { }

        public string Error() => plugin.ErrorMessage;

        public void CreateController(VigemController controller)
        {
            switch (controller)
            {
                case VigemController.DualShockController:
                    plugin.CreateDualShockController();
                    break;
                case VigemController.XBoxController:
                    plugin.CreateXBoxController();
                    break;
            }
        }

        public void SetDPad(VigemController controller, VigemDPadDirection direction)
        {
            switch (controller)
            {
                case VigemController.DualShockController:
                    {
                        switch (direction)
                        {
                            case VigemDPadDirection.Northwest:
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.Northwest);
                                break;
                            case VigemDPadDirection.West:
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.West);
                                break;
                            case VigemDPadDirection.Southwest:
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.Southwest);
                                break;
                            case VigemDPadDirection.South:
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.South);
                                break;
                            case VigemDPadDirection.Southeast:
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.Southeast);
                                break;
                            case VigemDPadDirection.East:
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.East);
                                break;
                            case VigemDPadDirection.Northeast:
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.Northeast);
                                break;
                            case VigemDPadDirection.North:
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.North);
                                break;
                            case VigemDPadDirection.None:
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.None);
                                break;
                        }
                        break;
                    }
                case VigemController.XBoxController:
                    {
                        bool left = false;
                        bool right = false;
                        bool up = false;
                        bool down = false;

                        switch (direction)
                        {
                            case VigemDPadDirection.Northwest:
                                up = true;
                                left = true;
                                break;
                            case VigemDPadDirection.West:
                                left = true;
                                break;
                            case VigemDPadDirection.Southwest:
                                down = true;
                                left = true;
                                break;
                            case VigemDPadDirection.South:
                                down = true;
                                break;
                            case VigemDPadDirection.Southeast:
                                down = true;
                                right = true;
                                break;
                            case VigemDPadDirection.East:
                                right = true;
                                break;
                            case VigemDPadDirection.Northeast:
                                up = true;
                                right = true;
                                break;
                            case VigemDPadDirection.North:
                                up = true;
                                break;
                        }

                        plugin.XBoxController.SetButtonState(Xbox360Button.Left, left);
                        plugin.XBoxController.SetButtonState(Xbox360Button.Right, right);
                        plugin.XBoxController.SetButtonState(Xbox360Button.Up, up);
                        plugin.XBoxController.SetButtonState(Xbox360Button.Down, down);
                        break;
                    }
            }
        }
        public void SetDPad(VigemController controller, float x, float y, float threshold)
        {
            bool left = x < -threshold;
            bool right = x > threshold;
            bool up = y > threshold;
            bool down = y < -threshold;

            switch (controller)
            {
                case VigemController.DualShockController:
                    {
                        if (left)
                        {
                            if (up)
                            {
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.Northwest);
                            }
                            else if (down)
                            {
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.Southwest);
                            }
                            else
                            {
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.West);
                            }
                        }
                        else if (right)
                        {
                            if (up)
                            {
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.Northeast);
                            }
                            else if (down)
                            {
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.Southeast);
                            }
                            else
                            {
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.East);
                            }
                        }
                        else
                        {
                            if (up)
                            {
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.North);
                            }
                            else if (down)
                            {
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.South);
                            }
                            else
                            {
                                plugin.DualShockController.SetDPadDirection(DualShock4DPadDirection.None);
                            }
                        }
                        break;
                    }
                case VigemController.XBoxController:
                    {
                        plugin.XBoxController.SetButtonState(Xbox360Button.Left, left);
                        plugin.XBoxController.SetButtonState(Xbox360Button.Right, right);
                        plugin.XBoxController.SetButtonState(Xbox360Button.Up, up);
                        plugin.XBoxController.SetButtonState(Xbox360Button.Down, down);
                        break;
                    }
            }
        }

        public void SetButtonState(VigemController controller, VigemButton button, bool pressed)
        {
            switch (controller)
            {
                case VigemController.DualShockController:
                    {
                        DualShock4Button dualShockButton = null;
                        switch (button)
                        {
                            case VigemButton.ThumbRight:
                                dualShockButton = DualShock4Button.ThumbRight;
                                break;
                            case VigemButton.ThumbLeft:
                                dualShockButton = DualShock4Button.ThumbLeft;
                                break;
                            case VigemButton.ShoulderRight:
                                dualShockButton = DualShock4Button.ShoulderRight;
                                break;
                            case VigemButton.ShoulderLeft:
                                dualShockButton = DualShock4Button.ShoulderLeft;
                                break;
                            case VigemButton.Options:
                            case VigemButton.Start:
                            case VigemButton.Back:
                                dualShockButton = DualShock4Button.Options;
                                break;
                            case VigemButton.Share:
                            case VigemButton.Guide:
                                dualShockButton = DualShock4Button.Share;
                                break;
                            case VigemButton.Triangle:
                            case VigemButton.Y:
                                dualShockButton = DualShock4Button.Triangle;
                                break;
                            case VigemButton.Square:
                            case VigemButton.X:
                                dualShockButton = DualShock4Button.Square;
                                break;
                            case VigemButton.Circle:
                            case VigemButton.B:
                                dualShockButton = DualShock4Button.Circle;
                                break;
                            case VigemButton.Cross:
                            case VigemButton.A:
                                dualShockButton = DualShock4Button.Cross;
                                break;
                        }
                        if (dualShockButton != null)
                            plugin.DualShockController.SetButtonState(dualShockButton, pressed);
                        break;
                    }
                case VigemController.XBoxController:
                    {
                        Xbox360Button xboxButton = null;
                        switch (button)
                        {
                            case VigemButton.ThumbRight:
                                xboxButton = Xbox360Button.RightThumb;
                                break;
                            case VigemButton.ThumbLeft:
                                xboxButton = Xbox360Button.LeftThumb;
                                break;
                            case VigemButton.ShoulderRight:
                                xboxButton = Xbox360Button.RightShoulder;
                                break;
                            case VigemButton.ShoulderLeft:
                                xboxButton = Xbox360Button.LeftShoulder;
                                break;
                            case VigemButton.Options:
                            case VigemButton.Start:
                                xboxButton = Xbox360Button.Start;
                                break;
                            case VigemButton.Back:
                                xboxButton = Xbox360Button.Back;
                                break;
                            case VigemButton.Share:
                            case VigemButton.Guide:
                                xboxButton = Xbox360Button.Guide;
                                break;
                            case VigemButton.Triangle:
                            case VigemButton.Y:
                                xboxButton = Xbox360Button.Y;
                                break;
                            case VigemButton.Square:
                            case VigemButton.X:
                                xboxButton = Xbox360Button.X;
                                break;
                            case VigemButton.Circle:
                            case VigemButton.B:
                                xboxButton = Xbox360Button.B;
                                break;
                            case VigemButton.Cross:
                            case VigemButton.A:
                                xboxButton = Xbox360Button.A;
                                break;
                        }
                        if (xboxButton != null)
                            plugin.XBoxController.SetButtonState(xboxButton, pressed);
                        break;
                    }
            }
        }

        public void SetTrigger(VigemController controller, VigemSide side, float trigger)
        {
            byte byteTrigger = (byte)Math.Max(byte.MinValue, Math.Min(byte.MaxValue, byte.MaxValue * trigger));

            switch (controller)
            {
                case VigemController.DualShockController:
                    switch (side)
                    {
                        case VigemSide.Left:
                            plugin.DualShockController.SetSliderValue(DualShock4Slider.LeftTrigger, byteTrigger);
                            break;
                        case VigemSide.Right:
                            plugin.DualShockController.SetSliderValue(DualShock4Slider.RightTrigger, byteTrigger);
                            break;
                    }
                    break;
                case VigemController.XBoxController:
                    switch (side)
                    {
                        case VigemSide.Left:
                            plugin.XBoxController.SetSliderValue(Xbox360Slider.LeftTrigger, byteTrigger);
                            break;
                        case VigemSide.Right:
                            plugin.XBoxController.SetSliderValue(Xbox360Slider.RightTrigger, byteTrigger);
                            break;
                    }
                    break;
            }
        }

        public void SetStick(VigemController controller, VigemSide side, float x, float y)
        {
           
            switch (controller)
            {
                case VigemController.DualShockController:
                    byte byteX = (byte)Math.Max(byte.MinValue, Math.Min(byte.MaxValue, byte.MaxValue * (0.5f + 0.5 * x)));
                    byte byteY = (byte)Math.Max(byte.MinValue, Math.Min(byte.MaxValue, byte.MaxValue * (0.5f + 0.5 * y)));

                    switch (side)
                    {
                        case VigemSide.Left:
                            plugin.DualShockController.SetAxisValue(DualShock4Axis.LeftThumbX, byteX);
                            plugin.DualShockController.SetAxisValue(DualShock4Axis.LeftThumbY, byteY);
                            break;
                        case VigemSide.Right:
                            plugin.DualShockController.SetAxisValue(DualShock4Axis.RightThumbX, byteX);
                            plugin.DualShockController.SetAxisValue(DualShock4Axis.RightThumbY, byteY);
                            break;
                    }
                    break;
                case VigemController.XBoxController:
                    short shortX = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, short.MaxValue * x));
                    short shortY = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, short.MaxValue * y));
                    switch (side)
                    {
                        case VigemSide.Left:
                            plugin.XBoxController.SetAxisValue(Xbox360Axis.LeftThumbX, shortX);
                            plugin.XBoxController.SetAxisValue(Xbox360Axis.LeftThumbY, shortY);
                            break;
                        case VigemSide.Right:
                            plugin.XBoxController.SetAxisValue(Xbox360Axis.RightThumbX, shortX);
                            plugin.XBoxController.SetAxisValue(Xbox360Axis.RightThumbY, shortY);
                            break;
                    }
                    break;
            }
        }
    }
}
