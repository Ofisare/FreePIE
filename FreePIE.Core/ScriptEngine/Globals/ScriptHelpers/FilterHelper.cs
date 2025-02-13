﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using FreePIE.Core.Common;
using FreePIE.Core.Contracts;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers.Strategies;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [GlobalEnum]
    public enum Units
    {
        Degrees = 1,
        Radians = 2
    }

    [Global(Name = "filters")]
    public class FilterHelper : IScriptHelper
    {
        private readonly Dictionary<string, double> deltaLastSamples;
        private readonly Dictionary<string, double> simpleLastSamples;
        private readonly Dictionary<string, ContinuesRotationStrategy> continousRotationStrategies;
        private readonly Dictionary<string, Stopwatch> stopwatches; 

        public FilterHelper()
        {
            deltaLastSamples = new  Dictionary<string, double>();
            simpleLastSamples = new Dictionary<string, double>();
            continousRotationStrategies = new Dictionary<string, ContinuesRotationStrategy>();
            stopwatches = new Dictionary<string, Stopwatch>();
        }

        [NeedIndexer]
        public double simple(double x, double smoothing, string indexer)
        {
            if(smoothing < 0 || smoothing > 1)
                throw new ArgumentException("Smoothing must be a value between 0 and 1");

            var lastSample = x;
            if (simpleLastSamples.ContainsKey(indexer))
                lastSample = simpleLastSamples[indexer];

            lastSample = (lastSample*smoothing) + (x*(1 - smoothing));
            simpleLastSamples[indexer] = lastSample;

            return lastSample;
        }

        [NeedIndexer]
        public double delta(double x, string indexer)
        {
            var lastSample = x;
            if (deltaLastSamples.ContainsKey(indexer))
                lastSample = deltaLastSamples[indexer];

            deltaLastSamples[indexer] = x;

            return x - lastSample;
        }

        [Deprecated("continuousRotation")]
        [NeedIndexer]
        public double continousRotation(double x, string indexer)
        {
            return continuousRotation(x, indexer);
        }

        [Deprecated("continuousRotation")]
        [NeedIndexer]
        public double continousRotation(double x, Units unit, string indexer)
        {
            return continuousRotation(x, unit, indexer);
        }

        [NeedIndexer]
        public double continuousRotation(double x, string indexer)
        {
            return continuousRotation(x, Units.Radians, indexer);
        }

        [NeedIndexer]
        public double continuousRotation(double x, Units unit, string indexer)
        {
            if(!continousRotationStrategies.ContainsKey(indexer))
                continousRotationStrategies[indexer] = new ContinuesRotationStrategy(unit);

            var strategy = continousRotationStrategies[indexer];
            strategy.Update(x);

            return strategy.Out;
        }


        public double deadzone(double x, double deadZone, double minY, double maxY) => Maths.DeadZone(x, deadZone, minY, maxY);


        public double deadband(double x, double deadZone, double minY, double maxY) => Maths.Deadband(x, deadZone, minY, maxY);
        

        public double deadband(double x, double deadZone) => Maths.Deadband(x, deadZone);

        public double mapRange(double x, double xMin, double xMax, double yMin, double yMax) => Maths.MapRange(x, xMin, xMax, yMin, yMax);

        public double ensureMapRange(double x, double xMin, double xMax, double yMin, double yMax) => Maths.EnsureMapRange(x, xMin, xMax, yMin, yMax);

        [NeedIndexer]
        public bool stopWatch(bool state, int milliseconds, string indexer)
        {
            if (!stopwatches.ContainsKey(indexer) && state)
            {
                stopwatches[indexer] = new Stopwatch();
                stopwatches[indexer].Start();
            }

            if (stopwatches.ContainsKey(indexer) && !state)
            {
                stopwatches[indexer].Stop();
                stopwatches.Remove(indexer);
            }

            if (!state) return false;

            var watch = stopwatches[indexer];
            if (watch.ElapsedMilliseconds >= milliseconds)
            {
                watch.Stop();
                stopwatches.Remove(indexer);
                return true;
            }

            return false;
        }
    }
}
