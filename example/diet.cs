using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILOG.Concert;
using ILOG.CPLEX;


namespace diet_example
{
    class diet_example
    {
        static void Main(string[] args)
        {
            Cplex m = new Cplex();
            INumVar x1 = m.NumVar(0, System.Double.MaxValue, NumVarType.Float, "food.1");
            INumVar x2 = m.NumVar(0, System.Double.MaxValue, NumVarType.Float, "food.2");
            INumVar x3 = m.NumVar(0, System.Double.MaxValue, NumVarType.Float, "food.3");
            INumVar x4 = m.NumVar(0, System.Double.MaxValue, NumVarType.Float, "food.4");
            INumVar x5 = m.NumVar(0, System.Double.MaxValue, NumVarType.Float, "food.5");
            IObjective obj = m.AddMinimize(m.Sum(
                m.Prod(x1, 20), m.Prod(x2, 10), m.Prod(x3, 31), m.Prod(x4, 11), m.Prod(x5, 12)));
            INumExpr ironLHS = m.Sum(m.Prod(x1, 2), m.Prod(x3, 3), m.Prod(x4, 1), m.Prod(x5, 2));
            IRange con1 = m.AddGe(ironLHS, 21, "nutrient.iron");
            INumExpr calciumLHS = m.Sum(m.Prod(x2, 1), m.Prod(x3, 2), m.Prod(x4, 2), m.Prod(x5, 1));
            IRange con2 = m.AddGe(calciumLHS, 12, "nutrient.calcium");
            Console.WriteLine("**** Solver Output:\n");
            m.Solve();
            m.ExportModel("diet.lp");
            Console.WriteLine("\n**** Diet Program Output:\n");
            Console.WriteLine("Objective Value = {0}", m.GetObjValue());
            Console.WriteLine("{0} = {1}, reduced cost = {2}", x1.Name, m.GetValue(x1), m.GetReducedCost(x1));
            Console.WriteLine("{0} = {1}, reduced cost = {2}", x2.Name, m.GetValue(x2), m.GetReducedCost(x2));
            Console.WriteLine("{0} = {1}, reduced cost = {2}", x3.Name, m.GetValue(x3), m.GetReducedCost(x3));
            Console.WriteLine("{0} = {1}, reduced cost = {2}", x4.Name, m.GetValue(x4), m.GetReducedCost(x4));
            Console.WriteLine("{0} = {1}, reduced cost = {2}", x5.Name, m.GetValue(x5), m.GetReducedCost(x5));
            Console.WriteLine("{0}, slack = {1}, pi = {2}", con1.Name, m.GetSlack(con1), m.GetDual(con1));
            Console.WriteLine("{0}, slack = {1}, pi = {2}", con2.Name, m.GetSlack(con2), m.GetDual(con2));
            m.End();
        }
    }
}