using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable
{
    public class Glyph
    {
        public string Uid { get; set; } = string.Empty;
        public string Css { get; set; } = string.Empty;
        public int Code { get; set; }
        public string Src { get; set; } = string.Empty;
        public bool? Selected { get; set; }
        public SvgDetails Svg { get; set; } = new();
        public List<string> Search { get; set; } = new();
    }
}
