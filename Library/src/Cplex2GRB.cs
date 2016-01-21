using System;
using System.IO;
using Gurobi;
using ILOG;
using ILOG.CPLEX;
using ILOG.Concert;
using System.Collections.Generic;

namespace ILOG
{
    namespace CPLEX
    {
        public class Cplex : IMPModeler
        {
            private GRBEnv _env = null;
            protected GRBModel _model = null;
            private int _status = -1;
            public double ObjValue = 0;

            public static class Status
            {
                public static int Infeasible = GRB.Status.INFEASIBLE;
            }

            public Cplex()
            {
                this._env = new GRBEnv("gurobi.log");
                this._model = new GRBModel(_env);
                _model.GetEnv().Set(GRB.IntParam.UpdateMode, 1);
                this._status = 0;
            }

            public void ImportModel(string file)
            {
                if (_model != null)
                {
                    _model.Dispose();
                    _model = null;
                }
                this._model = new GRBModel(_env, file);
            }
            public bool Solve()
            {
                _model.Optimize();
                ObjValue = _model.Get(GRB.DoubleAttr.ObjVal);
                _status = _model.Get(GRB.IntAttr.Status);
                return (_status == GRB.Status.OPTIMAL);
            }
            public double GetObjValue()
            {
                return _model.Get(GRB.DoubleAttr.ObjVal);
            }
            public string GetStatus()
            {
                _status = _model.Get(GRB.IntAttr.Status);
                return status2string(_status);
            }
            static string[] statusArray = { "Unused",    // 0
                                     "Loaded",      // 1
                                     "Optimal",     // 2
                                     "Infeasible",  // 3
                                     "Infeasible or Unbounded", // 4
                                     "Bounded",     // 5
                                     "Cutoff",      // 6
                                     "Iteration Limit",      // 7
                                     "Node Limit",      // 8
                                     "Time Limit",      // 9
                                     "Solution Limit",      // 10
                                     "Interrupted",      // 11
                                     "Numberic",      // 12
                                     "Suboptimal",      // 13
                                     "In Progress"      // 14
                                     };
            string status2string(int status){
                return statusArray[status];
            }
            public void End()
            {
                _model.Dispose();
                _env.Dispose();
            }

            // *****************************************************************
            //
            // Add Variables to model
            //
            // *****************************************************************
            public INumVar[] BoolVarArray(int num)
            {
                return NumVarArray(num, 0, 1, NumVarType.Bool);
            }
            public INumVar[] NumVarArray(int num, double lb, double ub)
            {
                return NumVarArray(num, lb, ub, NumVarType.Float);
            }
            public INumVar[] IntVarArray(int num, double lb, double ub)
            {
                return NumVarArray(num, lb, ub, NumVarType.Int);
            }
            public INumVar[] NumVarArray(int num, double lb, double ub, NumVarType type)
            {
                double[] ubs = new double[num];
                double[] lbs = new double[num];
                NumVarType[] types = new NumVarType[num];
                for (int i = 0; i < num; i++)
                {
                    lbs[i] = lb;
                    ubs[i] = ub;
                    types[i] = type;
                }
                return NumVarArray(num, lbs, ubs, types, null);
            }

