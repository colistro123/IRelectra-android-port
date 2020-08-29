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
    class IREncoder
    {
        public IREncoder(uint unitLengthInUsec)
        {
            _unitLength = unitLengthInUsec;
            _data = new MarkSpaceArray(unitLengthInUsec); //Populate it so it's not null
        }

        public MarkSpaceArray data()
        {
            return _data;
        }

        public void addZero()
        {
            _data.addMark(_unitLength);
            _data.addSpace(_unitLength);
        }

        public void addOne()
        {
            _data.addSpace(_unitLength);
            _data.addMark(_unitLength);
        }

        public void addBit(ulong bit)
        {
            if (bit != 0)
                addOne();
            else
                addZero();
        }

        public void addNumber(ulong n, int numberOfBits)
        {
            for (int j = numberOfBits - 1; j >= 0; j--)
            {
                addBit(Convert.ToByte(((n >> j) & 1)));
            }
        }

        public MarkSpaceArray _data;
        public uint _unitLength;
    }
}