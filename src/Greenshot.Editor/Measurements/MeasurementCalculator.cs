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
using System.Linq;
using Greenshot.Base.Interfaces.Drawing;
using Greenshot.Editor.Drawing;

namespace Greenshot.Editor.Measurements
{
    public static class MeasurementCalculator
    {
        public static double LineLength(double width, double height) => Math.Sqrt(width * width + height * height);

        public static (string Label, double Pixels)[] GetDimensions(IDrawableContainer container)
        {
            if (container is DrawableContainer transformed && transformed.HasObjectTransform)
                return MeasurementGeometry.GetProperties(transformed).Where(p => p.Key == "L1" || p.Key == "L2" || p.Key == "D1" || p.Key == "D2").Select(p => (p.Key, p.Pixels)).ToArray();
            // Exact types exclude crop tools and other specialized descendants.
            var type = container?.GetType();
            if (type == typeof(LineContainer) || type == typeof(ArrowContainer))
                return new[] { ("L1", LineLength(container.Width, container.Height)) };
            if (type == typeof(RectangleContainer))
                return new[] { ("L1", Math.Abs((double)container.Width)), ("L2", Math.Abs((double)container.Height)) };
            if (type == typeof(EllipseContainer))
                return new[] { ("D1", Math.Abs((double)container.Width)), ("D2", Math.Abs((double)container.Height)) };
            return Array.Empty<(string, double)>();
        }
    }
}
