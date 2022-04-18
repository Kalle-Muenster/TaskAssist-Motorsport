using System;
using System.Collections.Generic;



namespace Stepflow.TaskAssist
{
    public interface ITaskAssistor<A,L>
        : ITaskAssistor<IActionDriver<A,ILapFinish<L>,L>,A,L>
    where A : class
    where L : class
    {
        int GetAssistence(A forThis);
        int ReleaseAssist(A forThis);
    }

    public interface ITaskAssistor<D,A,L>
        where D : IActionDriver
        where A : class
        where L : class
    {
        D                    driver { get; set; }
        A                    action { get; set; }
        ITaskAssistor<D,A,L> assist();
    }

    public interface IAsistableVehicle<DriverType,EventsType>
        where DriverType : IActionDriver
        where EventsType : IFinishedLap
    {
        int   StartAssist();
        int   StoptAssist();
    }

    public interface ITaskAsistableVehicle<A,L> 
        : IAsistableVehicle<IActionDriver<A,ILapFinish<L>,L>,ILapFinish<L>>
        where A : class
        where L : class
    {
        ITaskAssistor<A,L>         assist { get; set; }
        ITaskAsistableVehicle<A,L> task();
    }

    public class TaskAssist<DriverType,ActionType,LapAction>
        : ITaskAssistor<ActionType,LapAction>
        where DriverType : DriveAbstractor, new()
        where ActionType : class
        where LapAction  : class
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
                drv = new DriverType();
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

        private   int                                          startnumber;
        protected ITaskAsistableVehicle<ActionType,LapAction>  vehicle;
        public ITaskAssistor<ActionType, LapAction>            assist { get { return this; } }
        public ActionType                                      action { get; set; }
        public DriverType                                      driver;


        IActionDriver<ActionType,ILapFinish<LapAction>,LapAction>
        ITaskAssistor<IActionDriver<ActionType,ILapFinish<LapAction>,LapAction>,ActionType,LapAction>.driver { 
            get { return driver as IActionDriver<ActionType, ILapFinish<LapAction>,LapAction>; } 
            set { driver = value as DriverType; }
        }

        /// <summary>TaskAssist
        /// a component which adds abillity to objects (here 'vehicles') to let distinct functionallity be called steady over short durations (few milli seconds till several minutes)
        /// it connects the vehicle object to a timer (here 'driver') which then continuously executes events on the vehicle in a configurable interval. the 'drivers' are shared
        /// application wide in the background. which means when in the actually running assembly allready timers driving loops, then TaskAssist tries making reuse of them. when a matching driver
        /// can be found, a newly registered action of some vehicle wich requests assistence for being driven, then will be hooked with into that driver's actualy executing timer loop. 
        /// - it can make usage of either timers (here called 'drivers') executed within application owned Threads as well as drivers which execute within their own, independantly running threads
        /// - it uses an abstract ActionDriver interface which enables possibillity to implement diffeent types of 'drivers' to be used. (e.g. for driving loops barrierd, or with varying cycle intervals, etc.)  
        /// </summary>
        /// <param name="vehicle"> The 'vehicle' object - an object which registeres for having distinct methods called consectuitively over an unknown period </param>
        /// <param name="control"> A delegate which calles methods on the vehicle object for controling some functionallity of the driven vehicle </param>
        /// <param name="persecs"> an abstract value which describes the 'speed' at which the 'control' for the 'vehicle' will be triggered (like a poll rate) - how that speed value actually will be interpreted is left up open to the DriverType implementation (if it assumes miliseconds, Hz, fps, machine ticks metronome beats or anythin else highly depends on the actual used DriverType implementation (generic parameter) </param>
        public TaskAssist( ITaskAsistableVehicle<ActionType,LapAction> vehicle, ActionType control, uint persecs )
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

        ITaskAssistor<IActionDriver<ActionType,ILapFinish<LapAction>,LapAction>,ActionType,LapAction>
        ITaskAssistor<IActionDriver<ActionType,ILapFinish<LapAction>,LapAction>,ActionType,LapAction>.assist()
        {
            return this;
        }

        int ITaskAssistor<ActionType, LapAction>.GetAssistence( ActionType steerWheel )
        {
            if(!driver.controls().Drive( steerWheel ) ) {
                driver.controls().Start( steerWheel );
                return ++counted[startnumber]; 
            } return 0;
        }

        int ITaskAssistor<ActionType, LapAction>.ReleaseAssist( ActionType steerWheel )
        {
            if( driver.controls().Drive( steerWheel ) ) {
                driver.controls().Stopt( steerWheel );
                return --counted[startnumber];
            } return counted[startnumber];
        }
    }
}
