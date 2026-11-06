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

using System.Linq;
using Greenshot.Base.Interfaces.Drawing;
using Greenshot.Editor.Drawing;
using Greenshot.Editor.Measurements;

namespace Greenshot.Editor.Memento
{
    internal sealed class DimensionBackgroundMemento : IMemento
    {
        private readonly DrawableContainer _owner;
        private readonly int[] _colors;
        private readonly bool[] _overrides;
        internal DimensionBackgroundMemento(DrawableContainer owner)
        { _owner = owner; _colors = owner.DimensionBackgrounds; _overrides = owner.DimensionBackgroundOverrides; }
        public IMemento Restore()
        {
            var inverse = new DimensionBackgroundMemento(_owner); _owner.RestoreDimensionBackgrounds(_colors, _overrides);
            if (_owner.Parent != null) _owner.Parent.Modified = true; return inverse;
        }
        public bool Merge(IMemento other) => false;
        public void Dispose() { }
    }
    internal sealed class DimensionVisibilityMemento : IMemento
    {
        private readonly DrawableContainer _owner;
        private readonly bool[] _flags;
        internal DimensionVisibilityMemento(DrawableContainer owner) { _owner = owner; _flags = owner.DimensionVisibility; }
        public IMemento Restore()
        {
            var inverse = new DimensionVisibilityMemento(_owner); _owner.RestoreDimensionVisibility(_flags);
            if (_owner.Parent != null) _owner.Parent.Modified = true; return inverse;
        }
        public bool Merge(IMemento other) => false;
        public void Dispose() { }
    }
    internal sealed class DimensionFormatMemento : IMemento
    {
        private readonly DrawableContainer _owner;
        private readonly int[] _formats;
        internal DimensionFormatMemento(DrawableContainer owner) { _owner = owner; _formats = owner.DimensionFormats; }
        public IMemento Restore()
        {
            var inverse = new DimensionFormatMemento(_owner); _owner.RestoreDimensionFormats(_formats);
            if (_owner.Parent != null) _owner.Parent.Modified = true; return inverse;
        }
        public bool Merge(IMemento other) => false;
        public void Dispose() { }
    }
    internal sealed class AllDimensionFormatsMemento : IMemento
    {
        private readonly Surface _surface;
        private readonly DimensionStyle _style;
        private readonly DrawableContainer[] _owners;
        private readonly int[][] _formats;
        internal AllDimensionFormatsMemento(Surface surface, DrawableContainer[] owners)
        { _surface = surface; _style = surface.DimensionStyle; _owners = owners; _formats = owners.Select(owner => owner.DimensionFormats).ToArray(); }
        public IMemento Restore()
        {
            var inverse = new AllDimensionFormatsMemento(_surface, _owners);
            for (int i = 0; i < _owners.Length; i++) _owners[i].RestoreDimensionFormats(_formats[i]);
            _surface.RestoreDimensionStyle(_style); _surface.Modified = true; return inverse;
        }
        public bool Merge(IMemento other) => false;
        public void Dispose() { }
    }
    internal sealed class DimensionStyleMemento : IMemento
    {
        private readonly Surface _surface;
        private readonly DimensionStyle _style;
        internal DimensionStyleMemento(Surface surface) { _surface = surface; _style = surface.DimensionStyle; }
        public IMemento Restore()
        { var inverse = new DimensionStyleMemento(_surface); _surface.RestoreDimensionStyle(_style); _surface.Modified = true; return inverse; }
        public bool Merge(IMemento other) => false;
        public void Dispose() { }
    }
}
