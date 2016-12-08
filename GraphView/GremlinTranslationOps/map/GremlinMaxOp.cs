﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphView.GremlinTranslationOps.map
{
    internal class GremlinMaxOp: GremlinTranslationOperator
    {
        public Scope Scope;
        public GremlinMaxOp() { }
        public GremlinMaxOp(Scope scope)
        {
            Scope = scope;
        }

        public override GremlinToSqlContext GetContext()
        {
            GremlinToSqlContext inputContext = GetInputContext();

            WScalarExpression parameter = GremlinUtil.GetStarColumnReferenceExpression(); //TODO

            inputContext.ProcessProjectWithFunctionCall(Labels, "max", parameter);

            return inputContext;
        }
    }
}
