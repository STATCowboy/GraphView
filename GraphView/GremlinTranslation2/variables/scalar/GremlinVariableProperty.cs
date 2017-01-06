﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphView
{
    internal class GremlinVariableProperty : GremlinScalarVariable
    {
        public GremlinVariable GremlinVariable { get; private set; }
        public string VariableProperty { get; private set; }

        public GremlinVariableProperty(GremlinVariable gremlinVariable, string variableProperty)
        {
            GremlinVariable = gremlinVariable;
            VariableProperty = variableProperty;
        }

        public override WSelectElement ToSelectElement()
        {
            return new WSelectScalarExpression()
            {
                SelectExpr = GremlinUtil.GetColumnReferenceExpr(GremlinVariable.VariableName, VariableProperty)
            };
        }

        public override WScalarExpression ToScalarExpression()
        {
            return GremlinUtil.GetColumnReferenceExpr(GremlinVariable.VariableName, VariableProperty);
        }
    }
}
