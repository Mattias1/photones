﻿using System;
using System.Diagnostics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.Photones.GameUI {
    class FpsMeasurer {

        private int _frameNrMod60 = 0;

        // FPS
        private const double FPS_SMOOTHING_FACTOR = 0.05;
        private double _fpsSmoothedAvg = 60;
        private double _fpsSmoothedDev = 10;
        private double _fpsCurrentMax = 0;
        private double _fpsCurrentMin = 100;
        private double _fpsPreviousMax = 0;
        private double _fpsPreviousMin = 100;

        // Frametime
        private const double FRAMETIME_SMOOTHING_FACTOR = 0.01;
        private double _frametimeSmoothedAvg = 3000;
        private double _frametimeSmoothedDev = 1000;
        private double _frametimeCurrentMax = 0;
        private double _frametimeCurrentMin = 10000;
        private double _frametimePreviousMax = 0;
        private double _frametimePreviousMin = 10000;

        private Stopwatch _watch;

        public void StartFrame(TimeSpan elapsedTime) {
            if (elapsedTime < new TimeSpan(0.0001)) {
                // This is basically to skip the first frame, because its elapsed time is out of whack
                return;
            }

            if (_frameNrMod60 >= 60) {
                _frameNrMod60 = 0;
            }

            CalculateFpsStats(elapsedTime);

            _watch = Stopwatch.StartNew();

        }

        public void EndFrame() {
            if (_watch == null)
                return;

            _watch.Stop();

            CalculateFrametimeStats(_watch);

            _frameNrMod60++;
            return;
        }

        private void CalculateFpsStats(TimeSpan elapsedTime) {
            var fps = TimeSpan.One / elapsedTime;
            _fpsSmoothedAvg = FPS_SMOOTHING_FACTOR * fps + (1 - FPS_SMOOTHING_FACTOR) * _fpsSmoothedAvg;
            var fpsDev = Math.Abs(_fpsSmoothedAvg - fps);
            _fpsSmoothedDev = FPS_SMOOTHING_FACTOR * fpsDev + (1 - FPS_SMOOTHING_FACTOR) * _fpsSmoothedDev;

            _fpsCurrentMax = Math.Max(fps, _fpsCurrentMax);
            _fpsCurrentMin = Math.Min(fps, _fpsCurrentMin);

            if (_frameNrMod60 == 0) {
                _fpsPreviousMax = _fpsCurrentMax;
                _fpsPreviousMin = _fpsCurrentMin;
                _fpsCurrentMax = 0;
                _fpsCurrentMin = double.MaxValue;
            }
        }

        private void CalculateFrametimeStats(Stopwatch watch) {
            // frametime is in microseconds
            var frametime = watch.Elapsed.Ticks / (System.TimeSpan.TicksPerMillisecond / 1000);
            _frametimeSmoothedAvg = FRAMETIME_SMOOTHING_FACTOR * frametime + (1 - FRAMETIME_SMOOTHING_FACTOR) * _frametimeSmoothedAvg;
            var frametimeDev = Math.Abs(_frametimeSmoothedAvg - frametime);
            _frametimeSmoothedDev = FRAMETIME_SMOOTHING_FACTOR * frametimeDev + (1 - FRAMETIME_SMOOTHING_FACTOR) * _frametimeSmoothedDev;

            _frametimeCurrentMax = Math.Max(frametime, _frametimeCurrentMax);
            _frametimeCurrentMin = Math.Min(frametime, _frametimeCurrentMin);

            if (_frameNrMod60 == 0) {
                _frametimePreviousMax = _frametimeCurrentMax;
                _frametimePreviousMin = _frametimeCurrentMin;
                _frametimeCurrentMax = 0;
                _frametimeCurrentMin = double.MaxValue;
            }
        }

        public string FpsString => $"{(int)_fpsSmoothedAvg}+/-{(int)_fpsSmoothedDev} [{(int)_fpsPreviousMin},{(int)_fpsPreviousMax}]";
        public string FrameTimeString => $"{(int)_frametimeSmoothedAvg}+/-{(int)_frametimeSmoothedDev} [{(int)_frametimePreviousMin},{(int)_frametimePreviousMax}]";
    }
}
