using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ContactCenter.Core.Models
{
    public enum BotMainDialog
    {
        Qna,
        CallAgents,
        Departments,
        ComposerDialog
    }
    public enum BotCallAgentsMode
    {
        All,
        LastCalled
    }
    public class BotSettings
    {
        public int Id { get; set; }                                                 // Primary Key
        public int GroupId { get; set; }                                            // FK Group to group
        public virtual ICollection<ChatChannel> ChatChannels { get; }
        public string Name { get; set; }
        public string WelcomePhrase { get; set; }
        public string AskNamePhrase { get; set; }
        public string TransferAgentPhrase { get; set; }
        public BotMainDialog BotMainDialog { get; set; }
        public BotCallAgentsMode BotCallAgentsMode { get; set; }
        public bool EnableProfileDialog { get; set; }
        public bool EnableProfileDialog2 { get; set; }
        public bool EnableQnA { get; set; }
        public bool EnableCheckIntent { get; set; }
        public string QnAKnowledgebaseId { get; set; }
        public string QnAEndpointKey { get; set; }
        public string QnAEndpointHostName { get; set; }
        public string OutOfOfficeHoursPhrase { get; set; }
        public string CustomProfileSettings { get; set; }
        public string ComposerDialogName { get; set; }
        [StringLength(1024)]
        public string DepartmentMenuPhrase { get; set; }

        public void CopyFrom(BotSettings botSettings)
        {
            foreach (PropertyInfo property in typeof(BotSettings).GetProperties().Where(p => p.CanWrite))
            {
                property.SetValue(this, property.GetValue(botSettings, null), null);
            }
        }
    }

    public class CustomProfileSettings
    {
        public int boardId { get; set; }
        [StringLength(256)]
        public string question { get; set; }
        public int fieldId { get; set; }
	}
}
