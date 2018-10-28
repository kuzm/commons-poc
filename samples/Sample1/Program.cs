using System;
using OneOf;

namespace Sample1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Check dependencies
            
            new Library1.Class1();
            new Library2.Class1();

            new OneOf<Library1.Class1, Library2.Class1>();
        }
    }
}
