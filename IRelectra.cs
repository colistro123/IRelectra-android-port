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
            IRElectraModeAuto = 0b011,
            IRElectraModeDry = 0b100,
            IRElectraModeFan = 0b101
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

        public static ushort    UNIT = 1000; //992
        public static ushort    HDR_UNIT = 2976;
        public static ushort    END_UNIT = 3968;
        public static byte      NUM_BITS = 34;
        public static int       ELECTRA_FREQ_HZ = 38000;
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
        public bool sendElectra(bool power, IRElectraMode mode, IRElectraFan fan, int temperature, bool swing, IRElectraIFeel iFeel, bool sleep, bool notify)
        {
            // get the data representing the configuration
            ulong code = encodeElectra(power, mode, fan, temperature, swing, iFeel, sleep, notify);

            // get the raw data itself with headers, repetition, etc.
            List<int> data = generateSignal(code).Select(i => (int)i).ToList();
            string dataString = string.Join(", ", data.ToArray());
            

            string lines = string.Join(System.Environment.NewLine, dataString.Split()
    .Select((word, index) => new { word, index })
    .GroupBy(x => x.index / 6)
    .Select(grp => string.Join(" ", grp.Select(x => x.word))));

            DDebug.WriteLine(lines);

            MainActivity.mCIR.Transmit(ELECTRA_FREQ_HZ, data.ToArray());
            return true;
        }

        public List<uint> generateSignal(ulong code)
        {
            MarkSpaceArray markspace = new MarkSpaceArray(UNIT);

            // The whole packet looks this:
            //  3 Times: 
            //    3000 usec MARK
            //    3000 used SPACE
            //    Maxchester encoding of the data, clock is ~1000usec
            // 4000 usec MARK
            for (int k = 0; k < 3; k++)
            {
                markspace.addMark(3); //mark
                markspace.addSpace(3); //space 3

                markspace.addNumberWithManchesterCode(code, NUM_BITS);
            }
            markspace.addMark(4);
            return markspace.data();
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
        public ulong encodeElectra(bool power, IRElectraMode mode, IRElectraFan fan, int temperature, bool swing, IRElectraIFeel iFeel, bool sleep, bool notify)
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
            num |= 2;

            return num;
        }
    }
}