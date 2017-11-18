using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DivisasMVVM2.Classes;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Reflection;
using System.Windows.Input;

namespace DivisasMVVM2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Atributos
        public string Message
        {
            set
            {
                if (message != value)
                {
                    message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
                }
            }
            get
            {
                return message;
            }
        }
        private ExchangeRates exchangeRates;

        private decimal amount;

        private double sourceRate;

        private double targetRate;

        private bool isRunning;

        private bool isEnabled;
        #endregion
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events
        #region Propiedades
        public ObservableCollection<Rate> Rates { get; set; }
        private string message;
        public decimal Amount
        {
            set
            {
                if (amount != value)
                {
                    amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Amount"));
                }
            }
            get
            {
                return amount;
            }
        }

        public double SourceRate
        {
            set
            {
                if (sourceRate != value)
                {
                    sourceRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourceRate"));
                }
            }
            get
            {
                return sourceRate;
            }
        }

        public double TargetRate
        {
            set
            {
                if (targetRate != value)
                {
                    targetRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetRate"));
                }
            }
            get
            {
                return targetRate;
            }
        }

        public bool IsRunning
        {
            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get
            {
                return isRunning;
            }
        }

        public bool IsEnabled
        {
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
            get
            {
                return isEnabled;
            }
        }

        #endregion
        #region Constructores
        public MainViewModel()
        {
            Rates = new ObservableCollection<Rate>();
            Message = "Enter an amount, select a source rate, select a target rate and press the convert button";
            //IsEnabled = false;
            LoadRates();
            //GetRates();
        }
        #endregion
        #region Metodos
        private async void LoadRates()
        {
            IsRunning = true;
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://openexchangerates.org");
                var url = "/api/latest.json?app_id=f490efbcd52d48ee98fd62cf33c47b9e";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Message = response.StatusCode.ToString();
                    //await App.Current.MainPage.DisplayAlert("Error", response.StatusCode.ToString(), "Aceptar");
                    //IsRunning = false;
                    //IsEnabled = false;
                    IsRunning = false;
                    return;
                }
                var result = await response.Content.ReadAsStringAsync();
                exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(result);
            }
            catch(Exception ex)
            {
                Message = ex.Message;
                IsRunning = false;
                return;
            }
            ConvertRates();
            IsRunning = false;
            IsEnabled = true;
            
        }

        private void ConvertRates()
        {
            //throw new NotImplementedException();
            Rates.Clear();
            var type = typeof(Rates);
            var properties = type.GetRuntimeFields();

            foreach (var property in properties)
            {
                var code = property.Name.Substring(1, 3);
                Rates.Add(new Rate
                {
                    Code = code,
                    TaxRate = (double)property.GetValue(exchangeRates.Rates),
                });
            }
        }

        private async void GetRates()
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://openexchangerates.org");
                var url = "/api/latest.json?app_id=f490efbcd52d48ee98fd62cf33c47b9e";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Error", response.StatusCode.ToString(), "Aceptar");
                    IsRunning = false;
                    IsEnabled = false;
                    return;
                }

                var result = await response.Content.ReadAsStringAsync();
                exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(result);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "Aceptar");
                IsRunning = false;
                IsEnabled = false;
                return;
            }

            LoadRates();
            IsRunning = false;
            IsEnabled = true;
        }
        #endregion
        #region Comandos
        public ICommand ConvertCommand { get { return new GalaSoft.MvvmLight.Command.RelayCommand(ConvertMoney); } }

        private async void ConvertMoney()
        {
            if (Amount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter an amount", "Acept");
                return;
            }

            if (SourceRate <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must select a source rate", "Acept");
                return;
            }

            if (TargetRate <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must select a target rate", "Acept");
                return;
            }

            decimal amountConverted = amount / (decimal)sourceRate * (decimal)targetRate;

            Message = string.Format("{0:N2} = {1:N2}", amount, amountConverted);
        }
        #endregion
    }
}
