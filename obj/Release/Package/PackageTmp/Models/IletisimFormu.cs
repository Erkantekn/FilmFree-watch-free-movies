//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FilmFree.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class IletisimFormu
    {
        public int id { get; set; }
        public string Ad { get; set; }
        public string Mail { get; set; }
        public string Konu { get; set; }
        public string Mesaj { get; set; }
        public System.DateTime Tarih { get; set; }
        public bool OkunduMu { get; set; }
    }
}
