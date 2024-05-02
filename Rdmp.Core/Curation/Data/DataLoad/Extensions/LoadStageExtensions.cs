// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.Curation.Data.DataLoad.Extensions;

/// <summary>
///     Static extensions for <see cref="LoadStage" />
/// </summary>
public static class LoadStageExtensions
{
    /// <summary>
    ///     Converts a <see cref="LoadStage" /> into a <see cref="LoadBubble" />
    /// </summary>
    /// <param name="loadStage"></param>
    /// <returns></returns>
    public static LoadBubble ToLoadBubble(this LoadStage loadStage)
    {
        return loadStage switch
        {
            LoadStage.GetFiles => LoadBubble.Raw,
            LoadStage.Mounting => LoadBubble.Raw,
            LoadStage.AdjustRaw => LoadBubble.Raw,
            LoadStage.AdjustStaging => LoadBubble.Staging,
            LoadStage.PostLoad => LoadBubble.Live,
            _ => throw new ArgumentOutOfRangeException(nameof(loadStage), $"Unknown value for LoadStage: {loadStage}")
        };
    }
}