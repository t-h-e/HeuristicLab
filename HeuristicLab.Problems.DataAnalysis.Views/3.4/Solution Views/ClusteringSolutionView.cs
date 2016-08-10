﻿#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2016 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * This file is part of HeuristicLab.
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * HeuristicLab is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with HeuristicLab. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using System.Windows.Forms;
using HeuristicLab.Core;
using HeuristicLab.MainForm;

namespace HeuristicLab.Problems.DataAnalysis.Views {
  [View("ClusteringSolution View")]
  [Content(typeof(ClusteringSolution), false)]
  public partial class ClusteringSolutionView : DataAnalysisSolutionView {
    public ClusteringSolutionView() {
      InitializeComponent();
    }

    public new ClusteringSolution Content {
      get { return (ClusteringSolution)base.Content; }
      set { base.Content = value; }
    }

    #region drag and drop
    protected override void itemsListView_DragEnter(object sender, DragEventArgs e) {
      validDragOperation = false;
      if (ReadOnly) return;

      var dropData = e.Data.GetData(HeuristicLab.Common.Constants.DragDropDataFormat);
      if (dropData is IClusteringProblemData) validDragOperation = true;
      else if (dropData is IClusteringProblem) validDragOperation = true;
      else if (dropData is IValueParameter) {
        var param = (IValueParameter)dropData;
        if (param.Value is ClusteringProblemData) validDragOperation = true;
      }
    }
    #endregion
  }
}
