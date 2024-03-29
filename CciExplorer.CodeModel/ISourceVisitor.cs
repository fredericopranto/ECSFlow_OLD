﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;

namespace TourreauGilles.CciExplorer.CodeModel
{
    public interface ISourceVisitor : ICodeVisitor
    {
        void Visit(IAndOperation andExpression);

        void Visit(IOrOperation orExpression);
    }
}
