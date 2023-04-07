// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.Curation.Data.DataLoad.Extensions;

/// <summary>
/// Static type extensions for Enum <see cref="LoadBubble"/>
/// </summary>
public static class LoadBubbleExtensions
{
    /// <summary>
    /// Converts a <see cref="LoadBubble"/> into a <see cref="LoadStage"/>
    /// </summary>
    /// <param name="bubble"></param>
    /// <returns></returns>
    public static LoadStage ToLoadStage(this LoadBubble bubble)
    {
        switch (bubble)
        {
            case LoadBubble.Raw:
                return LoadStage.AdjustRaw;
            case LoadBubble.Staging:
                return LoadStage.AdjustStaging;
            case LoadBubble.Live:
                return LoadStage.PostLoad;
            case LoadBubble.Archive:
                throw new Exception("LoadBubble.Archive refers to _Archive tables, therefore it cannot be translated into a LoadStage");
            default:
                throw new ArgumentOutOfRangeException("bubble");
        }
    }
}