using ConsoleApp1.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace CinemaTicketSystem.Services
{
    public class TicketService
    {
        private readonly DatabaseService _dbService;

        public TicketService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public Ticket GetTicketBySeat(int sectionId, int row, int column)
        {
            const string sql = @"
        SELECT 
            ticketID, 
            lineNo, 
            columnNo, 
            price, 
            state
        FROM ticket 
        WHERE sectionID = :p_sectionId 
          AND lineNo = :p_row 
          AND columnNo = :p_col";

            var parameters = new[]
            {
        new OracleParameter("p_sectionId", OracleDbType.Int32) { Value = sectionId },
        new OracleParameter("p_row", OracleDbType.Int32) { Value = row },
        new OracleParameter("p_col", OracleDbType.Int32) { Value = column }
    };

            try
            {
                DataTable dt = _dbService.ExecuteQuery(sql, parameters);
                if (dt.Rows.Count == 0) return null;

                DataRow rowData = dt.Rows[0];
                return new Ticket
                {
                    TicketID = rowData["ticketID"].ToString(),
                    SectionID = sectionId,
                    LineNo = Convert.ToInt32(rowData["lineNo"]),
                    ColumnNo = Convert.ToInt32(rowData["columnNo"]),
                    Price = Convert.ToInt32(rowData["price"]),  // 建议用Decimal存储金额
                    State = rowData["state"].ToString()
                };
            }
            catch (OracleException ex) when (ex.Number == 1745)
            {
                Console.WriteLine("[数据库错误] 绑定变量名无效");
                Console.WriteLine($"请检查SQL中的参数名是否使用保留字: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[系统错误] 查询失败: {ex.Message}");
                return null;
            }
        }

        // 获取某个场次的所有座位状态
        public DataTable GetSeatStatus(int sectionId)
        {
            string sql = @"
            SELECT lineNo, columnNo, state 
            FROM ticket 
            WHERE sectionID = :sectionId
            ORDER BY lineNo, columnNo";

            var parameter = new OracleParameter("sectionId", OracleDbType.Int32)
            {
                Value = sectionId
            };

            return _dbService.ExecuteQuery(sql, parameter);
        }

        // 检查并锁定特定座位
        public bool LockSpecificSeat(int sectionId, int lineNo, int columnNo)
        {
            string sql = @"
            UPDATE ticket 
            SET state = '锁定中'
            WHERE sectionID = :sectionId 
            AND lineNo = :lineNo 
            AND columnNo = :columnNo
            AND state = '可售'";

            var parameters = new[]
            {
            new OracleParameter("sectionId", OracleDbType.Int32) { Value = sectionId },
            new OracleParameter("lineNo", OracleDbType.Int32) { Value = lineNo },
            new OracleParameter("columnNo", OracleDbType.Int32) { Value = columnNo }
        };

            return _dbService.ExecuteNonQuery(sql, parameters) > 0;
        }



        public bool SellTicket(string ticketId)
        {
            string sql = "UPDATE ticket SET state = '已售' WHERE ticketID = :ticketId";
            var parameter = new OracleParameter("ticketId", OracleDbType.Varchar2, 20)
            {
                Value = ticketId
            };

            return _dbService.ExecuteNonQuery(sql, parameter) > 0;
        }

        public bool ReleaseTicket(string ticketId)
        {
            string sql = "UPDATE ticket SET state = '可售' WHERE ticketID = :ticketId";
            var parameter = new OracleParameter("ticketId", OracleDbType.Varchar2, 20)
            {
                Value = ticketId
            };

            return _dbService.ExecuteNonQuery(sql, parameter) > 0;
        }

        public DataTable GetValidTickets()
        {
            string sql = @"
                SELECT t.ticketID, t.sectionID, s.filmName, 
                       t.lineNo, t.columnNo, t.state, t.price
                FROM ticket t
                JOIN section s ON t.sectionID = s.sectionID
                WHERE t.state IN ('可售', '已锁定')
                ORDER BY t.sectionID, t.lineNo, t.columnNo";

            return _dbService.ExecuteQuery(sql);
        }


        public int GetAvailableTicketsCount(int sectionId)
        {
            string sql = "SELECT COUNT(*) FROM ticket WHERE sectionID = :sectionId AND state = '可售'";
            var parameter = new OracleParameter("sectionId", OracleDbType.Int32)
            {
                Value = sectionId
            };

            var result = _dbService.ExecuteScalar(sql, parameter);
            return Convert.ToInt32(result);
        }
    }
}