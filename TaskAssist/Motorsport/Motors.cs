#if DEBUG
#if EXTREM
#define EXTREM_DEBUG
#endif
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Stepflow;
#if EXTREM_DEBUG
using Consola;
#endif


namespace Stepflow.TaskAssist
{
    public class LapEventArgs : EventArgs
    {
        public int Drivers;
        public LapEventArgs(int drivers)
        {
            Drivers = drivers;
        }
        public static implicit operator LapEventArgs(int cast)
        {
            return new LapEventArgs( cast );
        }
    }

    public delegate void LapFinished(object sender, LapEventArgs e);
    public delegate void RaceOver();

    [Flags]
    public enum MotorConfiguration
    {
        Default = 0,
        Task = 1,
        Thread = 2,
        Detached = 3 
    }

    public abstract class TimedLoop<A,L,T>
        : ActionDriver<A,L,T>
        where A : class
        where L : LapAbstractor, ILapFinish<T>, new()
        where T : class
    {
        private object _drive;
        private bool    thread;
        protected T                  getMotor<T>() where T : class {
            return _drive as T;
        }
        protected HashSet<A>         users;
        protected volatile int       activ;
        protected long               ticks;

        private LapFinished          round;
        private RaceOver             ended = null;
        public override void OnEnded( RaceOver handler ) { ended = handler; }
        public override bool IsBreak() { return ended != null; }
        protected void nextLap( int drivers ) { if( drivers == 0 ) { ended?.Invoke(); } if( round != null ) round(this, drivers); }

        public event LapFinished     Round {
            add { round += value; }
            remove { round -= value; }
        }

        override public float Speed {
            get { return (float)TimeSpan.TicksPerSecond / ticks; }
            set { ticks = (long)( ( 1.0f / value ) * TimeSpan.TicksPerSecond ); }
        }

        virtual protected void motor()
        {
            IEnumerator<A> drvrs = users.GetEnumerator();
            TimeSpan idealline = new TimeSpan(ticks);
            TimeSpan takenline;
            DateTime cycletime;
            int drivers = users.Count;
            while( activ > 0 && drivers > 0 ) {
                cycletime = DateTime.Now;
                while( drvrs.MoveNext() ) ( drvrs.Current as Delegate ).DynamicInvoke();
                drvrs.Reset();
                nextLap(drivers = users.Count);
                if( activ > 0 ) {
                    takenline = DateTime.Now - cycletime;
                    Thread.Sleep(idealline - takenline);
                }
            }
            activ = -1;
            users.Clear();
            tackt.SetHashSet(users);
        }

        public override L Tackt {
            get { return tackt; }
            set { if( !value.UseExternals ) {
                    tackt.SetHashSet(( users = value.ToHashSet<A>() ));
                }
            }
        }

        public TimedLoop( MotorConfiguration config ) : base()
        {
            users = new HashSet<A>();
            if( thread = config == MotorConfiguration.Thread ) {
                _drive = new Thread( motor );
                getMotor<Thread>().SetApartmentState( ApartmentState.STA );
            } else {
                _drive = new Task( motor, TaskCreationOptions.PreferFairness | (config == MotorConfiguration.Detached ? TaskCreationOptions.LongRunning : TaskCreationOptions.AttachedToParent ) );
            } return;
        }

        protected bool motor_reset()
        {
            if( thread ) {
                Thread drive = getMotor<Thread>();
                if( (int)( drive.ThreadState & ThreadState.Stopped | ThreadState.Aborted ) != 0 ) {
                    _drive = new Thread( motor );
                    drive = getMotor<Thread>();
                    drive.SetApartmentState( ApartmentState.STA );
                    activ = 0;
                    return true;
                } else return false;
            } else {
                Task drive = getMotor<Task>();
                if( drive.IsCompleted || drive.IsFaulted ) {
                    _drive = new Task( motor, drive.CreationOptions );
                    activ = 0;
                    return true;
                } else return false;
            }
        }

        protected void motor_start()
        {
            if( thread ) {
                if( getMotor<Thread>().ThreadState != ThreadState.Running ) {
                    activ = 1;
                    getMotor<Thread>().Start();
                }
            } else {
                if( !getMotor<Task>().Status.HasFlag( TaskStatus.Running ) ) {
                    activ = 1;
                    getMotor<Task>().Start();
                }
            }
        }

        public override void Launch()
        {
            if( !motor_reset() ) if( activ > 0 ) return;
            motor_start();
        }

        private void awaitloop()
        {
            Thread drive = getMotor<Thread>();
            if ( drive != Thread.CurrentThread )
            while( drive.ThreadState == ThreadState.Running ) {
                Thread.Sleep( 10 );
            }
        }

        public override Task Tribune()
        {
            --activ;
            if( thread ) {
                Task awaiter = new Task( awaitloop );
                awaiter.Start();
                return awaiter;
            } else {
                return getMotor<Task>();
            }
        }

        public override void Start( A action )
        {
            if( activ == 0 ) {
                users.Add( action );
                tackt.SetHashSet( users );
                Launch();
            }
        }

        public override bool Drive( A action )
        {
            if( activ == 0 )
                return users.Contains( action );
            else return tackt.Contains( action );
        }

        public override async void Stopt( A action )
        {
            Task finished = activ > 0
               ? Tribune() : null;
            users.Clear();
            tackt.SetHashSet( users );
            if( finished != null )
                await finished;
        }
    }

