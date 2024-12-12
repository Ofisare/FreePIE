using FreePIE.Core.Contracts;

using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FreePIE.Core.Plugins.joystick
{
    public class JoyProp
    {
        public string name; public JoystickOffset offset; public ObjectProperties props;
    }

    [Global(Name = "joystick")]
    public class JoystickGlobal
    {

        private readonly Joystick _joystick;

        private JoystickState state;
        internal JoystickState State
        {
            get { return state ?? (state = _joystick.GetCurrentState()); }
        }

        public event EventHandler<JoystickEventArgs> Updated;
        public event EventHandler Started, Stopped;

        internal Capabilities caps { get { return _joystick.Capabilities; } }

        internal int numButtons { get { return _joystick.Capabilities.ButtonCount; } }

        internal int numAxes { get { return _joystick.Capabilities.AxeCount; } }

        internal int numPovs { get { return _joystick.Capabilities.PovCount; } }

        internal Dictionary<JoystickOffset, JoyProp> AxisInfo { get; }


        private static List<JoystickOffset> joystickAxisOffsets = new List<JoystickOffset>() { JoystickOffset.X, JoystickOffset.Y, JoystickOffset.Z, JoystickOffset.RotationX, JoystickOffset.RotationY, JoystickOffset.RotationZ, JoystickOffset.Sliders0, JoystickOffset.Sliders1 };


        public JoystickGlobal(int index, Joystick joystick)
        {
            this._joystick = joystick;

            Debug.WriteLine("Found {0} \"{1}\"", joystick.Information.Type, joystick.Information.ProductName);

            buttons = new bool[numButtons];

            AxisInfo = new Dictionary<JoystickOffset, JoyProp>();
            for (int i = 0; i < joystickAxisOffsets.Count; i++)
            {
                try
                {
                    var mightGoBoom = joystick.GetObjectInfoByName(joystickAxisOffsets[i].ToString());
                    AxisInfo.Add(joystickAxisOffsets[i], new JoyProp { offset = joystickAxisOffsets[i], name = mightGoBoom.Name, props = joystick.GetObjectPropertiesById(mightGoBoom.ObjectId) });
                }
                catch { }
            }
          
        }        
        

        public string name { get { return _joystick?.Information.ProductName ?? ""; } }
        
        internal void Update(params object[] args)
        {
            state = _joystick.GetCurrentState();

            buttons = state.Buttons.Take(_joystick.Capabilities.ButtonCount).ToArray();

            Updated?.Invoke(this, new JoystickEventArgs(state));
        }

        private double mapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return yMin + (yMax - yMin) * (x - xMin) / (xMax - xMin);
        }

        private double ensureMapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return Math.Max(Math.Min(mapRange(x, xMin, xMax, yMin, yMax), Math.Max(yMin, yMax)), Math.Min(yMin, yMax));
        }

        #region stuff




        public float x
        {
            get 
            {
                return normalize(State.X, JoystickOffset.X);
            }
        }

        public float y
        {
            get { return normalize(State.Y, JoystickOffset.Y); }
        }

        public float z
        {
            get { return normalize(State.Z, JoystickOffset.Z); }
        }

        public float rotationX
        {
            get { return normalize(State.RotationX, JoystickOffset.RotationX); }
        }

        public float rotationY
        {
            get { return normalize(State.RotationY, JoystickOffset.RotationZ); }
        }

        public float rotationZ
        {
            get { return normalize(State.RotationZ, JoystickOffset.RotationZ); }
        }

        

        public bool[] buttons { get; private set; }

        public float[] sliders
        {
            get { return State.Sliders.Select((s,i) => normalize(s, i == 0 ? JoystickOffset.Sliders0 : JoystickOffset.Sliders1 )).ToArray(); }
        }

        public int[] pov
        {
            get
            {
                return numPovs > 0 ? State.PointOfViewControllers.Select(p =>
                {
                    var r = p > 0 ? p / 100 : p;
                    return r;
                }).ToArray() : new[] { -1, -1, -1, -1 };
            }
        }

        private float normalize(int value, JoystickOffset axis )
        {
            // Revisit this later if we want to make the output similar to GLovepie values between -1 and 1
            //if (AxisInfo.TryGetValue(axis, out JoyProp p))
            //{
            //    return ensureMapRange(value, p.props.Range.Minimum, p.props.Range.Maximum, -1, 1);
            //}
            return value;
        }
        #endregion
    }
}
