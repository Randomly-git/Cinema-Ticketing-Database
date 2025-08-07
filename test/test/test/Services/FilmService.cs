using test.Models;
using test.Repositories; // 引用 Repositories 命名空间
using System;
using System.Collections.Generic;
using System.Linq;

namespace test.Services
{
    /// <summary>
    /// 影片业务服务实现。
    /// </summary>
    public class FilmService : IFilmService
    {
        private readonly IFilmRepository _filmRepository;

        public FilmService(IFilmRepository filmRepository)
        {
            _filmRepository = filmRepository;
        }

        /// <summary>
        /// 获取所有当前可供查询的电影列表。
        /// </summary>
        public List<Film> GetAvailableFilms()
        {
            // 可以在这里添加业务逻辑，例如只返回正在上映的电影
            return _filmRepository.GetAllFilms();
        }

        /// <summary>
        /// 获取电影详细信息，包括演职人员和场次。
        /// </summary>
        public Film GetFilmDetails(string filmName)
        {
            var film = _filmRepository.GetFilmByName(filmName);
            if (film != null)
            {
                film.CastMembers = _filmRepository.GetCastByFilmName(filmName);
                film.Sections = _filmRepository.GetSectionsByFilmName(filmName);
                // 场次中的 MovieHall 和 TimeSlot 对象在 GetSectionsByFilmName 中已经填充
            }
            return film;
        }

        /// <summary>
        /// 获取电影演职人员。
        /// </summary>
        public List<Cast> GetFilmCast(string filmName)
        {
            return _filmRepository.GetCastByFilmName(filmName);
        }

        /// <summary>
        /// 获取电影场次。
        /// </summary>
        public List<Section> GetFilmSections(string filmName)
        {
            return _filmRepository.GetSectionsByFilmName(filmName);
        }

        // FilmService.cs
        public List<Film> GetFilmStatistics(string orderBy)
        {
            var films = _filmRepository.GetAllFilms();

            switch (orderBy)
            {
                case "score":
                    return films.OrderByDescending(f => f.Score).ToList();
                case "admissions":
                    return films.OrderByDescending(f => f.Admissions).ToList();
                case "filmLength":
                    return films.OrderByDescending(f => f.FilmLength).ToList();
                case "releaseDate":
                    return films.OrderByDescending(f => f.ReleaseDate ?? DateTime.MinValue).ToList();
                case "normalPrice":
                    return films.OrderByDescending(f => f.NormalPrice).ToList();
                default:
                    return films.OrderByDescending(f => f.BoxOffice).ToList();
            }
        }
    }

}