            public INumVar[] NumVarArray(int num, double[] lbs, double[] ubs)
            {
                return NumVarArray(num, lbs, ubs, null, null);
            }
            public INumVar[] NumVarArray(int num, double[] lbs, double[] ubs, NumVarType[] types)
            {
                return NumVarArray(num, lbs, ubs, types, null);
            }
            public INumVar[] NumVarArray(int num, double[] lbs, double[] ubs, string[] names)
            {
                return NumVarArray(num, lbs, ubs, null, names);
            }
            public INumVar[] NumVarArray(int num, double[] lbs, double[] ubs, NumVarType[] types, string[] names)
            {
                INumVar[] result = new INumVar[num];
                char[] gurobi_types = null;
                if (types != null)
                {
                    gurobi_types = new char[num];
                    for (int i = 0; i < num; i++)
                    {
                        gurobi_types[i] = types[i].type;
                    }
                }
                GRBVar[] vars = _model.AddVars(lbs, ubs, null, gurobi_types, names);
                for (int i = 0; i < num; i++)
                {
                    result[i] = new INumVar();
                    result[i].var = vars[i];
                }
                return result;
            }
            public INumVar NumVar(double lb, double ub)
            {
                INumVar result = new INumVar();
                result.var = _model.AddVar(lb, ub, 0, GRB.CONTINUOUS, null);
                return result;
            }
            public INumVar NumVar(double lb, double ub, NumVarType type)
            {
                INumVar result = new INumVar();
                result.var = _model.AddVar(lb, ub, 0, type.type, null);
                return result;
            }
            public INumVar NumVar(double lb, double ub, NumVarType type, string name)
            {
                INumVar result = new INumVar();
                result.name = name;
                result.var = _model.AddVar(lb, ub, 0, type.type, name);
                return result;
            }
            public INumVar NumVar(Column col, double lb, double ub)
            {
                INumVar result = new INumVar();
                result.var = _model.AddVar(lb, ub, col.objCoef, GRB.CONTINUOUS, col.column, null);
                return result;
            }
            public INumVar NumVar(Column col, double lb, double ub, NumVarType type)
            {
                INumVar result = new INumVar();
                result.var = _model.AddVar(lb, ub, col.objCoef, type.type, col.column, null);
                return result;
            }
            public INumVar NumVar(Column col, double lb, double ub, string name)
            {
                INumVar result = new INumVar();
                result.name = name;
                result.var = _model.AddVar(lb, ub, col.objCoef, GRB.CONTINUOUS, col.column, name);
                return result;
            }
            // *****************************************************************
            //
            // Get Variable data
            //
            // *****************************************************************
            public double GetValue(INumVar var)
            {
                return var.var.Get(GRB.DoubleAttr.X);
            }
            public double GetReducedCost(INumVar var)
            {
                return var.var.Get(GRB.DoubleAttr.RC);
            }
            public double[] GetValues(INumVar[] vars)
            {
                double[] result = new double[vars.Length];
                GRBVar[] grbVars = new GRBVar[vars.Length];
                for (int i = 0; i < vars.Length; i++)
                {
                    grbVars[i] = vars[i].var;
                }
                result = _model.Get(GRB.DoubleAttr.X, grbVars);
                return result;
            }
            public double[] GetReducedCosts(INumVar[] vars)
            {
                double[] result = new double[vars.Length];
                GRBVar[] grbVars = new GRBVar[vars.Length];
                for (int i = 0; i < vars.Length; i++)
                {
                    grbVars[i] = vars[i].var;
                }
                result = _model.Get(GRB.DoubleAttr.RC, grbVars);
                return result;
            }
            public void GetObjSA(double[] lbs, double[] ubs, INumVar[] vars)
            {
                for (int i = 0; i < vars.Length; i++)
                {
                    if (lbs != null)
                    {
                        lbs[i] = vars[i].var.Get(GRB.DoubleAttr.SAObjLow);
                    }
                    if (ubs != null)
                    {
                        ubs[i] = vars[i].var.Get(GRB.DoubleAttr.SAObjUp);
                    }
                }
            }
            public void Add(VoidClass value)
            {
            }
            public VoidClass Conversion(INumVar var, NumVarType type)
            {
                var.var.Set(GRB.CharAttr.VType, type.type);
                return new VoidClass();
            }
            public class VoidClass
            {  
                // hack for above method
            }

