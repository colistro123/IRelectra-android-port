using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Electra_Remote
{
    class MarkSpaceArray
    {
        ushort _unitLength = 0;
        List<uint> _data = new List<uint>();

        public MarkSpaceArray(ushort unitLengthInUsec)
        {
            _unitLength = unitLengthInUsec;
        }
        public void addMark(ushort units)
        {
            if (Convert.ToBoolean(currentState()))
            {
                addUnitsToCurrentState(units);
            }
            else
            {
                addUnitsToNextState(units);
            }
        }

        public void addSpace(ushort units)
        {
            if (!Convert.ToBoolean(currentState()) != false)
            {
                addUnitsToCurrentState(units);
            }
            else
            {
                addUnitsToNextState(units);
            }
        }

        public void addBitWithManchesterCode(byte bit)
        {
            if (currentState() == Convert.ToByte((bit & 1) != 0))
            {
                addUnitsToNextState(1);
            }
            else
            {
                addUnitsToCurrentState(1);
            }
            addUnitsToNextState(1);
        }
        public void addNumberWithManchesterCode(ulong code, byte numberOfBits)
        {
            for (int j = numberOfBits - 1; j >= 0; j--)
            {
                addBitWithManchesterCode(Convert.ToByte(((code >> j) & 1) != 0));
            }
        }
        public void addUnitsToCurrentState(ushort units)
        {
            _data[_data.Count - 1] += Convert.ToUInt32(_unitLength * units);
        }

        public void addUnitsToNextState(ushort units)
        {
            _data.Add(Convert.ToUInt32(_unitLength * units));
        }

        public List<uint> data()
        {
            return _data;
        }

        //Returns 1 for mark, 0 for state
        public byte currentState()
        {
            return Convert.ToByte(_data.Count % 2);
        }

    }
}