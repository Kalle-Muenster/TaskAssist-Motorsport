using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
/*
namespace Stepflow.Elements
{

    public interface IElement<E> : IElement where E : IElement
    {
        E element();
    }

    public class Element : IElement<Element>
    {
        private Element attached;
        public virtual Element Init(Element attach)
        {
            attached = attach;
            return this;
        }

        public virtual Element Init( Element attach, ITuple param )
        {
            return Init(attach);
        }

        public E Add<E>() where E : Element
        {
            throw new NotImplementedException();
        }

        public E Add<B, E>()
            where B : Element
            where E : B
        {
            throw new NotImplementedException();
        }

        public E Add<E>( ITuple arg ) where E : Element
        {
            throw new NotImplementedException();
        }

        public E Add<B, E>( ITuple arg )
            where B : Element
            where E : B
        {
            throw new NotImplementedException();
        }

        public Element cluster()
        {
            Element find = this;
            while( find != find.attached ) {
                   find = find.attached;
            } return find;
        }

        public Element element()
        {
            throw new NotImplementedException();
        }

        public T Elm<T>()
        {
            throw new NotImplementedException();
        }

        public T Elm<T>( int idx )
        {
            throw new NotImplementedException();
        }

        public E Get<E>() where E : Element
        {
            throw new NotImplementedException();
        }

        public E Get<B, E>()
            where B : Element
            where E : B
        {
            throw new NotImplementedException();
        }

        public E Get<E>( int idx ) where E : Element
        {
            throw new NotImplementedException();
        }

        public E Get<B, E>( int idx )
            where B : Element
            where E : B
        {
            throw new NotImplementedException();
        }

        public E Rem<E>() where E : Element
        {
            throw new NotImplementedException();
        }

        public E Rem<E>( int idx ) where E : Element
        {
            throw new NotImplementedException();
        }
    }

    public class Elementse : Element, IElement<Elementse>
    {
        public Elementse element() { return this; }
    }

    public interface IElement
    {
        Element cluster();

        E Get<E>() where E : Element;
        E Get<B,E>() where B : Element where E : B;
        E Get<E>(int idx) where E : Element;
        E Get<B,E>(int idx) where B : Element where E : B;

        E Add<E>() where E : Element;
        E Add<B,E>() where B : Element where E : B;
        E Add<E>( ITuple arg ) where E : Element;
        E Add<B,E>( ITuple arg ) where B : Element where E : B;

        T Elm<T>();
        T Elm<T>( int idx );

        E Rem<E>() where E : Element;
        E Rem<E>(int idx) where E : Element;
    }

    public interface IElmPtr
    {
        IntPtr pointer(); 
    }
    public interface IElementar<T> : IElmPtr
    {
        T entity { get; set; }
    }

    public class ElmPointer<T> : IElementar<T>
    {
        private IntPtr ptr;
        public T entity {
            get { unsafe { return *(T*)pointer(); } }
            set { unsafe { *(T*)pointer() = value; } }
        }
        public IntPtr pointer()
        {
            return ptr;
        }
    }

    public interface ICluster : IElement
    {
        ICluster Cluster { get; }
        ICluster Add<E>() where E : Element;
        E Get<E>() where E : Element;
        E Get<E>( int idx ) where E : Element;
        E Get<E>( Enum id ) where E : Element;
        E Find<E>( string nam ) where E : Element;
        T Elm<T>(  );
        IElmPtr<T> Ptr<T>();
    }
}
*/