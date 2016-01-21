using System;
using System.Collections;
using System.Collections.Generic;
using ILOG.Concert;
using ILOG.CPLEX;

namespace facility_parametric
{
    class FacilityLocation
    {
        const double EPS = 1e-4;
        static void Main(string[] args)
        {
            double[] Demand = new double[] { 15, 18, 14, 20 };
            double[] Capacity = new double[] { 20, 22, 17, 19, 18 };
            double[,] ShipCosts =
                new double[,] { { 4000, 2500, 1200, 2200 }, 
                { 2000, 2600, 1800, 2600 }, 
                { 3000, 3400, 2600, 3100 }, 
                { 2500, 3000, 4100, 3700 },
                { 4500, 4000, 3000, 3200 } };
            int nWarehouses = Capacity.Length;
            int nCustomers = Demand.Length;

            Cplex m = new Cplex();

            INumVar[,] Ship = new INumVar[nWarehouses, nCustomers];
            for (int i = 0; i < nWarehouses; ++i)
                for (int j = 0; j < nCustomers; ++j)
                    Ship[i, j] = m.NumVar(0, System.Double.MaxValue, NumVarType.Float, "ship." + i + "." + j);

            INumVar[] Shortage = new INumVar[nCustomers];
            for (int j = 0; j < nCustomers; ++j)
                Shortage[j] = m.NumVar(0, System.Double.MaxValue, NumVarType.Float, "shortage." + j);

            INumVar TotalShortage = m.NumVar(0, System.Double.MaxValue, NumVarType.Float, "TotalShortage");
            INumVar TotalShippingCost = m.NumVar(0, System.Double.MaxValue, NumVarType.Float, "TotalShippingCost");
            IObjective obj = m.AddMinimize(TotalShippingCost);

            IConstraint[] DemandCon = new IConstraint[nCustomers];
            for (int j = 0; j < nCustomers; ++j)
            {
                INumExpr lhs = m.LinearNumExpr(0.0);
                INumExpr rhs = m.LinearNumExpr(Demand[j]);
                for (int i = 0; i < nWarehouses; ++i)
                    lhs = m.Sum(lhs, Ship[i, j]);
                lhs = m.Sum(lhs, Shortage[j]);
                DemandCon[j] = m.AddEq(lhs, rhs, "demand." + j);
            }

            IConstraint[] CapacityCon = new IConstraint[nWarehouses];
            for (int i = 0; i < nWarehouses; ++i)
            {
                INumExpr lhs = m.LinearNumExpr(0.0);
                for (int j = 0; j < nCustomers; ++j)
                    lhs = m.Sum(lhs, Ship[i, j]);
                CapacityCon[i] = m.AddLe(lhs, Capacity[i], "capacity." + i);
            }

            INumExpr expr = m.Sum(Shortage);
            IConstraint TotalShortageCon = m.AddEq(expr, TotalShortage, "total_shortage");

            expr = m.LinearNumExpr(0.0);
            for (int i = 0; i < nWarehouses; ++i)
                for (int j = 0; j < nCustomers; ++j)
                    expr = m.Sum(expr, m.Prod(ShipCosts[i, j], Ship[i, j]));
            IConstraint TotalShippingCon = m.AddEq(expr, TotalShippingCost, "total_shipping");
            m.ExportModel("Facility.lp");
            while (true)
            {
                Console.WriteLine("\nSolver Output:");
                m.Solve();
                double OptShortage = m.GetValue(TotalShortage);
                double OptShipping = m.GetValue(TotalShippingCost);
                Console.WriteLine("\nFacility Program Output:");
                Console.WriteLine("\nTotalShortage = {0}", OptShortage);
                Console.WriteLine("TotalShippingCost= {0}\n", OptShipping);
                if (OptShortage < EPS) break;
                INumVar[] varArr = new INumVar[26];
                double[] ubs = new double[26];
                double[] lbs = new double[26];
                varArr[0] = TotalShortage;
                varArr[1] = TotalShippingCost;
                for (int i = 0; i < nWarehouses; ++i)
                    for (int j = 0; j < nCustomers; ++j)
                        varArr[4 * i + j + 2] = Ship[i, j];
                for (int j = 0; j < nCustomers; ++j)
                    varArr[j + 22] = Shortage[j];
                m.GetObjSA(lbs, ubs, varArr);
                double ObjectiveBound = ubs[0];
                m.SetLinearCoef(obj, ((1 + EPS) * ObjectiveBound), TotalShortage);
            } // end while

            for (int i = 0; i < nWarehouses; ++i)
                for (int j = 0; j < nCustomers; ++j)
                    Console.WriteLine("{0} = {1}", Ship[i, j].Name, m.GetValue(Ship[i, j]));
            for (int j = 0; j < nCustomers; ++j)
                Console.WriteLine("{0} = {1}", Shortage[j].Name, m.GetValue(Shortage[j]));
            Console.WriteLine("{0} = {1}", TotalShortage.Name, m.GetValue(TotalShortage));
            Console.WriteLine("{0} = {1}", TotalShippingCost.Name, m.GetValue(TotalShippingCost));
        } // end Main

    } // end class FacilityLocation

} // end namespace facility_parametric