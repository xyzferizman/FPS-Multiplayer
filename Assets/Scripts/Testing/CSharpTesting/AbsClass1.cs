using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Testing.CSharpTesting
{
    public abstract class AbsClass1 : Interface1
    {
        public int x { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual void Func1()
        {
            Debug.Log("default Func1 implementation");
        }

        public abstract void Func2();

        public virtual void Func3()
        {
            Debug.Log("default Func3 implementation");
        }
    }
}