    public abstract class BaseIntervalDrive<A,L,T> 
        : TimedLoop<A,L,T>
        where A : class
        where L : LapAbstractor, ILapFinish<T>, new()
        where T : class
    {      
        protected Queue<A>      kuehe;

        protected override void motor()
        {
#if EXTREM_DEBUG
            int dbgcount = 0;
#endif
            TimeSpan longWait = new TimeSpan( ticks * 5 );
            TimeSpan shortWait = new TimeSpan( ticks );
            while (activ > 0) { bool events = false;
                DateTime BeginTimeLoop = DateTime.Now;
                if ( kuehe.Count > 0 ) { activ += 2;
                    while ( kuehe.Count > 0 ) {
                        A kuh = kuehe.Dequeue();
                        if ( !users.Remove( kuh ) ) users.Add( kuh );
                        tackt.SetHashSet( users );
                    } activ -= 2; 
                events = true; }
                int drivers = users.Count;
                if( drivers > 0 ) { activ += 4;
                    IEnumerator<A> drvr = users.GetEnumerator();
                    while( drvr.MoveNext() ) (drvr.Current as Delegate).DynamicInvoke();
                    activ -= 4; events = true;
                } else activ = 0;
                nextLap( drivers );
#if EXTREM_DEBUG
                if (++dbgcount == 100 ) {
                    dbgcount = 0;
                    StdStream.Out.Write("Thread: '{0}' - 100!",Thread.CurrentThread.ManagedThreadId);
                }
                TimeSpan amount = events
                       ? shortWait - (DateTime.Now - BeginTimeLoop)
                       : longWait - (DateTime.Now - BeginTimeLoop);
                if( amount > TimeSpan.Zero )
                    Thread.Sleep( amount );
#else
                if( activ > 0 ) {
                    TimeSpan sleep = (DateTime.Now - BeginTimeLoop);
                    sleep = (events ? shortWait : longWait) - sleep;
                    if( sleep.Ticks > 0 ) Thread.Sleep( sleep );
                }
#endif    
            } activ = -1;
            users.Clear();
            tackt.SetHashSet( users );
        }
    
        public BaseIntervalDrive() : this( MotorConfiguration.Detached ){}

        public BaseIntervalDrive( MotorConfiguration config ) : base( config )
        {
#if EXTREM_DEBUG
            StdStream.Init();
#endif
            kuehe = new Queue<A>();
            tackt.setAddAndRemove<A>(Start, Stopt);
            return;
        }

        public BaseIntervalDrive( float intervalFPS, MotorConfiguration config ) : this( config )
        {
            Speed = intervalFPS;
            return;
        }

        public override L Tackt {
            get { return tackt; }
            set { if ( !value.UseExternals ) {
                    tackt.SetHashSet( (users = value.ToHashSet<A>()) );
                }
            }
        }

        public override void Start( A action )
        {
            if ( activ < 4 ) { users.Add( action );
                tackt.SetHashSet( users );
            } else kuehe.Enqueue( action );
            Launch();
        }

        public override bool Drive( A action )
        {
            if( activ < 4 ) {
                bool contains = users.Contains( action );
                return contains;
            } else return tackt.Contains( action );
        }

        public override void Stopt( A action )
        {
            if (activ < 4) { users.Remove( action );
                tackt.SetHashSet( users );
            } else kuehe.Enqueue( action );
        }
    }
    

    
    public class BarrieredDrive : BaseIntervalDrive<Func<ulong,ulong>,LapFinish<Func<ulong,ulong>>,Func<ulong,ulong>>
    {
        private ulong     m_barrier;
        private Hashtable m_mapping;
        private bool      m_oneshot;
    
        public BarrieredDrive( float interval, bool stopOnBarriersCleared, MotorConfiguration config ) : base(interval,config)
        {
            m_oneshot = stopOnBarriersCleared;
            m_mapping = new Hashtable();
            m_barrier = 0;
        }
    
        public event Action<ulong>     Done;
        public event Func<ulong,ulong> Step {
            add { Start( value ); }
            remove { Stopt( value ); }
        }
    
