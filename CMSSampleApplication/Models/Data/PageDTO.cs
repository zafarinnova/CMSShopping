using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CMSSampleApplication.Models.Data
{
    [Table("tblPages")]
    public class PageDTO
    {
        
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength =3)]
        public string Title { get; set; }
        public string Slug { get; set; }
        [Required]
        [StringLength(int.MaxValue)]
        public string Body { get; set; }
        public int Sorting { get; set; }
        public bool HasSideBar { get; set; }

    }
}