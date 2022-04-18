using System;
using System.Collections.Generic;
using Consola;
using Consola.Tests;

namespace TestAssist
{
    public class MotorTest : TestSuite
    {
        public MotorTest( bool verbose, bool xmlreport ) 
            : base(verbose,xmlreport)
        {
            // TODO: prepare any test data needed

            // define test cases
            TestRun += MotorTest_Case1_Motors;
            TestRun += MotorTest_Case2_Drivers;
            TestRun += MotorTest_Case3_Circutes;
            TestRun += MotorTest_Case4_Vehicles;
            TestRun += MotorTest_Case5_Slaloms;
        }

        private void MotorTest_Case5_Slaloms()
        {
            NextCase("Slalom");
            PassStep("Driving Slalom");

            // TODO: implement tests for controller pin jacking, pointer patch casters, pointer typed accessors  
            // maybe also test all these being type safe accessible from within python scripts
            
            CloseCase( CurrentCase );
        }

        private void MotorTest_Case4_Vehicles()
        {
            NextCase("Vehicles");
            PassStep("Building up and driving vehicles");

            TestVehicle vehicle = new TestVehicle();
            vehicle.Value = '1';
            vehicle.Sprint( 500, TestDing.TimeMode.Pollrate );

            System.Threading.Thread.Sleep( 5000 );
            vehicle.Enabled = false;

            CheckStep(vehicle.Count > 0, "Assistence release by Enabled=false works - Count:{0}", vehicle.Count );

            // TODO: implement testing different vehicle implementations
            // should ensure: Objects which implement the ITaskAssistableVehicle<D,T> interface can be constructed and initialized without issues
            // should ensure: A constructed Vehicle object can register an implemented method for being 'control'
            // should ensure: A registerd control, when activated can enter a loop at once (at least within timeframes below a millisecond)
            // should ensure: An actively running control sequence can be dismissed and released again from being triggered at once (at least within les then a millisecond)

            FailStep("There always something must go wrong!");
            CloseCase( CurrentCase );
        }

        private void MotorTest_Case3_Circutes()
        {
            NextCase("Circutes");
            PassStep("Driving circute test drive");

            // TODO: ensure LapCounters working properly and vehicles can register drivers and obtain correct startnumbers for these.
            // ensure circute can be entered and left by drivers at any time, during motors working, and also during motor-sleep phasis  
            CloseCase( CurrentCase );
        }

        private void MotorTest_Case2_Drivers()
        {
            NextCase("Drivers");
            PassStep("Registered drivers obtaining startnumbers");

            // TODO: implement tests for all pre-implementet driver interfaces
            CloseCase( CurrentCase );
        }

        private void MotorTest_Case1_Motors()
        {
            NextCase("Motors");
            PassStep("Different kinds of moters");
            // TODO: implement tests for all of the pre-implemeted motor types 
            // ensure all motors genarating loops which cycle at consistent frequencies 
            CloseCase( CurrentCase );
        }

    }
}
