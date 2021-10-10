using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UserWebAPI
{
    [Table("Client", Schema = "dbo")]
    public class Client
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName ="nvarchar(50)")]
        public string Name { get; set; }
        public DateTime? LastAccessed { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
