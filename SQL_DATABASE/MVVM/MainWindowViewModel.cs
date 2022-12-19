using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using SQL_DATABASE.Datenbanken;
using SQL_DATABASE.Datenbanken.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace SQL_DATABASE.MVVM
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ExecuteQuery QueryInstance = null;

        public MainWindowViewModel()
        {
            this.PreparedConnections = new ObservableCollection<Connection>();

            foreach (var item in ConnectionManager.Instance.GetAllConnections().ToList())
            {
                this.PreparedConnections.Add(new Connection(item.Key, item.Value));
            }

            this.RefreshEnabled = false;
        }

        public Connection SelectedConnection
        {
            get => base.GetProperty<Connection>(nameof(SelectedConnection));
            set
            {
                base.SetProperty(nameof(SelectedConnection), value);
                this.DataBaseSelected(value);
            }
        }

        public ObservableCollection<Connection> PreparedConnections
        {
            get => base.GetProperty<ObservableCollection<Connection>>(nameof(PreparedConnections));
            set => base.SetProperty(nameof(PreparedConnections), value);
        }

        public List<DataTable> CurrentTables
        {
            get => base.GetProperty<List<DataTable>>(nameof(CurrentTables));
            set => base.SetProperty(nameof(CurrentTables), value);
        }

        public bool RefreshEnabled
        {
            get => base.GetProperty<bool>(nameof(RefreshEnabled));
            set => base.SetProperty(nameof(RefreshEnabled), value);
        }

        public string TableHeader
        {
            get
            {
                if (this.CurrentTables == null)
                    return $"No Database Selected";
                else if (this.CurrentTables.Count <= 1)
                    return $"{this.SelectedConnection.Name} - Table ({this.CurrentTables.Count})";
                else
                    return $"{this.SelectedConnection.Name} - Tables ({this.CurrentTables.Count})";
            }
        }

        private void DataBaseSelected(Connection db)
        {
            if (db == null)
                return;

            Debug.WriteLine($"User wählt Db [{db.Name}] aus.");

            this.QueryInstance = new ExecuteQuery(db.MyConnection);
            this.CurrentTables = this.QueryInstance.GetAllContentFromDatabase($"{this.SelectedConnection.Name}");
            this.RefreshEnabled = true;
            base.OnPropertyChanged(nameof(TableHeader));
        }

        public ICommand AddConnection => new RelayCommand(param =>
        {
            try
            {
                //TODO AddConnection Window
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(MainWindowViewModel)},{nameof(AddConnection)},\nEX :[{ex}]");
            }
        });

        public ICommand Refresh => new RelayCommand(param =>
        {
            try
            {
                this.CurrentTables = this.QueryInstance.GetAllContentFromDatabase($"{this.SelectedConnection.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(MainWindowViewModel)},{nameof(Refresh)},\nEX :[{ex}]");
            }
        });

        public ICommand Update => new RelayCommand(param =>
        {
            try
            {
                foreach (var table in this.CurrentTables)
                {
                    this.QueryInstance.Update(table, this.SelectedConnection.Name);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(MainWindowViewModel)},{nameof(Update)},\nEX :[{ex}]");
            }
        });

        public ICommand AddTestTables => new RelayCommand(param =>
        {
            try
            {
                this.QueryInstance.CreateTestTables();
                this.CurrentTables = this.QueryInstance.GetAllContentFromDatabase($"{this.SelectedConnection.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(MainWindowViewModel)},{nameof(AddTestTables)},\nEX :[{ex}]");
            }
        });
    }
}
