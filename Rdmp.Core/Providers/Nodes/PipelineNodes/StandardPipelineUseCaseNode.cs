// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Comments;

namespace Rdmp.Core.Providers.Nodes.PipelineNodes;

/// <summary>
///     Collection of all the Pipelines compatible with a given use case.
/// </summary>
public class StandardPipelineUseCaseNode : SingletonNode, IKnowWhatIAm
{
    private readonly CommentStore _commentStore;
    public PipelineUseCase UseCase { get; set; }
    public List<Pipeline> Pipelines { get; } = new();

    public StandardPipelineUseCaseNode(string caption, PipelineUseCase useCase, CommentStore commentStore) :
        base(caption)
    {
        _commentStore = commentStore;
        UseCase = useCase;
    }

    public string WhatIsThis()
    {
        var useCaseType = UseCase.GetType();
        return
            $"Collection of all the Pipelines compatible with a given use case.  This node's use case is:{Environment.NewLine}{useCaseType.Name}{Environment.NewLine} {_commentStore.GetTypeDocumentationIfExists(useCaseType, false, true)}";
    }
}