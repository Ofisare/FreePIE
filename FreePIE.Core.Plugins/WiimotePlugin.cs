﻿using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Wiimote;
using System.IO;
using System.Linq;

namespace FreePIE.Core.Plugins
{
    [GlobalEnum]
    public enum WiimoteButtons : ushort
    {
        DPadLeft = 0x0100,
        DPadRight = 0x0200,
        DPadDown = 0x0400,
        DPadUp = 0x0800,
        A = 0x0008,
        B = 0x0004,
        Plus = 0x1000,
        Minus = 0x0010,
        One = 0x0002,
        Two = 0x0001,
        Home = 0x080
    }

    [GlobalEnum]
    public enum NunchuckButtons
    {
        Z = 0x01, C = 0x02
    }

    [GlobalEnum]
    public enum GuitarButtons : ushort
    {
        StrumDown = 0x4000,
        StrumUp = 0x0001,
        Green = 0x0010,
        Red = 0x0040,
        Yellow = 0x0008,
        Blue = 0x0020,
        Orange = 0x0080,
        Plus = 0x0400,
        Minus = 0x1000
    }
    [GlobalEnum]
    public enum ClassicControllerButtons : ushort
    {
        DPadLeft = 0x0002,
        DPadRight = 0x8000,
        DPadDown = 0x4000,
        DPadUp = 0x0001,
        A = 0x0010,
        B = 0x0040,
        X = 0x0008,
        Y = 0x0020,
        TriggerLeft = 0x2000,
        TriggerRight = 0x0200,
        ZLeft = 0x0080,
        ZRight = 0x0004,
        Plus = 0x0400,
        Minus = 0x1000,
        Home = 0x0800
    }

    [GlobalEnum, Flags]
    public enum WiimoteCapabilities : ushort
    {
        None = 0,
        MotionPlus = 2,
        Extension = 4,
        IR = 8
    }

    public enum WiimoteDataValid
    {
        Acceleration = 0x0002,
        MotionPlus = 0x0004, 
        Nunchuck = 0x0008,
        Guitar = 0x0010,
        BalanceBoard = 0x0020,
        ClassicController = 0x0001
    }

    [GlobalEnum, Flags]
    public enum WiimoteExtensions
    {
        None = 0,
        Nunchuck = 1,
        ClassicController = 2,
        ClassicControllerPro = 4,
        GuitarHeroGuitar = 8,
        GuitarHeroDrums = 16,
        MotionPlus = 32,
        BalanceBoard = 64,
        //Skip passthrough as its not an exposed id.
        Unknown = 256
    }

    public enum FusionType
    {
        SimpleIntegration,
        Mahony
    }

