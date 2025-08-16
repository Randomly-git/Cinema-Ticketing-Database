using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.Models;

namespace cinemaapp.Services
{
    public interface ITicketService
    {
        Ticket GetTicketWithSection(string ticketId);
    }

}
