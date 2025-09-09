using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ControlActividades.Models.db
{
    public class tbUsuariosFcmTokens
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TokenId { get; set; }
        [ForeignKey("IdentityUser")]
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string Token {  get; set; }
        
        public virtual ApplicationUser IdentityUser { get; set; }
    }
}
