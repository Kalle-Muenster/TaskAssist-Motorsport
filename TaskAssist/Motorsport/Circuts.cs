using System;
using System.Collections.Generic;


namespace Stepflow.TaskAssist
{
    public interface IFinishedLap
    {
        void SetHashSet<A>(HashSet<A> set);
        HashSet<A> ToHashSet<A>();
        bool Contains<A>(A resgistered) where A : class;
        IFinishedLap lap();
        void Invoke();
        void setAddAndRemove<A>(Action<A> AddFunc, Action<A> RemFunc);
        bool UseExternals { get; }
    }

    public class LapAbstractor : IFinishedLap
    {
        protected Delegate externAdd;
        protected Delegate externRem;

        public IFinishedLap lap() { return this; }

        public LapAbstractor()
        {
            externAdd = externRem = null;
        }
        public LapAbstractor(LapAbstractor copy)
        {
            externAdd = copy.externAdd;
            externRem = copy.externRem;
        }

        public bool UseExternals
        { get { return externAdd != null && externRem != null; } }

        virtual public void setAddAndRemove<A>( Action<A> AddFunc, Action<A> RemFunc )
        { externAdd = AddFunc; externRem = RemFunc; }

        virtual public bool Contains<A>( A resgistered ) where A : class
        { throw new Exception("must implement override"); }

        void IFinishedLap.Invoke()
        { throw new Exception("must implement override"); }

        virtual public void SetHashSet<A>( HashSet<A> set )
        { throw new Exception("must implement override"); }

        virtual public HashSet<A> ToHashSet<A>()
        { throw new Exception("must implement override"); }
    }

    public interface ILapFinish<A> : IFinishedLap where A : class
    {
        void setAddAndRemove( Action<A> AddFunc, Action<A> RemFunc );
        ILapFinish<A> Lap();
    }

    public class LapFinish<A> : LapAbstractor, ILapFinish<A> where A : class
    {
        private Delegate cylinders;

        public LapFinish() : base()
        {
            cylinders = null;
        }
        public LapFinish( Delegate theFirst ) : base()
        {
            cylinders = theFirst;
            return;
        }
        public LapFinish( LapFinish<A> copy )
        {
            externAdd = copy.externAdd;
            externRem = copy.externRem;
            cylinders = copy.cylinders;
        }

        public static LapFinish<A> FromHashSet( HashSet<A> fromSet )
        {
            LapFinish<A> create = new LapFinish<A>( (Delegate)null );
            create.SetHashSet( fromSet );
            return create;
        }

        public override void SetHashSet<T>(HashSet<T> set)
        {
            cylinders = null;
            if( set.Count > 0 ) {
                IEnumerator<T> combo = set.GetEnumerator();
                combo.MoveNext();
                cylinders = combo.Current as Delegate;
                while( combo.MoveNext() ) cylinders =
                        Delegate.Combine( cylinders,
                           combo.Current as Delegate );
            }
        }

        public override HashSet<T> ToHashSet<T>()
        {
            if( cylinders == null ) return null;
            else return new HashSet<T>( cylinders.GetInvocationList() as T[] );
        }

        public static implicit operator Delegate(LapFinish<A> cast)
        {
            return cast.cylinders;
        }

        public static explicit operator LapFinish<A>(Delegate cast)
        {
            return new LapFinish<A>(cast);
        }

        public Delegate this[int idx]
        {
            get { return cylinders.GetInvocationList()[idx]; }
        }

        public override bool Contains<T>(T resgistered)
        {
            if (cylinders == null) return false;
            Delegate[] all = cylinders.GetInvocationList();
            for (int i = 0; i < all.Length; ++i)
                if (all[i] == (resgistered as Delegate))
                    return true;
            return false;
        }

        public static LapFinish<A> operator +(LapFinish<A> This, Delegate That)
        {
            if ( This.lap().UseExternals )
                 This.externAdd.DynamicInvoke(new object[] { That });
            if ( This.cylinders == null )
                 This.cylinders = That;
            else This.cylinders = Delegate.Combine( This.cylinders, That );
            return This;
        }

        public static LapFinish<A> operator -(LapFinish<A> This, Delegate That)
        {
            if( This.lap().UseExternals )
                This.externRem.DynamicInvoke(new object[] { That });
            if( This.cylinders != null ) {
                HashSet<Delegate> list = new HashSet<Delegate>( This.cylinders.GetInvocationList() );
                if( list.Contains(That) ) {
                    list.Remove(That);
                    if( list.Count == 0 ) {
                        This.cylinders = null;
                    } else {
                        IEnumerator<Delegate> add = list.GetEnumerator();
                        while( add.MoveNext() ) {
                            This.cylinders = Delegate.Combine( This.cylinders, add.Current );
                        }
                    }
                }
            } return This;
        }

