/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2026 Thomas Braun, Jens Klingen, Robin Krom
 *
 * For more information see: https://getgreenshot.org/
 * The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Text;

namespace Greenshot.Editor.Measurements
{
    /// <summary>Optional fixed trailer inside the legacy element payload. No new serialized object types.</summary>
    internal static class MeasurementMetadata
    {
        private const string Magic = "GSPMEAS1";
        internal const int Size = 24;

        internal static void Write(Stream stream, MeasurementState state, bool force = false)
        {
            if (state == null || !state.IsCalibrated && !force) return;
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.Write(Encoding.ASCII.GetBytes(Magic));
            writer.Write(1);
            writer.Write(state.MetresPerPixel);
            writer.Write((int)state.Unit);
        }

        internal static MeasurementState Read(Stream stream, long payloadStart, long payloadLength)
        {
            long position = stream.Position;
            try
            {
                if (payloadLength < Size || payloadStart < 0 || payloadLength > stream.Length - payloadStart)
                    return MeasurementState.Pixels;
                stream.Position = payloadStart + payloadLength - Size;
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                if (Encoding.ASCII.GetString(reader.ReadBytes(8)) != Magic || reader.ReadInt32() != 1)
                    return MeasurementState.Pixels;
                double scale = reader.ReadDouble();
                var unit = (MeasurementUnit)reader.ReadInt32();
                return new MeasurementState(scale, unit);
            }
            catch (ArgumentException) { return MeasurementState.Pixels; }
            catch (IOException) { return MeasurementState.Pixels; }
            finally { stream.Position = position; }
        }
    }
}
