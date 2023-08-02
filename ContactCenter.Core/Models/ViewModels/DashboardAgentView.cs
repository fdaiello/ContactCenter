using ContactCenter.Core.Models;

namespace ContactCenter.Core.Models
{
    public class DashboardAgentView
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string PictureFile { get; set; }
        public int Msg_rec { get; set; }
        public int Msg_env { get; set; }
        public int Contacts { get; set; }
        public int Cards { get; set; }
    }
}
