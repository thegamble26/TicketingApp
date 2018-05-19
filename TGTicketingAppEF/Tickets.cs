using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGTicketingAppEF
{
    public partial class Ticket
    {
        public void DisplayUsers()
        {
            Console.WriteLine("UserID: {0} - Name: {1}", this.User.UserID, this.User.FirstName + this.User.LastName);
        }
        public void DisplayTickets()
        {
            Console.WriteLine("TicketID: {0} - Summary: {1}", this.TicketID, this.Summary);

            Console.WriteLine("Watching Users:");
            foreach(var u in this.WatchingUsers)
            {
                Console.WriteLine(u.User);
            }
        }
    }
}
