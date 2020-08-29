using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Diagnostics;
using DDebug = System.Diagnostics.Debug;
using System.IO;

namespace Electra_Remote
{
    //Originally from https://github.com/barakwei/IRelectra
    class IRelectra
    {
        public enum IRElectraMode
        {
            IRElectraModeCool = 0b001,
            IRElectraModeHeat = 0b010,
            IRElectraModeFan = 0b101,
            IRElectraModeDry = 0b100,
            IRElectraModeAuto = 0b011,
        };

        public enum IRElectraFan
        {
            IRElectraFanLow = 0b00,
            IRElectraFanMedium = 0b01,
            IRElectraFanHigh = 0b10,
            IRElectraFanAuto = 0b11
        };

        public enum IRElectraIFeel
        {
            Off = 0b0,
	        On = 0b1
        };

        [Flags]
        public enum ElectraFlags
        {
            Zero = 0,
            One = 1,
            Sleep = 18,
            Temperature = 19,
            IFeel = 24,
            Swing = 25,
            Notify = 27,
            Fan = 28
        };

        public struct ElectraProperties
        {
            public bool Sleep;
            public int Temperature;
            public bool IFeel;
            public bool Swing;
            public bool Notify;
            public int Fan;
        }

        public static uint    UNIT = 992; //992
        public static ushort    HDR_UNIT = 2976;
        public static ushort    END_UNIT = 3968;
        public static byte      NUM_BITS = 34;
        public static int       ELECTRA_FREQ_HZ = 33000;
        public ElectraProperties acProperties;

        public void setACTemp(int temp)
        {
            acProperties = getElectraProperties();
            acProperties.Temperature = temp;
        }
        public int getACTemp()
        {
            return getElectraProperties().Temperature;
        }
        public ElectraProperties getElectraProperties()
        {
            if (acProperties.Equals(default(ElectraProperties)))
            {
                acProperties = new ElectraProperties();
            }
            return acProperties;
        }
        public int codeRepetitions()
        {
            return 3;
        }

        public int modulationFrequency()
        {
            return 33;
        }

        public List<uint> packetHeader()
        {
            return new List<uint>(new uint[] { IRelectra.UNIT });
        }

        public List<uint> packetTail()
        {
            return new List<uint>(new uint[] { IRelectra.UNIT * 4 });
        }

        public List<uint> codeHeader()
        {
            return new List<uint>(new uint[] { IRelectra.UNIT * 3, IRelectra.UNIT * 3 });
        }

        public List<uint> codeTail()
        {
            return new List<uint>(new uint[] { });
        }

        public bool sendElectra(bool power, IRElectraMode mode, IRElectraFan fan, int temperature, bool swing, IRElectraIFeel iFeel, bool sleep, bool notify)
        {
            // get the data representing the configuration

            // get the raw data itself with headers, repetition, etc.
            List<int> data = fullPacket(power, mode, fan, temperature, swing, iFeel, sleep, notify).Select(i => (int)i).ToList();
            string dataString = string.Join(", ", data.ToArray());
            

            string lines = string.Join(System.Environment.NewLine, dataString.Split()
    .Select((word, index) => new { word, index })
    .GroupBy(x => x.index / 6)
    .Select(grp => string.Join(" ", grp.Select(x => x.word))));

            DDebug.WriteLine($"({data.Count}) {lines}");

            MainActivity.mCIR.Transmit(ELECTRA_FREQ_HZ, data.ToArray());
            return true;
        }

        public List<uint> fullPacket(bool power, IRElectraMode mode, IRElectraFan fan, int temperature, bool swing, IRElectraIFeel iFeel, bool sleep, bool notify)
        {
            MarkSpaceArray packet = new MarkSpaceArray(UNIT);
            MarkSpaceArray codeArr = encodeElectra(power, mode, fan, temperature, swing, iFeel, sleep, notify);
            //MarkSpaceArray markspace = new MarkSpaceArray(UNIT);
            packet.addArray(packetHeader());
            for (int k = 0; k < codeRepetitions(); k++)
            {
                packet.addArray(codeHeader());
                packet.addArray(codeArr);
                packet.addArray(codeTail());
            }
            packet.addArray(packetTail());
            return packet.data();
        }

        // Encodes specific A/C configuration to a number that describes
        // That configuration has a total of 34 bits
        //    33: Power bit, if this bit is ON, the A/C will toggle its power.
        // 32-30: Mode - Cool, heat etc.
        // 29-28: Fan - Low, medium etc.
        // 27-26: Zeros
        //    25: Swing On/Off
        // 24-23: Zeros
        // 22-19: Temperature, where 15 is 0000, 30 is 1111
        //    18: Sleep mode On/Off
        // 17- 2: Zeros
        //     1: One
        //     0: Zero
        public MarkSpaceArray encodeElectra(bool power, IRElectraMode mode, IRElectraFan fan, int temperature, bool swing, IRElectraIFeel iFeel, bool sleep, bool notify)
        {
            //Notify is apparently used in conjunction with iFeel
            temperature -= notify ? 15 : 5;

            ulong num = 0;
            num |= ((Convert.ToUInt64(power) & 1) << 33);
            num |= ((Convert.ToUInt64(mode) & 7) << 30);
            num |= ((Convert.ToUInt64(fan) & 3) << 28);
            num |= ((Convert.ToUInt64(notify) & 1) << 27);
            num |= ((Convert.ToUInt64(swing) & 1) << 25);
            num |= ((Convert.ToUInt64(iFeel) & 1) << 24); //ifeel
            num |= ((Convert.ToUInt64(temperature) & 31) << 19); //ANDing is used to prevent an overflow/invalid temps
            num |= ((Convert.ToUInt64(sleep) & 1) << 18);
            num |= 2; //fullstate flags?

            IREncoder encoder = new IREncoder(UNIT);
            encoder.addNumber(num, NUM_BITS);
            return encoder.data();
        }
    }
}