using System;
using WebsiteRipper.CommandLine;

namespace WebsiteRipper
{
    static class Program
    {
        static int Main(string[] args)
        {
            //args = new[] { "help", "rip" };
            args = new[] { "rip", "http://www.centralenum.org/galerie-publique/2014-2015/Raid/Photos%20Diapo/J3/page/4/", "-o", "rip", "-t", "5", "--maxDepth", "1", "-l", "fr" };
            //args = new[] { "update" };
            return Verb.Process(args);
        }
    }
}
