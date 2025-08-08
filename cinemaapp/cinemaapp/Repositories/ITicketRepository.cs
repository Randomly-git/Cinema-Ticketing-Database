using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.Models;

namespace cinemaapp.Repositories
{
    public interface ITicketRepository
    {
        //根据票号返回section相关信息
        Ticket GetTicketWithSection(string ticketId);
      
    }
}
