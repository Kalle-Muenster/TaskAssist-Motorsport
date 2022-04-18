using System;
using System.Threading.Tasks;


namespace Stepflow.TaskAssist
{
    public interface IActionDriver
    {
        void Init( object assistorinstance );
        IActionDriver controls();
        float Speed { get; set; }

        void Launch();

        void Start<A>( A action );
        bool Drive<A>( A action );
        void Stopt<A>( A action );

        void OnEnded(RaceOver handler);
        bool IsBreak();
        Task Tribune();
    }

    public interface IActionDriver<A,L,T> : IActionDriver where A : class where L : IFinishedLap where T : class
    {
        ITaskAssistor<A,T> assist();
        L Tackt { get; set; }

        void Start( A action );
        bool Drive( A action );
        void Stopt( A action );
    }

    public class DriveAbstractor : IActionDriver
    {
        public virtual void Init( object assistorinstance ) { throw new Exception("must implement"); }
        public virtual float Speed { get; set; }
        public virtual Task Tribune() { return null; }
        public virtual void Launch() { }
        public virtual IActionDriver controls() { return this; }

        void IActionDriver.Start<A>(A action) { }
        bool IActionDriver.Drive<A>(A action) { return false; }
        void IActionDriver.Stopt<A>(A action) { }

        public virtual void OnEnded( RaceOver handler ) { }
        public virtual bool IsBreak() { return false; }
    }

    public class ActionDriver<A,L,T> : DriveAbstractor, IActionDriver<A,L,T> where A : class where L : class, ILapFinish<T>, new() where T : class
    {
        protected L tackt;

        protected ITaskAssistor<A,T> assistor;

        public ITaskAssistor<A,T> assist() { return assistor; }
        public override void Init(object assistorinstance) { assistor = assistorinstance as ITaskAssistor<A,T>; }
        public virtual L Tackt { get; set; }

        public ActionDriver() : base()
        {
            tackt = new L();
        }

        public ActionDriver(int polllrate) : base()
        {
            ((IActionDriver<A,L,T>)this).Speed = (float)((1.0f / polllrate) * TimeSpan.TicksPerSecond);
        }

        public virtual void Start<CallType>(CallType call) { Start(call as A); }
        public virtual bool Drive<CallType>(CallType call) { return Drive(call as A); }
        public virtual void Stopt<CallType>(CallType call) { Stopt(call as A); }

        public virtual void Start(A action) { }
        public virtual bool Drive(A action) { return false; }
        public virtual void Stopt(A action) { }
    }

    /// <summary>
    /// A Driver implementation used to consectuitievely trigger actions of type 'Action' per a configureable interval 
    /// </summary>
    public class SteadyAction : BaseIntervalDrive<Action,LapFinish<Action>,Action>
    {
        private static bool threadding = true;
        public static void SetDefaultThradingMode( bool independant ) { threadding = independant; }

        public SteadyAction() : base( threadding ) {}
        public SteadyAction( float fps ) : base( fps, threadding ) { }
        public SteadyAction( bool independant ) : base( independant ) { }
        public SteadyAction( float fps, bool independant ) : base( fps, independant ) { }
    }

    public class SteadyEvent : BaseIntervalDrive<EventHandler,LapFinish<EventHandler>,EventHandler>
    {
        private static bool threadding = true;
        public static void SetDefaultThradingMode(bool independant) { threadding = independant; }

        public SteadyEvent() : base( threadding ) { }
        public SteadyEvent( float triggerRate ) : base( triggerRate, threadding ) { }
        public SteadyEvent( bool independant ) : base( independant ) { }
        public SteadyEvent( float triggerRate, bool independant ) : base( triggerRate, independant ) { }
    }
}
