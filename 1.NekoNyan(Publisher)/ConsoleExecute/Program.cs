using System;
using NekoNyanStatic.Crypto.V1;

namespace ConsoleExecute
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetBufferSize(1280, 720);

            foreach(var pkgPath in args)
            {

                if (ArchiveCrypto.IsVaildPackage(pkgPath))
                {
                    ArchiveCrypto crypto = new(pkgPath);
                    crypto.Extract();
                    crypto.Dispose();

                    Console.WriteLine(string.Concat(pkgPath, "  Extract Success"));
                }
                else
                {
                    Console.WriteLine(string.Concat(pkgPath, "  Invaild Package"));
                }
            }
            Console.WriteLine("Press Any Key To Exit");
            Console.Read();
        }
    }
}
