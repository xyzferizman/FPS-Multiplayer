using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Testing.CSharpTesting
{
    internal class ConcreteClass2 : ConcreteClass1
    {
        new int someVariable = 4;
        int _a;
        string someStr;

        protected override string Name { 
            get
            {
                return "string";
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    someStr = value;
                }
                else
                {
                    someStr = "Missing_Value";
                }
            }
        }

        public ConcreteClass2(int a, int b) : base(a)
        {
            someVariable = b;
        }

        public override void Func1()
        {
            base.Func1();
            Debug.Log("drugi override bazne metode Func1");
        }

        public void Ispis() // zasto se ne buni za public metodu u internal klasi ?
        {
            Debug.Log(someVariable);
        }
    }
}
