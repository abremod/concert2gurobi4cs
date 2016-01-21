using System;
using System.IO;
using Gurobi;
using ILOG;
using ILOG.CPLEX;
using ILOG.Concert;
using System.Collections.Generic;

namespace ILOG
{
    // Classes not in ILOG.CPLEX or ILOG.Concert

    public class Constraint : IConstraint
    {
        public GRBConstr constr;
    }

    public class CVCList    // Constraints, (Gurobi) Variables, Coefficients
    {
        public Dictionary<GRBVar, double> newDict;
        public Dictionary<GRBVar, double> oldDict;
        public Dictionary<GRBVar, double> updDict;
        public GRBConstr[] constrs;

        public void formNewList(INumExpr expr)
        {
            newDict = new Dictionary<GRBVar, double>();
            for (int i = 0; i < expr.expr.Size; i++)
            {
                GRBVar var = expr.expr.GetVar(i);
                double val = expr.expr.GetCoeff(i);
                if (newDict.ContainsKey(var))
                {
                    newDict[var] += val;
                }
                else
                {
                    newDict[var] = val;
                }
            }
        }

        public void getOldList(GRBModel model, GRBConstr constr)
        {
            model.Update();
            GRBLinExpr linExpr = model.GetRow(constr);
            oldDict = new Dictionary<GRBVar, double>();
            for (int i = 0; i < linExpr.Size; i++)
            {
                GRBVar var = linExpr.GetVar(i);
                double val = linExpr.GetCoeff(i);
                if (oldDict.ContainsKey(var))
                {
                    oldDict[var] += val;
                }
                else
                {
                    oldDict[var] = val;
                }
            }
            constrs = new GRBConstr[1];
            constrs[0] = constr;
        }

        public void mergeLists()
        {
            updDict = new Dictionary<GRBVar, double>();
            foreach (KeyValuePair<GRBVar, double> entry in oldDict)
            {
                updDict[entry.Key] = 0;       // clear old GRBVar coefficients
            }
            foreach (KeyValuePair<GRBVar, double> entry in newDict)
            {
                if (updDict.ContainsKey(entry.Key))
                {
                    updDict[entry.Key] += entry.Value;
                }
                else
                {
                    updDict[entry.Key] = entry.Value;
                }
            }
            GRBConstr constr = constrs[0];
            constrs = new GRBConstr[updDict.Count];
            for (int i = 0; i < updDict.Count; i++)
            {
                constrs[i] = constr;
            }
        }
    }

    public class Range : IRange
    {
        GRBModel model;
        public double LB { get; set; }
        public double UB { get; set; }
        INumExpr _expr;
        public INumExpr Expr
        {
            get { return this._expr; }
            set
            {
                this._expr = value;
                // and update gurobi constraint here
                CVCList cvcList = new CVCList();
                cvcList.formNewList(value);
                cvcList.getOldList(model, constr);
                cvcList.mergeLists();
                GRBVar[] vars = new GRBVar[cvcList.updDict.Keys.Count];
                cvcList.updDict.Keys.CopyTo(vars, 0);
                double[] vals = new double[cvcList.updDict.Keys.Count];
                cvcList.updDict.Values.CopyTo(vals, 0);
                model.ChgCoeffs(cvcList.constrs, vars, vals);
            }
        }
        GRBConstr constr { get; set; }
        string _name;
        public string Name
        {
            get { return this._name; }
            set
            {
                this._name = value;
                if (constr != null)
                {
                    constr.Set(GRB.StringAttr.ConstrName, value);
                }
            }
        }

        public Range(GRBModel model, double lb, double ub)
        {
            RangeSetup(model, lb, null, ub, null);
        }
        public Range(GRBModel model, double lb, double ub, string name)
        {
            RangeSetup(model, lb, null, ub, name);
        }
        public Range(GRBModel model, double lb, INumExpr expr, double ub)
        {
            RangeSetup(model, lb, expr, ub, null);
        }
        public Range(GRBModel model, double lb, INumExpr expr, double ub, string name)
        {
            RangeSetup(model, lb, expr, ub, name);
        }
        public void RangeSetup(GRBModel model, double lb, INumExpr expr, double ub, string name)
        {
            this.model = model;
            this.LB = lb;
            this.UB = ub;
            this._name = name;
            if (expr == null)
            {
                _expr = new INumExpr();
            }
            else
            {
                _expr = expr;
            }
            this.constr = null;
            if (lb > -System.Double.MaxValue && ub < System.Double.MaxValue)
            {
                // "lb < expr < ub"    -->    " expr - newvar = lb, 0 < newvar < ub - lb"
                GRBVar var = model.AddVar(0, ub - lb, 0, GRB.CONTINUOUS, null);
                GRBLinExpr modExpr = Expr.expr - var;
                this.constr = model.AddConstr(modExpr, GRB.EQUAL, lb, name);
            }
            else if (lb > -System.Double.MaxValue)
            {
                this.constr = model.AddConstr(Expr.expr, GRB.GREATER_EQUAL, lb, name);
            }
            else
            {
                this.constr = model.AddConstr(Expr.expr, GRB.LESS_EQUAL, ub, name);
            }
        }
        public GRBConstr GetConstr()
        {
            return constr;
        }
    }

} //end namespace ILOG