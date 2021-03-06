#region License Information
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

using System;
using System.Linq;
using HeuristicLab.Analysis;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Optimization.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;

namespace HeuristicLab.Algorithms.EvolutionStrategy {
  [Item("Evolution Strategy (ES)", "An evolution strategy.")]
  [Creatable(CreatableAttribute.Categories.PopulationBasedAlgorithms, Priority = 200)]
  [StorableClass]
  public sealed class EvolutionStrategy : HeuristicOptimizationEngineAlgorithm, IStorableContent {
    public string Filename { get; set; }

    #region Problem Properties
    public override Type ProblemType {
      get { return typeof(ISingleObjectiveHeuristicOptimizationProblem); }
    }
    public new ISingleObjectiveHeuristicOptimizationProblem Problem {
      get { return (ISingleObjectiveHeuristicOptimizationProblem)base.Problem; }
      set { base.Problem = value; }
    }
    #endregion

    #region Parameter Properties
    private ValueParameter<IntValue> SeedParameter {
      get { return (ValueParameter<IntValue>)Parameters["Seed"]; }
    }
    private ValueParameter<BoolValue> SetSeedRandomlyParameter {
      get { return (ValueParameter<BoolValue>)Parameters["SetSeedRandomly"]; }
    }
    private ValueParameter<IntValue> PopulationSizeParameter {
      get { return (ValueParameter<IntValue>)Parameters["PopulationSize"]; }
    }
    private ValueParameter<IntValue> ParentsPerChildParameter {
      get { return (ValueParameter<IntValue>)Parameters["ParentsPerChild"]; }
    }
    private ValueParameter<IntValue> ChildrenParameter {
      get { return (ValueParameter<IntValue>)Parameters["Children"]; }
    }
    private ValueParameter<IntValue> MaximumGenerationsParameter {
      get { return (ValueParameter<IntValue>)Parameters["MaximumGenerations"]; }
    }
    private ValueParameter<BoolValue> PlusSelectionParameter {
      get { return (ValueParameter<BoolValue>)Parameters["PlusSelection"]; }
    }
    private IFixedValueParameter<BoolValue> ReevaluateElitesParameter {
      get { return (IFixedValueParameter<BoolValue>)Parameters["ReevaluateElites"]; }
    }
    public IConstrainedValueParameter<IManipulator> MutatorParameter {
      get { return (IConstrainedValueParameter<IManipulator>)Parameters["Mutator"]; }
    }
    public IConstrainedValueParameter<ICrossover> RecombinatorParameter {
      get { return (IConstrainedValueParameter<ICrossover>)Parameters["Recombinator"]; }
    }
    private ValueParameter<MultiAnalyzer> AnalyzerParameter {
      get { return (ValueParameter<MultiAnalyzer>)Parameters["Analyzer"]; }
    }
    public IConstrainedValueParameter<IStrategyParameterCreator> StrategyParameterCreatorParameter {
      get { return (IConstrainedValueParameter<IStrategyParameterCreator>)Parameters["StrategyParameterCreator"]; }
    }
    public IConstrainedValueParameter<IStrategyParameterCrossover> StrategyParameterCrossoverParameter {
      get { return (IConstrainedValueParameter<IStrategyParameterCrossover>)Parameters["StrategyParameterCrossover"]; }
    }
    public IConstrainedValueParameter<IStrategyParameterManipulator> StrategyParameterManipulatorParameter {
      get { return (IConstrainedValueParameter<IStrategyParameterManipulator>)Parameters["StrategyParameterManipulator"]; }
    }
    #endregion