            // *****************************************************************
            //
            // Add Constraints to model
            //
            // *****************************************************************
            public IRange AddEq(INumExpr expr, double rhs)
            {
                Range result = new Range(_model, rhs, expr, rhs);
                return result;
            }
            public IRange AddEq(INumExpr expr, double rhs, string name)
            {
                Range result = new Range(_model, rhs, expr, rhs, name);
                return result;
            }
            public IConstraint AddEq(INumVar var, INumExpr expr)
            {
                Constraint result = new Constraint();
                result.constr = _model.AddConstr(var.var, GRB.EQUAL, expr.expr, null);
                return result;
            }
            public IConstraint AddEq(INumExpr lhs, INumExpr rhs, string name)
            {
                Constraint result = new Constraint();
                result.constr = _model.AddConstr(lhs.expr, GRB.EQUAL, rhs.expr, name);
                return result;
            }
            public IConstraint AddEq(INumExpr lhs, INumVar var, string name)
            {
                Constraint result = new Constraint();
                result.constr = _model.AddConstr(lhs.expr, GRB.EQUAL, var.var, name);
                return result;
            }
            public IRange AddLe(INumExpr expr, double rhs)
            {
                Range result = new Range(_model, System.Double.MinValue, expr, rhs);
                return result;
            }
            public IRange AddLe(INumExpr expr, double rhs, string name)
            {
                Range result = new Range(_model, System.Double.MinValue, expr, rhs, name);
                return result;
            }
            public IConstraint AddLe(INumVar var, double rhs)
            {
                Constraint result = new Constraint();
                result.constr = _model.AddConstr(var.var, GRB.LESS_EQUAL, rhs, null);
                return result;
            }
            public IConstraint AddLe(INumVar var, INumExpr expr)
            {
                Constraint result = new Constraint();
                result.constr = _model.AddConstr(var.var, GRB.LESS_EQUAL, expr.expr, null);
                return result;
            }
            public IRange AddGe(INumExpr expr, double lhs, string name)
            {
                Range result = new Range(_model, lhs, expr, System.Double.MaxValue, name);
                return result;
            }
            public IRange AddRange(double lb, INumExpr expr, double ub)
            {
                return new Range(_model, lb, expr, ub);
            }
            public IRange AddRange(double lb, double ub)
            {
                return new Range(_model, lb, ub);
            }
            public IRange AddRange(double lb, double ub, string name)
            {
                return new Range(_model, lb, ub, name);
            }
            public double GetSlack(IRange rng)
            {
                return rng.GetConstr().Get(GRB.DoubleAttr.Slack);
            }
            public double[] GetSlacks(IRange[] rng)
            {
                double[] result = new double[rng.Length];
                GRBConstr[] grbConstrs = new GRBConstr[rng.Length];
                for (int i = 0; i < rng.Length; i++)
                {
                    grbConstrs[i] = rng[i].GetConstr();
                }
                result = _model.Get(GRB.DoubleAttr.Slack, grbConstrs);
                return result;
            }
            public double GetDual(IRange rng)
            {
                GRBConstr[] grbConstrs = new GRBConstr[1];
                grbConstrs[0] = rng.GetConstr();
                double result = _model.Get(GRB.DoubleAttr.Pi, grbConstrs)[0];
                return result;
            }
            public double[] GetDuals(IRange[] rng)
            {
                double[] result = new double[rng.Length];
                GRBConstr[] grbConstrs = new GRBConstr[rng.Length];
                for (int i = 0; i < rng.Length; i++)
                {
                    grbConstrs[i] = rng[i].GetConstr();
                }
                result = _model.Get(GRB.DoubleAttr.Pi, grbConstrs);
                return result;
            }

            // *****************************************************************
            //
            // Add objective function to model
            //
            // *****************************************************************
            public IObjective AddMinimize(INumExpr expr)
            {
                IObjective result = new IObjective(this._model, expr, GRB.MINIMIZE);
                _model.SetObjective(result.expr, result.sense);
                return result;
            }
            public IObjective AddMinimize(INumVar var)
            {
                IObjective result = new IObjective(this._model, new INumExpr(var), GRB.MINIMIZE);
                _model.SetObjective(result.expr, result.sense);
                return result;
            }
            public IObjective AddMinimize()
            {
                IObjective result = new IObjective(this._model, GRB.MINIMIZE);
                _model.SetObjective(result.expr, result.sense);
                return result;
            }
            public IObjective Minimize(INumExpr expr)
            {
                return AddMinimize(expr);
            }
            public IObjective AddMaximize(INumExpr expr)
            {
                IObjective result = new IObjective(this._model, expr, GRB.MAXIMIZE);
                _model.SetObjective(result.expr, result.sense);
                return result;
            }
            public IObjective AddMaximize()
            {
                IObjective result = new IObjective(this._model, GRB.MAXIMIZE);
                _model.SetObjective(result.expr, result.sense);
                return result;
            }
            public IObjective Maximize(INumExpr expr)
            {
                return AddMaximize(expr);
            }
            public void Add(IObjective expr)
            {
                _model.SetObjective(expr.expr, expr.sense);
            }
            public void Add(INumVar var)
            {
                _model.SetObjective(new GRBLinExpr(var.var, 1.0));
            }
            public void SetLinearCoef(IObjective obj, double val, INumVar var)
            {
                var.var.Set(GRB.DoubleAttr.Obj, val);
            }

