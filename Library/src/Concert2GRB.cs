using System;
using System.IO;
using Gurobi;
using ILOG;
using ILOG.CPLEX;
using ILOG.Concert;
using System.Collections.Generic;

namespace ILOG
{
    namespace Concert
    {
        public class Exception : System.Exception
        {
            string _ex;
            public Exception(string ex)
            {
                _ex = ex;
            }
        }

        public class NumVarType
        {
            public char type { get; set; }
            public static NumVarType Int = new NumVarType(GRB.INTEGER);
            public static NumVarType Float = new NumVarType(GRB.CONTINUOUS);
            public static NumVarType Bool = new NumVarType(GRB.BINARY);

            public NumVarType(char type)
            {
                this.type = type;
            }
        }

        public class Column : IColumn
        {
            public double objCoef;
            public bool objCoefSet;     // true, if ObjCoef has been set
            public GRBColumn column { get; set; }

            public Column(IObjective obj, double objCoef)
            {
                this.objCoef = objCoef;
                this.objCoefSet = true;
                this.column = new GRBColumn();
            }

            public Column(IRange constraint, double constrCoef)
            {
                this.objCoef = 0;
                this.objCoefSet = false;
                this.column = new GRBColumn();
                this.column.AddTerm(constrCoef, constraint.GetConstr());
            }

            public Column And(Column col)
            {
                if (col.objCoefSet)
                {
                    this.objCoef = col.objCoef;
                }
                // for each constraint in col.column, add it to this.column
                double[] coefs = new double[col.column.Size];
                GRBConstr[] constrs = new GRBConstr[col.column.Size];
                for (int i = 0; i < col.column.Size; i++)
                {
                    coefs[i] = col.column.GetCoeff(i);
                    constrs[i] = col.column.GetConstr(i);
                }
                this.column.AddTerms(coefs, constrs);
                return this;
            }
        }

        public interface IColumn
        {
        }

        public interface IConstraint
        {
        }

        public class IIntExpr : INumExpr
        {
        }

        public interface IModeler
        {
            void Add(IObjective expr);
            IRange AddEq(INumExpr expr, double rhs);
            IRange AddEq(INumExpr expr, double rhs, string name);
            IConstraint AddEq(INumVar var, INumExpr expr);
            IRange AddLe(INumExpr expr, double rhs);
            IRange AddLe(INumExpr expr, double rhs, string name);
            IObjective AddMaximize(INumExpr expr);
            IObjective AddMinimize(INumExpr expr);
            IRange AddRange(double lb, INumExpr expr, double ub);
            IObjective Maximize(INumExpr expr);
            IObjective Minimize(INumExpr expr);
            INumVar NumVar(double lb, double ub, NumVarType type);
            INumVar[] NumVarArray(int num, double[] lbs, double[] ubs);
            INumVar[] NumVarArray(int num, double[] lbs, double[] ubs, NumVarType[] types);
            INumVar[] NumVarArray(int num, double[] lbs, double[] ubs, string[] names);
            INumExpr Prod(double coef, INumVar var);
            INumExpr ScalProd(Double[] coefs, INumVar[] vars);
            INumExpr ScalProd(INumVar[] vars, double[] coefs);
            INumExpr Sum(params INumExpr[] les);
        }

        public interface IMPModeler : IModeler
        {
            IObjective AddMaximize();
            IObjective AddMinimize();
            IRange AddRange(double lb, double ub);
            IRange AddRange(double lb, double ub, string name);
            Column Column(IObjective obj, double objCoef);
            Column Column(IRange constraint, double coef);
            INumVar NumVar(Column col, double lb, double ub, NumVarType type);
            INumVar NumVar(Column col, double lb, double ub, string name);
        }

        public class INumExpr
        {
            GRBLinExpr _expr;
            public GRBLinExpr expr
            {
                get { return this._expr; }
                set { this._expr = value; }
            }
            public INumExpr()
            {
                this.expr = new GRBLinExpr();
            }
            public INumExpr(GRBLinExpr expr)
            {
                this.expr = expr;
            }
        }

        public class INumVar
        {
            public double LB { get; set; }
            public double UB { get; set; }
            public GRBVar var { get; set; }
            string _name;
            public string name
            {
                get { return this._name; }
                set { this._name = value; }
            }
            public string Name
            {
                get { return this._name; }
                set
                {
                    this._name = value;
                    var.Set(GRB.StringAttr.VarName, value);
                }
            }
        }

        public class IObjective : INumExpr
        {
            GRBModel model;
            public INumExpr Expr
            {
                set { this.expr = value.expr; 
                    this.model.SetObjective(this.expr);
                }
            }
            public int sense { get; set; }
            public IObjective(GRBModel model)
                : base()
            {
                this.model = model;
                this.sense = GRB.MINIMIZE;    // default
            }
            public IObjective(GRBModel model, int sense)
                : base()
            {
                this.model = model;
                this.sense = sense;
            }
            public IObjective(GRBModel model, INumExpr expr)
                : base(expr.expr)
            {
                this.model = model;
                this.sense = GRB.MINIMIZE;    // default
            }
            public IObjective(GRBModel model, INumExpr expr, int sense)
                : base(expr.expr)
            {
                this.model = model;
                this.sense = sense;
            }
        }

        public interface IRange : IConstraint
        {
            double LB { get; set; }
            double UB { get; set; }
            INumExpr Expr { get; set; }
            string Name { get; set; }
            GRBConstr GetConstr();
        }

    } // end namespace Concert

} //end namespace ILOG