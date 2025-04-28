using FreePIE.Core.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreePIE.Core.Plugins.Telemetry
{
    /// <summary>
    /// this can be anything you want
    /// </summary>
    public struct ExampleData
    {
        public double X;
        public double Y;
        public double Z;
        public double Yaw;
        public double Pitch;
        public double Roll;
    }

    [GlobalType(Type = typeof(MMFGlobal))]
    public class MMFPlugin : Plugin
    {
        private MmfTelemetry<ExampleData> _telemetry;

        public ExampleData Data { get; private set; }

        public override string FriendlyName => "MMF";


        bool sending = false;
        
        public override object CreateGlobal()
        {
            return new MMFGlobal(this);
        }

        public override Action Start()
        {
             _telemetry = new MmfTelemetry<ExampleData>(new MmfTelemetryConfig()
            {
                Name = "FreePIE" // this is the name of the memory mapped file
            });

            return null;
        }

        public override void Stop()
        {
            _telemetry.Dispose();
        }

        public override void DoBeforeNextExecute()
        {
            if(_telemetry != null)
            {
                if (sending)
                {
                    // this would probably be similar to the TrackIr plugin
                }                
                
                Data = _telemetry.Receive();

                OnUpdate();
            }
        }
    }

    public class MMFGlobal : UpdateblePluginGlobal<MMFPlugin>
    {
        
        public MMFGlobal(MMFPlugin plugin) : base(plugin)
        {
        }


        //not sure how we would make setting data generic from the global
        public ExampleData data
        {
            get => plugin.Data;
            //set => plugin.telemetry = value;
        }

    }
}
