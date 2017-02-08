using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMDCubed.Materials
{
    class ZMode
    {
        /// <summary> If false, ZBuffering is disabled and the Z buffer is not updated. </summary>
        public bool Enable;

        /// <summary> Determines the comparison that is performed. The newely rasterized Z value is on the left while the value from the Z buffer is on the right. If the result of the comparison is false, the newly rasterized pixel is discarded. </summary>
        public GXCompareType Function;

        /// <summary> If true, the Z buffer is updated with the new Z value after a comparison is performed. 
        /// Example: Disabling this would prevent a write to the Z buffer, useful for UI elements or other things
        /// that shouldn't write to Z Buffer. See glDepthMask. </summary>
        public bool UpdateEnable;

        public ZMode()
        {
        }

        public ZMode(bool enabled, GXCompareType func, bool updateEnabled)
        {
            Enable = enabled;
            Function = func;
            UpdateEnable = updateEnabled;
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(Enable);
            writer.Write((byte)Function);
            writer.Write(UpdateEnable);
            // Pad entry to 4 bytes
            writer.Write((byte)0xFF);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ZMode))
                return Compare((ZMode)obj);
            else
                return false;
        }

        private bool Compare(ZMode obj)
        {
            if (Enable == obj.Enable && Function == obj.Function && UpdateEnable == obj.UpdateEnable)
                return true;
            else
                return false;
        }

        public static bool operator ==(ZMode left, ZMode right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;

            if (((object)left == null) || ((object)right == null))
                return false;

            if (left.Enable == right.Enable && left.Function == right.Function && left.UpdateEnable == right.UpdateEnable)
                return true;

            else
                return false;
        }

        public static bool operator !=(ZMode left, ZMode right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;

            if (((object)left == null) || ((object)right == null))
                return true;

            if (left.Enable == right.Enable && left.Function == right.Function && left.UpdateEnable == right.UpdateEnable)
                return false;

            else
                return true;
        }
    }
}
