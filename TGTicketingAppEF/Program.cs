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

            logger.Debug("Starting application");

            do
            {
                // User search removed to Notepad for easier look at Ticket
                if (MSelection == "2") // Tickets
                {
                    logger.Debug("Ticket Records Accessed)");

                    TicketMenu();
                    LSelection = Console.ReadLine().Trim();
                    do
                    {
                        if (LSelection == "1") // Display Ticket Records
                        {
                            logger.Debug("Searching Ticket Records");

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
                            logger.Debug("New Ticket Record");

                            Console.WriteLine("Enter a summary");
                            var sum = Console.ReadLine();
                            var priority = "";
                            string stats;
                            var valid = false;
                            do
                            {
                                TicketPriority();
                                priority = Console.ReadLine();
                                if (priority.Trim() == "1")
                                {
                                    priority = "Low";
                                    logger.Debug("Priority Added");
                                    valid = true;
                                }
                                else if (priority.Trim() == "2")
                                {
                                    priority = "Medium";
                                    logger.Debug("Priority Added");
                                    valid = true;
                                }
                                else if (priority.Trim() == "3")
                                {
                                    priority = "High";
                                    logger.Debug("Priority Added");
                                    valid = true;
                                }
                                else
                                {
                                    Console.WriteLine("Bad Input");
                                    break;
                                }
                            } while (valid == false);

                            do
                            {
                                TicketStatus();
                                stats = Console.ReadLine();

                                if (stats.Trim() == "1")
                                {
                                    stats = "Open";
                                    logger.Debug("Status Added");
                                    valid = true;
                                }
                                else if (stats.Trim() == "2")
                                {
                                    stats = "Closed";
                                    logger.Debug("Status Added");
                                    valid = true;
                                }
                                else
                                {
                                    Console.WriteLine("Bad input");
                                    break;
                                }
                            } while (valid == false);

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

                                // validate userID
                                int subUser = 0;
                                while (!int.TryParse(sUser, out subUser))
                                {
                                    Console.WriteLine("Enter the numerical user ID");
                                    sUser = Console.ReadLine();
                                    logger.Debug("Invalid UserID");
                                }
                                var user = dbContext.Users.Where(u => u.UserID == subUser).FirstOrDefault();
                                if (user != null)
                                {
                                    User u = new User();
                                    u.UserID = subUser;

                                    // add an submitter and create WatchingUser item
                                    record.Submitter = u.UserID;
                                    record.WatchingUsers.Add(new WatchingUser { UserID = u.UserID });
                                }
                                string addUser = "";
                                do
                                {
                                    Console.WriteLine("Enter a user ID - when finished enter 'end'");
                                    addUser = Console.ReadLine();

                                    if (addUser != "end")
                                    {
                                        int aUser = 0;

                                        // validate userID
                                        while (!int.TryParse(addUser, out aUser))
                                        {
                                            Console.WriteLine("Enter the numerical user ID of the submitter");
                                            addUser = Console.ReadLine();
                                            logger.Debug("Invalid UserID");
                                        }

                                        user = dbContext.Users.Where(u => u.UserID == aUser).FirstOrDefault();

                                        if (user != null)
                                        {
                                            User u = new User();
                                            u.UserID = aUser;

                                            // add an assigned user and create a new WatchingUser item
                                            record.Assigned = u.UserID;
                                            record.WatchingUsers.Add(new WatchingUser { UserID = u.UserID }); 
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
                        {
                            logger.Debug("Updating Ticket Record");
                            Console.WriteLine("Enter search term for ticket to update");
                            var entry = Console.ReadLine();
                            using (var dbContext = new TicketContext())
                            {
                                var results = dbContext.Tickets
                                .Include(x => x.WatchingUsers.Select(u => u.User))
                                .Include(t => t.TicketType)
                                .Where(d => d.Summary.Contains(entry))
                                .ToList();

                                int tCount = 0;
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

                                //ensure TicketID in db
                                var valid = false;
                                Console.WriteLine("Enter the TicketID of the ticket to update");
                                string tID = Console.ReadLine();
                                int tIDint;
                                do
                                {
                                    while (!int.TryParse(tID, out tIDint))
                                    {
                                        Console.WriteLine("Enter the numerical TicketID of the ticket to update");
                                        tID = Console.ReadLine();
                                        logger.Debug("Invalid data type TicketID");
                                    }
                                    var ticket = dbContext.Tickets.Where(t => t.TicketID == tIDint).FirstOrDefault();
                                    if (ticket != null)
                                    {
                                        valid = true;
                                        ticket.TicketID = tIDint;
                                        logger.Debug("Updating Ticket Record {0}", tIDint);
                                        // New Priority
                                        do
                                        {
                                            TicketPriority();
                                            var newPrior = Console.ReadLine();
                                        
                                            if (newPrior.Trim() == "1")
                                            {
                                                newPrior = "Low";
                                                ticket.Priority = newPrior;
                                                valid = true;
                                                logger.Debug("Priority Updated");
                                            }
                                            else if (newPrior.Trim() == "2")
                                            {
                                                newPrior = "Medium";
                                                ticket.Priority = newPrior;
                                                valid = true;
                                                logger.Debug("Priority Updated");
                                            }
                                            else if (newPrior.Trim() == "3")
                                            {
                                                newPrior = "High";
                                                ticket.Priority = newPrior;
                                                valid = true;
                                                logger.Debug("Priority Updated");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Bad input");
                                                Console.WriteLine();
                                                valid = false;
                                            }
                                        } while (valid == false);

                                        // New Status
                                        do
                                        {
                                            TicketStatus();
                                            var newStat = Console.ReadLine();
                                        
                                            if (newStat.Trim() == "1")
                                            {
                                                newStat = "Open";
                                                ticket.Status = newStat;
                                                valid = true;
                                                logger.Debug("Status Updated");
                                            }
                                            else if (newStat.Trim() == "2")
                                            {
                                                newStat = "Closed";
                                                ticket.Status = newStat;
                                                valid = true;
                                                logger.Debug("Status Updated");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Bad input");
                                                Console.WriteLine();
                                                valid = false;
                                            }
                                        } while (valid == false);

                                        // Add new Assigned and WatchingUsers
                                        string addUser = "";
                                        do
                                        {
                                            Console.WriteLine("Enter a user ID - when finished enter 'end'");
                                            addUser = Console.ReadLine();

                                            if (addUser != "end")
                                            {
                                                int aUser = 0;

                                                // validate userID
                                                while (!int.TryParse(addUser, out aUser))
                                                {
                                                    Console.WriteLine("Enter the numerical user ID of the submitter");
                                                    addUser = Console.ReadLine();
                                                    logger.Debug("Invalid UserID");
                                                }

                                                var user = dbContext.Users.Where(u => u.UserID == aUser).FirstOrDefault();

                                                if (user != null)
                                                {
                                                    User u = new User();
                                                    u.UserID = aUser;

                                                    // add an assigned user and create a new WatchingUser item
                                                    ticket.Assigned = u.UserID;
                                                    ticket.WatchingUsers.Add(new WatchingUser { UserID = u.UserID });
                                                }
                                            }
                                        } while (addUser != "end");

                                        // save changes
                                        dbContext.SaveChanges();
                                        Console.WriteLine("You updated {0}", tID);
                                        logger.Trace("Ticket {0} was updated", tID);
                                    }
                                    else
                                    {
                                        valid = false;
                                    }

                                } while (valid == false);
                            }

                                Console.WriteLine();
                                Console.WriteLine("-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*");
                                Console.WriteLine();
                                break;
                            }

                        else if (LSelection == "4") // Exit
                        {
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Bad Input");
                            Console.WriteLine();
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
                    Console.WriteLine();
                    break;
                }
                DisplayMenu(false);
                MSelection = Console.ReadLine().Trim();
            } while (MSelection != "3");
            return;
        }

        //Display Main Menu
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

        //Display Menu for User Record Interactions
        public static void UserMenu()
        {
            Console.WriteLine("1) Display User Information");
            Console.WriteLine("2) Add New User");
            Console.WriteLine("3) Update User Information");
            Console.WriteLine("4) Exit Application");
        }

        //Display Menu for Ticket Record Interactions
        public static void TicketMenu()
        {
            Console.WriteLine("1) Display Tickets");
            Console.WriteLine("2) Add New Ticket");
            Console.WriteLine("3) Update Existing Ticket");
            Console.WriteLine("4) Exit Application");
        }

        public static void TicketStatus()
        {
            Console.WriteLine("Enter status of ticket");
            Console.WriteLine("1) Open");
            Console.WriteLine("2) Closed");
        }

        public static void TicketPriority()
        {
            Console.WriteLine("Enter priority of ticket");
            Console.WriteLine("1) Low");
            Console.WriteLine("2) Medium");
            Console.WriteLine("3) High");
        }

        public void ValidateUserID(string test, int IDint)
        {
            while (!int.TryParse(test, out IDint))
            {
                Console.WriteLine("Enter the numerical user ID");
                test = Console.ReadLine();
                logger.Debug("Invalid UserID");
            }
        }
    }
}
