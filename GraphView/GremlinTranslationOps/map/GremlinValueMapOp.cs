﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphView
{
    internal class GremlinValueMapOp: GremlinTranslationOperator
    {
        public GremlinValueMapOp() { }

        public override GremlinToSqlContext GetContext()
        {
            GremlinToSqlContext inputContext = GetInputContext();

            //inputContext.SetCurrProjection(GremlinUtil.GetFunctionCall("ValueMap"));

            //GremlinToSqlContext newContext = new GremlinToSqlContext();

            //newDerivedVariable = new GremlinMapVariable(inputContext.ToSelectQueryBlock());
            //newContext.AddNewVariable(newDerivedVariable, Labels);
            //newContext.SetDefaultProjection(newDerivedVariable);
            //newContext.SetCurrVariable(newDerivedVariable);

            //TODO: inherit some variable?

            return inputContext;
        }
    }
}
