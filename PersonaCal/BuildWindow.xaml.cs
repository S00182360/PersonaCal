﻿using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PersonaCal
{
    public partial class BuildWindow : Window
    {
        public PersonasContainers db = new PersonasContainers();
        public List<Persona> masterList;
        public List<Persona> personaOneList;

        public BuildWindow()
        {
            InitializeComponent();
            masterList = db.Personas.ToList();
            lbxTeamSelect.ItemsSource = masterList;
            MainWindow.teamList = new List<Persona>();
            cbxSearchType.ItemsSource = MainWindow.sortBy;
            cbxSearchType.SelectedIndex = 0;
        }

        #region NAV BUTTONS
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = Owner as MainWindow;
            this.Close();
        }

        private void BtnCalc_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = Owner as MainWindow;
            CalculateWindow calc = new CalculateWindow();
            this.Close();
            calc.ShowDialog();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = Owner as MainWindow;
            SearchWindow search = new SearchWindow();
            this.Close();
            search.ShowDialog();
        }

        private void BtnBuild_Click(object sender, RoutedEventArgs e)
        {
            //No action
        }
        #endregion NAV BUTTONS

        #region ADD/REMOVE BUTTONS
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            //Check selection is not null
            if(lbxTeamSelect.SelectedItem != null)
            {
                //Check if Persona is already in list and notify user to select a different one.
                if(CheckDuplicate(lbxTeamSelect.SelectedItem as Persona))
                {
                    MessageBox.Show("That Persona is already in your team.\nPlease choose a different Persona.");
                }
                else
                {
                    //Max of 8 personas in a team
                    if(MainWindow.teamList.Count < 8)
                    {
                        MainWindow.teamList.Add(lbxTeamSelect.SelectedItem as Persona);
                        //displays selected personas in listbox
                        lbxTeam.ItemsSource = null;
                        MainWindow.teamList.Sort();
                        lbxTeam.ItemsSource = MainWindow.teamList;
                    }
                    else
                    {
                        MessageBox.Show("Max 8 Personas per team.\nPlease remove 1 or more to add this selection.",
                                        "Team Builder", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
        }

        private bool CheckDuplicate(Persona p1)
        {
            bool isDuplicate = false;
            foreach (var per in MainWindow.teamList)
            {
                if (per.Id == p1.Id)
                    return isDuplicate = true;
                else
                    isDuplicate = false;
            }
            return isDuplicate;
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            //Remove a persona from team
            if(lbxTeam.SelectedItem != null)
            {
                MainWindow.teamList.Remove(lbxTeam.SelectedItem as Persona);
                lbxTeam.ItemsSource = null;
                lbxTeam.ItemsSource = MainWindow.teamList;
            }
        }
        #endregion NAV BUTTONS

        #region SAVE/LOAD BUTTONS
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string json = JsonConvert.SerializeObject(
                                                    MainWindow.teamList, Formatting.Indented, 
                                                    new JsonSerializerSettings()
                                                    {
                                                        //Since arcana is an object that Persona has
                                                        //ignore looping when writing. Does not affect loading
                                                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                    });

            //Choose where to save team file, save as .json by default
            SaveFileDialog sfd = new SaveFileDialog
            {
                DefaultExt = ".json"
            };
            bool? result = sfd.ShowDialog();


            if (result == true)
            {
                //Save persona info to json file
                string filename = sfd.FileName;
                using (StreamWriter sw = new StreamWriter(filename))
                {
                    sw.Write(json);
                }
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            //Choose file to load
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                string fileName = ofd.FileName;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string json = sr.ReadToEnd();
                    MainWindow.teamList.Clear();
                    MainWindow.teamList = JsonConvert.DeserializeObject<List<Persona>>(json);
                }
            }
            //reset team listbox 
            lbxTeam.ItemsSource = null;
            lbxTeam.ItemsSource = MainWindow.teamList;
        }
        #endregion

        #region SEARCH LOGIC
        private void TbxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Search etiher by name or ny arcana. Name by default
            if (cbxSearchType.SelectedIndex == 0)
            {
                lbxTeamSelect.ItemsSource = null;
                string search = tbxSearch.Text.ToLower();
                var filterList = masterList.Where(p => p.Name.ToLower().Contains(search));
                lbxTeamSelect.ItemsSource = filterList.ToList();
            }
            else if (cbxSearchType.SelectedIndex == 1)
            {
                lbxTeamSelect.ItemsSource = null;
                string search = tbxSearch.Text.ToLower();
                var filterList = masterList.Where(p => p.Arcana.ArcanaName.ToLower().Contains(search));
                lbxTeamSelect.ItemsSource = filterList.ToList();
            }
        }

        private void CbxSearchType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbxSearch.Clear();

        }
        #endregion
    }
}
