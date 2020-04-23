using System.ComponentModel.DataAnnotations;
using System;
namespace WeddingPlanner.Models
{
    public class Guest
    {
        [Key]
        public int GuestId {get; set;}
        public int UserId {get; set;}
        public User Attendee {get; set;}
        public int WeddingId {get; set;}
        public Wedding RSVP {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
    }
}