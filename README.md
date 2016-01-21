# concert2gurobi4cs

## A dropin C# dll library to reroute [Cplex](http://www-03.ibm.com/software/products/en/ibmilogcpleoptistud) calls to [Gurobi](http://www.gurobi.com/index).

#### Instructions:
Instead of using the references ILOG.Concert and ILOG.CPLEX in your project, use a reference to this dll, concert2gurobi4cs, and to Gurobi65.NET.

Compile as usual.

#### Will it work?
In all likelihood, no.  ILOG.Concert and ILOG.CPLEX contain hundreds of classes, interfaces and methods, and only a small number of them are implemented in this library. Perhaps a very basic program will run. The purpose of this library is more of a starting point for further development.



#### Example output from diet.cs linked to ILOG.CPLEX and ILOG.Concert:
```
**** Solver Output:

Tried aggregator 1 time.
LP Presolve eliminated 0 rows and 3 columns.
Reduced LP has 2 rows, 2 columns, and 4 nonzeros.
Presolve time = 0.00 sec. (0.00 ticks)

Iteration log . . .
Iteration:     1   Dual objective     =           126.000000

**** Diet Program Output:

Objective Value = 131
food.1 = 0, reduced cost = 11.3333333333333
food.2 = 0, reduced cost = 6.66666666666667
food.3 = 0, reduced cost = 11.3333333333333
food.4 = 1, reduced cost = 0
food.5 = 10, reduced cost = 0
nutrient.iron, slack = 0, pi = 4.33333333333333
nutrient.calcium, slack = 0, pi = 3.33333333333333
```

#### Example output from diet.cs linked to concert2gurobi4cs.dll and Gurobi65.NET.dll:
```
**** Solver Output:

Optimize a model with 2 rows, 5 columns and 8 nonzeros
Coefficient statistics:
  Matrix range    [1e+00, 3e+00]
  Objective range [1e+01, 3e+01]
  Bounds range    [0e+00, 0e+00]
  RHS range       [1e+01, 2e+01]
Presolve removed 0 rows and 3 columns
Presolve time: 0.00s
Presolved: 2 rows, 2 columns, 4 nonzeros

Iteration    Objective       Primal Inf.    Dual Inf.      Time
       0    0.0000000e+00   1.650000e+01   0.000000e+00      0s
       2    1.3100000e+02   0.000000e+00   0.000000e+00      0s

Solved in 2 iterations and 0.00 seconds
Optimal objective  1.310000000e+02

**** Diet Program Output:

Objective Value = 131
food.1 = 0, reduced cost = 11.3333333333333
food.2 = 0, reduced cost = 6.66666666666667
food.3 = 0, reduced cost = 11.3333333333333
food.4 = 1, reduced cost = 0
food.5 = 10, reduced cost = 0
nutrient.iron, slack = 0, pi = 4.33333333333333
nutrient.calcium, slack = 0, pi = 3.33333333333333
```