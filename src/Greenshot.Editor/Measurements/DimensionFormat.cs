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
using System.Globalization;

namespace Greenshot.Editor.Measurements
{
    public enum DimensionFormatMode { Automatic, DecimalPlaces, SignificantFigures }

    /// <summary>Display precision only; geometry and calibration always retain their full precision.</summary>
    public sealed class DimensionFormat : IEquatable<DimensionFormat>
    {
        public static DimensionFormat Automatic { get; } = new DimensionFormat(DimensionFormatMode.Automatic, 0);
        public DimensionFormatMode Mode { get; }
        public int Digits { get; }
        public DimensionFormat(DimensionFormatMode mode, int digits)
        {
            if (mode < DimensionFormatMode.Automatic || mode > DimensionFormatMode.SignificantFigures ||
                (mode == DimensionFormatMode.Automatic ? digits != 0 : digits < (mode == DimensionFormatMode.SignificantFigures ? 1 : 0) || digits > 10))
                throw new ArgumentOutOfRangeException(nameof(digits));
            Mode = mode; Digits = digits;
        }
        internal int Packed => (int)Mode * 16 + Digits;
        internal static DimensionFormat Unpack(int value)
        {
            if (value < 0) return null;
            try { return new DimensionFormat((DimensionFormatMode)(value / 16), value % 16); }
            catch (ArgumentOutOfRangeException) { return null; }
        }
        internal string Format(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return "Out of range";
            var culture = CultureInfo.CurrentCulture;
            if (Mode == DimensionFormatMode.Automatic)
                return value.ToString(value != 0 && (Math.Abs(value) < .00005 || Math.Abs(value) >= 1e9) ? "0.###E+0" : "0.####", culture);
            if (Mode == DimensionFormatMode.DecimalPlaces)
                return value.ToString(Math.Abs(value) >= 1e15 ? "E" + Digits : "F" + Digits, culture);
            // Round in scientific notation first, so a carry across a power of ten gets the correct exponent.
            string scientific = value.ToString("E" + (Digits - 1), CultureInfo.InvariantCulture);
            int marker = scientific.IndexOf('E');
            int exponent = int.Parse(scientific.Substring(marker + 1), CultureInfo.InvariantCulture);
            if (exponent < -4 || exponent >= 9) return value.ToString("E" + (Digits - 1), culture);
            bool negative = scientific[0] == '-';
            string digits = scientific.Substring(negative ? 1 : 0, marker - (negative ? 1 : 0)).Replace(".", "");
            int point = exponent + 1;
            string result = point <= 0 ? "0." + new string('0', -point) + digits :
                point >= digits.Length ? digits + new string('0', point - digits.Length) : digits.Insert(point, ".");
            return (negative ? culture.NumberFormat.NegativeSign : "") + result.Replace(".", culture.NumberFormat.NumberDecimalSeparator);
        }
        public bool Equals(DimensionFormat other) => other != null && Mode == other.Mode && Digits == other.Digits;
        public override bool Equals(object other) => Equals(other as DimensionFormat);
        public override int GetHashCode() => Packed;
    }
}
