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
using Greenshot.Base.Interfaces;
using Greenshot.Base.Interfaces.Drawing;
using Greenshot.Editor.Drawing;

namespace Greenshot.Editor.Memento
{
    internal sealed class GroupMembershipMemento : IMemento
    {
        private readonly Surface _surface;
        private readonly DrawableContainer[] _members;
        private readonly Guid[] _ids;
        private readonly double[] _angles;
        internal GroupMembershipMemento(Surface surface, DrawableContainer[] members)
        {
            _surface = surface; _members = members; _ids = members.Select(m => m.GroupId).ToArray();
            _angles = members.Select(m => m.GroupReferenceAngle).ToArray();
        }
        public IMemento Restore()
        {
            var inverse = new GroupMembershipMemento(_surface, _members);
            for (int i = 0; i < _members.Length; i++) { _members[i].GroupId = _ids[i]; _members[i].GroupReferenceAngle = _angles[i]; }
            _surface.RefreshGroupSelection(_members); _surface.Modified = true; return inverse;
        }
        public bool Merge(IMemento other) => false;
        public void Dispose() { }
    }
}