    #region Properties
    public IntValue Seed {
      get { return SeedParameter.Value; }
      set { SeedParameter.Value = value; }
    }
    public BoolValue SetSeedRandomly {
      get { return SetSeedRandomlyParameter.Value; }
      set { SetSeedRandomlyParameter.Value = value; }
    }
    public IntValue PopulationSize {
      get { return PopulationSizeParameter.Value; }
      set { PopulationSizeParameter.Value = value; }
    }
    public IntValue ParentsPerChild {
      get { return ParentsPerChildParameter.Value; }
      set { ParentsPerChildParameter.Value = value; }
    }
    public IntValue Children {
      get { return ChildrenParameter.Value; }
      set { ChildrenParameter.Value = value; }
    }
    public IntValue MaximumGenerations {
      get { return MaximumGenerationsParameter.Value; }
      set { MaximumGenerationsParameter.Value = value; }
    }
    public BoolValue PlusSelection {
      get { return PlusSelectionParameter.Value; }
      set { PlusSelectionParameter.Value = value; }
    }
    public bool ReevaluteElites {
      get { return ReevaluateElitesParameter.Value.Value; }
      set { ReevaluateElitesParameter.Value.Value = value; }
    }
    public IManipulator Mutator {
      get { return MutatorParameter.Value; }
      set { MutatorParameter.Value = value; }
    }
    public ICrossover Recombinator {
      get { return RecombinatorParameter.Value; }
      set { RecombinatorParameter.Value = value; }
    }
    public MultiAnalyzer Analyzer {
      get { return AnalyzerParameter.Value; }
      set { AnalyzerParameter.Value = value; }
    }
    public IStrategyParameterCreator StrategyParameterCreator {
      get { return StrategyParameterCreatorParameter.Value; }
      set { StrategyParameterCreatorParameter.Value = value; }
    }
    public IStrategyParameterCrossover StrategyParameterCrossover {
      get { return StrategyParameterCrossoverParameter.Value; }
      set { StrategyParameterCrossoverParameter.Value = value; }
    }
    public IStrategyParameterManipulator StrategyParameterManipulator {
      get { return StrategyParameterManipulatorParameter.Value; }
      set { StrategyParameterManipulatorParameter.Value = value; }
    }

    private RandomCreator RandomCreator {
      get { return (RandomCreator)OperatorGraph.InitialOperator; }
    }
    private SolutionsCreator SolutionsCreator {
      get { return (SolutionsCreator)RandomCreator.Successor; }
    }
    private EvolutionStrategyMainLoop MainLoop {
      get { return FindMainLoop(SolutionsCreator.Successor); }
    }
    [Storable]
    private BestAverageWorstQualityAnalyzer qualityAnalyzer;
    #endregion

