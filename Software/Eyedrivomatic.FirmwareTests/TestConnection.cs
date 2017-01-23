﻿using System;
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
        private const bool DefaultUseResponse = true;
        private const bool DefaultUseChecksum = true;

        public SerialPort Serial { set; get; }

        public StreamReader Reader { set; get; }

        public void Initialize()
        {
            Serial = new SerialPort("COM3", 19200) { Encoding = Encoding.ASCII, ReadTimeout = 3000 };
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
                if (!ReadMessage(out message, false, false)) return false;
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
            return msg.Length > 3 && !msg.EndsWith($"#{GetCheckChar(msg.Substring(0, msg.Length - 3)):X2}");
        }

        public bool ReadMessage(out string message, bool? useChecksum = null, bool? sendResponse = null)
        {
            while (true)
            {
                message = Reader.ReadLine();
                Console.WriteLine($"<<{message}");

                if (message == null) return false;

                message = message.TrimEnd('\n');
                message = message.Trim('\0');

                if (message.Length == 0) continue;
                if (message.StartsWith("LOG:")) continue;
                if (message.StartsWith("VM")) continue; //Visual Micro debug code.

                if (useChecksum ?? DefaultUseChecksum)
                {
                    if (!ValidateChecksum(message))
                    {
                        if (sendResponse ?? DefaultUseResponse) Serial.Write(new [] {Nak}, 0, 1);
                        return false; //No mercy. No retry.
                    }
                    message = message.Substring(0, message.Length - 3);
                }

                if (sendResponse ?? DefaultUseResponse) Serial.Write(new [] {Ack}, 0, 1);
                break;
            }
            return true;
        }

        public bool SendMessage(string message, bool? useChecksum = null, bool? expectResponse = null)
        {
            if (useChecksum ?? DefaultUseChecksum) message = $"{message}#{GetCheckChar(message):X2}";
            Console.WriteLine($">>{message}");
            Serial.WriteLine(message);
            return !(expectResponse ?? DefaultUseResponse) || Reader.Read() == Ack;
        }

    }
}