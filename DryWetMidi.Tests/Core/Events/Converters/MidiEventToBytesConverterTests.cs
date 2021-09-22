﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiEventToBytesConverterTests
    {
        #region Test methods

        [Test]
        public void Write()
        {
            using (var midiEventToBytesConverter = new MidiEventToBytesConverter())
            {
                Convert(
                    midiEventToBytesConverter,
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    },
                    new byte[] { 0x92, 0x12, 0x56 });
                Convert(
                    midiEventToBytesConverter,
                    new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x0
                    },
                    new byte[] { 0x80, 0x12, 0x00 });
                Convert(
                    midiEventToBytesConverter,
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    },
                    new byte[] { 0xB3, 0x23, 0x7F });
            }
        }

        [Test]
        public void Convert_MinSize()
        {
            using (var midiEventToBytesConverter = new MidiEventToBytesConverter())
            {
                Convert(
                    midiEventToBytesConverter,
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    },
                    new byte[] { 0x92, 0x12, 0x56, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                    10);
            }
        }

        [Test]
        public void Convert_Settings()
        {
            using (var midiEventToBytesConverter = new MidiEventToBytesConverter())
            {
                midiEventToBytesConverter.WritingSettings.TextEncoding = Encoding.UTF8;

                var text = "Test▶▶▶";
                var bytes = Encoding.UTF8.GetBytes(text);
                Convert(
                    midiEventToBytesConverter,
                    new TextEvent(text),
                    new byte[] { 0xFF, 0x01 }.Concat(DataTypesUtilities.GetVlqBytes(bytes.Length)).Concat(bytes).ToArray());
            }
        }

        [Test]
        public void Convert_WriteDeltaTimes_SingleByte([Values(0, 127)] long deltaTime)
        {
            using (var midiEventToBytesConverter = new MidiEventToBytesConverter())
            {
                midiEventToBytesConverter.WriteDeltaTimes = true;

                Convert(
                    midiEventToBytesConverter,
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2,
                        DeltaTime = deltaTime
                    },
                    new byte[] { (byte)deltaTime, 0x92, 0x12, 0x56 });
            }
        }

        [Test]
        public void Convert_WriteDeltaTimes_MultipleBytes()
        {
            using (var midiEventToBytesConverter = new MidiEventToBytesConverter())
            {
                midiEventToBytesConverter.WriteDeltaTimes = true;

                Convert(
                    midiEventToBytesConverter,
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2,
                        DeltaTime = 128
                    },
                    new byte[] { 0x81, 0x00, 0x92, 0x12, 0x56 });
            }
        }

        [Test]
        public void Convert_Multiple()
        {
            using (var midiEventToBytesConverter = new MidiEventToBytesConverter())
            {
                Convert(
                    midiEventToBytesConverter,
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2,
                            DeltaTime = 120
                        },
                        new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                        {
                            Channel = (FourBitNumber)0x0
                        }
                    },
                    new byte[] { 0x92, 0x12, 0x56, 0x80, 0x12, 0x00 });
            }
        }

        [Test]
        public void Convert_Multiple_WriteDeltaTimes()
        {
            using (var midiEventToBytesConverter = new MidiEventToBytesConverter())
            {
                midiEventToBytesConverter.WriteDeltaTimes = true;

                Convert(
                    midiEventToBytesConverter,
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2,
                            DeltaTime = 120
                        },
                        new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                        {
                            Channel = (FourBitNumber)0x0,
                            DeltaTime = 128
                        }
                    },
                    new byte[] { 0x78, 0x92, 0x12, 0x56, 0x81, 0x00, 0x80, 0x12, 0x00 });
            }
        }

        [Test]
        public void Convert_Multiple_Settings()
        {
            using (var midiEventToBytesConverter = new MidiEventToBytesConverter())
            {
                midiEventToBytesConverter.WritingSettings.NoteOffAsSilentNoteOn = true;

                Convert(
                    midiEventToBytesConverter,
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2,
                            DeltaTime = 120
                        },
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                        {
                            Channel = (FourBitNumber)0x0
                        }
                    },
                    new byte[] { 0x92, 0x12, 0x56, 0x90, 0x12, 0x00 });
            }
        }

        [Test]
        public void Convert_Multiple_Settings_RunningStatus()
        {
            using (var midiEventToBytesConverter = new MidiEventToBytesConverter())
            {
                midiEventToBytesConverter.WritingSettings.NoteOffAsSilentNoteOn = true;
                midiEventToBytesConverter.WritingSettings.UseRunningStatus = true;

                Convert(
                    midiEventToBytesConverter,
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56),
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    },
                    new byte[] { 0x90, 0x12, 0x56, 0x12, 0x00 });
            }
        }

        [Test]
        public void Convert_MultipleThenOne()
        {
            using (var midiEventToBytesConverter = new MidiEventToBytesConverter())
            {
                Convert(
                    midiEventToBytesConverter,
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2,
                            DeltaTime = 120
                        },
                        new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                        {
                            Channel = (FourBitNumber)0x0,
                            DeltaTime = 128
                        }
                    },
                    new byte[] { 0x92, 0x12, 0x56, 0x80, 0x12, 0x00 });

                midiEventToBytesConverter.WritingSettings.TextEncoding = Encoding.UTF8;

                var text = "Test▶▶▶";
                var bytes = Encoding.UTF8.GetBytes(text);
                Convert(
                    midiEventToBytesConverter,
                    new TextEvent(text),
                    new byte[] { 0xFF, 0x01 }.Concat(DataTypesUtilities.GetVlqBytes(bytes.Length)).Concat(bytes).ToArray());
            }
        }

        #endregion

        #region Private methods

        private void Convert(
            MidiEventToBytesConverter midiEventToBytesConverter,
            MidiEvent midiEvent,
            byte[] expectedBytes,
            int minSize = 0)
        {
            var bytes = midiEventToBytesConverter.Convert(midiEvent, minSize);
            CollectionAssert.AreEqual(expectedBytes, bytes, $"Event [{midiEvent}] converted to bytes incorrectly.");
        }

        private void Convert(
            MidiEventToBytesConverter midiEventToBytesConverter,
            IEnumerable<MidiEvent> midiEvents,
            byte[] expectedBytes)
        {
            var bytes = midiEventToBytesConverter.Convert(midiEvents);
            CollectionAssert.AreEqual(expectedBytes, bytes, "Events converted to bytes incorrectly.");
        }

        #endregion
    }
}
