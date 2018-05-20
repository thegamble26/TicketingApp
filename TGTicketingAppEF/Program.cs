using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.IO;
using System.Data.SqlClient;
using System.Data.Entity;

namespace TGTicketingAppEF
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        string MyValue { get; set;}
        static void Main(string[] args)
        {
            DisplayMenu(true);
            string MSelection = Console.ReadLine().Trim();
            string LSelection = "";

            // connect to DB
            logger.Debug("Starting application");

            do
            {
                // User search removed to Notepad for easier look at Ticket
                if (MSelection == "2") // Tickets
                {
                    TicketMenu();
                    LSelection = Console.ReadLine().Trim();
                    do
                    {
                        if (LSelection == "1") // Display
                        {
                            Console.WriteLine("You chose Display Tickets");
                            int tCount = 0;

                            Console.WriteLine("Enter ticket summary information to search");
                            string entry = Console.ReadLine();

                            using (var dbContext = new TicketContext())
                            {
                                var results = dbContext.Tickets
                                    .Include(x => x.WatchingUsers.Select(u => u.User))
                                    .Include(t => t.TicketType)
                                    .Where(d => d.Summary.Contains(entry))
                                    .ToList();

                                foreach (var record in results)
                                {
                                    tCount++;

                                    if (tCount % 20 == 0)
                                    {
                                        Console.WriteLine("Display more records? y/n");
                                        string continueDisp = Console.ReadLine();
                                        if (continueDisp.ToUpper() == "N")
                                        {
                                            break;
                                        }
                                    }
                                    record.DisplayTickets();
                                }
                            }
                            logger.Info("{0} was search, {1} returned", entry, tCount);
                            Console.WriteLine();
                            Console.WriteLine("-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*");
                            Console.WriteLine();
                            break;
                        }
                        else if (LSelection == "2") // Add
                        {
                            Console.WriteLine("Enter a summary");
                            var sum = Console.ReadLine();
                            Console.WriteLine("Enter priority level");
                            var priority = Console.ReadLine();
                            Console.WriteLine("Enter the status of the ticket");
                            var stats = Console.ReadLine();

                            using (var dbContext = new TicketContext())
                            {
                                var record = new Ticket();

                                record.Summary = sum;
                                record.Priority = priority;
                                record.Status = stats;

                                TicketType typeCheck = null;

                                do
                                {
                                    Console.WriteLine("Enter the ticket type");
                                    var ttype = Console.ReadLine();

                                    typeCheck = dbContext.TicketTypes.Where(t => t.Description == ttype).SingleOrDefault();
                                } while (typeCheck == null);

                                record.TicketType = typeCheck;

                                Console.WriteLine("Enter the user ID of the submitter");
                                var sUser = Console.ReadLine();
                                int subUser = 0;
                                while(!int.TryParse(sUser, out subUser))
                                {
                                    Console.WriteLine("Enter the numerical user ID of the submitter");
                                    sUser = Console.ReadLine();
                                }
                                var user = dbContext.Users.Where(u => u.UserID == subUser).FirstOrDefault();
                                if (user != null)
                                {
                                    User u = new User();
                                    u.UserID = subUser;
                                    record.Submitter = u.UserID;
                                }
                                string addUser = "";
                                do
                                {
                                    Console.WriteLine("Enter a user ID - when finished enter 'end'");
                                    addUser = Console.ReadLine();

                                    if (addUser != "end")
                                    {
                                        int aUser = Convert.ToInt32(addUser);
                                        user = dbContext.Users.Where(u => u.UserID == aUser).FirstOrDefault();

                                        if (user != null)
                                        {
                                            User u = new User();
                                            u.UserID = aUser;
                                            record.Assigned = u.UserID;
                                            record.WatchingUsers.Add(new WatchingUser { UserID = u.UserID }); // this should add an assigned user, creating a new WatchingUser item
                                        }
                                    }
                                } while (addUser != "end");

                                dbContext.Tickets.Add(record);
                                dbContext.SaveChanges();
                                logger.Info("Ticket {0} was created", record.TicketID);
                            }

                            Console.WriteLine();
                            Console.WriteLine("-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*");
                            Console.WriteLine();
                            break;
                        }
                        else if (LSelection == "3") // Update
                        { }
                        else if (LSelection == "4") // Exit
                        {
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Bad Input");
                            break;
                        }
                    } while (LSelection != "4");
                }
                else if (MSelection == "3") // Exit
                {
                    return;
                }
                else
                {
                    Console.WriteLine("Bad Input");
                    break;
                }
                DisplayMenu(false);
                MSelection = Console.ReadLine().Trim();
            } while (MSelection != "3");
            return;
        }

        public static void DisplayMenu(bool First)
        {
            if (First)
            {
                Console.WriteLine("Welcome to the User and Ticketing System");
            }
            Console.WriteLine("Select what you would like to do");
            //Console.WriteLine("1) Users");
            Console.WriteLine("2) Tickets");
            Console.WriteLine("3) Exit Application");
        }

        public static void UserMenu()
        {
            Console.WriteLine("1) Display User Information");
            Console.WriteLine("2) Add New User");
            Console.WriteLine("3) Update User Information");
            Console.WriteLine("4) Exit Application");
        }

        public static void TicketMenu()
        {
            Console.WriteLine("1) Display Tickets");
            Console.WriteLine("2) Add New Ticket");
            Console.WriteLine("3) Update Existing Ticket");
            Console.WriteLine("4) Exit Application");
        }
    }
 }