        public ILapFinish<A> Lap() { return this; }

        void IFinishedLap.Invoke()
        {
            cylinders.DynamicInvoke();
        }

        public override void setAddAndRemove<T>(Action<T> AddFunc, Action<T> RemFunc)
        {
            if (typeof(T) == typeof(A)) {
                externAdd = AddFunc;
                externRem = RemFunc;
            } else throw new ArgumentException(
                string.Format("functions of type 'Action<{0}>' expected", typeof(A).Name)
            );
        }
        public void setAddAndRemove( Action<A> AddFunc, Action<A> RemFunc )
        {
            externAdd = AddFunc;
            externRem = RemFunc;
        }
    }

    public class ControllerCircuteLap : LapAbstractor, ILapFinish<ControllerBase>
    {

        private HashSet<ControllerBase> cylinders;

        public ControllerCircuteLap() : base()
        {
            cylinders = new HashSet<ControllerBase>();
        }

        public ControllerCircuteLap( ControllerCircuteLap copy )
        {
            externAdd = copy.externAdd;
            externRem = copy.externRem;
            cylinders = copy.cylinders;
        }

        public ControllerCircuteLap( ControllerBase from ) : base()
        {
            cylinders = new HashSet<ControllerBase>{from};
        }

        public override bool Contains<A>( A registered )
        {
            return cylinders.Contains( registered as ControllerBase );
        }

        public bool Contains( ControllerBase registered )
        {
            return cylinders.Contains( registered );
        }

        public void Invoke()
        {
            foreach( ControllerBase controller in cylinders ) controller.Step();
        }

        public ILapFinish<ControllerBase> Lap()
        {
            return this;
        }

        public void setAddAndRemove( Action<ControllerBase> AddFunc, Action<ControllerBase> RemFunc )
        {
            externAdd = AddFunc;
            externRem = RemFunc;
        }

        public override void setAddAndRemove<A>( Action<A> AddFunc, Action<A> RemFunc )
        {
            if( AddFunc.GetType().GetGenericArguments()[0].IsSubclassOf(typeof(ControllerBase)) ) {
                externAdd = AddFunc; externRem = RemFunc;
            } else throw new Exception("expected delegate type: 'Action<ControllerBase>'");
        }

        void IFinishedLap.SetHashSet<A>( HashSet<A> set )
        {
            cylinders.Clear();
            cylinders = set as HashSet<ControllerBase>;
        }

        HashSet<A> IFinishedLap.ToHashSet<A>()
        {
            if (typeof(A) == typeof(ControllerBase))
                return cylinders as HashSet<A>;
            else throw new ArrayTypeMismatchException(
                string.Format( "HashSet elements of wrong type {0}", typeof(A).Name )
            );
        }

        public void SetHashSet( HashSet<ControllerBase> set )
        {
            cylinders.Clear();
            cylinders = set;
        }

        public HashSet<ControllerBase> ToHashSet()
        {
            return cylinders;
        }

        public static implicit operator Action( ControllerCircuteLap cast )
        {
            return cast.Invoke;
        }

        public static explicit operator ControllerCircuteLap( ControllerBase cast )
        {
            return new ControllerCircuteLap( cast );
        }

        public ControllerBase this[int idx]
        {
            get { if( cylinders.Count > idx ) {
                    IEnumerator<ControllerBase> itr = cylinders.GetEnumerator();
                    do itr.MoveNext(); while (--idx >= 0);
                    return itr.Current;
                } else throw new IndexOutOfRangeException(
                  "just " + cylinders.Count + "triggers are registered"
              );
            }
        }

        public static ControllerCircuteLap operator +(ControllerCircuteLap This, ControllerBase That)
        {
            if (This.lap().UseExternals)
                This.externAdd.DynamicInvoke(new object[] { That });
            if(!This.cylinders.Contains(That))
                This.cylinders.Add(That);
            return This;
        }

        public static ControllerCircuteLap operator -(ControllerCircuteLap This, ControllerBase That)
        {
            if (This.lap().UseExternals)
                This.externRem.DynamicInvoke(new object[] { That });
            if (This.cylinders.Contains(That))
                This.cylinders.Remove(That);
            return This;
        }
    }
}
