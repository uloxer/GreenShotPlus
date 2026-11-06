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
using System.Drawing;
using System.IO;
using System.Text;

namespace Greenshot.Editor.Measurements
{
    /// <summary>Bounded primitive record before the unchanged final calibration trailer.</summary>
    internal static class DimensionMetadata
    {
        private const string Magic = "GSPDIM01";
        internal static void Write(Stream stream, DimensionStyle style)
        {
            using var body = new MemoryStream();
            using (var writer = new BinaryWriter(body, Encoding.UTF8, true))
            {
                byte[] family = Encoding.UTF8.GetBytes(style.Family);
                writer.Write(1); writer.Write((ushort)family.Length); writer.Write(family);
                writer.Write(style.Size); writer.Write((int)style.FontStyle); writer.Write(style.Color.ToArgb());
                // Keep the version-1 base readable by old readers, which ignore the rest of this bounded body.
                writer.Write(Encoding.ASCII.GetBytes("GSPFMT01")); writer.Write(1);
                writer.Write(style.Format.Packed); writer.Write(style.BackgroundColor.ToArgb());
            }
            body.WriteTo(stream);
            using var footer = new BinaryWriter(stream, Encoding.UTF8, true);
            footer.Write((int)body.Length); footer.Write(Encoding.ASCII.GetBytes(Magic));
        }
        internal static DimensionStyle Read(Stream stream, long start, long length)
        {
            long position = stream.Position;
            try
            {
                if (start < 0 || length < MeasurementMetadata.Size + 12 || length > stream.Length - start) return DimensionStyle.Default;
                long footer = start + length - MeasurementMetadata.Size - 12;
                stream.Position = footer;
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                int count = reader.ReadInt32();
                if (Encoding.ASCII.GetString(reader.ReadBytes(8)) != Magic || count < 18 || count > 4096 || count > footer - start) return DimensionStyle.Default;
                stream.Position = footer - count;
                using var body = new MemoryStream(reader.ReadBytes(count), false);
                using var data = new BinaryReader(body, Encoding.UTF8, true);
                if (data.ReadInt32() != 1) return DimensionStyle.Default;
                int familyLength = data.ReadUInt16();
                if (familyLength < 1 || familyLength > 512 || familyLength > body.Length - body.Position - 12) return DimensionStyle.Default;
                string family = new UTF8Encoding(false, true).GetString(data.ReadBytes(familyLength));
                var style = new DimensionStyle(family, data.ReadSingle(), (FontStyle)data.ReadInt32(), Color.FromArgb(data.ReadInt32()));
                // A malformed extension must never discard a valid base font/colour or the independent calibration trailer.
                if (body.Length - body.Position != 20) return style;
                if (Encoding.ASCII.GetString(data.ReadBytes(8)) != "GSPFMT01" || data.ReadInt32() != 1) return style;
                var format = DimensionFormat.Unpack(data.ReadInt32());
                var background = Color.FromArgb(data.ReadInt32());
                return format == null ? style : new DimensionStyle(style.Family, style.Size, style.FontStyle, style.Color, format, background);
            }
            catch (ArgumentException) { return DimensionStyle.Default; }
            catch (IOException) { return DimensionStyle.Default; }
            finally { stream.Position = position; }
        }
    }
}
