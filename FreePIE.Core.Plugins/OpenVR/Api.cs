using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.OculusVR
{
    public static class Api
    {
        [DllImport("OVRFreePIE.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int ovr_freepie_init();
        [DllImport("OVRFreePIE.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int ovr_freepie_read(out OpenVrData output);
        [DllImport("OVRFreePIE.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int ovr_freepie_destroy();
        [DllImport("OVRFreePIE.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int ovr_freepie_reset_orientation();
        [DllImport("OVRFreePIE.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int ovr_freepie_trigger_haptic_pulse(uint controllerIndex, uint durationMicroSec, float frequency, float amplitude);

        public static bool Init()
        {
            return ovr_freepie_init() == 0;
        }

        public static void Read(out OpenVrData output)
        {
            ovr_freepie_read(out output);
        }

        public static bool Dispose()
        {
            return ovr_freepie_destroy() == 0;
        }

        public static bool Center()
        {
            return ovr_freepie_reset_orientation() == 0;
        }

        public static void TriggerHapticPulse(uint controllerIndex, uint durationMicroSec, float frequency, float amplitude)
        {
            ovr_freepie_trigger_haptic_pulse(controllerIndex, durationMicroSec, frequency, amplitude);
        }
    }
}
