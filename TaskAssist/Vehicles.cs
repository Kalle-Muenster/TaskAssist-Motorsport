using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stepflow;



namespace Stepflow.TaskAssist
{
    public interface ITaskAssistor<D> where D : IActionDriver
    {
        D                driver{ get; set; }
        ITaskAssistor<D> assist();
    }

    public interface ITaskAssistor<A,L> : ITaskAssistor<IActionDriver<A,L>> where A : class where L : IFinishedLap
    {
        A                   action{ get; set; }
        int                 GetAssistence( A forThis );
        int                 ReleaseAssist( A forThis );
    }

    public interface ITaskAsistableVehicle<DriverType> where DriverType : IActionDriver
    {
        ITaskAsistableVehicle<DriverType> task();
        ITaskAssistor<DriverType>         assist();
        int                               StartAssist();
        int                               StoptAssist();
    }

    public interface ITaskAsistableVehicle<A,T> : ITaskAsistableVehicle<IActionDriver<A,ILapFinish<T>>> where A : class where T : class
    {
        ITaskAssistor<A,ILapFinish<T>> assist<D>() where D : ActionDriver<A,LapFinish<T>>, new();
    }

    public class TaskAssist<DriverType,ActionType>
        : ITaskAssistor<DriverType>
        where DriverType : DriveAbstractor, new()
        where ActionType : class
    {
        private static List<DriverType> drivers;
        private static int[]            counted;

        public static void AtExit( object sender, EventArgs e )
        {
            foreach( DriverType driver in drivers )
                driver.controls().Tribune();
        }

        static TaskAssist()
        {
            drivers = new List<DriverType>(0);
            counted = new int[0];
        }

        public static void Init( int preferedPollRate )
        {
            DriverType drv = null;
            for ( int startNum = 0; startNum < drivers.Count; ++startNum) {
                if ( preferedPollRate == (int)drivers[startNum].Speed ) {
                    drv = drivers[startNum]; break;
                }
            } if ( drv == null ) {
                drv =  new DriverType();
                drv.controls().Speed = preferedPollRate;
                drivers.Add( drv );
                int[] extender = new int[drivers.Count];
                if( counted.Length > 0 )
                    counted.CopyTo( extender, 0 );
                extender[counted.Length] = 0;
                counted = extender;
                drivers[drivers.Count-1].controls().Launch();
            }
        }

        private   int                               startnumber;
        protected ITaskAsistableVehicle<DriverType> vehicle;
        public    ITaskAssistor<DriverType>         assist() { return this; }
        public    ActionType                        action;
        public    DriverType                        driver;
        DriverType        ITaskAssistor<DriverType>.driver { get { return driver; } set { driver = value; } }

        /// <summary>TaskAssist
        /// a component which adds abillity to objects (here 'vehicles') to let distinct functionallity be called steady over short durations (few milli seconds till several minutes)
        /// it connects the vehicle object to a timer (here 'driver') which then continuously executes events on the vehicle in a configured interval. the 'drivers' are application wide
        /// shared in the background. which means when in the actually running assembly allready timers driving loops, then TaskAssist tries making reuse of them. when a matching driver
        /// can be found, a newly registered action of some vehicle wich requests assistence for being driven, then will be hooked with into that driver's actualy executing timer loop. 
        /// - it can make usage of either timers (here called 'drivers') executed within application owned Threads as well as drivers which execute within their own, independantly running threads
        /// - it uses an abstract ActionDriver interface which enables possibillity to implement diffeent types of 'drivers' to be used. (e.g. for driving loops barrierd, or varying cycle speed, etc.)  
        /// </summary>
        /// <param name="vehicle"> The 'vehicle' object - an object which registeres for having a distinct method called consectuitively over an unknown period </param>
        /// <param name="control"> A delegate which calles the method on the vehicle object when executed </param>
        /// <param name="persecs"> a value which describes the interval at which the 'control' for the 'vehicle' will be triggered (poll rate) - how the interval is interpreted (e.g. miliseconds, Hz, fps, machine ticks ) depends on the actual used DriverType (generic parameter) </param>
        public TaskAssist( ITaskAsistableVehicle<DriverType> vehicle, ActionType control, uint persecs )
        {
            startnumber = -1;
            this.vehicle = vehicle;
            for( int i = 0; i < drivers.Count; ++i ) {
                if( persecs == (uint)drivers[i].controls().Speed ) {
                    startnumber = i;
                    break; }
            } if ( startnumber < 0 ) {
                startnumber = drivers.Count;
                DriverType tmr = new DriverType();
                tmr.Init(this);
                tmr.controls().Speed = persecs;
                drivers.Add( tmr );
                int[] extender = new int[drivers.Count];
                counted.CopyTo( extender, 0 );
                counted = extender;
                counted[startnumber] = 0;
            } driver = drivers[startnumber];
            action = control;
            if(!driver.IsBreak())
                driver.OnEnded( ()=>{ counted[startnumber] = 0; } );
        }

        public int GetAssistence( ActionType steerWheel )
        {
            if(!driver.controls().Drive( steerWheel ) ) {
                driver.controls().Start( steerWheel );
                return ++counted[startnumber]; 
            } return 0;
        }

        public int ReleaseAssist( ActionType steerWheel )
        {
            if( driver.controls().Drive( steerWheel ) ) {
                driver.controls().Stopt( steerWheel );
                return --counted[startnumber];
            } return counted[startnumber];
        }

    }
}
