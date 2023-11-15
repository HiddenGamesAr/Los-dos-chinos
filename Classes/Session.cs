using System;

namespace Los_dos_chinos
{
    public class Session
    {
        //public int SesionID { get; set; }
        public int UserID { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public Session(int _userID, DateTime _date, string _startTime, string _endTime = null)
        {
            UserID = _userID;
            Date = _date;
            StartTime = _startTime;
            EndTime = _endTime;
        }
    }
}
