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
using System.Drawing.Drawing2D;

namespace Greenshot.Editor.Measurements
{
    public enum MeasurementUnit { Pixels, Millimetres, Centimetres, Metres, Inches, Feet }

    /// <summary>Immutable, document-owned calibration. Drawing objects stay in image pixels.</summary>
    public sealed class MeasurementState
    {
        public static readonly MeasurementState Pixels = new MeasurementState(0, MeasurementUnit.Pixels);
        public double MetresPerPixel { get; }
        public MeasurementUnit Unit { get; }
        public bool IsCalibrated => MetresPerPixel > 0;

        public MeasurementState(double metresPerPixel, MeasurementUnit unit)
        {
            if (double.IsNaN(metresPerPixel) || double.IsInfinity(metresPerPixel) || metresPerPixel < 0)
                throw new ArgumentOutOfRangeException(nameof(metresPerPixel));
            if (!Enum.IsDefined(typeof(MeasurementUnit), unit)) throw new ArgumentOutOfRangeException(nameof(unit));
            MetresPerPixel = metresPerPixel;
            Unit = metresPerPixel == 0 ? MeasurementUnit.Pixels : unit;
        }

        public static MeasurementState Calibrate(double pixels, double distance, MeasurementUnit unit)
        {
            if (!IsPositiveFinite(pixels) || !IsPositiveFinite(distance) || unit == MeasurementUnit.Pixels)
                throw new ArgumentOutOfRangeException(nameof(distance), "Enter a positive distance and draw a nonzero line.");
            var scale = distance * MetresPerUnit(unit) / pixels;
            if (!IsPositiveFinite(scale)) throw new ArgumentOutOfRangeException(nameof(distance));
            return new MeasurementState(scale, unit);
        }

        private static bool IsPositiveFinite(double value) => value > 0 && !double.IsInfinity(value) && !double.IsNaN(value);
        public MeasurementState WithUnit(MeasurementUnit unit) => new MeasurementState(MetresPerPixel, unit);
        public double Convert(double pixels) => Unit == MeasurementUnit.Pixels ? pixels : pixels * MetresPerPixel / MetresPerUnit(Unit);
        public double ToPixels(double value) => Unit == MeasurementUnit.Pixels ? value : value * MetresPerUnit(Unit) / MetresPerPixel;

        public static double MetresPerUnit(MeasurementUnit unit) => unit switch
        {
            MeasurementUnit.Millimetres => 0.001,
            MeasurementUnit.Centimetres => 0.01,
            MeasurementUnit.Metres => 1,
            MeasurementUnit.Inches => 0.0254,
            MeasurementUnit.Feet => 0.3048,
            _ => throw new ArgumentOutOfRangeException(nameof(unit))
        };

        public static string Symbol(MeasurementUnit unit) => unit switch
        {
            MeasurementUnit.Millimetres => "mm", MeasurementUnit.Centimetres => "cm",
            MeasurementUnit.Metres => "m", MeasurementUnit.Inches => "in", MeasurementUnit.Feet => "ft", _ => "px"
        };

        public MeasurementState Transform(Matrix matrix)
        {
            if (!IsCalibrated || matrix == null) return this;
            var m = matrix.Elements;
            double sx = Math.Sqrt((double)m[0] * m[0] + (double)m[1] * m[1]);
            double sy = Math.Sqrt((double)m[2] * m[2] + (double)m[3] * m[3]);
            double dot = (double)m[0] * m[2] + (double)m[1] * m[3];
            if (!IsPositiveFinite(sx) || !IsPositiveFinite(sy) || Math.Abs(sx - sy) > 1e-5 * Math.Max(sx, sy)
                || Math.Abs(dot) > 1e-5 * sx * sy) return Pixels;
            return new MeasurementState(MetresPerPixel / sx, Unit);
        }
    }

    public interface IMeasurementSurface
    {
        MeasurementState Measurements { get; }
        void RestoreMeasurements(MeasurementState state);
    }
}
