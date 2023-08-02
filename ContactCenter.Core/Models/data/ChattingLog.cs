using System;
using System.ComponentModel.DataAnnotations;

namespace ContactCenter.Core.Models
{
    public enum MessageSource           // Indicates which role sent message: customer, bot, agent
    {
        Bot,
        Contact,
        Agent,
        A2A                             //agent <-> agent
    }
    public enum ChatMsgType             // Type of Message
    {
        Text,                           // Text
        Voice,                          // Voce - audio Ogg/Wav
        Image,                          // Images - Gif, Jpg, Png
        PDF,                            // Docs - pdf
        Word,                           // Docs - doc, docx
        Excel,                          // Excel - xls,xlsx,csv
        File,                           // Other files
        Location,                       // Geografic Location
        Contacts,						// Contact Card
        Vcard,                          // VCard
        TextSms,                        // Text with reduced caracter code - Gsm code page only
        Video,
        Email
    }
    public enum MsgStatus
    {
        None,
        Enqueued,
        Failed,
        Sent,
        Delivered,
        Read
    }
    public class ChattingLog
    {
        public long Id { get; set; }                            // Primary Key
        public string ChatChannelId { get; set; }               // Foreing Key to Channel.Id
        public ChatMsgType Type { get; set; }                   // Type of Message
        public string Text { get; set; }                        // Text - in case type is text
        public string Filename { get; set; }                    // Filename - in case type is voice, image or other file format
        public MessageSource Source { get; set; }               // Indicates which role sent message: user / bot / agent
        public bool Read { get; set; }                          // Indicates if the message was read; Only used for messages destinated to Agents.
        public DateTime Time { get; set; }                      // Date and time message was sent
        public string ContactId { get; set; }                   // User who sent or receved the message
        public virtual Contact Contact { get; set; }            // Contact Descritor
        public string ApplicationUserId { get; set; }           // In case message was sent by Agent, Agent ID is saved here
        public string ToAgentId { get; set; }                   // agent to agent 
        [StringLength(200)]
        public string ActivityId { get; set; }                  // ActivityID
        public MsgStatus Status { get; set; }                   // None, Enqueued, Failed, Sent, Delivered, Read
        public DateTime StatusTime { get; set; }				// Time when status was last update
        public int GroupId { get; set; }                        // Group
        public string QuotedActivityId { get; set; }            // When a message is "quoted" ( cited ), this shows the ActivityID of the message beeing quoted
        public bool IsHsm { get; set; }                         // Tru when sending a HSM ( template ) WhatsApp Message
        public string FailedReason { get; set; }                // When getting a 'failed' status, error is saved here
        public int? SendingId { get; set; }                     // Quando a mensagem foi enviada automaticamente, indica o Id do envio - Sending. Nullable.
    }
}
