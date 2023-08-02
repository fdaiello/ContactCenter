using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ContactCenter.Core.Models
{
    public class LandingDto
    {
        public LandingDto( Landing landing)
        {
            if (landing != null)
            {
                foreach (PropertyInfo property in typeof(LandingDto).GetProperties())
                {
                    var x = landing.GetType().GetProperty(property.Name).GetValue(landing, null);
                    property.SetValue(this, x, null);
                }
            }
        }
        public int Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Title { get; set; }
        public string Html { get; set; }
        public string JsonContent { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Code { get; set; }
        public int? Index { get; set; }
        public int PageViews { get; set; }
        public int Leads { get; set; }
        public int? BoardId { get; set; }
        public string EmailAlert { get; set; }
        public Uri RedirUri { get; set; }
        public virtual Board Board { get; set; }

    }
}

