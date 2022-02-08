using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Testing.CSharpTesting
{
    class ConcreteClass1 : AbsDerivedClass1
    {
        protected int someVariable = 2;

        protected virtual string Name { get; set; }

        public ConcreteClass1()
        {

        }

        public ConcreteClass1(int a)
        {
            // konstruktor konkretne klase 1
        }

        private protected override void AbstraktnaMetoda()
        {
            throw new NotImplementedException();
        }

        public override void Func1()
        {
            base.Func1();
        }

        public override void Func2()
        {
            throw new NotImplementedException();
        }
    }
}
