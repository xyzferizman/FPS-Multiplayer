using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Testing.CSharpTesting
{
    internal class ConcreteClass3 : ConcreteClass2
    {
        public ConcreteClass3(int a, int b) : base(a, b)
        {
            
        }

        public new void Func1()
        {
            Debug.Log("'new' implementacija Func1(), moze li se naslijediti? posto ova varijanta nema 'virtual' keyword kod sebe");
        }
    }
}
