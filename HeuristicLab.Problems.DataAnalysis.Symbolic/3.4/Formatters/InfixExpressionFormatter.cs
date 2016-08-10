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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic {
  /// <summary>
  /// Formats mathematical expressions in infix form. E.g. x1 * (3.0 * x2 + x3)
  /// </summary>
  [StorableClass]
  [Item("Infix Symbolic Expression Tree Formatter", "A string formatter that converts symbolic expression trees to infix expressions.")]

  public sealed class InfixExpressionFormatter : NamedItem, ISymbolicExpressionTreeStringFormatter {


    [StorableConstructor]
    private InfixExpressionFormatter(bool deserializing) : base(deserializing) { }
    private InfixExpressionFormatter(InfixExpressionFormatter original, Cloner cloner) : base(original, cloner) { }
    public InfixExpressionFormatter()
      : base() {
      Name = ItemName;
      Description = ItemDescription;
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new InfixExpressionFormatter(this, cloner);
    }

    public string Format(ISymbolicExpressionTree symbolicExpressionTree) {
      // skip root and start symbols
      StringBuilder strBuilder = new StringBuilder();
      FormatRecursively(symbolicExpressionTree.Root.GetSubtree(0).GetSubtree(0), strBuilder);
      return strBuilder.ToString();
    }

    private void FormatRecursively(ISymbolicExpressionTreeNode node, StringBuilder strBuilder) {
      if (node.SubtreeCount > 1) {
        var token = GetToken(node.Symbol);
        if (token == "+" || token == "-" || token == "OR" || token == "XOR") {
          strBuilder.Append("(");
          FormatRecursively(node.Subtrees.First(), strBuilder);

          foreach (var subtree in node.Subtrees.Skip(1)) {
            strBuilder.Append(" ").Append(token).Append(" ");
            FormatRecursively(subtree, strBuilder);
          }
          strBuilder.Append(")");

        } else if (token == "*" || token == "/" || token == "AND") {
          strBuilder.Append("(");
          FormatRecursively(node.Subtrees.First(), strBuilder);

          foreach (var subtree in node.Subtrees.Skip(1)) {
            strBuilder.Append(" ").Append(token).Append(" ");
            FormatRecursively(subtree, strBuilder);
          }
          strBuilder.Append(")");
        }
      } else if (node.SubtreeCount == 1) {
        var token = GetToken(node.Symbol);
        if (token == "-" || token == "NOT") {
          strBuilder.Append("(").Append(token).Append("(");
          FormatRecursively(node.GetSubtree(0), strBuilder);
          strBuilder.Append("))");
        } else if (token == "/") {
          strBuilder.Append("1/");
          FormatRecursively(node.GetSubtree(0), strBuilder);
        } else if (token == "+" || token == "*") {
          FormatRecursively(node.GetSubtree(0), strBuilder);
        } else {
          // function
          strBuilder.Append(token).Append("(");
          FormatRecursively(node.GetSubtree(0), strBuilder);
          strBuilder.Append(")");
        }
      } else {
        // no subtrees
        if (node.Symbol is Variable) {
          var varNode = node as VariableTreeNode;
          if (!varNode.Weight.IsAlmost(1.0)) {
            strBuilder.Append("(");
            strBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", varNode.Weight);
            strBuilder.Append("*");
          }
          if (varNode.VariableName.Contains("'")) {
            strBuilder.AppendFormat("\"{0}\"", varNode.VariableName);
          } else {
            strBuilder.AppendFormat("'{0}'", varNode.VariableName);
          }
          if (!varNode.Weight.IsAlmost(1.0)) {
            strBuilder.Append(")");
          }
        } else if (node.Symbol is Constant) {
          var constNode = node as ConstantTreeNode;
          if (constNode.Value >= 0.0)
            strBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", constNode.Value);
          else
            strBuilder.AppendFormat(CultureInfo.InvariantCulture, "({0})", constNode.Value);     // (-1)
        }
      }
    }

    private string GetToken(ISymbol symbol) {
      var tok = InfixExpressionParser.knownSymbols.GetBySecond(symbol).SingleOrDefault();
      if (tok == null)
        throw new ArgumentException(string.Format("Unknown symbol {0} found.", symbol.Name));
      return tok;
    }
  }
}