    public EvolutionStrategy()
      : base() {
      Parameters.Add(new ValueParameter<IntValue>("Seed", "The random seed used to initialize the new pseudo random number generator.", new IntValue(0)));
      Parameters.Add(new ValueParameter<BoolValue>("SetSeedRandomly", "True if the random seed should be set to a random value, otherwise false.", new BoolValue(true)));
      Parameters.Add(new ValueParameter<IntValue>("PopulationSize", "µ (mu) - the size of the population.", new IntValue(20)));
      Parameters.Add(new ValueParameter<IntValue>("ParentsPerChild", "ρ (rho) - how many parents should be recombined.", new IntValue(1)));
      Parameters.Add(new ValueParameter<IntValue>("Children", "λ (lambda) - the size of the offspring population.", new IntValue(100)));
      Parameters.Add(new ValueParameter<IntValue>("MaximumGenerations", "The maximum number of generations which should be processed.", new IntValue(1000)));
      Parameters.Add(new ValueParameter<BoolValue>("PlusSelection", "True for plus selection (elitist population), false for comma selection (non-elitist population).", new BoolValue(true)));
      Parameters.Add(new FixedValueParameter<BoolValue>("ReevaluateElites", "Flag to determine if elite individuals should be reevaluated (i.e., if stochastic fitness functions are used.)", new BoolValue(false)) { Hidden = true });
      Parameters.Add(new OptionalConstrainedValueParameter<ICrossover>("Recombinator", "The operator used to cross solutions."));
      Parameters.Add(new ConstrainedValueParameter<IManipulator>("Mutator", "The operator used to mutate solutions."));
      Parameters.Add(new OptionalConstrainedValueParameter<IStrategyParameterCreator>("StrategyParameterCreator", "The operator that creates the strategy parameters."));
      Parameters.Add(new OptionalConstrainedValueParameter<IStrategyParameterCrossover>("StrategyParameterCrossover", "The operator that recombines the strategy parameters."));
      Parameters.Add(new OptionalConstrainedValueParameter<IStrategyParameterManipulator>("StrategyParameterManipulator", "The operator that manipulates the strategy parameters."));
      Parameters.Add(new ValueParameter<MultiAnalyzer>("Analyzer", "The operator used to analyze each generation.", new MultiAnalyzer()));

      RandomCreator randomCreator = new RandomCreator();
      SolutionsCreator solutionsCreator = new SolutionsCreator();
      SubScopesCounter subScopesCounter = new SubScopesCounter();
      UniformSubScopesProcessor strategyVectorProcessor = new UniformSubScopesProcessor();
      Placeholder strategyVectorCreator = new Placeholder();
      ResultsCollector resultsCollector = new ResultsCollector();
      EvolutionStrategyMainLoop mainLoop = new EvolutionStrategyMainLoop();
      OperatorGraph.InitialOperator = randomCreator;

      randomCreator.RandomParameter.ActualName = "Random";
      randomCreator.SeedParameter.ActualName = SeedParameter.Name;
      randomCreator.SeedParameter.Value = null;
      randomCreator.SetSeedRandomlyParameter.ActualName = SetSeedRandomlyParameter.Name;
      randomCreator.SetSeedRandomlyParameter.Value = null;
      randomCreator.Successor = solutionsCreator;

      solutionsCreator.NumberOfSolutionsParameter.ActualName = PopulationSizeParameter.Name;
      solutionsCreator.Successor = subScopesCounter;

      subScopesCounter.Name = "Initialize EvaluatedSolutions";
      subScopesCounter.ValueParameter.ActualName = "EvaluatedSolutions";
      subScopesCounter.Successor = strategyVectorProcessor;

      strategyVectorProcessor.Operator = strategyVectorCreator;
      strategyVectorProcessor.Successor = resultsCollector;

      strategyVectorCreator.OperatorParameter.ActualName = "StrategyParameterCreator";

      resultsCollector.CollectedValues.Add(new LookupParameter<IntValue>("Evaluated Solutions", null, "EvaluatedSolutions"));
      resultsCollector.ResultsParameter.ActualName = "Results";
      resultsCollector.Successor = mainLoop;

      mainLoop.RandomParameter.ActualName = RandomCreator.RandomParameter.ActualName;
      mainLoop.PopulationSizeParameter.ActualName = PopulationSizeParameter.Name;
      mainLoop.ParentsPerChildParameter.ActualName = ParentsPerChildParameter.Name;
      mainLoop.ChildrenParameter.ActualName = ChildrenParameter.Name;
      mainLoop.MaximumGenerationsParameter.ActualName = MaximumGenerationsParameter.Name;
      mainLoop.PlusSelectionParameter.ActualName = PlusSelectionParameter.Name;
      mainLoop.ReevaluateElitesParameter.ActualName = ReevaluateElitesParameter.Name;
      mainLoop.MutatorParameter.ActualName = MutatorParameter.Name;
      mainLoop.RecombinatorParameter.ActualName = RecombinatorParameter.Name;
      mainLoop.AnalyzerParameter.ActualName = AnalyzerParameter.Name;
      mainLoop.ResultsParameter.ActualName = "Results";
      mainLoop.EvaluatedSolutionsParameter.ActualName = "EvaluatedSolutions";

      qualityAnalyzer = new BestAverageWorstQualityAnalyzer();
      ParameterizeAnalyzers();
      UpdateAnalyzers();

      Initialize();
    }
    [StorableConstructor]
    private EvolutionStrategy(bool deserializing) : base(deserializing) { }
    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      // BackwardsCompatibility3.3
      #region Backwards compatible code, remove with 3.4
      if (!Parameters.ContainsKey("ReevaluateElites")) {
        Parameters.Add(new FixedValueParameter<BoolValue>("ReevaluateElites", "Flag to determine if elite individuals should be reevaluated (i.e., if stochastic fitness functions are used.)", (BoolValue)new BoolValue(false).AsReadOnly()) { Hidden = true });
      }
      #endregion

