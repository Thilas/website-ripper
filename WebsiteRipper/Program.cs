using WebsiteRipper.CommandLine;

namespace WebsiteRipper
{
    static class Program
    {
        static int Main(string[] args)
        {
            //args = new[] { "help", "rip" };
            //args = new[] { "rip", "http://www.centralenum.org/galerie-publique/2014-2015/Raid/Photos Diapo/J3/page/4/", "-o", "rip", "-t", "5", "--maxDepth", "1", "-l", "fr,en" };
            //args = new[] { "rip", "view-source:http://www.inductive.com/MyNote.xml", "-o", "rip", "-t", "5", "-d1" };
            //args = new[] { "update" };
            return Verb.Process(args);
        }
    }
}
