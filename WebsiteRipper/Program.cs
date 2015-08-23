using WebsiteRipper.CommandLine;

namespace WebsiteRipper
{
    static class Program
    {
        static int Main(string[] args)
        {
#if (DEBUG)
            //args = new[] { "help" };
            //args = new[] { "help", "create" };
            //args = new[] { "create", "http://www.centralenum.org/galerie-publique/2014-2015/Raid/Photos Diapo/J3/page/4/", "-o", "website", "-t5", "--max-depth", "1", "-l", "fr,en" };
            //args = new[] { "create", "http://www.w3.org/TR/2014/REC-html5-20141028/", "-o", "website", "-t5", "--max-depth", "1" };
            //args = new[] { "create", "http://www.w3.org/TR/2008/REC-xml-20081126/REC-xml-20081126.xml", "-o", "website", "--max-depth", "1" };
            //args = new[] { "update-default-extensions" };
            //args = new[] { "update-default-extensions", "--skip-iana" };
#endif
            return Verb.Process(args);
        }
    }
}