      Initialize();
    }

    private EvolutionStrategy(EvolutionStrategy original, Cloner cloner)
      : base(original, cloner) {
      qualityAnalyzer = cloner.Clone(original.qualityAnalyzer);
      Initialize();
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new EvolutionStrategy(this, cloner);
    }

    public override void Prepare() {
      if (Problem != null) base.Prepare();
    }

    #region Events
    protected override void OnProblemChanged() {
      ParameterizeStochasticOperator(Problem.SolutionCreator);
      ParameterizeStochasticOperator(Problem.Evaluator);
      foreach (IOperator op in Problem.Operators.OfType<IOperator>()) ParameterizeStochasticOperator(op);
      ParameterizeSolutionsCreator();
      ParameterizeMainLoop();
      ParameterizeAnalyzers();
      ParameterizeIterationBasedOperators();
      UpdateRecombinators();
      UpdateMutators();
      UpdateAnalyzers();
      Problem.Evaluator.QualityParameter.ActualNameChanged += new EventHandler(Evaluator_QualityParameter_ActualNameChanged);
      base.OnProblemChanged();
    }
    protected override void Problem_SolutionCreatorChanged(object sender, EventArgs e) {
      ParameterizeStochasticOperator(Problem.SolutionCreator);
      ParameterizeSolutionsCreator();
      base.Problem_SolutionCreatorChanged(sender, e);
    }
    protected override void Problem_EvaluatorChanged(object sender, EventArgs e) {
      ParameterizeStochasticOperator(Problem.Evaluator);
      ParameterizeSolutionsCreator();
      ParameterizeMainLoop();
      ParameterizeAnalyzers();
      Problem.Evaluator.QualityParameter.ActualNameChanged += new EventHandler(Evaluator_QualityParameter_ActualNameChanged);
      base.Problem_EvaluatorChanged(sender, e);
    }
    protected override void Problem_OperatorsChanged(object sender, EventArgs e) {
      foreach (IOperator op in Problem.Operators.OfType<IOperator>()) ParameterizeStochasticOperator(op);
      ParameterizeIterationBasedOperators();
      UpdateRecombinators();
      UpdateMutators();
      UpdateAnalyzers();
      base.Problem_OperatorsChanged(sender, e);
    }
    private void Evaluator_QualityParameter_ActualNameChanged(object sender, EventArgs e) {
      ParameterizeMainLoop();
      ParameterizeAnalyzers();
    }
    private void PopulationSizeParameter_ValueChanged(object sender, EventArgs e) {
      PopulationSize.ValueChanged += new EventHandler(PopulationSize_ValueChanged);
      PopulationSize_ValueChanged(null, EventArgs.Empty);
    }
    private void PopulationSize_ValueChanged(object sender, EventArgs e) {
      if (PopulationSize.Value <= 0) PopulationSize.Value = 1;
      if (!PlusSelection.Value && Children.Value < PopulationSize.Value)
        Children.Value = PopulationSize.Value;
      if (PopulationSize.Value < ParentsPerChild.Value)
        ParentsPerChild.Value = PopulationSize.Value;
    }
    private void ParentsPerChildParameter_ValueChanged(object sender, EventArgs e) {
      ParentsPerChild.ValueChanged += new EventHandler(ParentsPerChild_ValueChanged);
      ParentsPerChild_ValueChanged(null, EventArgs.Empty);
    }
    private void ParentsPerChild_ValueChanged(object sender, EventArgs e) {
      if (ParentsPerChild.Value < 1 || ParentsPerChild.Value > 1 && RecombinatorParameter.ValidValues.Count == 0)
        ParentsPerChild.Value = 1;
      if (ParentsPerChild.Value > 1 && Recombinator == null) Recombinator = RecombinatorParameter.ValidValues.First();
      if (ParentsPerChild.Value > 1 && ParentsPerChild.Value > PopulationSize.Value)
        PopulationSize.Value = ParentsPerChild.Value;
    }
    private void ChildrenParameter_ValueChanged(object sender, EventArgs e) {
      Children.ValueChanged += new EventHandler(Children_ValueChanged);
      Children_ValueChanged(null, EventArgs.Empty);
    }
    private void Children_ValueChanged(object sender, EventArgs e) {
      if (Children.Value <= 0) Children.Value = 1;
      if (!PlusSelection.Value && Children.Value < PopulationSize.Value)
        PopulationSize.Value = Children.Value;
    }
    private void PlusSelectionParameter_ValueChanged(object sender, EventArgs e) {
      PlusSelection.ValueChanged += new EventHandler(PlusSelection_ValueChanged);
      PlusSelection_ValueChanged(null, EventArgs.Empty);
    }
    private void PlusSelection_ValueChanged(object sender, EventArgs e) {
      if (!PlusSelection.Value && Children.Value < PopulationSize.Value)
        Children.Value = PopulationSize.Value;
    }
    private void RecombinatorParameter_ValueChanged(object sender, EventArgs e) {
      if (Recombinator == null && ParentsPerChild.Value > 1) ParentsPerChild.Value = 1;
      else if (Recombinator != null && ParentsPerChild.Value == 1) ParentsPerChild.Value = 2;
      if (Recombinator != null && Mutator is ISelfAdaptiveManipulator && StrategyParameterCrossover == null) {
        if (StrategyParameterCrossoverParameter.ValidValues.Count > 0)
          StrategyParameterCrossover = StrategyParameterCrossoverParameter.ValidValues.First();
      }
    }
    private void MutatorParameter_ValueChanged(object sender, EventArgs e) {
      if (Mutator is ISelfAdaptiveManipulator) {
        UpdateStrategyParameterOperators();
      } else {
        StrategyParameterCreatorParameter.ValidValues.Clear();
        StrategyParameterCrossoverParameter.ValidValues.Clear();
        StrategyParameterManipulatorParameter.ValidValues.Clear();
        UpdateRecombinators();
      }
    }
    private void StrategyParameterCreatorParameter_ValueChanged(object sender, EventArgs e) {
      if (Mutator is ISelfAdaptiveManipulator && StrategyParameterCreator == null && StrategyParameterCreatorParameter.ValidValues.Count > 0)
        StrategyParameterCreator = StrategyParameterCreatorParameter.ValidValues.First();
    }
    private void StrategyParameterCrossoverParameter_ValueChanged(object sender, EventArgs e) {
      if (Mutator is ISelfAdaptiveManipulator && Recombinator != null && StrategyParameterCrossover == null && StrategyParameterCrossoverParameter.ValidValues.Count > 0)
        StrategyParameterCrossover = StrategyParameterCrossoverParameter.ValidValues.First();
    }
    #endregion

