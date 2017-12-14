using System;
using System.Threading.Tasks;
using ICTBroadcast;

namespace c_sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            BroadcastApi apiClient = new BroadcastApi("Campaign_Contact_Import");
            apiClient.add_parameter("campaign_id", "13");
            apiClient.add_parameter("type", "file");
            apiClient.set_attachment("source_file", "contact_sample.csv");
            Task<String> result = apiClient.post();
            Console.WriteLine(result.Result);
        }
    }
}