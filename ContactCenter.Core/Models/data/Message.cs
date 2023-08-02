using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ContactCenter.Core.Models
{
    public enum MessageType
	{
        email,
        whatsapp,
        sms,
        grouplink,
        landing
	}
    public class Message
    {
        public int Id { get; set; }
        public int GroupId { get; set; }                                            // Group Id , foreign Key
        public DateTime? CreatedDate { get; set; }
        [StringLength(256)]
        public string Title { get; set; }
        public string Content { get; set; }
        public Uri FileUri { get; set; }
        public string Html { get; set; }
        public MessageType? MessageType { get; set; }
        public string Thumbnail { get; set; }
        [StringLength(64)]
        public string SmartCode { get; set; }
        public int? SmartIndex { get; set; }
        public virtual ICollection<Sending> Sendings { get; set; }                  // envios que usam esta mensagem
        /*
         * -------------------------------------------------------------------------------
         * Codifica o index da smart page em caracteres
         * -------------------------------------------------------------------------------
         */
        public string CodeIndex(int index)
        {
            if (index < 0)
                return string.Empty;

            string baseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int baseCount = baseChars.Length;
            string code = string.Empty;

            int step = 0;
            int rest = 0;
            int quocient = 0;

            do
            {
                quocient = index / baseCount ^ step;
                rest = index % baseCount ^ step;
                code = baseChars.Substring(rest, 1) + code;

                index = quocient;

            } while (quocient != 0);


            return code;
        }
        /*
         * -------------------------------------------------------------------------------
         * Decodifica o index da smart page
         * -------------------------------------------------------------------------------
         */
        public int UnCodeIndex(string code)
        {
            string baseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            double index = 0;
            int power = code.Length - 1;

            for (int x = 0; x < code.Length; x++)
            {
                int val1 = baseChars.IndexOf(code.Substring(x, 1));
                index += val1 * Math.Pow(baseChars.Length, power);
                power--;
            }


            return Convert.ToInt32(index);
        }

    }
}

