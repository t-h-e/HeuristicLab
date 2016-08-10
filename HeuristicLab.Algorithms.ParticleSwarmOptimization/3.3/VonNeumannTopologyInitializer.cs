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

using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Data;

namespace HeuristicLab.Algorithms.ParticleSwarmOptimization {
  [Item("Von Neumann Topology Initializer", "Every particle is connected with the two following and the two previous particles wrapping around at the beginning and the end of the population.")]
  [StorableClass]
  public sealed class VonNeumannTopologyInitializer : TopologyInitializer {

    #region Construction & Cloning

    [StorableConstructor]
    private VonNeumannTopologyInitializer(bool deserializing) : base(deserializing) { }
    private VonNeumannTopologyInitializer(VonNeumannTopologyInitializer original, Cloner cloner) : base(original, cloner) { }
    public VonNeumannTopologyInitializer() : base() { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new VonNeumannTopologyInitializer(this, cloner);
    }

    #endregion

    public override IOperation Apply() {
      ItemArray<IntArray> neighbors = new ItemArray<IntArray>(SwarmSize);
      for (int i = 0; i < SwarmSize; i++) {
        neighbors[i] = new IntArray(new[] {
          (SwarmSize + i-2) % SwarmSize,
          (SwarmSize + i-1) % SwarmSize,
          (i+1) % SwarmSize,
          (i+2) % SwarmSize
        });
      }
      Neighbors = neighbors;
      return base.Apply();
    }
  }
}
