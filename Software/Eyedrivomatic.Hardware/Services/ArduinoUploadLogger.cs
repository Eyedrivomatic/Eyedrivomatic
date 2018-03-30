using System;
using System.ComponentModel.Composition;
using ArduinoUploader;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Hardware.Services
{
    [Export(typeof(IArduinoUploaderLogger))]
    public class ArduinoUploadLogger : IArduinoUploaderLogger
    {
        public void Error(string message, Exception exception)
        {
            Log.Error(this, $"{message} - [{exception}].");
        }

        public void Warn(string message)
        {
            Log.Warn(this, message);
        }

        public void Info(string message)
        {
            Log.Info(this, message);
        }

        public void Debug(string message)
        {
            Log.Debug(this, message);
        }

        public void Trace(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}