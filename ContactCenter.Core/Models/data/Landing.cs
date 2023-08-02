using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ContactCenter.Core.Models
{
    public class Landing
    {
        public int Id { get; set; }
        public int GroupId { get; set; }                                            // Group Id , foreign Key
        public DateTime? CreatedDate { get; set; }
        [StringLength(256)]
        public string Title { get; set; }
        public string Html { get; set; }
        public string JsonContent { get; set; }
        public string ThumbnailUrl { get; set; }
        [StringLength(64)]
        public string Code { get; set; }
        public int? Index { get; set; }
        public int PageViews { get; set; }
        public int Leads { get; set; }
        public int? BoardId { get; set; }
        public Uri RedirUri { get; set; }
        [StringLength(256)]
        public string EmailAlert { get; set; }
        public virtual Board Board { get; set; }
        /*
         * -------------------------------------------------------------------------------
         * Codifica o index da Landing page em caracteres
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

        // Used by API to clone contact
        public void CopyFrom(Landing landing)
        {
            foreach (PropertyInfo property in typeof(Landing).GetProperties().Where(p => p.CanWrite))
            {
                property.SetValue(this, property.GetValue(landing, null), null);
            }
        }


    }
}