    #region Helpers
    private void Initialize() {
      PopulationSizeParameter.ValueChanged += new EventHandler(PopulationSizeParameter_ValueChanged);
      PopulationSize.ValueChanged += new EventHandler(PopulationSize_ValueChanged);
      ParentsPerChildParameter.ValueChanged += new EventHandler(ParentsPerChildParameter_ValueChanged);
      ParentsPerChild.ValueChanged += new EventHandler(ParentsPerChild_ValueChanged);
      ChildrenParameter.ValueChanged += new EventHandler(ChildrenParameter_ValueChanged);
      Children.ValueChanged += new EventHandler(Children_ValueChanged);
      PlusSelectionParameter.ValueChanged += new EventHandler(PlusSelectionParameter_ValueChanged);
      PlusSelection.ValueChanged += new EventHandler(PlusSelection_ValueChanged);
      RecombinatorParameter.ValueChanged += new EventHandler(RecombinatorParameter_ValueChanged);
      MutatorParameter.ValueChanged += new EventHandler(MutatorParameter_ValueChanged);
      StrategyParameterCrossoverParameter.ValueChanged += new EventHandler(StrategyParameterCrossoverParameter_ValueChanged);
      StrategyParameterCreatorParameter.ValueChanged += new EventHandler(StrategyParameterCreatorParameter_ValueChanged);
      if (Problem != null)
        Problem.Evaluator.QualityParameter.ActualNameChanged += new EventHandler(Evaluator_QualityParameter_ActualNameChanged);
    }
    private void ParameterizeSolutionsCreator() {
      SolutionsCreator.EvaluatorParameter.ActualName = Problem.EvaluatorParameter.Name;
      SolutionsCreator.EvaluatorParameter.Hidden = true;
      SolutionsCreator.SolutionCreatorParameter.ActualName = Problem.SolutionCreatorParameter.Name;
      SolutionsCreator.SolutionCreatorParameter.Hidden = true;
    }
    private void ParameterizeMainLoop() {
      MainLoop.BestKnownQualityParameter.ActualName = Problem.BestKnownQualityParameter.Name;
      MainLoop.EvaluatorParameter.ActualName = Problem.EvaluatorParameter.Name;
      MainLoop.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
      MainLoop.QualityParameter.ActualName = Problem.Evaluator.QualityParameter.ActualName;
    }
    private void ParameterizeStochasticOperator(IOperator op) {
      if (op is IStochasticOperator) {
        IStochasticOperator stOp = (IStochasticOperator)op;
        stOp.RandomParameter.ActualName = RandomCreator.RandomParameter.ActualName;
        stOp.RandomParameter.Hidden = true;
      }
    }
    private void ParameterizeAnalyzers() {
      qualityAnalyzer.ResultsParameter.ActualName = "Results";
      qualityAnalyzer.ResultsParameter.Hidden = true;
      if (Problem != null) {
        qualityAnalyzer.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
        qualityAnalyzer.MaximizationParameter.Hidden = true;
        qualityAnalyzer.QualityParameter.ActualName = Problem.Evaluator.QualityParameter.ActualName;
        qualityAnalyzer.QualityParameter.Depth = 1;
        qualityAnalyzer.QualityParameter.Hidden = true;
        qualityAnalyzer.BestKnownQualityParameter.ActualName = Problem.BestKnownQualityParameter.Name;
        qualityAnalyzer.BestKnownQualityParameter.Hidden = true;
      } else {
        qualityAnalyzer.MaximizationParameter.Hidden = false;
        qualityAnalyzer.QualityParameter.Hidden = false;
        qualityAnalyzer.BestKnownQualityParameter.Hidden = false;
      }
    }
    private void ParameterizeIterationBasedOperators() {
      if (Problem != null) {
        foreach (IIterationBasedOperator op in Problem.Operators.OfType<IIterationBasedOperator>()) {
          op.IterationsParameter.ActualName = "Generations";
          op.IterationsParameter.Hidden = true;
          op.MaximumIterationsParameter.ActualName = "MaximumGenerations";
          op.MaximumIterationsParameter.Hidden = true;
        }
      }
    }
    private void UpdateStrategyParameterOperators() {
      IStrategyParameterCreator oldStrategyCreator = StrategyParameterCreator;
      IStrategyParameterCrossover oldStrategyCrossover = StrategyParameterCrossover;
      IStrategyParameterManipulator oldStrategyManipulator = StrategyParameterManipulator;
      ClearStrategyParameterOperators();
      ISelfAdaptiveManipulator manipulator = (Mutator as ISelfAdaptiveManipulator);
      if (manipulator != null) {
        var operators = Problem.Operators.OfType<IOperator>().Where(x => manipulator.StrategyParameterType.IsAssignableFrom(x.GetType())).OrderBy(x => x.Name);
        foreach (IStrategyParameterCreator strategyCreator in operators.OfType<IStrategyParameterCreator>())
          StrategyParameterCreatorParameter.ValidValues.Add(strategyCreator);
        foreach (IStrategyParameterCrossover strategyRecombinator in operators.OfType<IStrategyParameterCrossover>())
          StrategyParameterCrossoverParameter.ValidValues.Add(strategyRecombinator);
        foreach (IStrategyParameterManipulator strategyManipulator in operators.OfType<IStrategyParameterManipulator>())
          StrategyParameterManipulatorParameter.ValidValues.Add(strategyManipulator);

        if (StrategyParameterCrossoverParameter.ValidValues.Count == 0)
          RecombinatorParameter.ValidValues.Clear(); // if there is no strategy parameter crossover, there can be no crossover when the mutation operator needs strategy parameters

        if (oldStrategyCreator != null) {
          IStrategyParameterCreator tmp1 = StrategyParameterCreatorParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldStrategyCreator.GetType());
          if (tmp1 != null) StrategyParameterCreator = tmp1;
        } else if (StrategyParameterCreatorParameter.ValidValues.Count > 0) StrategyParameterCreator = StrategyParameterCreatorParameter.ValidValues.First();
        if (oldStrategyCrossover != null) {
          IStrategyParameterCrossover tmp2 = StrategyParameterCrossoverParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldStrategyCrossover.GetType());
          if (tmp2 != null) StrategyParameterCrossover = tmp2;
        } else if (StrategyParameterCrossoverParameter.ValidValues.Count > 0) StrategyParameterCrossover = StrategyParameterCrossoverParameter.ValidValues.First();
        if (oldStrategyManipulator != null) {
          IStrategyParameterManipulator tmp3 = StrategyParameterManipulatorParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldStrategyManipulator.GetType());
          if (tmp3 != null) StrategyParameterManipulator = tmp3;
        } else if (StrategyParameterManipulatorParameter.ValidValues.Count > 0) StrategyParameterManipulator = StrategyParameterManipulatorParameter.ValidValues.First();
      }
    }
    private void ClearStrategyParameterOperators() {
      StrategyParameterCreatorParameter.ValidValues.Clear();
      StrategyParameterCrossoverParameter.ValidValues.Clear();
      StrategyParameterManipulatorParameter.ValidValues.Clear();
    }
    private void UpdateRecombinators() {
      ICrossover oldRecombinator = Recombinator;
      RecombinatorParameter.ValidValues.Clear();
      foreach (ICrossover recombinator in Problem.Operators.OfType<ICrossover>().OrderBy(x => x.Name)) {
        RecombinatorParameter.ValidValues.Add(recombinator);
      }
      if (oldRecombinator != null) {
        ICrossover recombinator = RecombinatorParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldRecombinator.GetType());
        if (recombinator != null) RecombinatorParameter.Value = recombinator;
      }
    }
    private void UpdateMutators() {
      IManipulator oldMutator = MutatorParameter.Value;
      MutatorParameter.ValidValues.Clear();
      foreach (IManipulator mutator in Problem.Operators.OfType<IManipulator>().OrderBy(x => x.Name))
        MutatorParameter.ValidValues.Add(mutator);
      if (oldMutator != null) {
        IManipulator mutator = MutatorParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldMutator.GetType());
        if (mutator != null) MutatorParameter.Value = mutator;
      } else if (MutatorParameter.ValidValues.Count > 0 && Problem.Operators.OfType<ISelfAdaptiveManipulator>().Count() > 0) {
        ISelfAdaptiveManipulator mutator = Problem.Operators.OfType<ISelfAdaptiveManipulator>().First();
        if (mutator != null) MutatorParameter.Value = mutator;
      }
    }
    private void UpdateAnalyzers() {
      Analyzer.Operators.Clear();
      if (Problem != null) {
        foreach (IAnalyzer analyzer in Problem.Operators.OfType<IAnalyzer>()) {
          foreach (IScopeTreeLookupParameter param in analyzer.Parameters.OfType<IScopeTreeLookupParameter>())
            param.Depth = 1;
          Analyzer.Operators.Add(analyzer, analyzer.EnabledByDefault);
        }
      }
      Analyzer.Operators.Add(qualityAnalyzer, qualityAnalyzer.EnabledByDefault);
    }
    private EvolutionStrategyMainLoop FindMainLoop(IOperator start) {
      IOperator mainLoop = start;
      while (mainLoop != null && !(mainLoop is EvolutionStrategyMainLoop))
        mainLoop = ((SingleSuccessorOperator)mainLoop).Successor;
      if (mainLoop == null) return null;
      else return (EvolutionStrategyMainLoop)mainLoop;
    }
    #endregion
  }
}
