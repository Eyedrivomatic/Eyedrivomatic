//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace FirmwareTests
{
    public class TestConnection
    {
        private const char Ack = (char)0x06;
        private const char Nak = (char)0x15;
        private const bool DefaultUseChecksum = true;

        public SerialPort Serial { set; get; }

        public StreamReader Reader { set; get; }

        public void Initialize()
        {
            Serial = new SerialPort("COM4", 19200) { Encoding = Encoding.ASCII, ReadTimeout = 3000 };
            Serial.Open();
            Serial.DiscardInBuffer();
            Serial.DiscardOutBuffer();
            Serial.DtrEnable = true;
            Thread.Sleep(100);
            Serial.DtrEnable = false; //reset device

            Reader = new StreamReader(Serial.BaseStream, Encoding.ASCII);
        }

        public void Stop()
        {
            Serial.DtrEnable = true;
            Thread.Sleep(100);
            Serial.DtrEnable = false; //reset device prevents serial buffer overflow into the next test.

            Reader.Dispose();

            Serial.Close();
            Serial.Dispose();
        }


        public bool ReadStartup()
        {
            string message;

            while (true)
            {
                if (!ReadMessage(out message)) return false;
                if (message.StartsWith("START:")) break;
            }

            return ReadMessage(out message) && message.StartsWith("STATUS:");
        }

        private static byte GetCheckChar(string msg)
        {
            // ReSharper disable once RedundantAssignment
            return Encoding.ASCII.GetBytes(msg).Aggregate((a, c) => c ^= a);
        }

        private static bool ValidateChecksum(string msg)
        {
            var expectedCheckChar = GetCheckChar(msg.Substring(0, msg.Length - 3));
            return msg.Length > 3 && msg.EndsWith($"#{expectedCheckChar:X2}");
        }

        public bool ReadMessage(out string message, bool? useChecksum = null)
        {
            while (true)
            {
                message = Reader.ReadLine();
                Console.WriteLine($">>{message}");

                if (message == null) return false;

                message = message.TrimEnd('\n');
                message = message.Trim('\0');

                if (message.Length == 0) continue;
                if (message.StartsWith("LOG:")) continue;
                if (message.StartsWith("VM")) continue; //Visual Micro debug code.

                if (useChecksum ?? DefaultUseChecksum)
                {
                    if (!ValidateChecksum(message)) return false;
                    message = message.Substring(0, message.Length - 3);
                }

                break;
            }
            return true;
        }

        public bool SendMessage(string message, bool? useChecksum = null, bool? expectResponse = null)
        {
            if (useChecksum ?? DefaultUseChecksum) message = $"{message}#{GetCheckChar(message):X2}";
            Console.WriteLine($"<<{message}");
            Serial.WriteLine(message);

            while (true)
            {
                var response = Reader.Read();
                if (response == Ack) return  true;
                if (response == Nak)
                {
                    var logProbably = Reader.ReadLine();
                    Console.WriteLine(logProbably);
                    return false;
                }

                var restOfLogProbably = Reader.ReadLine();
                Console.WriteLine((char)response + restOfLogProbably);
            }
        }

    }
}