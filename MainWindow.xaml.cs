using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace TeamCreation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Private Fields
        private static int _playersPerTeam;
        private readonly Style _styleSuccess = Application.Current.FindResource("StyleSuccess") as Style;
        private readonly Style _styleFailure = Application.Current.FindResource("StyleFailure") as Style;
        private static FileReadErrorCode _fileError;
        #endregion

        #region Private Properties
        private TeamInput TeamInput { get; set; }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TeamInput();
        }

        #region Button Click
        private void btnMakeTeams_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsValidUserInput())
                    return;
            
                GetUserInput();
                
                // extract the player names/ratings to dictionary and also keep the player names in a separate list
                Dictionary<string, float> participants = GetParticipantsFromCsv(TeamInput.FilePath);
                List<string> names = participants.Keys.ToList();
                
                // output the enum description as the error message if there are issues
                if (_fileError != FileReadErrorCode.Ok)
                {
                    tbMessage.Text = "Failed to read data.  Error: " + _fileError.GetDescription();                    
                    tbMessage.Style = _styleFailure;
                    return;
                }

                // how many teams should we have?  int will drop off the remainder so unassigned players are possible  
                int numOfTeams = names.Count / _playersPerTeam;

                // assign the players to teams
                List<Team> finalTeams = BuildTeams(participants, names, _playersPerTeam, numOfTeams);

                // output the result to a csv file in the same directory that was used for the player list
                DataTable dt = CreateTable(finalTeams, _playersPerTeam);
                string outputFilePath = Path.GetDirectoryName(TeamInput.FilePath) + @"\finalTeams_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                CreateCSVFile(dt, outputFilePath);

                tbMessage.Style = _styleSuccess;
                tbMessage.Text = "Success!  File Path: " + outputFilePath;
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
                tbMessage.Style = _styleFailure;
                tbMessage.Text = "Oops!  Something went wrong.";
            }
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    DefaultExt = ".csv",
                    Filter = "csv files (*.csv)|*.csv"
                };

                // Display OpenFileDialog by calling ShowDialog method 
                bool? result = fileDialog.ShowDialog();
                
                // Get the selected file name and display in a TextBox 
                if (result == true)
                {
                    // Open document 
                    string filename = fileDialog.FileName;
                    txtFile.Text = filename;
                }

                // update validation
                var be = BindingOperations.GetBindingExpression(txtFile, TextBox.TextProperty);
                if (be != null)
                    be.UpdateSource();
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }
        }
        #endregion

        #region Validation
        private bool IsValidUserInput()
        {
            string errors = string.Empty;

            if (Validation.GetHasError(txtFile))
            {
                var filePathErrors = Validation.GetErrors(txtFile);

                foreach (ValidationError error in filePathErrors)
                {
                    errors += (error.ErrorContent ?? string.Empty).ToString();
                }

                tbMessage.Style = _styleFailure;
                tbMessage.Text = errors;
                return false;
            }

            if (Validation.GetHasError(cbPlayersPerTeam))
            {
                var playerCountErrors = Validation.GetErrors(cbPlayersPerTeam);

                foreach (ValidationError error in playerCountErrors)
                {
                    errors += (error.ErrorContent ?? string.Empty).ToString();
                }

                tbMessage.Style = _styleFailure;
                tbMessage.Text = errors;
                return false;
            }

            return true;
        }
        #endregion

        #region Retrieve User Input
        private void GetUserInput()
        {
            TeamInput = new TeamInput {FilePath = txtFile.Text};

            // retrieve file path and the number of players per team
            string comboBoxString = ((ComboBoxItem)cbPlayersPerTeam.SelectedItem).Content.ToString();
            int tempInt;
            if (int.TryParse(comboBoxString, out tempInt))
                _playersPerTeam = tempInt;
        }
        #endregion

        #region File Handling
        private Dictionary<string, float> GetParticipantsFromCsv(string filePath)
        {
            _fileError = FileReadErrorCode.Ok;
            var participants = new Dictionary<string, float>();

            // validate file (error code set here and checked later)
            if (!IsValidFile(filePath))
                return participants;

            // extract data from csv
            using (var csvParser = new TextFieldParser(filePath))
            {
                csvParser.CommentTokens = new[] { "#" };
                csvParser.SetDelimiters(",");
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    // error code set here and checked later
                    if (fields != null && HasValidRowData(fields[0], fields[1]))
                        participants.Add(fields[0], float.Parse(fields[1]));
                    else
                        break;          
                }
            }
            
            return participants;
        }

        private bool HasValidRowData(string name, string rating)
        {
            float tempFloat;

            //validate Name field
            if (string.IsNullOrEmpty(name))
            {
                _fileError = FileReadErrorCode.BlankName;
                return false;
            }

            //validate Rating field
            if (!float.TryParse(rating, out tempFloat))
            {
                _fileError = FileReadErrorCode.InvalidRating;
                return false;
            }

            return true;
        }

        private bool IsValidFile(string filePath)
        { 
            //file must exist
            if (!File.Exists(filePath))
            {
                _fileError = FileReadErrorCode.InvalidFilePath;
                return false;
            }

            //must be a csv file
            if (Path.GetExtension(filePath) != ".csv")
            {
                _fileError = FileReadErrorCode.InvalidFileType;
                return false;
            }

            return true;
        }

        private void CreateCSVFile(DataTable dt, string strFilePath)
        {
            // Create the CSV file to which grid data will be exported.
            using (var sw = new StreamWriter(strFilePath, false))
            {
                // First we will write the headers.
                //DataTable dt = m_dsProducts.Tables[0];
                int iColCount = dt.Columns.Count;

                for (int i = 0; i < iColCount; i++)
                {
                    sw.Write(dt.Columns[i]);
                    if (i < iColCount - 1)
                    {
                        sw.Write(",");
                    }
                }

                sw.Write(sw.NewLine);

                // Now write all the rows.
                foreach (DataRow dr in dt.Rows)
                {
                    for (int i = 0; i < iColCount; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            sw.Write(dr[i].ToString());
                        }

                        if (i < iColCount - 1)
                        {
                            sw.Write(",");
                        }
                    }

                    sw.Write(sw.NewLine);
                }
            }
        }
        #endregion

        #region Team Creation Logic
        private static List<Team> BuildTeams(Dictionary<string, float> participants, List<string> names, int playersPerTeam, int numOfTeams)
        {
            //initialize
            //teams = new List<Team>();           
            var finalTeams = new List<Team>();
            
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            // the average of the players ratings is the same number as the average of all possible team combinations
            double avg = participants.Average(s => s.Value) * playersPerTeam;

            while (finalTeams.Count < numOfTeams)
            {
                // remove the highest player from the list (they will be put into a team first)
                var highestPlayer = new KeyValuePair<string, float>();
                foreach (var kvp in participants)
                {
                    if (kvp.Value > highestPlayer.Value)
                        highestPlayer = kvp;
                }
                participants.Remove(highestPlayer.Key);
                names.Remove(highestPlayer.Key);

                // now we can get the possible combinations with 1 fewer player
                var tList = GetKCombs(names, playersPerTeam - 1);

                // find team that is closest to our target score once the highest player is added back in
                double targetTeamScore = avg - highestPlayer.Value;
                Team selectedTeam = PlaceTeamsIntoClass(tList, participants, targetTeamScore);

                // add the targeted player back to the team
                selectedTeam.Player.Add(highestPlayer.Key, highestPlayer.Value);
                selectedTeam.TeamRating = selectedTeam.TeamRating + highestPlayer.Value;

                // remove the used players from the list of participants
                foreach (var player in selectedTeam.Player)
                {
                    if (participants.Contains(player))
                        participants.Remove(player.Key);
                }

                finalTeams.Add(selectedTeam);
            }
            
            //sw.Stop(); 

            return finalTeams;
        }
        
        private static Team PlaceTeamsIntoClass(IEnumerable<IEnumerable<string>> teamList, Dictionary<string, float> participants, double targetScore)
        {
            int count = 0;
            var distanceFromTargetScore = double.MaxValue;
            var selectedTeam = new Team();

            foreach (var team in teamList)
            //Parallel.For(0, teamList.Count(), i =>
            //Parallel.ForEach(teamList, (team) =>
            {
                var teamClass = new Team();
                float teamScore = 0.0F;

                foreach (var player in team)
                {
                    float tempFloat;
                    if (participants.TryGetValue(player, out tempFloat))
                    {
                        teamScore += tempFloat;
                        teamClass.Player.Add(player, tempFloat);
                    }
                }

                teamClass.TeamRating = teamScore;
                teamClass.Id = count;
                //teams.Add(teamClass);

                //find team with closest score to targetScore
                double tempDistance = Math.Abs(targetScore - teamScore);
                if (tempDistance < distanceFromTargetScore)
                {
                    selectedTeam = teamClass;
                    distanceFromTargetScore = tempDistance;
                }

                count++;
            }

            return selectedTeam;
        }        

        static IEnumerable<IEnumerable<T>> GetKCombs<T>(List<T> nameList, int length) where T : IComparable
        {
            if (length == 1) 
                return nameList.Select(t => new[] { t });

            //get k combinations with no repititions (order doesn't matter)
            return GetKCombs(nameList, length - 1)
                .SelectMany(t => nameList.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new[] { t2 }));
        }
        #endregion

        #region Teams To DataTable
        static DataTable CreateTable(List<Team> finalTeams, int playersPerTeam)
        {
            // create the columns
            var dt = new DataTable();
            dt.Columns.Add("TeamRating", typeof(int));

            for (int i = 1; i <= playersPerTeam; i++)
            {
                string colText = i.ToString();
                dt.Columns.Add("Name" + colText, typeof(string));
                dt.Columns.Add("Rating" + colText, typeof(int));
            }

            // fill in the rows
            foreach (Team t in finalTeams)
            {
                var pList = t.Player.Keys.ToList();
                var row = dt.NewRow();
                row["TeamRating"] = t.TeamRating;

                for (int i = 0; i < pList.Count; i++)
                {
                    string colText = (i + 1).ToString();
                    row["Name" + colText] = pList[i];
                    row["Rating" + colText] = t.Player[pList[i]];
                }

                dt.Rows.Add(row);
            }

            return dt;
        }
        #endregion

        #region Logging
        private static void LogError(string error)
        {
            // ensure the log folder exists
            string folder = Directory.GetCurrentDirectory() + @"\Logs\";
            Directory.CreateDirectory(folder);

            // format the file name
            string date = DateTime.Now.ToString("yyyyMMdd");
            string fileName = folder + @"\TeamCreation_" + date + ".log";

            // write log in existing file (if exists) or write log in new file
            var timeStamp = DateTime.UtcNow.AddHours(-6);
            if (File.Exists(fileName))
            {
                using (StreamWriter sw = File.AppendText(fileName))
                    WriteLog(sw, timeStamp, error);
            }
            else
            {
                using (var sw = new StreamWriter(fileName))
                    WriteLog(sw, timeStamp, error);
            }
        }

        private static void WriteLog(StreamWriter sw, DateTime timeStamp, string error)
        {
            sw.WriteLine(timeStamp + " - " + error);
        }
        #endregion
    }
}
