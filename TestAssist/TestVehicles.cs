using System;
using System.Collections.Generic;
using Stepflow;
using Stepflow.TaskAssist;

namespace TestAssist
{
    public abstract class TestDing
    {
        public enum TimeMode
        {
            Pollrate, Realtime, FasterThenAll, Accelleration
        }

        public TestDing()
        {
            Value = 0;
            Count = 0;
        }

        public int Count;
        public int Value;
        abstract public bool Enabled { get; set; }

        public void Input()
        {
            Value = Consola.StdStream.Inp.GetChar();
        }

        public void Sprint( int distance, TimeMode interpret )
        {
            Count = distance;
            Enabled = true;
        }

        public void HaltAt( int distance, TimeMode interpret )
        {
            if (distance < 0) Count = -distance;
            else if (distance > 0) Count = distance;
            else Enabled = false;
        }

        public void Interupt()
        {
            HaltAt( 0, TimeMode.FasterThenAll );
        }

        public void Output()
        {
            if (Count > 0) { --Count;
                Consola.StdStream.Out.Write( Value.ToString() );
            } else { Value = 0; Interupt(); }
        }
    }

    public class TestVehicle
        : TestDing
        , ITaskAsistableVehicle<Action,Action>
    {
        public override bool Enabled {
            get { return task().assist.driver.Drive( task().assist.action ); }
            set { if (value) { task().StartAssist(); } else { task().StoptAssist(); } }
        }



        static TestVehicle()
        {
            TaskAssist<SteadyAction,Action,LapFinish<Action>>.Init( 100 );
        }
        public TestVehicle() : base()
        {
            task().assist = new TaskAssist<SteadyAction,Action,Action>( this, Output, 100 );
        }


        ITaskAssistor<Action, Action> ITaskAsistableVehicle<Action, Action>.assist { get; set; }

        public ITaskAsistableVehicle<Action,Action> task()
        {
            return this;
        }

        int IAsistableVehicle<IActionDriver<Action, ILapFinish<Action>,Action>, ILapFinish<Action>>.StartAssist()
        {
            return task().assist.GetAssistence( task().assist.action );
        }

        int IAsistableVehicle<IActionDriver<Action, ILapFinish<Action>,Action>, ILapFinish<Action>>.StoptAssist()
        {
            return task().assist.ReleaseAssist( task().assist.action );
        }
    }

  
}
