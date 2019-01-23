using System;
using System.ComponentModel;
using System.IO;


namespace TeamCreation
{
    public class TeamInput : IDataErrorInfo, INotifyPropertyChanged
    {
        public string FilePath { get; set; }
        public string PlayersPerTeamString { get; set; }
        
        #region IDataErrorInfo
        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }

        string IDataErrorInfo.this[string colName]
        {
            get
            {
                string result = null;
                
                if (colName == "FilePath")
                {
                    if (string.IsNullOrEmpty(FilePath))
                        result += "Please select a file.  ";

                    if (!File.Exists(FilePath))
                        result += "No file found at specified path.  ";

                    if (Path.GetExtension(FilePath) != ".csv")
                        result += "Must be a .csv file.  ";
                }
                if (colName == "PlayersPerTeamString")
                {
                    if (string.IsNullOrEmpty(PlayersPerTeamString))
                        result += "Please choose the number of players for each team.  ";
                }

                return result;
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
