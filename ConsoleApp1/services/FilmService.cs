using CinemaTicketSystem.Services;
using ConsoleApp1.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
// 新增的FilmService类
public class FilmService
{
    private readonly DatabaseService _dbService;

    public FilmService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public DataTable GetAllFilms()
    {
        string sql = "SELECT FilmName, Genre, Score FROM film WHERE endDate IS NULL OR endDate >= SYSDATE";
        return _dbService.ExecuteQuery(sql);
    }

    public DataTable GetFilmSections(string filmName, DateTime day)
    {
        string sql = @"
    SELECT s.sectionID, s.hallNo, s.timeID, 
           TO_CHAR(t.startTime, 'YYYY-MM-DD HH24:MI') AS showTime
    FROM section s
    JOIN timeslot t ON s.timeID = t.timeID
    WHERE s.filmName = :filmName 
    AND TRUNC(t.startTime) = TRUNC(:day)
    ORDER BY t.startTime";

        var parameters = new[]
        {
        new OracleParameter("filmName", OracleDbType.Varchar2, 50) { Value = filmName },
        new OracleParameter("day", OracleDbType.Date) { Value = day }
    };

        return _dbService.ExecuteQuery(sql, parameters);
    }
}