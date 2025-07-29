using System;
using System.Collections.Generic;
using test.Models; 

namespace test.Services 
{
    /// <summary>
    /// 票务调度业务服务接口。
    /// </summary>
    public interface IShowingService
    {
        List<Section> GetFilmShowings(string filmName, DateTime? date = null); // 获取电影的场次列表
        // 修复点：将参数从 int sectionId 改为 Section section 
        Dictionary<string, List<string>> GetAvailableSeats(Section section); // 获取指定场次的可用座位
    }
}
