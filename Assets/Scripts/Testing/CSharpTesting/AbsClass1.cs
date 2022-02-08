using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Testing.CSharpTesting
{
    public abstract class AbsClass1 : Interface1
    {
        public int x { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual void Func1()
        {
            Console.WriteLine("default Func1 implementation");
        }

        public abstract void Func2();

        public virtual void Func3()
        {
            Console.WriteLine("default Func3 implementation");
        }
    }
}
