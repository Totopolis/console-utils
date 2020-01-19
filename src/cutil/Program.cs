using ConsoleHelpers;

namespace cutil
{
    class Program
    {
        static void Main(string[] args)
        {
            "Hello greed World!".OutGreen();
            "Hello white World!".Out();
            "Hello red World!".OutRed();

            var config = ConfigUtils.LoadConfig("sample.yaml");
            string sval = config.start;
            double dval = config.pi;
            int ival = config.french_hens;
        }
    }
}
