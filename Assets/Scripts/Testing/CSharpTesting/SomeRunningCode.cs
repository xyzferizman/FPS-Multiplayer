using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Testing.CSharpTesting
{
    static class SomeRunningCode
    {
        internal static void ExecuteCode()
        {
            ConcreteClass4 cc4 = new ConcreteClass4(1,2);
            cc4.Func1();    // verzija Func1() iz ConcreteClass3 

            ConcreteClass3 cc3 = new ConcreteClass3(1, 2);
            cc3.Func1();    // verzija Func1() iz ConcreteClass3 

            ConcreteClass2 cc2 = new ConcreteClass2(1,2);
            cc2.Func1();    // verzija Func1() iz ConcreteClass2 + bazni poziv iz ConcreteClass1 + bazni poziv iz AbsClass1

            AbsClass1 ac1 = (AbsClass1)cc2;
            ac1.Func1();    // bazna implementacija metode Func1()
        }
    }
}
