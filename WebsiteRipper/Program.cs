using WebsiteRipper.CommandLine;

namespace WebsiteRipper
{
    static class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            //args = new[] { "help", "rip" };
            //args = new[] { "rip", "http://www.centralenum.org/galerie-publique/2014-2015/Raid/Photos Diapo/J3/page/4/", "-o", "rip", "-t5", "--max-depth", "1", "-l", "fr,en" };
            //args = new[] { "rip", "http://www.w3.org/TR/2014/REC-html5-20141028/", "-o", "rip", "-t5", "--max-depth", "1" };
            //args = new[] { "rip", "http://www.w3.org/TR/2008/REC-xml-20081126/REC-xml-20081126.xml", "-o", "rip", "--max-depth", "1" };
            //args = new[] { "update" };
            //args = new[] { "update", "--skip-iana" };
#endif
            return Verb.Process(args);
        }
    }
}
