using test.Models; 
using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;


namespace test.Repositories 
{
    /// <summary>
    /// 票务调度数据仓储接口。
    /// </summary>
    public interface IShowingRepository
    {
        List<Section> GetAvailableSections(string filmName, DateTime? date = null); // 获取指定电影在某日期（或所有日期）的可用场次
        List<SeatHall> GetHallSeatLayout(int hallNo); // 获取指定影厅的座位布局
        List<Ticket> GetSoldSeatsForSection(int sectionId); // 获取指定场次已售出的座位
        void AddTicket(Ticket ticket, OracleTransaction transaction = null); // 新增：添加票务记录
        Section GetSectionById(int sectionId); // 根据id获取场次信息
    }
}
