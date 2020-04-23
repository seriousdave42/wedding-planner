using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]
        public int WeddingId {get; set;}
        [Required]
        public string Wedder1 {get; set;}
        [Required]
        public string Wedder2 {get; set;}
        [Required]
        [FutureDate]
        public DateTime Date {get; set;}
        [Required]
        public string Address {get; set;}
        public int UserId {get; set;}
        public User Planner {get; set;}
        public List<Guest> Attendees {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpatedAt {get; set;} = DateTime.Now;
    }
}