using cinemaapp.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.Models;

namespace cinemaapp.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public Ticket GetTicketWithSection(string ticketId)
        {
            return _ticketRepository.GetTicketWithSection(ticketId);
        }
    }

}
