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

namespace Electra_Remote
{
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

        [StructLayout(LayoutKind.Explicit, Size = 1, Pack = 1, CharSet = CharSet.Ansi)]
        public struct ElectraCode
        {
            public ElectraCode(int dummy)
            {
                // allocate the bitfield
                num = new BitVector32(0);

                // initialize bitfield sections
                zeros1 = BitVector32.CreateSection(1);
                ones1 = BitVector32.CreateSection(1, zeros1);
                zeros2 = BitVector32.CreateSection(16, ones1);
                sleep = BitVector32.CreateSection(1, zeros2);
                temperature = BitVector32.CreateSection(4, sleep);
                zeros3 = BitVector32.CreateSection(2, temperature);
                swing = BitVector32.CreateSection(1, zeros3);
                zeros4 = BitVector32.CreateSection(2, swing);
                fan = BitVector32.CreateSection(2, zeros4);
                mode = BitVector32.CreateSection(3, fan);
                power = BitVector32.CreateSection(1, mode);
            }

            // Creates and initializes a BitVector32.
            [FieldOffset(0)]
            public BitVector32 num;
            public static BitVector32.Section zeros1;
            public static BitVector32.Section ones1;
            public static BitVector32.Section zeros2;
            public static BitVector32.Section sleep;
            public static BitVector32.Section temperature;
            public static BitVector32.Section zeros3;
            public static BitVector32.Section swing;
            public static BitVector32.Section zeros4;
            public static BitVector32.Section fan;
            public static BitVector32.Section mode;
            public static BitVector32.Section power;
            public bool Zeros1
            {
                get { return num[zeros1] != 0; }
                set { num[zeros1] = value ? 1 : 0; }
            }
            public bool Ones1
            {
                get { return num[ones1] != 0; }
                set { num[ones1] = value ? 1 : 0; }
            }
            public bool Zeros2
            {
                get { return num[zeros2] != 0; }
                set { num[zeros2] = value ? 1 : 0; }
            }
            public bool Sleep
            {
                get { return num[sleep] != 0; }
                set { num[sleep] = value ? 1 : 0; }
            }
            public int Temperature
            {
                get { return num[temperature]; }
                set { num[temperature] = value; }
            }
            public bool Zeros3
            {
                get { return num[zeros3] != 0; }
                set { num[zeros3] = value ? 1 : 0; }
            }
            public bool Swing
            {
                get { return num[swing] != 0; }
                set { num[swing] = value ? 1 : 0; }
            }
            public bool Zeros4
            {
                get { return num[zeros4] != 0; }
                set { num[zeros4] = value ? 1 : 0; }
            }
            public int Fan
            {
                get { return num[fan]; }
                set { num[fan] = value; }
            }
            public int Mode
            {
                get { return num[mode]; }
                set { num[mode] = value; }
            }
            public bool Power
            {
                get { return num[power] != 0; }
                set { num[power] = value ? 1 : 0; }
            }
        }

        public static ushort    UNIT = 992; //992
        public static ushort    HDR_UNIT = 2976;
        public static ushort    END_UNIT = 3968;
        public static byte      NUM_BITS = 34;
        public static int       ELECTRA_FREQ_HZ = 33000;
        public bool sendElectra(bool power, IRElectraMode mode, IRElectraFan fan, int temperature, bool swing, IRElectraIFeel iFeel, bool sleep, bool notify)
        {
            ulong code = encodeElectra(power, mode, fan, temperature, swing, iFeel, sleep, notify);
            List<int> data = generateSignal(code).Select(i => (int)i).ToList();
            string dataString = string.Join(",", data.ToArray());
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
                markspace.addSpace(3); //space

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

            #region oldElectraBitmasks
            /*
            var electraCode = new ElectraCode(0)
            {
                Ones1 = true,
                Sleep = sleep,
                Temperature = temperature,
                Swing = swing,
                Fan = Convert.ToInt32(fan),
                Mode = Convert.ToInt32(mode),
                Power = power
            };


            ulong code = Convert.ToUInt64(electraCode.num.Data);
            */
            #endregion
            //Notify is apparently used in conjunction with iFeel
            temperature -= notify ? 15 : 5;

            ulong num = 0;
            num |= ((Convert.ToUInt64(power) & 1) << 33);
            num |= ((Convert.ToUInt64(mode) & 7) << 30);
            num |= ((Convert.ToUInt64(fan) & 3) << 28);
            num |= ((Convert.ToUInt64(notify) & 1) << 27);
            num |= ((Convert.ToUInt64(swing) & 1) << 25);
            num |= ((Convert.ToUInt64(iFeel) & 1) << 24); //ifeel
            num |= ((Convert.ToUInt64(temperature) & 31) << 19);
            num |= ((Convert.ToUInt64(sleep) & 1) << 18);
            num |= 2;

            return num;
        }
        /*
         * API 21+, this is wrong don't use it
        */
        public int[] convertTransmission(int[] transmission)
        {
            int cycleLengthMicroSeconds = Convert.ToInt32(ELECTRA_FREQ_HZ * 0.00005);
            int[] transmissionConverted = transmission.ToArray();
            for (int i = 0; i < transmission.Count(); i++)
                transmissionConverted[i] = transmission[i] * cycleLengthMicroSeconds;

            return transmissionConverted;
        }
    }
}