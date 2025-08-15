using System;
using System.Collections.Generic;
using System.Linq;
using test.Models;
using test.Repositories;
using test.Services;
using System.Text;

namespace test.Models
{
    // VIP等级和所需经验值，可以根据需要调整
    public static class VipLevels
    {
        public static Dictionary<int, int> PointsRequired = new Dictionary<int, int>
    {
        { 0, 0 },     // 普通会员
        { 1, 100 },  // VIP1
        { 2, 300 },  // VIP2
        { 3, 600 },  // VIP3
        { 4, 1000 }  // VIP4
    };

        public static int GetNextLevelPoints(int currentLevel)
        {
            if (PointsRequired.ContainsKey(currentLevel + 1))
            {
                return PointsRequired[currentLevel + 1];
            }
            return -1; // -1 表示已经是最高等级
        }

        /// <summary>
        /// 新增方法：根据总积分计算出当前的VIP等级
        /// </summary>
        /// <param name="totalPoints">用户总积分</param>
        /// <returns>当前VIP等级</returns>
        public static int CalculateVipLevel(int totalPoints)
        {
            // 从最高等级开始向下遍历，找到第一个积分满足的等级
            // PointsRequired.Keys.OrderByDescending() 可以确保从高到低检查
            foreach (var level in PointsRequired.Keys.OrderByDescending(k => k))
            {
                if (totalPoints >= PointsRequired[level])
                {
                    return level;
                }
            }
            return 0; // 默认等级为0
        }
    }
}