    /*
    [GlobalType(Type = typeof(WiimoteGlobal), IsIndexed = true)]
    public class WiimotePlugin : Plugin
    {
        private LogLevel logLevel;
        private FusionType fuserType;
        private bool doLog;
        private IWiimoteBridge wiimoteBridge;
        private Dictionary<uint, Action> globalUpdators;
        private Dictionary<uint, Action> globalUpdatorsCapabilities;
        private Dictionary<uint, Action> globalUpdatorsStatus;
        private IList<uint> updatedWiimotes;
        private IList<uint> updatedWiimotesCapabilities;
        private IList<uint> updatedWiimotesStatus;

        private Dictionary<FusionType, Func<IMotionPlusFuser>> fuserFactories = new Dictionary <FusionType, Func<IMotionPlusFuser>>()
            {
                {FusionType.SimpleIntegration, () => new SimpleIntegrationMotionPlusFuser()},
                {FusionType.Mahony, () => new MahonyMotionPlusFuser()}
            };

        public override object CreateGlobal()
        {
            string applicationDataSubPath = @"FreePIE\dolphiimote.log";
            var applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationDataSubPath);
            var dolphiimoteLogPath = !Environment.CurrentDirectory.StartsWith(@"C:\Program Files (x86)\FreePIE") 
                ? @"dolphiimote.log" : applicationDataPath;
            wiimoteBridge = new DolphiimoteBridge(logLevel, doLog ? dolphiimoteLogPath : null, CreateMotionplusFuser);
            globalUpdators = new Dictionary<uint, Action>();
            globalUpdatorsCapabilities = new Dictionary<uint, Action>();
            globalUpdatorsStatus = new Dictionary<uint, Action>();
            updatedWiimotes = new List<uint>();
            updatedWiimotesCapabilities = new List<uint>();
            updatedWiimotesStatus = new List<uint>();
            return Enumerable.Range(0, 4).Select(i => new WiimoteGlobal(this, wiimoteBridge.GetData((uint)i), globalUpdators, globalUpdatorsCapabilities, globalUpdatorsStatus)).ToArray();
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            if (index > 2)
                return false;

            if (index == 0)
            {
                property.Name = "LogLevel";
                property.Caption = "Log level";

                foreach (var val in Enum.GetNames(typeof (LogLevel)))
                    property.Choices.Add(val, val);

                property.DefaultValue = LogLevel.Warning.ToString();
                property.HelpText = "Set the log level for Dolphiimote library";
            }

            if (index == 1)
            {
                property.Name = "DoLog";
                property.Caption = "Enable logging";
                property.DefaultValue = false;
                property.HelpText = "Enable logging for Dolphiimote library. Look for dolphiimote.log in the application directory.";
            }

            if (index == 2)
            {
                property.Name = "FuserType";
                property.Caption = "M+ fuser type";

                foreach (var val in Enum.GetNames(typeof(FusionType)))
                    property.Choices.Add(val, val);

                property.DefaultValue = FusionType.SimpleIntegration.ToString();
                property.HelpText = "Choose between a extended kalman filter (gyro and acc) or simple integration (only gyro)";
            }

            return true;
        }

        private IMotionPlusFuser CreateMotionplusFuser()
        {
            return fuserFactories[fuserType]();
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), (string)properties["LogLevel"]);
            doLog = (bool)properties["DoLog"];
            fuserType = (FusionType)Enum.Parse(typeof(FusionType), (string)properties["FuserType"]);
            return true;
        }

        public override Action Start()
        {
            wiimoteBridge.DataReceived += WiimoteDataReceived;
            wiimoteBridge.CapabilitiesChanged += CapabilitiesChanged;
            wiimoteBridge.StatusChanged += StatusChanged;
            wiimoteBridge.Init();
            return null;
        }

        public void Enable(byte wiimote, WiimoteCapabilities flags)
        {
            wiimoteBridge.Enable(wiimote, flags);
        }
        public void SetRumble(byte wiimote, Boolean shouldRumble)
        {
            wiimoteBridge.SetRumble(wiimote, shouldRumble);
        }
        public void SetLedState(byte wiimote, int led_state)
        {
            wiimoteBridge.SetLEDState(wiimote, led_state);
        }
        public void RequestStatus(byte wiimote)
        {
            wiimoteBridge.RequestStatus(wiimote);
        }
        private void WiimoteDataReceived(object sender, UpdateEventArgs<uint> updated)
        {
            updatedWiimotes.Add(updated.UpdatedValue);
        }

        private void CapabilitiesChanged(object sender, UpdateEventArgs<uint> updated)
        {
            updatedWiimotesCapabilities.Add(updated.UpdatedValue);
        }

        private void StatusChanged(object sender, UpdateEventArgs<uint> updated)
        {
            updatedWiimotesStatus.Add(updated.UpdatedValue);
        }
        public override void Stop()
        {
            wiimoteBridge.Dispose();
        }

        public override string FriendlyName
        {
            get { return "Wiimote"; }
        }

        public override void DoBeforeNextExecute()
        {
            updatedWiimotes.Clear();
            updatedWiimotesCapabilities.Clear();
            wiimoteBridge.DoTick();

            foreach(var wiimote in updatedWiimotes)
                globalUpdators[wiimote]();
            foreach (var wiimote in updatedWiimotesCapabilities)
                globalUpdatorsCapabilities[wiimote]();
            foreach (var wiimote in updatedWiimotesStatus)
                globalUpdatorsStatus[wiimote]();
        }
    }

    [Global(Name = "wiimote")]
    public class WiimoteGlobal
    {
        private readonly WiimotePlugin plugin;
        private readonly IWiimoteData data;

        private readonly Action accelerationTrigger;
        private readonly Action buttonTrigger;
        private readonly Action motionPlusTrigger;
        private readonly Action nunchuckTrigger;
        private readonly Action classicControllerTrigger;
        private readonly Action guitarTrigger;
        private readonly Action balanceBoardTrigger;
        private readonly Action capabilitiesTrigger;
        private readonly Action statusTrigger;

        private readonly Action accelerationCalibratedTrigger;
        private readonly Action motionPlusCalibratedTrigger;

        public WiimoteGlobal(WiimotePlugin plugin, IWiimoteData data, Dictionary<uint, Action> updaters, Dictionary<uint, Action> capabilitiesUpdaters, Dictionary<uint, Action> statusUpdaters)
        {
            this.plugin = plugin;
            this.data = data;

            acceleration = new AccelerationGlobal(data, out accelerationTrigger, out accelerationCalibratedTrigger);
            buttons = new WiimoteButtonState(data, out buttonTrigger);
            motionplus = new MotionPlusGlobal(data, out motionPlusTrigger, out motionPlusCalibratedTrigger);
            nunchuck = new NunchuckGlobal(data, out nunchuckTrigger);
            classicController = new ClassicControllerGlobal(data, out classicControllerTrigger);
            guitar = new GuitarGlobal(data, out guitarTrigger);
            balanceBoard = new BalanceBoardGlobal(data, out balanceBoardTrigger);
            capabilities = new CapabilitiesGlobal(data, out capabilitiesTrigger);
            //TODO: Allow setting led status
            status = new StatusGlobal(plugin, data, out statusTrigger);

            updaters[data.WiimoteNumber] = OnWiimoteDataReceived;
            capabilitiesUpdaters[data.WiimoteNumber] = capabilitiesTrigger;
            statusUpdaters[data.WiimoteNumber] = statusTrigger;
        }
        public void enable(WiimoteCapabilities flags)
        {
            plugin.Enable(data.WiimoteNumber, flags);
        }
        public void setRumble(Boolean shouldRumble)
        {
            plugin.SetRumble(data.WiimoteNumber, shouldRumble);
        }
        private void OnWiimoteDataReceived()
        {

            buttonTrigger();
            if (data.IsDataValid(WiimoteDataValid.Acceleration))
                accelerationTrigger();

            if (data.IsDataValid(WiimoteDataValid.MotionPlus))
                motionPlusTrigger();

            if (data.IsDataValid(WiimoteDataValid.Nunchuck))
                nunchuckTrigger();

            if (data.IsDataValid(WiimoteDataValid.ClassicController))
                classicControllerTrigger();

            if (data.IsDataValid(WiimoteDataValid.Guitar))
                guitarTrigger();

            if (data.IsDataValid(WiimoteDataValid.BalanceBoard))
                balanceBoardTrigger();

            if (data.Acceleration.DidCalibrate)
                accelerationCalibratedTrigger();

            if (data.MotionPlus.DidCalibrate)
                motionPlusCalibratedTrigger();
        }
        public CapabilitiesGlobal capabilities
        {
            get;
            private set;
        }
        public MotionPlusGlobal motionplus
        { 
            get;
            private set;
        }

        public EulerAngles ahrs
        {
            get { return data.MotionPlusEulerAngles; }
        }

        public AccelerationGlobal acceleration
        {
            get;
            private set;
        }

        public NunchuckGlobal nunchuck
        {
            get;
            private set;
        }

        public ClassicControllerGlobal classicController
        {
            get;
            private set;
        }

        public GuitarGlobal guitar
        {
            get;
            private set;
        }
        public BalanceBoardGlobal balanceBoard
        {
            get;
            private set;
        }
        public StatusGlobal status
        {
            get;
            private set;
        }
        public WiimoteButtonState buttons
        { 
            get;
            private set;
        }
    }
    */
}