        public override LapFinish<Func<ulong,ulong>> Tackt { 
            get { while( activ < 4 ) Thread.Sleep(1);
                  return base.Tackt; }
            set { while( activ >= 4 ) Thread.Sleep(1);
                  base.Tackt = value; }
        }
        public override void Start( Func<ulong,ulong> action )
    	{
            ulong reserved = 0;
            foreach( ulong mask in m_mapping.Values )
                reserved |= mask;
            for( int index = 0; index < 64; ++index )
                if( ((reserved >> index) & 1ul) == 0 )
                    m_mapping.Add( action, 1ul << index );
            base.Start( action );
    	}
    
    	public override bool Drive( Func<ulong,ulong> action )
    	{
            while( activ > 8 ) Thread.Sleep(1);
            return m_mapping.ContainsKey( action );
    	}
    
    	protected override void motor()
    	{
    		TimeSpan longWait = new TimeSpan( ticks * 5 );
            TimeSpan shortWait = new TimeSpan( ticks );
            while (activ > 0) { bool events = false;
                DateTime BeginTimeLoop = DateTime.Now;
                if ( kuehe.Count > 0 ) { activ += 2;
                    while ( kuehe.Count > 0) {
                        Func<ulong,ulong> kuh = kuehe.Dequeue();
                        if( !users.Remove(kuh) ) users.Add( kuh );
                        else m_mapping.Remove(kuh);
                    } activ -= 2;
                events = true; }
                int drivers = users.Count;
                if( drivers > 0 ) { activ += 4; int dones = 0;
                    IEnumerator<Func<ulong,ulong>> drvrs = users.GetEnumerator();
                    while( drvrs.MoveNext() ) {
                        activ += 8; 
                        ulong mask = (ulong)m_mapping[drvrs.Current];
                        activ -= 8;
                        if( (m_barrier & mask) == 0 ) {
                            m_barrier = drvrs.Current( m_barrier );
                            if( (m_barrier & mask) != 0 ) ++dones;
                        } else ++dones;
                    } if( dones == drivers ) {
                        Done?.Invoke( m_barrier );
                        if ( m_oneshot ) --activ;
                        else m_barrier = 0;
                    } activ -= 4; events = true;
                } nextLap( drivers );
                if( activ > 0 ) Thread.Sleep( events ?
                    shortWait.Subtract( DateTime.Now - BeginTimeLoop )
                   : longWait.Subtract( DateTime.Now - BeginTimeLoop )
                );
            }
    	}
    }

    public class ControllerDrive 
        : TimedLoop<ControllerBase,ControllerCircuteLap,ControllerBase>
    {
        protected Queue<ControllerBase> waits;

        override public float Speed {
            get { return (float)TimeSpan.TicksPerSecond / ticks; }
            set { ticks = (long)((1.0f/value)*TimeSpan.TicksPerSecond); }
        }

        public override ControllerCircuteLap Tackt {
            get { return tackt; }
            set { if (!value.UseExternals) {
                    users = value.ToHashSet();
                    tackt.SetHashSet( users );
                }
            }
        }

        protected override void motor()
        {
            TimeSpan longWait = new TimeSpan( ticks * 5 );
            TimeSpan shortWait = new TimeSpan( ticks );
            while (activ > 0) { bool events = false;
                DateTime BeginTimeLoop = DateTime.Now;
                if ( waits.Count > 0 ) { activ += 2;
                    while ( waits.Count > 0) {
                        ControllerBase var = waits.Dequeue();
                        if( !users.Remove( var ) ) users.Add( var );
                    } activ -= 2; 
                events = true; }
                int drivers = users.Count;
                if( drivers > 0 ) { activ += 4;
                    IEnumerator<ControllerBase> drvrs = users.GetEnumerator();
                    while( drvrs.MoveNext() ) drvrs.Current.Step();
                    activ -= 4; events = true;
                } nextLap( drivers );
                if( activ > 0 )
                    Thread.Sleep( events
                                ? shortWait - (DateTime.Now - BeginTimeLoop)
                                : longWait -  (DateTime.Now - BeginTimeLoop) );
            } activ = -1;
            users.Clear();
        }

        public ControllerDrive() : base( MotorConfiguration.Task )
        {
            waits = new Queue<ControllerBase>(1);
        }

        public ControllerDrive( float intervalFPS ) : base( MotorConfiguration.Task )
        {
            Speed = intervalFPS;
            waits = new Queue<ControllerBase>(1);
        }

        public ControllerDrive( float intervalFPS, MotorConfiguration config ) : base( config )
        {
            Speed = intervalFPS;
            waits = new Queue<ControllerBase>(1);
        }
    
        public override void Start( ControllerBase controller )
        {
            if( activ < 4 ) {
                users.Add( controller );
                tackt.SetHashSet( users );
            } else waits.Enqueue( controller );
        }
        public override bool Drive( ControllerBase controller )
        {
            if ( activ < 4 )
                 return users.Contains( controller );
            else return tackt.Contains( controller );
        }
        public override void Stopt( ControllerBase controller )
        {
            if( activ < 4 ) {
                users.Remove( controller );
                tackt.SetHashSet( users );
            } else waits.Enqueue( controller );
        }
    }
}
