using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Testing.CSharpTesting
{
    class ConcreteClass4 : ConcreteClass3
    {
        public ConcreteClass4(int a, int b) : base(a,b)
        {

        }

        public new void Func1()
        {
            Debug.Log("kompletno nova implementacija metode Func1(), de-facto sprjecavanje mogucnosti daljnjih override-anja");
        }
    }
}
