namespace WebsiteRipper.Helpers
{
    static class Helper
    {
        public static int CombineHashCodes(int hashCode1, int hashCode2)
        {
            return (hashCode1 << 5) + hashCode1 ^ hashCode2;
        }
    }
}
