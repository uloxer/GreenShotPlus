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
using Greenshot.Editor.Measurements;
using Greenshot.Editor.Memento;

namespace Greenshot.Editor.Drawing
{
    public sealed partial class Surface
    {
        public DimensionStyle DimensionStyle { get; private set; } = DimensionStyle.Default;
        public event EventHandler DimensionStyleChanged;
        public bool SetDimensionStyle(DimensionStyle style)
        {
            if (style == null) throw new ArgumentNullException(nameof(style));
            if (DimensionStyle.Equals(style)) return false;
            MakeUndoable(new DimensionStyleMemento(this), false); RestoreDimensionStyle(style); Modified = true; return true;
        }
        public bool ApplyDimensionFormatToAll(DimensionFormat format)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));
            var owners = Elements.OfType<DrawableContainer>().Where(owner => owner.DimensionFormats?.Any(value => value >= 0) == true).ToArray();
            if (DimensionStyle.Format.Equals(format) && owners.Length == 0) return false;
            MakeUndoable(new AllDimensionFormatsMemento(this, owners), false);
            foreach (var owner in owners) owner.RestoreDimensionFormats(null);
            RestoreDimensionStyle(DimensionStyle.WithFormat(format)); Modified = true; return true;
        }
        internal void RestoreDimensionStyle(DimensionStyle style)
        {
            DimensionStyle = style ?? DimensionStyle.Default; DimensionStyleChanged?.Invoke(this, EventArgs.Empty); Invalidate();
        }
    }
}
