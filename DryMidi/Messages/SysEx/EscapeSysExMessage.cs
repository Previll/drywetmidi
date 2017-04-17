﻿using System;

namespace Melanchall.DryMidi
{
    public sealed class EscapeSysExMessage : SysExMessage
    {
        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Escape SysEx message.");

            Data = reader.ReadBytes(size);
            Completed = true;
        }

        public override string ToString()
        {
            return "Escape SysEx";
        }

        #endregion
    }
}
