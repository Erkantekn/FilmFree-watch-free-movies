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
    
    public partial class BildirilenHatalar
    {
        public int id { get; set; }
        public int filmId { get; set; }
        public string Aciklama { get; set; }
        public Nullable<System.DateTime> Tarih { get; set; }
        public bool incelendiMi { get; set; }
    
        public virtual Film Film { get; set; }
    }
}
