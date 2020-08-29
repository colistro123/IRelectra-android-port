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
    //Originally from https://github.com/barakwei/IRelectra
    class MarkSpaceArray
    {
        // Class to create MARK-SPACE array. An IR code is a digital signal, which
        // means it's made out of 0's (space) and 1's (mark). This class helps
        // create these kinds of signals. It has the ability to add marks and spaces
        // at any time, and to add single bit using Manchester code to the signal.
        // Once you added enough data to the array use the data() methods to get 
        // the raw data. Make sure that the first thing you add to the array is
        // at least one mark.

        uint _unitLength = 0;

        // Array containing timing for marks and spaces, starts with marks.
        List<uint> _data = new List<uint>();

        // Initialize the array with a specific unit length. This is the clock used
        // in the Manchester code.
        public MarkSpaceArray(uint unitLengthInUsec)
        {
            _unitLength = unitLengthInUsec;
        }

        public void addArray(List<uint> array)
        {
            List<uint> data = array;
            int i = 0;

            if (data.Count() > 0 && data[0] == 0)
            {
                ++i;
            }
            for (; i < data.Count(); ++i)
            {
                if (i % 2 == 0)
                {
                    addMark(data[i]);
                }
                else
                {
                    addSpace(data[i]);
                }
            }
        }

        public void addArray(MarkSpaceArray array)
        {
            List<uint> data = array.data();
            int i = 0;

            if (data.Count() > 0 && data[0] == 0)
            {
                ++i;
            }
            for (; i < data.Count(); ++i)
            {
                if (i % 2 == 0)
                {
                    addMark(data[i]);
                }
                else
                {
                    addSpace(data[i]);
                }
            }
        }

        // Add a number of time units with mark.
        public void addMark(uint units)
        {
            if (Convert.ToBoolean(currentState()))
            {
                addTimeToCurrentState(units);
            }
            else
            {
                addTimeToNextState(units);
            }
        }

        // Add a number of time units with space
        public void addSpace(uint units)
        {
            if (!Convert.ToBoolean(currentState()))
            {
                addTimeToCurrentState(units);
            }
            else
            {
                addTimeToNextState(units);
            }
        }

        // Encodes the bit with IEEE 802.3 Manchester coding and adds it to the array
        // A zero bit is one unit MARK and one unit SPACE
        // a one bit is one unit SPACE and one unit MARK
        public void addBitWithManchesterCode(byte bit)
        {
            if (currentState() == Convert.ToByte((bit & 1) != 0))
            {
                addTimeToNextState(1);
            }
            else
            {
                addTimeToCurrentState(1);
            }
            addTimeToNextState(1);
        }

        // Encodes a given number of bits from the given number bit by bit with 
        // IEEE 802.3 Manchester coding and adds it to the array. MSB first.
        public void addNumberWithManchesterCode(long code, byte numberOfBits)
        {
            for (int j = numberOfBits - 1; j >= 0; j--)
            {
                addBitWithManchesterCode(Convert.ToByte(((code >> j) & 1)));
                //var num = this.data();
            }
        }

        // Add more time units to the current state. For example, if the array
        // looks like this: { 1*UNIT, 1*UNIT } (equal to calling addMark(1)
        // followed by addSpace(1), the current state is SPACE (currentState()==0)
        // calling this function will change the array to { 1*UNIT, 2*UNIT }.
        public void addTimeToCurrentState(uint units)
        {
            //_data[_data.Count - 1] += _unitLength * units;
            if (_data.Count() == 0)
            {
                _data.Add(0);
                _data.Add(units);
            }
            else
            {
                _data[_data.Count() - 1] += units;
            }
        }

        // Add more time to the other state. For example, if the array
        // looks like this: { 1*UNIT, 1*UNIT } (equal to calling addMark(1)
        // followed by addSpace(1), the current state is SPACE (currentState()==0)
        // calling this function will change the array to { 1*UNIT, 1*UNIT, 1*UNIT }
        public void addTimeToNextState(uint units)
        {
            _data.Add(units);
        }

        //Returns data containing marks and spaces
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