            // *****************************************************************
            //
            // Helper functions
            //
            // *****************************************************************
            public INumExpr LinearNumExpr(double val)
            {
                return new INumExpr(val);
            }
            public INumExpr Sum(INumVar var, params INumExpr[] les)
            {
                INumExpr result = new INumExpr();
                result.expr = var.var + Sum(les).expr;
                return result;
            }
            public INumExpr Sum(params INumExpr[] les)
            {
                INumExpr result = new INumExpr();
                for (int i = 0; i < les.Length; i++)
                {
                    result.expr += les[i].expr;
                }
                return result;
            }
            public INumExpr Sum(INumVar[] vars)
            {
                INumExpr result = new INumExpr();
                for (int i = 0; i < vars.Length; i++)
                {
                    result.expr += vars[i].var;
                }
                return result;
            }
            public INumExpr Sum(INumExpr expr, INumVar var)
            {
                INumExpr result = new INumExpr();
                result.expr = expr.expr + var.var;
                return result;
            }
            public INumExpr Diff(double val, INumExpr expr)
            {
                INumExpr result = new INumExpr();
                result.expr = val - expr.expr;
                return result;
            }

            public INumExpr ScalProd(double[] coefs, INumVar[] vars)
            {
                INumExpr result = new INumExpr();
                for (int i = 0; i < vars.Length; i++)
                {
                    result.expr += coefs[i] * vars[i].var;
                }
                return result;
            }

            public INumExpr ScalProd(INumVar[] vars, double[] coefs)
            {
                return ScalProd(coefs, vars);
            }

            public INumExpr Prod(double coef, INumVar var)
            {
                INumExpr result = new INumExpr();
                result.expr += coef * var.var;
                return result;
            }
            public INumExpr Prod(INumVar var, double coef)
            {
                return Prod(coef, var);
            }

            public Column Column(IObjective obj, double objCoef)
            {
                return new Column(obj, objCoef);
            }

            public Column Column(IRange constraint, double coef)
            {
                return new Column(constraint, coef);
            }

            public void ExportModel(string filename)
            {
                _model.Update();
                _model.Write(filename);
            }

            public TextWriter Output()
            {
                return Console.Out;
            }

            public void SetParam(IntParam param, int value) 
            {
                this._model.GetEnv().Set(param.param, value);
            }

            public void SetParam(DoubleParam param, double value)
            {
                this._model.GetEnv().Set(param.param, value);
            }

            public double GetParam(DoubleParam param)
            {
                return this._model.GetEnv().Get(param.param);
            }

            public abstract class Param
            {
                //          GRB.<T>Param CPLEX_Parameter = GRB.<T>Param.GurobiParameter
                public static IntParam RootAlgorithm = new IntParam(GRB.IntParam.Method);

                public abstract class MIP
                {
                    public abstract class Tolerances
                    {
                        public static DoubleParam Integrality = new DoubleParam(GRB.DoubleParam.IntFeasTol);
                    }
                }
            }
            public class IntParam 
            {
                public GRB.IntParam param;
                public IntParam(GRB.IntParam param)
                {
                    this.param = param;
                }
            }

            public class DoubleParam
            {
                public GRB.DoubleParam param;
                public DoubleParam(GRB.DoubleParam param)
                {
                    this.param = param;
                }
            }

            public static class Algorithm
            {
                // CPLEX Parameter Value = Gurobi Paramenter Value
                public static int Primal = 0;       // Gurobi Method Parameter
                public static int Dual = 1;         // Gurobi Method Parameter
                public static int Barrier = 2;      // Gurobi Method Parameter
            }


        } // end class Cplex

    } // end namespace CPLEX

} //end namespace ILOG